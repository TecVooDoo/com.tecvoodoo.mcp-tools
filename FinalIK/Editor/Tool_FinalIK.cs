#if HAS_FINALIK
#nullable enable
using com.IvanMurzak.McpPlugin;

namespace MCPTools.FinalIK.Editor
{
    [McpPluginToolType]
    public partial class Tool_FinalIK
    {
        static string FormatVector3(UnityEngine.Vector3 v)
            => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
    }
}
#endif
