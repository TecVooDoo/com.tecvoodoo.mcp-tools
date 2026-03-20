#nullable enable
using System;
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
        [McpPluginTool("uc-configure-locomotion", Title = "UCC / Configure Locomotion")]
        [Description(@"Configures UltimateCharacterLocomotion properties on a character.
Only provided parameters are changed; others are left as-is.
Use 'uc-query' first to see the current configuration.
All UCC API access uses reflection for resilience.")]
        public string ConfigureLocomotion(
            [Description("Name of the GameObject with UltimateCharacterLocomotion component.")]
            string gameObjectName,
            [Description("Rigidbody mass. Null to keep current.")]
            float? mass = null,
            [Description("Character skin width for collision detection. Null to keep current.")]
            float? skinWidth = null,
            [Description("Whether gravity is applied. Null to keep current.")]
            bool? useGravity = null,
            [Description("Gravity magnitude (positive = downward force). Null to keep current.")]
            float? gravityMagnitude = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                Component? locomotion = FindComponentByTypeName(go, UCL_TYPE);
                if (locomotion == null)
                    throw new Exception($"'{go.name}' has no UltimateCharacterLocomotion component.");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"=== Configure Locomotion: {go.name} ===");
                int changeCount = 0;

                if (mass.HasValue)
                {
                    try
                    {
                        // Mass is typically on the Rigidbody, but UCC may wrap it
                        bool set = SetPropValue(locomotion, "Mass", mass.Value);
                        if (!set)
                        {
                            // Fallback: set on Rigidbody directly
                            Rigidbody rb = go.GetComponent<Rigidbody>();
                            if (rb != null)
                            {
                                rb.mass = mass.Value;
                                set = true;
                            }
                        }
                        if (set)
                        {
                            sb.AppendLine($"  Mass -> {mass.Value:F2}");
                            changeCount++;
                        }
                        else
                        {
                            sb.AppendLine($"  Mass: could not set (property not found)");
                        }
                    }
                    catch (Exception ex) { sb.AppendLine($"  Mass: error ({ex.Message})"); }
                }

                if (skinWidth.HasValue)
                {
                    try
                    {
                        bool set = SetPropValue(locomotion, "SkinWidth", skinWidth.Value);
                        if (set)
                        {
                            sb.AppendLine($"  SkinWidth -> {skinWidth.Value:F4}");
                            changeCount++;
                        }
                        else
                        {
                            sb.AppendLine($"  SkinWidth: could not set (property not found)");
                        }
                    }
                    catch (Exception ex) { sb.AppendLine($"  SkinWidth: error ({ex.Message})"); }
                }

                if (useGravity.HasValue)
                {
                    try
                    {
                        bool set = SetPropValue(locomotion, "UseGravity", useGravity.Value);
                        if (!set)
                        {
                            // Try alternative property names
                            set = SetPropValue(locomotion, "GravityEnabled", useGravity.Value);
                        }
                        if (set)
                        {
                            sb.AppendLine($"  UseGravity -> {useGravity.Value}");
                            changeCount++;
                        }
                        else
                        {
                            sb.AppendLine($"  UseGravity: could not set (property not found)");
                        }
                    }
                    catch (Exception ex) { sb.AppendLine($"  UseGravity: error ({ex.Message})"); }
                }

                if (gravityMagnitude.HasValue)
                {
                    try
                    {
                        bool set = SetPropValue(locomotion, "GravityMagnitude", gravityMagnitude.Value);
                        if (!set)
                        {
                            set = SetPropValue(locomotion, "GravityAmount", gravityMagnitude.Value);
                        }
                        if (set)
                        {
                            sb.AppendLine($"  GravityMagnitude -> {gravityMagnitude.Value:F2}");
                            changeCount++;
                        }
                        else
                        {
                            sb.AppendLine($"  GravityMagnitude: could not set (property not found)");
                        }
                    }
                    catch (Exception ex) { sb.AppendLine($"  GravityMagnitude: error ({ex.Message})"); }
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
