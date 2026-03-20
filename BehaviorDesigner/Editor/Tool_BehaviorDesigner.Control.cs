#if HAS_BEHAVIOR_DESIGNER
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Opsive.BehaviorDesigner.Runtime;
using UnityEngine;

namespace MCPTools.BehaviorDesigner.Editor
{
    public partial class Tool_BehaviorDesigner
    {
        [McpPluginTool("bd-control", Title = "Behavior Designer / Control Tree")]
        [Description("Control behavior tree execution: enable, disable, or restart a BehaviorTree on a GameObject.")]
        public string Control(
            [Description("Name of the GameObject with BehaviorTree component(s).")]
            string gameObjectName,
            [Description("Action to perform: 'enable', 'disable', or 'restart'.")]
            string action,
            [Description("Index of the tree if multiple BehaviorTree components exist. Default 0.")]
            int? treeIndex = 0
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                BehaviorTree[] trees = go.GetComponents<BehaviorTree>();
                if (trees.Length == 0)
                    throw new Exception($"'{gameObjectName}' has no BehaviorTree component.");

                int idx = treeIndex ?? 0;
                if (idx < 0 || idx >= trees.Length)
                    throw new Exception($"Tree index {idx} out of range. '{gameObjectName}' has {trees.Length} tree(s) (0-{trees.Length - 1}).");

                BehaviorTree tree = trees[idx];
                string actionLower = action.ToLower().Trim();

                switch (actionLower)
                {
                    case "enable":
                        try
                        {
                            tree.EnableBehavior();
                        }
                        catch
                        {
                            // Fallback: just enable the component
                            tree.enabled = true;
                        }
                        return $"Enabled BehaviorTree [{idx}] on '{go.name}'.";

                    case "disable":
                        try
                        {
                            tree.DisableBehavior();
                        }
                        catch
                        {
                            // Fallback: just disable the component
                            tree.enabled = false;
                        }
                        return $"Disabled BehaviorTree [{idx}] on '{go.name}'.";

                    case "restart":
                        try
                        {
                            tree.DisableBehavior();
                            tree.EnableBehavior();
                        }
                        catch
                        {
                            tree.enabled = false;
                            tree.enabled = true;
                        }
                        return $"Restarted BehaviorTree [{idx}] on '{go.name}'.";

                    default:
                        throw new Exception($"Unknown action '{action}'. Use 'enable', 'disable', or 'restart'.");
                }
            });
        }
    }
}
#endif
