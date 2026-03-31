#nullable enable
using System.ComponentModel;
using System.IO;
using System.Linq;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;

namespace MCPTools.Naninovel.Editor
{
    public partial class Tool_Naninovel
    {
        [McpPluginTool("nani-read-script", Title = "Naninovel / Read Script")]
        [Description(@"Reads the contents of a Naninovel .nani script file.
Specify the script name (without extension) or a partial path.
Example: 'Scene1_1' or 'Scene1_1_HolographicHorse'.
Returns the full text content with line numbers.")]
        public string ReadScript(
            [Description("Script name (without .nani extension) or partial path to match. Case-insensitive.")]
            string scriptName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var guids = AssetDatabase.FindAssets("", new[] { "Assets" });
                var naniFiles = guids
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => p.EndsWith(".nani"))
                    .ToList();

                string nameLower = scriptName.ToLowerInvariant();

                // Try exact match first, then partial
                var match = naniFiles.FirstOrDefault(p =>
                    Path.GetFileNameWithoutExtension(p).Equals(scriptName, System.StringComparison.OrdinalIgnoreCase));

                if (match == null)
                {
                    match = naniFiles.FirstOrDefault(p =>
                        Path.GetFileNameWithoutExtension(p)!.ToLowerInvariant().Contains(nameLower));
                }

                if (match == null)
                    return $"ERROR: No .nani script found matching '{scriptName}'. Use nani-list-scripts to see available scripts.";

                string fullPath = Path.GetFullPath(match);
                if (!File.Exists(fullPath))
                    return $"ERROR: File not found at '{fullPath}'.";

                string content = File.ReadAllText(fullPath);
                string[] lines = content.Split('\n');

                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"=== {Path.GetFileName(match)} ({match}) ===");
                sb.AppendLine($"  Lines: {lines.Length}");
                sb.AppendLine();

                for (int i = 0; i < lines.Length; i++)
                    sb.AppendLine($"  {i + 1,4}: {lines[i].TrimEnd('\r')}");

                return sb.ToString();
            });
        }
    }
}
