#nullable enable
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.ALINE.Editor
{
    [McpPluginToolType]
    public partial class Tool_ALINE
    {
        static Color ParseColor(float r, float g, float b, float a = 1f)
            => new Color(Mathf.Clamp01(r), Mathf.Clamp01(g), Mathf.Clamp01(b), Mathf.Clamp01(a));
    }
}
