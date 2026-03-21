#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using Component = UnityEngine.Component;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using UnityEngine;

namespace MCPTools.UCC.Editor
{
    public partial class Tool_UCC
    {
        [McpPluginTool("uc-query", Title = "UCC / Query Character")]
        [Description(@"Reads the full Ultimate Character Controller setup on a GameObject.
Reports locomotion state (enabled, grounded, active abilities, movement type),
attributes (health, stamina, etc.), and equipped items.
Use this to understand a character's configuration before making changes.
All UCC API access uses reflection for resilience.")]
        public string QueryCharacter(
            [Description("Name of the GameObject with UltimateCharacterLocomotion component.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"=== UCC Character: {go.name} ===");

                // --- UltimateCharacterLocomotion ---
                try
                {
                    Component? locomotion = FindComponentByTypeName(go, UCL_TYPE);
                    if (locomotion != null)
                    {
                        sb.AppendLine("\n-- UltimateCharacterLocomotion --");

                        object? enabled = GetPropValue(locomotion, "enabled");
                        sb.AppendLine($"  Enabled: {enabled}");

                        try
                        {
                            object? grounded = GetPropValue(locomotion, "Grounded");
                            sb.AppendLine($"  Grounded: {grounded}");
                        }
                        catch { sb.AppendLine("  Grounded: (unavailable)"); }

                        try
                        {
                            object? moving = GetPropValue(locomotion, "Moving");
                            sb.AppendLine($"  Moving: {moving}");
                        }
                        catch { sb.AppendLine("  Moving: (unavailable)"); }

                        // Active movement type
                        try
                        {
                            object? moveType = GetPropValue(locomotion, "ActiveMovementType");
                            string moveTypeName = moveType != null ? moveType.GetType().Name : "none";
                            sb.AppendLine($"  ActiveMovementType: {moveTypeName}");
                        }
                        catch { sb.AppendLine("  ActiveMovementType: (unavailable)"); }

                        // Abilities
                        try
                        {
                            object? abilities = GetPropValue(locomotion, "Abilities");
                            if (abilities is IList abilityList)
                            {
                                sb.AppendLine($"\n  === ABILITIES ({abilityList.Count}) ===");
                                for (int i = 0; i < abilityList.Count; i++)
                                {
                                    object? ability = abilityList[i];
                                    if (ability == null) continue;
                                    string abilityTypeName = ability.GetType().Name;
                                    object? isActive = GetPropValue(ability, "IsActive");
                                    object? abilityEnabled = GetPropValue(ability, "Enabled");
                                    sb.AppendLine($"    [{i}] {abilityTypeName} | Enabled: {abilityEnabled} | Active: {isActive}");
                                }
                            }
                        }
                        catch (Exception ex) { sb.AppendLine($"  Abilities: (error: {ex.Message})"); }

                        // Item abilities
                        try
                        {
                            object? itemAbilities = GetPropValue(locomotion, "ItemAbilities");
                            if (itemAbilities is IList itemAbilityList)
                            {
                                sb.AppendLine($"\n  === ITEM ABILITIES ({itemAbilityList.Count}) ===");
                                for (int i = 0; i < itemAbilityList.Count; i++)
                                {
                                    object? itemAbility = itemAbilityList[i];
                                    if (itemAbility == null) continue;
                                    string itemAbilityTypeName = itemAbility.GetType().Name;
                                    object? isActive = GetPropValue(itemAbility, "IsActive");
                                    object? itemAbilityEnabled = GetPropValue(itemAbility, "Enabled");
                                    sb.AppendLine($"    [{i}] {itemAbilityTypeName} | Enabled: {itemAbilityEnabled} | Active: {isActive}");
                                }
                            }
                        }
                        catch (Exception ex) { sb.AppendLine($"  ItemAbilities: (error: {ex.Message})"); }
                    }
                    else
                    {
                        sb.AppendLine("\n-- UltimateCharacterLocomotion: NOT FOUND --");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"\n-- UltimateCharacterLocomotion: ERROR ({ex.Message}) --");
                }

                // --- AttributeManager ---
                try
                {
                    Component? attrManager = FindComponentByTypeName(go, ATTR_MANAGER_TYPE);
                    if (attrManager != null)
                    {
                        sb.AppendLine("\n-- AttributeManager --");
                        try
                        {
                            object? attributes = GetPropValue(attrManager, "Attributes");
                            if (attributes is IList attrList)
                            {
                                sb.AppendLine($"  Attributes ({attrList.Count}):");
                                foreach (object? attr in attrList)
                                {
                                    if (attr == null) continue;
                                    object? attrName = GetPropValue(attr, "Name");
                                    object? attrValue = GetPropValue(attr, "Value");
                                    object? attrMax = GetPropValue(attr, "MaxValue");
                                    object? attrMin = GetPropValue(attr, "MinValue");
                                    sb.AppendLine($"    {attrName}: {attrValue} / {attrMax} (min: {attrMin})");
                                }
                            }
                            else
                            {
                                sb.AppendLine("  Attributes: (could not read list)");
                            }
                        }
                        catch (Exception ex) { sb.AppendLine($"  Attributes: (error: {ex.Message})"); }
                    }
                    else
                    {
                        sb.AppendLine("\n-- AttributeManager: NOT FOUND --");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"\n-- AttributeManager: ERROR ({ex.Message}) --");
                }

                // --- Inventory ---
                try
                {
                    Component? inventory = FindComponentByTypeName(go, INVENTORY_TYPE);
                    if (inventory != null)
                    {
                        sb.AppendLine("\n-- Inventory --");
                        try
                        {
                            // Try to get all item identifiers
                            object? allItems = CallMethod(inventory, "GetAllItemIdentifiers");
                            if (allItems is IList itemList)
                            {
                                sb.AppendLine($"  Items ({itemList.Count}):");
                                foreach (object? item in itemList)
                                {
                                    if (item == null) continue;
                                    object? itemName = GetPropValue(item, "name");
                                    if (itemName == null) itemName = item.ToString();
                                    sb.AppendLine($"    - {itemName}");
                                }
                            }
                        }
                        catch
                        {
                            // Fallback: just report that inventory exists
                            sb.AppendLine($"  Inventory component found: {inventory.GetType().Name}");
                            sb.AppendLine("  (Could not enumerate items -- API may differ)");
                        }
                    }
                    else
                    {
                        sb.AppendLine("\n-- Inventory: NOT FOUND --");
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"\n-- Inventory: ERROR ({ex.Message}) --");
                }

                return sb.ToString();
            });
        }
    }
}
