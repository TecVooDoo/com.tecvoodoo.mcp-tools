#if HAS_DIALOGUE_SYSTEM
#nullable enable
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.DialogueSystem.Editor
{
    [McpPluginToolType]
    public partial class Tool_DialogueSystem
    {
        static GameObject? FindGameObjectByName(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            GameObject go = GameObject.Find(name);
            return go;
        }

        static Transform? FindTransformByName(string? name)
        {
            GameObject? go = FindGameObjectByName(name);
            return go != null ? go.transform : null;
        }
    }
}
#endif
