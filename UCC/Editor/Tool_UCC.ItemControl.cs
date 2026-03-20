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
        const string ITEM_SET_MANAGER_TYPE = "Opsive.UltimateCharacterController.Inventory.ItemSetManagerBase";
        const string ITEM_TYPE = "Opsive.UltimateCharacterController.Items.Item";

        [McpPluginTool("uc-item-control", Title = "UCC / Item Control")]
        [Description(@"Query and control inventory and items on a UCC character.
Actions: 'list' shows all items in inventory, 'equip' equips an item set by index,
'unequip' unequips the current item set.
Use 'uc-query' first to see the character's current inventory state.
All UCC API access uses reflection for resilience.")]
        public string ItemControl(
            [Description("Name of the GameObject with Inventory component.")]
            string gameObjectName,
            [Description("Action to perform: 'list', 'equip', 'unequip'.")]
            string action,
            [Description("Item set index to equip/unequip (used with 'equip' and 'unequip' actions).")]
            int? itemSetIndex = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                string actionLower = action.ToLowerInvariant();

                switch (actionLower)
                {
                    case "list":
                        return ListItems(go);
                    case "equip":
                        return EquipItemSet(go, itemSetIndex);
                    case "unequip":
                        return UnequipItemSet(go, itemSetIndex);
                    default:
                        throw new Exception($"Unknown action '{action}'. Use 'list', 'equip', or 'unequip'.");
                }
            });
        }

        private static string ListItems(GameObject go)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"=== Items on '{go.name}' ===");

            // Try InventoryBase
            Component? inventory = FindComponentByTypeName(go, INVENTORY_TYPE);
            if (inventory != null)
            {
                sb.AppendLine($"\n-- Inventory ({inventory.GetType().Name}) --");

                // Try GetAllItemIdentifiers
                try
                {
                    MethodInfo? getAllMethod = inventory.GetType().GetMethod("GetAllItemIdentifiers",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (getAllMethod != null)
                    {
                        object? result = getAllMethod.Invoke(inventory, null);
                        if (result is IList identifiers)
                        {
                            sb.AppendLine($"  Item Identifiers ({identifiers.Count}):");
                            foreach (object? id in identifiers)
                            {
                                if (id == null) continue;
                                object? idName = GetPropValue(id, "name");
                                if (idName == null) idName = GetPropValue(id, "Name");
                                if (idName == null) idName = id.ToString();

                                // Try to get item count
                                try
                                {
                                    MethodInfo? countMethod = inventory.GetType().GetMethod("GetItemIdentifierAmount",
                                        BindingFlags.Public | BindingFlags.Instance);
                                    if (countMethod != null)
                                    {
                                        object? count = countMethod.Invoke(inventory, new object[] { id });
                                        sb.AppendLine($"    - {idName} (x{count})");
                                    }
                                    else
                                    {
                                        sb.AppendLine($"    - {idName}");
                                    }
                                }
                                catch
                                {
                                    sb.AppendLine($"    - {idName}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  Could not enumerate items: {ex.Message}");
                }
            }
            else
            {
                sb.AppendLine("\n-- Inventory: NOT FOUND --");
            }

            // Try to find Items on child objects
            try
            {
                Type? itemType = FindTypeInAllAssemblies(ITEM_TYPE);
                if (itemType != null)
                {
                    Component[] items = go.GetComponentsInChildren(itemType, true);
                    if (items.Length > 0)
                    {
                        sb.AppendLine($"\n-- Item Components ({items.Length}) --");
                        foreach (Component item in items)
                        {
                            object? itemId = GetPropValue(item, "ItemIdentifier");
                            string itemName = itemId != null ? itemId.ToString()! : item.gameObject.name;
                            object? slotId = GetPropValue(item, "SlotID");
                            sb.AppendLine($"    - {itemName} | Slot: {slotId} | GO: {item.gameObject.name}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"\n  Item scan: (error: {ex.Message})");
            }

            // Try ItemSetManager
            Component? itemSetManager = FindComponentByTypeName(go, ITEM_SET_MANAGER_TYPE);
            if (itemSetManager != null)
            {
                sb.AppendLine($"\n-- ItemSetManager ({itemSetManager.GetType().Name}) --");
                try
                {
                    object? itemSetGroups = GetPropValue(itemSetManager, "ItemSetGroups");
                    if (itemSetGroups == null)
                        itemSetGroups = GetPropValue(itemSetManager, "CategoryItemSets");

                    if (itemSetGroups is IList groupList)
                    {
                        sb.AppendLine($"  Groups/Categories: {groupList.Count}");
                        for (int g = 0; g < groupList.Count; g++)
                        {
                            object? group = groupList[g];
                            if (group == null) continue;

                            object? activeIndex = GetPropValue(group, "ActiveItemSetIndex");
                            object? itemSets = GetPropValue(group, "ItemSetList");
                            if (itemSets == null) itemSets = GetPropValue(group, "ItemSets");

                            int setCount = 0;
                            if (itemSets is IList setList) setCount = setList.Count;

                            sb.AppendLine($"  Group [{g}]: ActiveIndex={activeIndex}, Sets={setCount}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  Could not read item sets: {ex.Message}");
                }
            }

            return sb.ToString();
        }

        private static string EquipItemSet(GameObject go, int? itemSetIndex)
        {
            if (!itemSetIndex.HasValue)
                throw new Exception("itemSetIndex is required for 'equip' action.");

            Component? itemSetManager = FindComponentByTypeName(go, ITEM_SET_MANAGER_TYPE);
            if (itemSetManager == null)
                throw new Exception($"'{go.name}' has no ItemSetManager component.");

            StringBuilder sb = new StringBuilder();

            try
            {
                // Try to find and call equip methods
                // UCC typically uses ItemSetManager to change active item sets
                object? itemSetGroups = GetPropValue(itemSetManager, "ItemSetGroups");
                if (itemSetGroups == null)
                    itemSetGroups = GetPropValue(itemSetManager, "CategoryItemSets");

                if (itemSetGroups is IList groupList)
                {
                    if (groupList.Count == 0)
                        throw new Exception("No item set groups found.");

                    // Default to first group (category)
                    object? group = groupList[0];
                    if (group == null)
                        throw new Exception("First item set group is null.");

                    // Try calling UpdateItemSet or SetActiveItemSet
                    bool success = false;
                    try
                    {
                        Type groupType = group.GetType();
                        MethodInfo? setActiveMethod = groupType.GetMethod("UpdateActiveItemSet",
                            BindingFlags.Public | BindingFlags.Instance);
                        if (setActiveMethod == null)
                            setActiveMethod = groupType.GetMethod("SetActiveItemSet",
                                BindingFlags.Public | BindingFlags.Instance);

                        if (setActiveMethod != null)
                        {
                            setActiveMethod.Invoke(group, new object[] { itemSetIndex.Value });
                            success = true;
                        }
                    }
                    catch { /* try another approach */ }

                    if (!success)
                    {
                        // Try setting the property directly
                        success = SetPropValue(group, "ActiveItemSetIndex", itemSetIndex.Value);
                    }

                    if (success)
                    {
                        EditorUtility.SetDirty(go);
                        sb.AppendLine($"Equipped item set index {itemSetIndex.Value} on '{go.name}'.");
                    }
                    else
                    {
                        sb.AppendLine($"Could not equip item set. The equip API may differ in this UCC version.");
                        sb.AppendLine("Try using 'uc-item-control list' to see available item sets and their indices.");
                    }
                }
                else
                {
                    throw new Exception("Could not access item set groups.");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Equip failed: {ex.Message}");
            }

            return sb.ToString();
        }

        private static string UnequipItemSet(GameObject go, int? itemSetIndex)
        {
            Component? itemSetManager = FindComponentByTypeName(go, ITEM_SET_MANAGER_TYPE);
            if (itemSetManager == null)
                throw new Exception($"'{go.name}' has no ItemSetManager component.");

            StringBuilder sb = new StringBuilder();

            try
            {
                object? itemSetGroups = GetPropValue(itemSetManager, "ItemSetGroups");
                if (itemSetGroups == null)
                    itemSetGroups = GetPropValue(itemSetManager, "CategoryItemSets");

                if (itemSetGroups is IList groupList)
                {
                    if (groupList.Count == 0)
                        throw new Exception("No item set groups found.");

                    // Unequip by setting to -1 or calling unequip
                    int groupIndex = 0;
                    if (itemSetIndex.HasValue && itemSetIndex.Value < groupList.Count)
                        groupIndex = itemSetIndex.Value;

                    object? group = groupList[groupIndex];
                    if (group == null)
                        throw new Exception($"Item set group [{groupIndex}] is null.");

                    bool success = false;

                    // Try calling an unequip method
                    try
                    {
                        Type groupType = group.GetType();
                        MethodInfo? unequipMethod = groupType.GetMethod("UpdateActiveItemSet",
                            BindingFlags.Public | BindingFlags.Instance);
                        if (unequipMethod != null)
                        {
                            // Passing -1 typically means "unequip"
                            unequipMethod.Invoke(group, new object[] { -1 });
                            success = true;
                        }
                    }
                    catch { /* try direct set */ }

                    if (!success)
                    {
                        success = SetPropValue(group, "ActiveItemSetIndex", -1);
                    }

                    if (success)
                    {
                        EditorUtility.SetDirty(go);
                        sb.AppendLine($"Unequipped item set on '{go.name}' (group {groupIndex}).");
                    }
                    else
                    {
                        sb.AppendLine($"Could not unequip. The unequip API may differ in this UCC version.");
                    }
                }
                else
                {
                    throw new Exception("Could not access item set groups.");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"Unequip failed: {ex.Message}");
            }

            return sb.ToString();
        }
    }
}
