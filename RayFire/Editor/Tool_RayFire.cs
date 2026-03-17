#nullable enable
using com.IvanMurzak.McpPlugin;

namespace MCPTools.RayFire.Editor
{
    [McpPluginToolType]
    public partial class Tool_RayFire
    {
        static string FormatVector3(UnityEngine.Vector3 v)
            => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
    }
}
