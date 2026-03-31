#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using global::Naninovel;

namespace MCPTools.Naninovel.Editor
{
    public partial class Tool_Naninovel
    {
        [McpPluginTool("nani-list-backgrounds", Title = "Naninovel / List Backgrounds")]
        [Description(@"Lists all backgrounds registered in the Naninovel project configuration.
Shows background IDs and implementation types.
Does not require play mode — reads directly from project configuration assets.")]
        public string ListBackgrounds()
        {
            return MainThread.Instance.Run(() =>
            {
                var config = Configuration.GetOrDefault<BackgroundsConfiguration>();
                if (config == null)
                    return "ERROR: Could not load BackgroundsConfiguration.";

                var sb = new StringBuilder();
                sb.AppendLine("=== NANINOVEL BACKGROUNDS ===");

                var ids = new List<string>();
                config.Metadata.CollectIds(ids);

                if (ids.Count == 0)
                {
                    sb.AppendLine("  (no backgrounds registered)");
                    return sb.ToString();
                }

                sb.AppendLine($"  Total: {ids.Count}");
                sb.AppendLine();

                foreach (var id in ids)
                {
                    var meta = config.Metadata[id];
                    if (meta == null) continue;

                    sb.AppendLine($"  [{id}]");
                    sb.AppendLine($"    Implementation: {meta.Implementation ?? "(default)"}");
                    sb.AppendLine();
                }

                // Shared poses
                if (config.SharedPoses != null && config.SharedPoses.Count > 0)
                {
                    sb.AppendLine("  === SHARED POSES ===");
                    foreach (var pose in config.SharedPoses)
                        sb.AppendLine($"    {pose.Name}");
                }

                return sb.ToString();
            });
        }
    }
}
