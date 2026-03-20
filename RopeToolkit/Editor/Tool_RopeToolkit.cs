#if HAS_ROPE_TOOLKIT
#nullable enable
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.RopeToolkit.Editor
{
    [McpPluginToolType]
    public partial class Tool_RopeToolkit
    {
        static string FormatVector3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        static global::RopeToolkit.Rope GetRope(string gameObjectName)
        {
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new System.Exception($"GameObject '{gameObjectName}' not found.");
            var rope = go.GetComponent<global::RopeToolkit.Rope>();
            if (rope == null)
                throw new System.Exception($"'{gameObjectName}' has no Rope component.");
            return rope;
        }
    }
}
#endif
