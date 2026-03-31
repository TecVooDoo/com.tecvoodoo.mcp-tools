#if HAS_BEHAVIOR_DESIGNER
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Opsive.BehaviorDesigner.Runtime;
using UnityEngine;

namespace MCPTools.BehaviorDesigner.Editor
{
    public partial class Tool_BehaviorDesigner
    {
        [McpPluginTool("bd-list-trees", Title = "Behavior Designer / List All Trees")]
        [Description("Lists all BehaviorTree components in the current scene with their name and status.")]
        public string ListTrees()
        {
            return MainThread.Instance.Run(() =>
            {
                BehaviorTree[] allTrees = UnityEngine.Object.FindObjectsByType<BehaviorTree>(FindObjectsSortMode.None);
                if (allTrees.Length == 0)
                    return "No BehaviorTree components found in the scene.";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"=== Behavior Trees in Scene ({allTrees.Length}) ===");

                for (int i = 0; i < allTrees.Length; i++)
                {
                    BehaviorTree tree = allTrees[i];
                    string treeName = tree.Name ?? "(unnamed)";
                    string goName = tree.gameObject.name;
                    bool enabled = tree.enabled;
                    int varCount = tree.SharedVariables != null ? tree.SharedVariables.Length : 0;
                    string statusStr = Application.isPlaying ? $" | Status: {tree.Status}" : "";

                    sb.AppendLine($"  [{i}] GO: {goName} | Tree: {treeName} | Enabled: {enabled} | Variables: {varCount}{statusStr}");
                }

                return sb.ToString();
            });
        }
    }
}
#endif
