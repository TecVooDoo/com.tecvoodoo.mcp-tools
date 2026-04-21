#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;

namespace MCPTools.MKEdge.Editor
{
    public partial class Tool_MKEdge
    {
        [McpPluginTool("mkedge-toggle", Title = "MK Edge Detection / Toggle")]
        [Description(@"Enables or disables MK Edge Detection on a target.
For VolumeComponent: sets .active.
For RendererFeature: sets .isActive.")]
        public string Toggle(
            [Description("Target Volume GO, VolumeProfile, or UniversalRendererData name.")] string target,
            [Description("Enable (true) or disable (false).")] bool enabled
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var (vc, feature) = FindMKEdge(target);
                if (vc != null)
                {
                    vc.active = enabled;
                    EditorUtility.SetDirty(vc);
                    return $"OK: MK Edge Detection (Volume) on '{target}' active={enabled}.";
                }
                else if (feature != null)
                {
                    feature.SetActive(enabled);
                    return $"OK: MK Edge Detection (RendererFeature) on '{target}' isActive={enabled}.";
                }
                return "ERROR: target not found.";
            });
        }
    }
}
