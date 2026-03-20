#if HAS_BEHAVIOR_DESIGNER
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.GraphDesigner.Runtime.Variables;
using UnityEngine;

namespace MCPTools.BehaviorDesigner.Editor
{
    public partial class Tool_BehaviorDesigner
    {
        [McpPluginTool("bd-set-variable", Title = "Behavior Designer / Set Variable")]
        [Description("Get or set a SharedVariable on a BehaviorTree by name.")]
        public string SetVariable(
            [Description("Name of the GameObject with BehaviorTree.")]
            string gameObjectName,
            [Description("Name of the shared variable.")]
            string variableName,
            [Description("Action: 'get' or 'set'. Default 'get'.")]
            string? action = "get",
            [Description("Value to set (for 'set' action). Parsed based on valueType.")]
            string? value = null,
            [Description("Type hint: 'bool', 'float', 'int', 'string', 'vector3'. Default 'string'.")]
            string? valueType = "string",
            [Description("Index of the tree if multiple. Default 0.")]
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
                    throw new Exception($"Tree index {idx} out of range.");

                BehaviorTree tree = trees[idx];
                string act = (action ?? "get").ToLower().Trim();

                PropertyName propName = new PropertyName(variableName);
                SharedVariable? sv = tree.GetVariable(propName);

                if (act == "get")
                {
                    if (sv == null)
                        return $"Variable '{variableName}' not found on BehaviorTree [{idx}] on '{go.name}'.";

                    string typeName = sv.GetType().Name;
                    string valStr = "(unknown)";
                    try
                    {
                        object? val = sv.GetType().GetProperty("Value")?.GetValue(sv);
                        valStr = val?.ToString() ?? "null";
                    }
                    catch { }
                    return $"Variable '{variableName}' ({typeName}) = {valStr}";
                }
                else if (act == "set")
                {
                    if (string.IsNullOrEmpty(value))
                        throw new ArgumentException("Value is required for 'set' action.", nameof(value));

                    string vt = (valueType ?? "string").ToLower().Trim();
                    bool success = false;

                    switch (vt)
                    {
                        case "bool":
                            if (bool.TryParse(value, out bool bv))
                                success = tree.SetVariableValue<bool>(propName, bv);
                            else
                                throw new ArgumentException($"Cannot parse '{value}' as bool.");
                            break;
                        case "float":
                            if (float.TryParse(value, out float fv))
                                success = tree.SetVariableValue<float>(propName, fv);
                            else
                                throw new ArgumentException($"Cannot parse '{value}' as float.");
                            break;
                        case "int":
                            if (int.TryParse(value, out int iv))
                                success = tree.SetVariableValue<int>(propName, iv);
                            else
                                throw new ArgumentException($"Cannot parse '{value}' as int.");
                            break;
                        case "vector3":
                            string[] parts = value.Replace("(", "").Replace(")", "").Split(',');
                            if (parts.Length == 3 &&
                                float.TryParse(parts[0].Trim(), out float vx) &&
                                float.TryParse(parts[1].Trim(), out float vy) &&
                                float.TryParse(parts[2].Trim(), out float vz))
                            {
                                success = tree.SetVariableValue<Vector3>(propName, new Vector3(vx, vy, vz));
                            }
                            else
                                throw new ArgumentException($"Cannot parse '{value}' as Vector3. Use 'x,y,z' format.");
                            break;
                        case "string":
                        default:
                            success = tree.SetVariableValue<string>(propName, value);
                            break;
                    }

                    return success
                        ? $"Set variable '{variableName}' = {value} ({vt}) on BehaviorTree [{idx}] on '{go.name}'."
                        : $"Failed to set variable '{variableName}'. Variable may not exist or type mismatch.";
                }
                else
                {
                    throw new ArgumentException($"Unknown action '{action}'. Use 'get' or 'set'.");
                }
            });
        }
    }
}
#endif
