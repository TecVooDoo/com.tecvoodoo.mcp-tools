#nullable enable
using com.IvanMurzak.McpPlugin;
using Heathen.UnityPhysics;
using UnityEngine;

namespace MCPTools.HeathenPhysics.Editor
{
    [McpPluginToolType]
    public partial class Tool_HeathenPhysics
    {
        static string FormatVector3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        static T RequireComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            if (c == null)
                throw new System.Exception($"'{go.name}' has no {typeof(T).Name} component.");
            return c;
        }

        static GameObject FindGO(string name)
        {
            var go = GameObject.Find(name);
            if (go == null)
                throw new System.Exception($"GameObject '{name}' not found.");
            return go;
        }
    }
}
