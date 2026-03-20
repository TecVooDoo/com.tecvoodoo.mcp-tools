#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
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
        [McpPluginTool("uc-ability-control", Title = "UCC / Ability Control")]
        [Description(@"List, enable, disable, start, or stop abilities on a UCC character.
Actions: 'list' shows all abilities, 'enable'/'disable' toggles the Enabled flag,
'start'/'stop' calls TryStartAbility/TryStopAbility.
For 'enable'/'disable'/'start'/'stop', provide abilityName to match by type name.
Use abilityIndex if multiple instances of the same ability type exist (default 0).
All UCC API access uses reflection for resilience.")]
        public string AbilityControl(
            [Description("Name of the GameObject with UltimateCharacterLocomotion component.")]
            string gameObjectName,
            [Description("Action to perform: 'list', 'enable', 'disable', 'start', 'stop'.")]
            string action,
            [Description("Ability type name to match (e.g. 'Jump', 'Fall', 'Interact'). Required for enable/disable/start/stop.")]
            string? abilityName = null,
            [Description("Index when multiple abilities of the same type exist (default 0).")]
            int? abilityIndex = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                Component? locomotion = FindComponentByTypeName(go, UCL_TYPE);
                if (locomotion == null)
                    throw new Exception($"'{go.name}' has no UltimateCharacterLocomotion component.");

                string actionLower = action.ToLowerInvariant();

                if (actionLower == "list")
                {
                    return ListAbilities(locomotion, go.name);
                }

                if (string.IsNullOrEmpty(abilityName))
                    throw new Exception($"abilityName is required for action '{action}'.");

                int targetIndex = abilityIndex ?? 0;

                // Find the matching ability
                object? matchedAbility = FindAbilityByName(locomotion, abilityName!, targetIndex, out string searchInfo);
                if (matchedAbility == null)
                    throw new Exception($"Ability '{abilityName}' (index {targetIndex}) not found on '{go.name}'. {searchInfo}");

                StringBuilder sb = new StringBuilder();
                string abilityTypeName = matchedAbility.GetType().Name;

                switch (actionLower)
                {
                    case "enable":
                        SetPropValue(matchedAbility, "Enabled", true);
                        EditorUtility.SetDirty(go);
                        sb.AppendLine($"Enabled ability '{abilityTypeName}' on '{go.name}'.");
                        break;

                    case "disable":
                        SetPropValue(matchedAbility, "Enabled", false);
                        EditorUtility.SetDirty(go);
                        sb.AppendLine($"Disabled ability '{abilityTypeName}' on '{go.name}'.");
                        break;

                    case "start":
                        try
                        {
                            // TryStartAbility takes the ability as parameter
                            Type locoType = locomotion.GetType();
                            MethodInfo? startMethod = locoType.GetMethod("TryStartAbility",
                                BindingFlags.Public | BindingFlags.Instance,
                                null,
                                new Type[] { matchedAbility.GetType().BaseType ?? matchedAbility.GetType() },
                                null);

                            if (startMethod == null)
                            {
                                // Try with the exact type
                                startMethod = locoType.GetMethod("TryStartAbility",
                                    BindingFlags.Public | BindingFlags.Instance);
                            }

                            if (startMethod != null)
                            {
                                object? result = startMethod.Invoke(locomotion, new object[] { matchedAbility });
                                sb.AppendLine($"TryStartAbility('{abilityTypeName}') on '{go.name}' -> {result}");
                            }
                            else
                            {
                                sb.AppendLine($"TryStartAbility method not found on locomotion component. The ability may need to be started via input or other triggers.");
                            }
                        }
                        catch (Exception ex) { sb.AppendLine($"Start failed: {ex.Message}"); }
                        break;

                    case "stop":
                        try
                        {
                            Type locoType = locomotion.GetType();
                            MethodInfo? stopMethod = locoType.GetMethod("TryStopAbility",
                                BindingFlags.Public | BindingFlags.Instance,
                                null,
                                new Type[] { matchedAbility.GetType().BaseType ?? matchedAbility.GetType() },
                                null);

                            if (stopMethod == null)
                            {
                                stopMethod = locoType.GetMethod("TryStopAbility",
                                    BindingFlags.Public | BindingFlags.Instance);
                            }

                            if (stopMethod != null)
                            {
                                object? result = stopMethod.Invoke(locomotion, new object[] { matchedAbility });
                                sb.AppendLine($"TryStopAbility('{abilityTypeName}') on '{go.name}' -> {result}");
                            }
                            else
                            {
                                sb.AppendLine($"TryStopAbility method not found on locomotion component.");
                            }
                        }
                        catch (Exception ex) { sb.AppendLine($"Stop failed: {ex.Message}"); }
                        break;

                    default:
                        throw new Exception($"Unknown action '{action}'. Use 'list', 'enable', 'disable', 'start', or 'stop'.");
                }

                return sb.ToString();
            });
        }

        private static string ListAbilities(Component locomotion, string goName)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"=== Abilities on '{goName}' ===");

            try
            {
                object? abilities = GetPropValue(locomotion, "Abilities");
                if (abilities is IList abilityList)
                {
                    sb.AppendLine($"\n  --- Standard Abilities ({abilityList.Count}) ---");
                    for (int i = 0; i < abilityList.Count; i++)
                    {
                        object? ability = abilityList[i];
                        if (ability == null) continue;
                        string typeName = ability.GetType().Name;
                        object? isActive = GetPropValue(ability, "IsActive");
                        object? isEnabled = GetPropValue(ability, "Enabled");
                        object? priority = GetPropValue(ability, "Index");
                        sb.AppendLine($"    [{i}] {typeName} | Enabled: {isEnabled} | Active: {isActive} | Index: {priority}");
                    }
                }
                else
                {
                    sb.AppendLine("  Standard Abilities: (could not read)");
                }
            }
            catch (Exception ex) { sb.AppendLine($"  Standard Abilities: (error: {ex.Message})"); }

            try
            {
                object? itemAbilities = GetPropValue(locomotion, "ItemAbilities");
                if (itemAbilities is IList itemAbilityList)
                {
                    sb.AppendLine($"\n  --- Item Abilities ({itemAbilityList.Count}) ---");
                    for (int i = 0; i < itemAbilityList.Count; i++)
                    {
                        object? itemAbility = itemAbilityList[i];
                        if (itemAbility == null) continue;
                        string typeName = itemAbility.GetType().Name;
                        object? isActive = GetPropValue(itemAbility, "IsActive");
                        object? isEnabled = GetPropValue(itemAbility, "Enabled");
                        sb.AppendLine($"    [{i}] {typeName} | Enabled: {isEnabled} | Active: {isActive}");
                    }
                }
            }
            catch (Exception ex) { sb.AppendLine($"  Item Abilities: (error: {ex.Message})"); }

            return sb.ToString();
        }

        private static object? FindAbilityByName(Component locomotion, string abilityName, int targetIndex, out string searchInfo)
        {
            searchInfo = "";
            string searchLower = abilityName.ToLowerInvariant();
            int matchCount = 0;

            // Search standard abilities
            try
            {
                object? abilities = GetPropValue(locomotion, "Abilities");
                if (abilities is IList abilityList)
                {
                    foreach (object? ability in abilityList)
                    {
                        if (ability == null) continue;
                        string typeName = ability.GetType().Name.ToLowerInvariant();
                        if (typeName.Contains(searchLower))
                        {
                            if (matchCount == targetIndex)
                                return ability;
                            matchCount++;
                        }
                    }
                }
            }
            catch { /* continue searching */ }

            // Search item abilities
            try
            {
                object? itemAbilities = GetPropValue(locomotion, "ItemAbilities");
                if (itemAbilities is IList itemAbilityList)
                {
                    foreach (object? itemAbility in itemAbilityList)
                    {
                        if (itemAbility == null) continue;
                        string typeName = itemAbility.GetType().Name.ToLowerInvariant();
                        if (typeName.Contains(searchLower))
                        {
                            if (matchCount == targetIndex)
                                return itemAbility;
                            matchCount++;
                        }
                    }
                }
            }
            catch { /* continue searching */ }

            searchInfo = matchCount > 0
                ? $"Found {matchCount} match(es) but index {targetIndex} is out of range."
                : "No abilities matched that name.";
            return null;
        }
    }
}
