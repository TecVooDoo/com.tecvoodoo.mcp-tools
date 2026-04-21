#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using PampelGames.UltimateTerrain;
using UnityEngine;

namespace MCPTools.UltimateTerrain.Editor
{
    public partial class Tool_UltimateTerrain
    {
        [McpPluginTool("ut-query", Title = "Ultimate Terrain / Query")]
        [Description(@"Reports state for Ultimate Terrain instances.
If gameObjectName provided, reports just that instance.
Otherwise lists all active UltimateTerrain instances in the scene.
Reports: position, scale, duration, animation enabled, multi-terrain state, module/layer counts, and IsExecuting/IsPaused status.")]
        public string Query(
            [Description("Optional: GameObject with UltimateTerrain. Omit to list all active.")] string? gameObjectName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();

                if (gameObjectName != null)
                {
                    var ut = GetUT(gameObjectName);
                    AppendUT(sb, ut);
                    return sb.ToString();
                }

                var all = UltimateTerrainAPI.GetActiveUltimateTerrains();
                sb.AppendLine($"Active UltimateTerrains: {all.Count}");
                foreach (var ut in all)
                    AppendUT(sb, ut);
                return sb.ToString();
            });
        }

        static void AppendUT(StringBuilder sb, global::PampelGames.UltimateTerrain.UltimateTerrain ut)
        {
            sb.AppendLine($"\nUltimateTerrain on '{ut.gameObject.name}':");
            sb.AppendLine($"  Status: {ut.componentStatus}  Initialized: {ut.initialized}");
            sb.AppendLine($"  Position: {FormatVec2(ut.position)}  Scale: {FormatVec2(ut.scale)}");
            sb.AppendLine($"  Duration: {ut.duration:F2}s  EnableAnimation: {ut.enableAnimation}  DelaySync: {ut.delaySync}");
            sb.AppendLine($"  MultiTerrain: active={ut.multiTerrainActive} count={ut.multiTerrainList?.Count ?? 0}");
            sb.AppendLine($"  Modules: height={ut.heightModules?.Count ?? 0} texture={ut.textureLayers?.Count ?? 0} details={ut.detailsLayers?.Count ?? 0} tree={ut.treeLayers?.Count ?? 0} prefab={ut.prefabLayers?.Count ?? 0}");
            sb.AppendLine($"  Runtime: IsExecuting={ut.IsExecuting()} IsPaused={ut.IsPaused()}");
            if (ut.terrain != null)
                sb.AppendLine($"  Terrain: {ut.terrain.name}");
        }
    }
}
