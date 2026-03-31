#if HAS_BEHAVIOR_DESIGNER
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.BehaviorDesigner.Runtime.Components;
using Opsive.GraphDesigner.Runtime.Variables;
using UnityEngine;

namespace MCPTools.BehaviorDesigner.Editor
{
    public partial class Tool_BehaviorDesigner
    {
        [McpPluginTool("bd-query", Title = "Behavior Designer / Query Tree")]
        [Description("Query behavior tree state on a GameObject: tree name, enabled status, shared variables, and node info.")]
        public string Query(
            [Description("Name of the GameObject with BehaviorTree component(s).")]
            string gameObjectName,
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
                    throw new Exception($"Tree index {idx} out of range. '{gameObjectName}' has {trees.Length} tree(s).");

                BehaviorTree tree = trees[idx];
                StringBuilder sb = new StringBuilder();

                string treeName = tree.Name ?? "(unnamed)";
                sb.AppendLine($"=== BehaviorTree on '{go.name}' [{idx}/{trees.Length}] ===");
                sb.AppendLine($"  Name: {treeName}");
                sb.AppendLine($"  Enabled: {tree.enabled}");
                sb.AppendLine($"  StartWhenEnabled: {tree.StartWhenEnabled}");
                sb.AppendLine($"  PauseWhenDisabled: {tree.PauseWhenDisabled}");
                sb.AppendLine($"  UpdateMode: {tree.UpdateMode}");
                sb.AppendLine($"  EvaluationType: {tree.EvaluationType}");
                if (tree.EvaluationType == EvaluationType.Count)
                    sb.AppendLine($"  MaxEvaluationCount: {tree.MaxEvaluationCount}");

                // Runtime state (play mode only)
                if (Application.isPlaying)
                {
                    sb.AppendLine($"\n  --- Runtime State ---");
                    sb.AppendLine($"  Status: {tree.Status}");
                    sb.AppendLine($"  IsActive: {tree.IsActive()}");
                    sb.AppendLine($"  IsRunning: {tree.IsRunning()}");
                    sb.AppendLine($"  IsPaused: {tree.IsPaused()}");
                }

                // Shared Variables
                SharedVariable[]? vars = tree.SharedVariables;
                if (vars != null && vars.Length > 0)
                {
                    sb.AppendLine($"\n  --- Shared Variables ({vars.Length}) ---");
                    for (int i = 0; i < vars.Length; i++)
                    {
                        SharedVariable sv = vars[i];
                        try
                        {
                            string varName = sv.Name.ToString();
                            string typeName = sv.GetType().Name;
                            string valStr = "(unknown)";
                            try
                            {
                                object? val = sv.GetType().GetProperty("Value")?.GetValue(sv);
                                valStr = val?.ToString() ?? "null";
                            }
                            catch { }
                            sb.AppendLine($"    [{i}] {varName} ({typeName}) = {valStr}");
                        }
                        catch (Exception ex)
                        {
                            sb.AppendLine($"    [{i}] (error reading variable: {ex.Message})");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("\n  No shared variables.");
                }

                return sb.ToString();
            });
        }
    }
}
#endif
