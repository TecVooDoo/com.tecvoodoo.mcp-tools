#if HAS_FEEL
#nullable enable
using com.IvanMurzak.McpPlugin;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace MCPTools.Feel.Editor
{
    [McpPluginToolType]
    public partial class Tool_Feel
    {
        static MMF_Player GetPlayer(string gameObjectName)
        {
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new System.Exception($"GameObject '{gameObjectName}' not found.");
            var player = go.GetComponent<MMF_Player>();
            if (player == null)
                throw new System.Exception($"'{gameObjectName}' has no MMF_Player component.");
            return player;
        }
    }
}
#endif
