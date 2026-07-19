#if HAS_MALBERS_AC
#nullable enable
using com.IvanMurzak.McpPlugin;

namespace MCPTools.MalbersAC.Editor
{
    [McpPluginToolType]
    public partial class Tool_MalbersAC
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

        static T ParseEnum<T>(string value, T defaultValue) where T : struct, System.Enum
        {
            if (System.Enum.TryParse<T>(value, true, out var result))
                return result;
            return defaultValue;
        }
    }
}
#endif
