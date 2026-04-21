#nullable enable
using System;
using com.IvanMurzak.McpPlugin;
using PampelGames.UltimateTerrain;
using UnityEngine;

namespace MCPTools.UltimateTerrain.Editor
{
    [McpPluginToolType]
    public partial class Tool_UltimateTerrain
    {
        static GameObject FindGO(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) throw new Exception($"GameObject '{name}' not found.");
            return go;
        }

        static global::PampelGames.UltimateTerrain.UltimateTerrain GetUT(string gameObjectName)
        {
            var go = FindGO(gameObjectName);
            var ut = go.GetComponent<global::PampelGames.UltimateTerrain.UltimateTerrain>();
            if (ut == null) throw new Exception($"'{gameObjectName}' has no UltimateTerrain component.");
            return ut;
        }

        static string FormatVec2(Vector2 v) => $"({v.x:F2}, {v.y:F2})";
        static string FormatVec3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
    }
}
