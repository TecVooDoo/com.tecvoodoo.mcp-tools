#nullable enable
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;

namespace MCPTools.Naninovel.Editor
{
    public partial class Tool_Naninovel
    {
        [McpPluginTool("nani-list-scripts", Title = "Naninovel / List Scripts")]
        [Description(@"Lists all .nani scenario script files in the project.
Shows file names, paths, and file sizes. Searches the Assets/Scenario folder
and any other folders containing .nani files.")]
        public string ListScripts()
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== NANINOVEL SCRIPTS (.nani) ===");

                var guids = AssetDatabase.FindAssets("", new[] { "Assets" });
                var naniFiles = guids
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(p => p.EndsWith(".nani"))
                    .OrderBy(p => p)
                    .ToList();

                if (naniFiles.Count == 0)
                {
                    sb.AppendLine("  (no .nani files found)");
                    return sb.ToString();
                }

                sb.AppendLine($"  Total: {naniFiles.Count}");
                sb.AppendLine();

                string? currentFolder = null;
                foreach (var path in naniFiles)
                {
                    string folder = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "";
                    if (folder != currentFolder)
                    {
                        currentFolder = folder;
                        sb.AppendLine($"  [{folder}/]");
                    }

                    string fileName = Path.GetFileNameWithoutExtension(path);
                    string fullPath = Path.GetFullPath(path);
                    long size = File.Exists(fullPath) ? new FileInfo(fullPath).Length : 0;
                    sb.AppendLine($"    {fileName}.nani ({size} bytes)");
                }

                return sb.ToString();
            });
        }
    }
}
