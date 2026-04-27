#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.CityGen3D.Editor
{
    public partial class Tool_CityGen3D
    {
        [McpPluginTool("cg-find-road-at", Title = "CityGen3D / Find Road At Position")]
        [Description(@"Queries the road network at a world-space position.

mode options:
  at      -- Map.Instance.mapRoads.GetMapRoadAtWorldPosition(x, z, radius) -- exact lookup within radius
  nearest -- Map.Instance.mapRoads.GetNearestRoad(Vector2(x,z), out closestPos) -- nearest road regardless of distance

Returns the matched MapRoad's name and (for nearest) closest world position.
Returns 'no road found' if no match (mode='at') or 'no roads in map' (no roads loaded).")]
        public string FindRoadAt(
            [Description("World X coordinate.")] float x,
            [Description("World Z coordinate.")] float z,
            [Description("Lookup mode: 'at' (radius-based) or 'nearest'.")] string mode = "at",
            [Description("Search radius for mode='at' (default 5).")] float radius = 5f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireCityGen();
                var inst = MapInstance() ?? throw new System.Exception("CityGen3D.Map.Instance is null. No map loaded.");
                var mapRoads = Get(inst, "mapRoads") ?? throw new System.Exception("Map.mapRoads not available.");

                var sb = new StringBuilder();
                if (mode.Equals("at", System.StringComparison.OrdinalIgnoreCase))
                {
                    var road = Call(mapRoads, "GetMapRoadAtWorldPosition", x, z, radius);
                    if (road == null) return $"No road found at ({x}, {z}) within radius {radius}.";
                    sb.AppendLine($"Road at ({x}, {z}) within {radius}m:");
                    AppendRoad(sb, road);
                    return sb.ToString();
                }

                if (mode.Equals("nearest", System.StringComparison.OrdinalIgnoreCase))
                {
                    var pos = new Vector2(x, z);
                    var closestPos = Vector3.zero;
                    // GetNearestRoad(Vector2, ref Vector3)
                    var args = new object[] { pos, closestPos };
                    var method = mapRoads.GetType().GetMethod("GetNearestRoad", new[] { typeof(Vector2), typeof(Vector3).MakeByRefType() });
                    if (method == null)
                        throw new System.Exception("GetNearestRoad(Vector2, ref Vector3) not found on mapRoads.");
                    var road = method.Invoke(mapRoads, args);
                    var outPos = (Vector3)args[1];
                    if (road == null) return $"No nearest road found.";
                    sb.AppendLine($"Nearest road to ({x}, {z}):");
                    AppendRoad(sb, road);
                    sb.AppendLine($"  Closest point: ({outPos.x:F2}, {outPos.y:F2}, {outPos.z:F2})");
                    return sb.ToString();
                }

                throw new System.Exception($"Unknown mode '{mode}'. Use 'at' or 'nearest'.");
            });
        }

        static void AppendRoad(StringBuilder sb, object road)
        {
            string name = Get(road, "name") as string ?? Get(road, "Name") as string ?? road.ToString();
            sb.AppendLine($"  Name: {name}");
            // Common props on MapRoad: type, length, lanes, etc. -- print best-effort.
            foreach (var prop in new[] { "type", "Type", "length", "Length", "lanes", "Lanes", "speedLimit", "SpeedLimit", "highway", "Highway" })
            {
                var v = Get(road, prop);
                if (v != null) sb.AppendLine($"  {prop}: {v}");
            }
        }
    }
}
