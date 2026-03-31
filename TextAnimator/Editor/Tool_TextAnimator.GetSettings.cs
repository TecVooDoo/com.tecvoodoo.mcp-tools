#nullable enable
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Febucci.TextAnimatorForUnity;
using UnityEditor;

namespace MCPTools.TextAnimator.Editor
{
    public partial class Tool_TextAnimator
    {
        [McpPluginTool("ta-get-settings", Title = "Text Animator / Get Settings")]
        [Description(@"Lists all AnimatorSettings ScriptableObject assets in the project.
These control default behavior for Text Animator components (time scale, dynamic scaling, etc.).
Does not require play mode.")]
        public string GetSettings()
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== TEXT ANIMATOR SETTINGS ===");

                var guids = AssetDatabase.FindAssets("t:AnimatorSettingsScriptable");

                if (guids.Length == 0)
                {
                    sb.AppendLine("  (no AnimatorSettingsScriptable assets found)");
                    return sb.ToString();
                }

                sb.AppendLine($"  Total: {guids.Length}");
                sb.AppendLine();

                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var settings = AssetDatabase.LoadAssetAtPath<AnimatorSettingsScriptable>(path);
                    if (settings == null) continue;

                    sb.AppendLine($"  [{settings.name}]");
                    sb.AppendLine($"    Path: {path}");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }
    }
}
