#if HAS_BEHAVIOR_DESIGNER
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.GraphDesigner.Runtime.Variables;
using UnityEngine;

namespace MCPTools.BehaviorDesigner.Editor
{
    public partial class Tool_BehaviorDesigner
    {
        [McpPluginTool("bd-set-variable", Title = "Behavior Designer / Set Variable")]
        [Description("Get or set a SharedVariable on a BehaviorTree. Use action='get' to read, action='set' to write.")]
        public string SetVariable(
            [Description("Name of the GameObject with BehaviorTree component.")]
            string gameObjectName,
            [Description("Name of the shared variable to get or set.")]
            string variableName,
            [Description("Action to perform: 'get' or 'set'. Default 'get'.")]
            string? action = "get",
            [Description("Value to set (as string). Required when action='set'.")]
            string? value = null,
            [Description("Type hint for parsing: 'bool', 'float', 'int', 'string', 'vector3'. Helps parse the value correctly.")]
            string? valueType = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                BehaviorTree tree = go.GetComponent<BehaviorTree>();
                if (tree == null)
                    throw new Exception($"'{gameObjectName}' has no BehaviorTree component.");

                string effectiveAction = action ?? "get";

                SharedVariable variable = tree.GetVariable(variableName);
                if (variable == null)
                    throw new Exception($"Variable '{variableName}' not found on '{gameObjectName}'.");

                if (effectiveAction.Equals("get", StringComparison.OrdinalIgnoreCase))
                {
                    object currentValue = variable.GetValue();
                    string typeName = variable.GetType().Name;
                    string valueStr = currentValue != null ? currentValue.ToString()! : "null";
                    return $"[{typeName}] {variableName} = {valueStr}";
                }

                if (effectiveAction.Equals("set", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrEmpty(value))
                        throw new Exception("'value' parameter is required when action='set'.");

                    string effectiveType = valueType ?? InferType(variable);
                    object parsedValue = ParseValue(value!, effectiveType);

                    try
                    {
                        // Try SetVariableValue first (preferred BD API)
                        tree.SetVariableValue(variableName, parsedValue);
                    }
                    catch
                    {
                        try
                        {
                            // Fallback: set value directly on the variable
                            variable.SetValue(parsedValue);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Failed to set variable '{variableName}': {ex.Message}");
                        }
                    }

                    // Verify
                    object newValue = variable.GetValue();
                    string newValueStr = newValue != null ? newValue.ToString()! : "null";
                    return $"Set [{variable.GetType().Name}] {variableName} = {newValueStr}";
                }

                throw new Exception($"Unknown action '{effectiveAction}'. Use 'get' or 'set'.");
            });
        }

        static string InferType(SharedVariable variable)
        {
            string typeName = variable.GetType().Name.ToLower();
            if (typeName.Contains("bool")) return "bool";
            if (typeName.Contains("float")) return "float";
            if (typeName.Contains("int")) return "int";
            if (typeName.Contains("vector3")) return "vector3";
            return "string";
        }

        static object ParseValue(string value, string valueType)
        {
            string type = valueType.ToLower().Trim();
            switch (type)
            {
                case "bool":
                    if (bool.TryParse(value, out bool bVal)) return bVal;
                    throw new Exception($"Cannot parse '{value}' as bool.");
                case "float":
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float fVal)) return fVal;
                    throw new Exception($"Cannot parse '{value}' as float.");
                case "int":
                    if (int.TryParse(value, out int iVal)) return iVal;
                    throw new Exception($"Cannot parse '{value}' as int.");
                case "vector3":
                    return ParseVector3(value);
                case "string":
                    return value;
                default:
                    return value;
            }
        }

        static Vector3 ParseVector3(string value)
        {
            // Accept formats: "1,2,3" or "(1,2,3)" or "1 2 3"
            string cleaned = value.Trim().TrimStart('(').TrimEnd(')');
            string[] parts = cleaned.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                throw new Exception($"Cannot parse '{value}' as Vector3. Expected 3 components.");
            if (!float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
                throw new Exception($"Cannot parse X component '{parts[0]}' as float.");
            if (!float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
                throw new Exception($"Cannot parse Y component '{parts[1]}' as float.");
            if (!float.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                throw new Exception($"Cannot parse Z component '{parts[2]}' as float.");
            return new Vector3(x, y, z);
        }
    }
}
#endif
