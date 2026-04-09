#nullable enable
using System;
using com.IvanMurzak.McpPlugin;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace MCPTools.AINavigation.Editor
{
    [McpPluginToolType]
    public partial class Tool_AINavigation
    {
        static GameObject FindGO(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) throw new Exception($"GameObject '{name}' not found.");
            return go;
        }

        static NavMeshSurface GetSurface(string gameObjectName)
        {
            var go = FindGO(gameObjectName);
            var surface = go.GetComponent<NavMeshSurface>();
            if (surface == null) throw new Exception($"'{gameObjectName}' has no NavMeshSurface component.");
            return surface;
        }

        static NavMeshLink GetLink(string gameObjectName)
        {
            var go = FindGO(gameObjectName);
            var link = go.GetComponent<NavMeshLink>();
            if (link == null) throw new Exception($"'{gameObjectName}' has no NavMeshLink component.");
            return link;
        }

        static string FormatVector3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
    }
}
