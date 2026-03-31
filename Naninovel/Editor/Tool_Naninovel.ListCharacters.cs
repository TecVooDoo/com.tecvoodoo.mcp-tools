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
        [McpPluginTool("nani-list-characters", Title = "Naninovel / List Characters")]
        [Description(@"Lists all characters registered in the Naninovel project configuration.
Shows character IDs, display names, name colors, message colors, and implementation types.
Does not require play mode — reads directly from project configuration assets.")]
        public string ListCharacters()
        {
            return MainThread.Instance.Run(() =>
            {
                var config = Configuration.GetOrDefault<CharactersConfiguration>();
                if (config == null)
                    return "ERROR: Could not load CharactersConfiguration.";

                var sb = new StringBuilder();
                sb.AppendLine("=== NANINOVEL CHARACTERS ===");

                var ids = new List<string>();
                config.Metadata.CollectIds(ids);

                if (ids.Count == 0)
                {
                    sb.AppendLine("  (no characters registered)");
                    return sb.ToString();
                }

                sb.AppendLine($"  Total: {ids.Count}");
                sb.AppendLine();

                foreach (var id in ids)
                {
                    var meta = config.Metadata[id];
                    if (meta == null) continue;

                    sb.AppendLine($"  [{id}]");
                    if (!string.IsNullOrEmpty(meta.DisplayName))
                        sb.AppendLine($"    Display Name: {meta.DisplayName}");
                    sb.AppendLine($"    Implementation: {meta.Implementation ?? "(default)"}");
                    sb.AppendLine($"    Name Color: {meta.NameColor}");
                    sb.AppendLine($"    Message Color: {meta.MessageColor}");

                    if (meta.Poses != null && meta.Poses.Count > 0)
                    {
                        sb.Append("    Poses: ");
                        for (int i = 0; i < meta.Poses.Count; i++)
                        {
                            if (i > 0) sb.Append(", ");
                            sb.Append(meta.Poses[i].Name ?? "(unnamed)");
                        }
                        sb.AppendLine();
                    }
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
