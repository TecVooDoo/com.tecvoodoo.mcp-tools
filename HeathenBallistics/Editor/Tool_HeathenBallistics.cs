#nullable enable
using com.IvanMurzak.McpPlugin;
using Heathen.UnityPhysics;
using UnityEngine;

namespace MCPTools.HeathenBallistics.Editor
{
    [McpPluginToolType]
    public partial class Tool_HeathenBallistics
    {
        static string FormatVector3(Vector3 v) => $"({v.x:F3}, {v.y:F3}, {v.z:F3})";
        static string FormatVector2(Vector2 v) => $"({v.x:F3}, {v.y:F3})";

        static GameObject FindGO(string name)
        {
            var go = GameObject.Find(name);
            if (go == null)
                throw new System.Exception($"GameObject '{name}' not found.");
            return go;
        }
    }
}
