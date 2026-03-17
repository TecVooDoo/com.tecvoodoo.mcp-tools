#nullable enable
using com.IvanMurzak.McpPlugin;

namespace MCPTools.MagicaCloth2.Editor
{
    [McpPluginToolType]
    public partial class Tool_MagicaCloth
    {
        static string FormatVector3(UnityEngine.Vector3 v)
            => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
    }
}
