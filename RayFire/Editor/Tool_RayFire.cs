#nullable enable
using com.IvanMurzak.McpPlugin;

namespace MCPTools.RayFire.Editor
{
    [McpPluginToolType]
    public partial class Tool_RayFire
    {
        static string FormatVector3(UnityEngine.Vector3 v)
            => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        /// <summary>Instance id as a decimal-digit string -- matches the MCP EntityId wire format.
        /// Unity 6000.3 deprecated GetInstanceID, and 6000.5 made EntityId-to-int a hard error.</summary>
        static string InstanceIdOf(UnityEngine.Object obj)
#if UNITY_6000_3_OR_NEWER
            => obj.GetEntityId().ToString();
#else
            => obj.GetInstanceID().ToString();
#endif
    }
}
