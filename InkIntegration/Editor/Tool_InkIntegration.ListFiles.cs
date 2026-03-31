#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Ink.UnityIntegration;

namespace MCPTools.InkIntegration.Editor
{
    public partial class Tool_InkIntegration
    {
        [McpPluginTool("ink-list-files", Title = "Ink / List Files")]
        [Description(@"Lists all .ink files detected in the project via InkLibrary.
Shows file path, whether it's a master file (compilable), whether auto-compile is enabled,
and whether a compiled JSON asset exists. Does not require play mode.")]
        public string ListFiles()
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== INK FILES ===");

                var library = InkLibrary.instance;
                if (library == null || library.Count == 0)
                {
                    sb.AppendLine("  (no .ink files found — try rebuilding InkLibrary)");
                    return sb.ToString();
                }

                sb.AppendLine($"  Total: {library.Count}");
                sb.AppendLine();

                int limit = System.Math.Min(library.Count, 100);
                for (int i = 0; i < limit; i++)
                {
                    var file = library[i];
                    string path = file.filePath ?? "(unknown path)";
                    string master = file.isMaster ? "Master" : "Include";
                    string autoCompile = file.compileAutomatically ? ", Auto-compile" : "";
                    string hasJson = file.jsonAsset != null ? ", Compiled" : ", Not compiled";
                    sb.AppendLine($"  {path} [{master}{autoCompile}{hasJson}]");
                }

                if (library.Count > 100)
                    sb.AppendLine($"  ... and {library.Count - 100} more files.");

                return sb.ToString();
            });
        }
    }
}
