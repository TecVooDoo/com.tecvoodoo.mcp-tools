#nullable enable
using com.IvanMurzak.McpPlugin;

namespace MCPTools.MalbersAC.Editor
{
    [McpPluginToolType]
    public partial class Tool_MalbersAC
    {
        static string FormatVector3(UnityEngine.Vector3 v)
            => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        static T ParseEnum<T>(string value, T defaultValue) where T : struct, System.Enum
        {
            if (System.Enum.TryParse<T>(value, true, out var result))
                return result;
            return defaultValue;
        }
    }
}
