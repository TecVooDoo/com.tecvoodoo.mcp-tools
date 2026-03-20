#nullable enable
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    [McpPluginToolType]
    public partial class Tool_AStarPathfinding
    {
        static string FormatVector3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        static GameObject FindGO(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
                throw new System.Exception($"GameObject '{name}' not found.");
            return go;
        }

        static T GetOrAdd<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            if (component == null)
                component = go.AddComponent<T>();
            return component;
        }
    }
}
