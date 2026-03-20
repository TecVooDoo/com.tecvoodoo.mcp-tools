#nullable enable
using com.IvanMurzak.McpPlugin;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MCPTools.AnimationRigging.Editor
{
    [McpPluginToolType]
    public partial class Tool_AnimationRigging
    {
        static GameObject FindGO(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) throw new System.Exception($"GameObject '{name}' not found.");
            return go;
        }

        static Transform FindTransform(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) throw new System.Exception($"GameObject '{name}' not found.");
            return go.transform;
        }
    }
}
