#nullable enable
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.SensorToolkit.Editor
{
    [McpPluginToolType]
    public partial class Tool_SensorToolkit
    {
        static string FormatV3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        static GameObject FindGO(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) throw new System.Exception($"GameObject '{name}' not found.");
            return go;
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }

        static T ParseEnum<T>(string value, T defaultValue) where T : struct, System.Enum
        {
            if (System.Enum.TryParse<T>(value, true, out T result))
                return result;
            return defaultValue;
        }
    }
}
