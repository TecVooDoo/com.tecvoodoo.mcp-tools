#nullable enable
using com.IvanMurzak.McpPlugin;

namespace MCPTools.Flexalon.Editor
{
    [McpPluginToolType]
    public partial class Tool_Flexalon
    {
        static string FormatVector3(UnityEngine.Vector3 v)
            => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
    }
}
