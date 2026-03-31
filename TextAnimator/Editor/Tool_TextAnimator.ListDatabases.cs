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
        [McpPluginTool("ta-list-databases", Title = "Text Animator / List Databases")]
        [Description(@"Lists all AnimationsDatabase assets in the project.
These databases contain registered Text Animator effects.
Does not require play mode.")]
        public string ListDatabases()
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== TEXT ANIMATOR DATABASES ===");

                var guids = AssetDatabase.FindAssets("t:AnimationsDatabase");

                if (guids.Length == 0)
                {
                    sb.AppendLine("  (no AnimationsDatabase assets found)");
                    return sb.ToString();
                }

                sb.AppendLine($"  Total: {guids.Length}");
                sb.AppendLine();

                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var db = AssetDatabase.LoadAssetAtPath<AnimationsDatabase>(path);
                    if (db == null) continue;

                    sb.AppendLine($"  [{db.name}]");
                    sb.AppendLine($"    Path: {path}");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }
    }
}
