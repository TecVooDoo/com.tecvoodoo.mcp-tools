#nullable enable
using com.IvanMurzak.McpPlugin;
using Unity.Cinemachine;
using UnityEngine;

namespace MCPTools.Cinemachine.Editor
{
    [McpPluginToolType]
    public partial class Tool_Cinemachine
    {
        static string FormatV3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null) c = go.AddComponent<T>();
            return c;
        }

        static GameObject FindGO(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) throw new System.Exception($"GameObject '{name}' not found.");
            return go;
        }
    }
}
