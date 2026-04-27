#nullable enable
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.CityGen3D.Editor
{
    public partial class Tool_CityGen3D
    {
        [McpPluginTool("cg-find-feature-at", Title = "CityGen3D / Find Feature At Position")]
        [Description(@"Finds a CityGen3D feature (building / surface / entity / tree) within a search radius of a world-space position.

category options (case-insensitive):
  building -- Map.mapBuildings list
  surface  -- Map.mapSurfaces list
  feature  -- Map.mapFeatures list (parks, plazas, water bodies, etc.)
  entity   -- Map.mapEntities list (props, signs, fixtures)
  tree     -- Map.mapTrees list

Returns up to maxResults closest matches with name and bounds-center distance.")]
        public string FindFeatureAt(
            [Description("World X coordinate.")] float x,
            [Description("World Z coordinate.")] float z,
            [Description("Category: building | surface | feature | entity | tree")] string category,
            [Description("Search radius (meters, default 50).")] float radius = 50f,
            [Description("Max results (default 5).")] int maxResults = 5
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireCityGen();
                var inst = MapInstance() ?? throw new System.Exception("CityGen3D.Map.Instance is null. No map loaded.");

                string field = category.Trim().ToLowerInvariant() switch
                {
                    "building" => "mapBuildings",
                    "surface"  => "mapSurfaces",
                    "feature"  => "mapFeatures",
                    "entity"   => "mapEntities",
                    "tree"     => "mapTrees",
                    _ => throw new System.Exception($"Unknown category '{category}'. Use building | surface | feature | entity | tree.")
                };

                var collection = Get(inst, field) ?? throw new System.Exception($"{field} not available on Map.");
                // Most map collections expose a public list. Try common names.
                string[] listProps = { "Buildings", "Surfaces", "Features", "Entities", "Trees", "List", "Items" };
                IList? items = null;
                foreach (var p in listProps)
                {
                    var v = Get(collection, p);
                    if (v is IList ilist) { items = ilist; break; }
                }
                if (items == null && collection is IList directList) items = directList;

                if (items == null)
                    throw new System.Exception($"Could not enumerate {field} -- no IList property found.");

                var origin = new Vector2(x, z);
                var hits = new System.Collections.Generic.List<(object item, float dist)>();
                foreach (var entry in items)
                {
                    if (entry == null) continue;
                    Vector3? pos = TryGetPosition(entry);
                    if (pos == null) continue;
                    float d = Vector2.Distance(origin, new Vector2(pos.Value.x, pos.Value.z));
                    if (d <= radius) hits.Add((entry, d));
                }

                hits.Sort((a, b) => a.dist.CompareTo(b.dist));
                var sb = new StringBuilder();
                sb.AppendLine($"{category} matches within {radius}m of ({x}, {z}): {hits.Count}");
                foreach (var (item, dist) in hits.Take(maxResults))
                {
                    string name = Get(item, "name") as string ?? Get(item, "Name") as string ?? item.ToString();
                    sb.AppendLine($"  - {name}  dist={dist:F1}m");
                }
                if (hits.Count > maxResults) sb.AppendLine($"  ... ({hits.Count - maxResults} more)");
                return sb.ToString();
            });
        }

        static Vector3? TryGetPosition(object item)
        {
            // CityGen3D map objects expose various position accessors.
            foreach (var name in new[] { "Position", "position", "WorldPosition", "Center", "BoundsCenter", "GetPosition" })
            {
                var v = Get(item, name);
                if (v is Vector3 v3) return v3;
                if (v is Vector2 v2) return new Vector3(v2.x, 0, v2.y);
            }
            // Method calls
            foreach (var name in new[] { "GetPosition", "GetCenter", "GetWorldPosition" })
            {
                var v = Call(item, name);
                if (v is Vector3 v3) return v3;
                if (v is Vector2 v2) return new Vector3(v2.x, 0, v2.y);
            }
            // Transform-derived
            var go = Get(item, "gameObject");
            if (go is UnityEngine.GameObject g) return g.transform.position;
            return null;
        }
    }
}
