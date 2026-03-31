#nullable enable
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Ink.UnityIntegration;
using UnityEditor;

namespace MCPTools.InkIntegration.Editor
{
    public partial class Tool_InkIntegration
    {
        [McpPluginTool("ink-get-story-info", Title = "Ink / Get Story Info")]
        [Description(@"Loads a compiled Ink story (.json) and reports its structure:
total content lines, choice points, and global variables.
Specify the .ink file name (without extension). The file must be compiled first.")]
        public string GetStoryInfo(
            [Description("The .ink file name (without extension) whose compiled JSON to analyze.")]
            string fileName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var library = InkLibrary.instance;
                if (library == null || library.Count == 0)
                    return "ERROR: No .ink files found in the project.";

                string nameLower = fileName.ToLowerInvariant();
                var match = library.inkLibrary.FirstOrDefault(f =>
                {
                    string? path = f.filePath;
                    if (path == null) return false;
                    string name = Path.GetFileNameWithoutExtension(path);
                    return name.ToLowerInvariant() == nameLower ||
                           name.ToLowerInvariant().Contains(nameLower);
                });

                if (match == null)
                    return $"ERROR: No .ink file found matching '{fileName}'.";

                if (match.jsonAsset == null)
                    return $"ERROR: '{match.filePath}' has not been compiled yet. Use ink-compile first.";

                var sb = new StringBuilder();
                sb.AppendLine($"=== INK STORY: {match.filePath} ===");

                try
                {
                    var story = new Ink.Runtime.Story(match.jsonAsset.text);

                    // Count variables
                    int varCount = 0;
                    var varNames = new System.Collections.Generic.List<string>();
                    foreach (string varName in story.variablesState)
                    {
                        varCount++;
                        if (varCount <= 50)
                            varNames.Add($"    {varName} = {story.variablesState[varName]}");
                    }

                    sb.AppendLine($"  Can Continue: {story.canContinue}");
                    sb.AppendLine($"  Variables: {varCount}");
                    foreach (var v in varNames)
                        sb.AppendLine(v);
                    if (varCount > 50)
                        sb.AppendLine($"    ... and {varCount - 50} more variables.");

                    // Run through to count content and choices
                    int lineCount = 0;
                    int choicePointCount = 0;
                    while (story.canContinue)
                    {
                        story.Continue();
                        lineCount++;
                        if (story.currentChoices.Count > 0)
                            choicePointCount++;

                        if (lineCount > 10000)
                        {
                            sb.AppendLine("  (stopped counting at 10,000 lines)");
                            break;
                        }
                    }

                    sb.AppendLine($"  Content Lines (first path): {lineCount}");
                    sb.AppendLine($"  Choice Points Encountered: {choicePointCount}");
                    if (story.currentChoices.Count > 0)
                    {
                        sb.AppendLine($"  Final Choices:");
                        foreach (var choice in story.currentChoices)
                            sb.AppendLine($"    [{choice.index}] {choice.text}");
                    }

                    // Tags
                    if (story.globalTags != null && story.globalTags.Count > 0)
                    {
                        sb.AppendLine($"  Global Tags:");
                        foreach (var tag in story.globalTags)
                            sb.AppendLine($"    #{tag}");
                    }
                }
                catch (System.Exception ex)
                {
                    sb.AppendLine($"  ERROR loading story: {ex.Message}");
                }

                return sb.ToString();
            });
        }
    }
}
