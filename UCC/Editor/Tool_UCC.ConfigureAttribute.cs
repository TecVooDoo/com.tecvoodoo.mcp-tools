#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.UCC.Editor
{
    public partial class Tool_UCC
    {
        [McpPluginTool("uc-configure-attribute", Title = "UCC / Configure Attribute")]
        [Description(@"Configures a character attribute (Health, Stamina, etc.) on a UCC character.
Requires an AttributeManager component. Only provided parameters are changed.
Use 'uc-query' first to see available attributes and their current values.
All UCC API access uses reflection for resilience.")]
        public string ConfigureAttribute(
            [Description("Name of the GameObject with AttributeManager component.")]
            string gameObjectName,
            [Description("Attribute name to configure (e.g. 'Health', 'Stamina'). Case-insensitive match.")]
            string attributeName,
            [Description("Set the current value. Null to keep current.")]
            float? value = null,
            [Description("Set the maximum value. Null to keep current.")]
            float? maxValue = null,
            [Description("Set the minimum value. Null to keep current.")]
            float? minValue = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                Component? attrManager = FindComponentByTypeName(go, ATTR_MANAGER_TYPE);
                if (attrManager == null)
                    throw new Exception($"'{go.name}' has no AttributeManager component.");

                // Find the attribute by name
                object? targetAttr = null;
                string searchLower = attributeName.ToLowerInvariant();

                try
                {
                    object? attributes = GetPropValue(attrManager, "Attributes");
                    if (attributes is IList attrList)
                    {
                        foreach (object? attr in attrList)
                        {
                            if (attr == null) continue;
                            object? attrName = GetPropValue(attr, "Name");
                            if (attrName != null && attrName.ToString()!.ToLowerInvariant().Contains(searchLower))
                            {
                                targetAttr = attr;
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not read attributes: {ex.Message}");
                }

                if (targetAttr == null)
                {
                    // Try GetAttribute method as fallback
                    try
                    {
                        targetAttr = CallMethod(attrManager, "GetAttribute", attributeName);
                    }
                    catch { /* method may not exist */ }
                }

                if (targetAttr == null)
                    throw new Exception($"Attribute '{attributeName}' not found on '{go.name}'.");

                StringBuilder sb = new StringBuilder();
                object? foundName = GetPropValue(targetAttr, "Name");
                sb.AppendLine($"=== Configure Attribute '{foundName}' on '{go.name}' ===");
                int changeCount = 0;

                if (maxValue.HasValue)
                {
                    // Set maxValue before value to avoid clamping issues
                    try
                    {
                        bool set = SetPropValue(targetAttr, "MaxValue", maxValue.Value);
                        if (set)
                        {
                            sb.AppendLine($"  MaxValue -> {maxValue.Value:F2}");
                            changeCount++;
                        }
                        else
                        {
                            sb.AppendLine($"  MaxValue: could not set (property not found)");
                        }
                    }
                    catch (Exception ex) { sb.AppendLine($"  MaxValue: error ({ex.Message})"); }
                }

                if (minValue.HasValue)
                {
                    try
                    {
                        bool set = SetPropValue(targetAttr, "MinValue", minValue.Value);
                        if (set)
                        {
                            sb.AppendLine($"  MinValue -> {minValue.Value:F2}");
                            changeCount++;
                        }
                        else
                        {
                            sb.AppendLine($"  MinValue: could not set (property not found)");
                        }
                    }
                    catch (Exception ex) { sb.AppendLine($"  MinValue: error ({ex.Message})"); }
                }

                if (value.HasValue)
                {
                    try
                    {
                        bool set = SetPropValue(targetAttr, "Value", value.Value);
                        if (set)
                        {
                            sb.AppendLine($"  Value -> {value.Value:F2}");
                            changeCount++;
                        }
                        else
                        {
                            sb.AppendLine($"  Value: could not set (property not found)");
                        }
                    }
                    catch (Exception ex) { sb.AppendLine($"  Value: error ({ex.Message})"); }
                }

                if (changeCount == 0)
                {
                    sb.AppendLine("  No changes applied. Provide at least one parameter to configure.");
                }
                else
                {
                    EditorUtility.SetDirty(go);
                    sb.AppendLine($"\n  {changeCount} property(s) updated.");
                }

                return sb.ToString();
            });
        }
    }
}
