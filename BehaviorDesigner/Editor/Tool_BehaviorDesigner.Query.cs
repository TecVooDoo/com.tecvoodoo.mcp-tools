#if HAS_BEHAVIOR_DESIGNER
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.BehaviorDesigner.Runtime.Tasks;
using Opsive.GraphDesigner.Runtime.Variables;
using UnityEngine;

namespace MCPTools.BehaviorDesigner.Editor
{
    public partial class Tool_BehaviorDesigner
    {
        [McpPluginTool("bd-query", Title = "Behavior Designer / Query Tree")]
        [Description("Query behavior tree state on a GameObject. Shows tree info, shared variables, and active task.")]
        public string Query(
            [Description("Name of the GameObject with BehaviorTree component(s).")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                BehaviorTree[] trees = go.GetComponents<BehaviorTree>();
                if (trees.Length == 0)
                    throw new Exception($"'{gameObjectName}' has no BehaviorTree component.");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"GameObject: {go.name} ({trees.Length} tree(s))");

                for (int i = 0; i < trees.Length; i++)
                {
                    BehaviorTree tree = trees[i];
                    sb.AppendLine($"\n--- Tree [{i}] ---");

                    try
                    {
                        string treeName = tree.BehaviorName ?? "(unnamed)";
                        sb.AppendLine($"  Name: {treeName}");
                    }
                    catch
                    {
                        sb.AppendLine("  Name: (unable to read)");
                    }

                    try
                    {
                        string groupName = tree.Group ?? "(none)";
                        sb.AppendLine($"  Group: {groupName}");
                    }
                    catch
                    {
                        sb.AppendLine("  Group: (unable to read)");
                    }

                    sb.AppendLine($"  Enabled: {tree.enabled}");

                    // Shared variables
                    try
                    {
                        List<SharedVariable> variables = tree.GetAllVariables();
                        if (variables != null)
                        {
                            sb.AppendLine($"  Variables ({variables.Count}):");
                            foreach (SharedVariable v in variables)
                            {
                                try
                                {
                                    string typeName = v.GetType().Name;
                                    object value = v.GetValue();
                                    string valueStr = value != null ? value.ToString()! : "null";
                                    sb.AppendLine($"    [{typeName}] {v.Name} = {valueStr}");
                                }
                                catch
                                {
                                    sb.AppendLine($"    [?] {v.Name} = (unable to read)");
                                }
                            }
                        }
                        else
                        {
                            sb.AppendLine("  Variables: none");
                        }
                    }
                    catch
                    {
                        sb.AppendLine("  Variables: (unable to enumerate)");
                    }

                    // Active task (play mode only)
                    try
                    {
                        if (Application.isPlaying)
                        {
                            // Try to get currently running tasks
                            List<Task> activeTasks = tree.FindTasks<Task>();
                            if (activeTasks != null)
                            {
                                StringBuilder activeNames = new StringBuilder();
                                foreach (Task task in activeTasks)
                                {
                                    try
                                    {
                                        TaskStatus status = task.NodeData.NodeStatus;
                                        if (status == TaskStatus.Running)
                                        {
                                            if (activeNames.Length > 0) activeNames.Append(", ");
                                            activeNames.Append(task.FriendlyName);
                                        }
                                    }
                                    catch { /* skip tasks we can't read */ }
                                }
                                if (activeNames.Length > 0)
                                    sb.AppendLine($"  Active tasks: {activeNames}");
                                else
                                    sb.AppendLine("  Active tasks: none running");
                            }
                        }
                        else
                        {
                            sb.AppendLine("  Active tasks: (not in play mode)");
                        }
                    }
                    catch
                    {
                        sb.AppendLine("  Active tasks: (unable to query)");
                    }
                }

                return sb.ToString();
            });
        }
    }
}
#endif
