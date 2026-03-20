#if HAS_BEHAVIOR_DESIGNER
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.GraphDesigner.Runtime.Variables;
using UnityEngine;
using System.Collections.Generic;

namespace MCPTools.BehaviorDesigner.Editor
{
    public partial class Tool_BehaviorDesigner
    {
        [McpPluginTool("bd-list-trees", Title = "Behavior Designer / List Trees")]
        [Description("Lists all BehaviorTree components in the current scene with their name, group, enabled state, and variable count.")]
        public string ListTrees()
        {
            return MainThread.Instance.Run(() =>
            {
                BehaviorTree[] trees = UnityEngine.Object.FindObjectsByType<BehaviorTree>(FindObjectsSortMode.None);
                if (trees.Length == 0)
                    return "No BehaviorTree components found in the scene.";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Found {trees.Length} BehaviorTree(s):");

                for (int i = 0; i < trees.Length; i++)
                {
                    BehaviorTree tree = trees[i];
                    string goName = tree.gameObject.name;
                    int instanceId = tree.gameObject.GetInstanceID();

                    string treeName = "(unnamed)";
                    try { treeName = tree.BehaviorName ?? "(unnamed)"; } catch { /* DLL API safety */ }

                    string groupName = "(none)";
                    try { groupName = tree.Group ?? "(none)"; } catch { /* DLL API safety */ }

                    bool enabled = tree.enabled;

                    int varCount = 0;
                    try
                    {
                        List<SharedVariable> variables = tree.GetAllVariables();
                        if (variables != null)
                            varCount = variables.Count;
                    }
                    catch { /* DLL API safety */ }

                    sb.AppendLine($"  [{instanceId}] {goName} | Tree: {treeName} | Group: {groupName} | Enabled: {enabled} | Vars: {varCount}");
                }

                return sb.ToString();
            });
        }
    }
}
#endif
