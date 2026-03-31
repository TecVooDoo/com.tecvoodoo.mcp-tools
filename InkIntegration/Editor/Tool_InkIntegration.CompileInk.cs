#nullable enable
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Ink.UnityIntegration;

namespace MCPTools.InkIntegration.Editor
{
    public partial class Tool_InkIntegration
    {
        [McpPluginTool("ink-compile", Title = "Ink / Compile")]
        [Description(@"Compiles .ink files to JSON. Specify a file name to compile a specific file,
or leave empty to compile all master files. Returns compilation status.")]
        public string CompileInk(
            [Description("Optional .ink file name (without extension) to compile. Leave empty to compile all master files.")]
            string? fileName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();
                var library = InkLibrary.instance;

                if (library == null || library.Count == 0)
                    return "ERROR: No .ink files found in the project.";

                InkFile[] filesToCompile;

                if (!string.IsNullOrEmpty(fileName))
                {
                    string nameLower = fileName!.ToLowerInvariant();
                    var match = library.inkLibrary.FirstOrDefault(f =>
                    {
                        string? path = f.filePath;
                        if (path == null) return false;
                        string name = System.IO.Path.GetFileNameWithoutExtension(path);
                        return name.ToLowerInvariant() == nameLower ||
                               name.ToLowerInvariant().Contains(nameLower);
                    });

                    if (match == null)
                        return $"ERROR: No .ink file found matching '{fileName}'.";

                    filesToCompile = new[] { match };
                    sb.AppendLine($"Compiling: {match.filePath}");
                }
                else
                {
                    filesToCompile = library.inkLibrary.Where(f => f.isMaster).ToArray();
                    sb.AppendLine($"Compiling {filesToCompile.Length} master file(s)...");
                }

                InkCompiler.CompileInk(filesToCompile);
                sb.AppendLine("Compilation queued. Check Unity console for results.");

                return sb.ToString();
            });
        }
    }
}
