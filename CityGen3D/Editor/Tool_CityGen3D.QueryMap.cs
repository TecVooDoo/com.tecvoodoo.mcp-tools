#nullable enable
using System.Collections;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace MCPTools.CityGen3D.Editor
{
    public partial class Tool_CityGen3D
    {
        [McpPluginTool("cg-query-map", Title = "CityGen3D / Query Map")]
        [Description(@"Reports CityGen3D.Map.Instance state.

Reports map roads / buildings / features / surfaces / trees counts, the origin coordinate,
and whether a Generator prefab is present in the scene.

Returns a non-fatal message if no Map or Generator is loaded.")]
        public string QueryMap()
        {
            return MainThread.Instance.Run(() =>
            {
                RequireCityGen();
                var sb = new StringBuilder();
                var inst = MapInstance();
                if (inst == null)
                {
                    sb.AppendLine("CityGen3D.Map.Instance is null (no map loaded). Run cg-generate to build.");
                }
                else
                {
                    var data = Get(inst, "data");
                    var origin = data != null ? Call(data, "GetOrigin") : null;

                    sb.AppendLine("=== CityGen3D.Map.Instance ===");
                    sb.AppendLine($"  Origin:    {(origin != null ? origin.ToString() : "(none)")}");
                    sb.AppendLine($"  Roads:     {CountList(Get(inst, "mapRoads"), "Roads")}  (mapRoads)");
                    sb.AppendLine($"  Buildings: {CountList(Get(inst, "mapBuildings"), "Buildings")}  (mapBuildings)");
                    sb.AppendLine($"  Features:  {CountList(Get(inst, "mapFeatures"), "Features")}  (mapFeatures)");
                    sb.AppendLine($"  Surfaces:  {CountList(Get(inst, "mapSurfaces"), "Surfaces")}  (mapSurfaces)");
                    sb.AppendLine($"  Trees:     {CountList(Get(inst, "mapTrees"), "Trees")}  (mapTrees)");
                    sb.AppendLine($"  Entities:  {CountList(Get(inst, "mapEntities"), "Entities")}  (mapEntities)");
                }

                var gen = FindGenerator();
                sb.AppendLine();
                sb.AppendLine($"Generator in scene: {(gen != null ? "'" + gen.gameObject.name + "'" : "(none)")}");
                if (gen != null)
                {
                    sb.AppendLine($"  Generator GameObject path: {gen.gameObject.scene.name}/{gen.gameObject.name}");
                }
                return sb.ToString();
            });
        }
    }
}
