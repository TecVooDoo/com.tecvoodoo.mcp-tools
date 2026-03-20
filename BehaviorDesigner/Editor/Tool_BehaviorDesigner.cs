#if HAS_BEHAVIOR_DESIGNER
#nullable enable
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.BehaviorDesigner.Editor
{
    [McpPluginToolType]
    public partial class Tool_BehaviorDesigner
    {
        static GameObject FindGO(string gameObjectName)
        {
            GameObject go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new System.Exception($"GameObject '{gameObjectName}' not found.");
            return go;
        }
    }
}
#endif
