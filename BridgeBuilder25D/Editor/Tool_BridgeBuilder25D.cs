#if HAS_BRIDGEBUILDER25D
#nullable enable
using com.IvanMurzak.McpPlugin;
using Kamgam.BridgeBuilder25D;
using UnityEngine;

namespace MCPTools.BridgeBuilder25D.Editor
{
    [McpPluginToolType]
    public partial class Tool_BridgeBuilder25D
    {
        static Bridge25D GetBridge(string gameObjectName)
        {
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new System.Exception($"GameObject '{gameObjectName}' not found.");
            var bridge = go.GetComponent<Bridge25D>();
            if (bridge == null)
                throw new System.Exception($"'{gameObjectName}' has no Bridge25D component.");
            return bridge;
        }
    }
}
#endif
