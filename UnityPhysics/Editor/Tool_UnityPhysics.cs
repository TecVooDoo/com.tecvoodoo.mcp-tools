#nullable enable
using com.IvanMurzak.McpPlugin;
using Unity.Mathematics;
using UnityEngine;

namespace MCPTools.UnityPhysics.Editor
{
    [McpPluginToolType]
    public partial class Tool_UnityPhysics
    {
        static string FormatV3(Vector3 v) => $"({v.x:F3}, {v.y:F3}, {v.z:F3})";

        static string FormatFloat3(float3 v) => $"({v.x:F3}, {v.y:F3}, {v.z:F3})";

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }

        static GameObject FindGO(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) throw new System.Exception($"GameObject '{name}' not found.");
            return go;
        }
    }
}
