#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Febucci.TextAnimatorForUnity;
using UnityEngine;

namespace MCPTools.TextAnimator.Editor
{
    public partial class Tool_TextAnimator
    {
        [McpPluginTool("ta-find-components", Title = "Text Animator / Find Components")]
        [Description(@"Finds all TextAnimator components in the current scene.
Shows the GameObject name, component type, animation loop mode, and current text.
Requires a scene to be open.")]
        public string FindComponents()
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== TEXT ANIMATOR COMPONENTS IN SCENE ===");

                var components = Object.FindObjectsByType<TextAnimatorComponentBase>(FindObjectsSortMode.None);

                if (components.Length == 0)
                {
                    sb.AppendLine("  (no TextAnimator components found in scene)");
                    return sb.ToString();
                }

                sb.AppendLine($"  Total: {components.Length}");
                sb.AppendLine();

                foreach (var comp in components)
                {
                    sb.AppendLine($"  [{comp.gameObject.name}]");
                    sb.AppendLine($"    Type: {comp.GetType().Name}");
                    sb.AppendLine($"    Animation Loop: {comp.animationLoop}");
                    sb.AppendLine($"    Dynamic Scaling: {comp.useDynamicScaling}");
                    string text = comp.textFull ?? "(null)";
                    if (text.Length > 100) text = text.Substring(0, 100) + "...";
                    sb.AppendLine($"    Text: {text}");
                    sb.AppendLine();
                }

                return sb.ToString();
            });
        }
    }
}
