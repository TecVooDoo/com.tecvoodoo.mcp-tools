#nullable enable
using System.Collections;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace MCPTools.ORK.Editor
{
    public partial class Tool_ORK
    {
        [McpPluginTool("ork-inventory", Title = "ORK / Inventory")]
        [Description(@"Operates on a combatant's (or group's) Inventory.

action options:
  list   -- enumerate inventory contents (item name + count)
  add    -- add item by name, optionally quantity (default 1)
  remove -- remove item by name, optionally quantity (default 1)
  count  -- get count of a specific item

If combatantName is omitted, operates on the active group's shared Inventory (Group.Inventory).
Requires Play mode.")]
        public string Inventory(
            [Description("'list' | 'add' | 'remove' | 'count'")]
            string action,
            [Description("Item name (required for add/remove/count).")]
            string? itemName = null,
            [Description("Combatant name (optional). Omit to use Group.Inventory.")]
            string? combatantName = null,
            [Description("Quantity for add/remove (default 1).")]
            int quantity = 1
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireORKInitialized();
                action = (action ?? "").Trim().ToLowerInvariant();

                object? inv;
                if (!string.IsNullOrEmpty(combatantName))
                {
                    var c = FindCombatantByName(combatantName) ?? throw new System.Exception($"Combatant '{combatantName}' not found.");
                    inv = Get(c, "Inventory") ?? throw new System.Exception("Combatant has no Inventory.");
                }
                else
                {
                    var g = ActiveGroup() ?? throw new System.Exception("No active group.");
                    inv = Get(g, "Inventory") ?? throw new System.Exception("Group has no Inventory.");
                }

                if (action == "list")
                {
                    var sb = new StringBuilder();
                    int count = Get(inv, "Count") is int cnt ? cnt : 0;
                    sb.AppendLine($"Inventory contents ({count} entries):");

                    // Inventory.GetContent() returns IContent (item list provider).
                    var content = Call(inv, "GetContent") ?? Get(inv, "GetContent");
                    if (content is IEnumerable enumerable)
                    {
                        int i = 0;
                        foreach (var entry in enumerable)
                        {
                            if (entry == null) continue;
                            string n = Call(entry, "GetName") as string ?? entry.ToString();
                            sb.AppendLine($"  - {n}");
                            if (++i >= 50) { sb.AppendLine("  ..."); break; }
                        }
                    }
                    return sb.ToString();
                }

                if (string.IsNullOrEmpty(itemName))
                    throw new System.Exception("itemName is required for add/remove/count.");

                var itemSetting = FindSettingByName(Items(), itemName)
                                  ?? throw new System.Exception($"Item '{itemName}' not found in ORK.Items DB.");

                // Many ORK item-adding paths take a derived IShortcut (ItemShortcut) -- try via item setting's CreateShortcut() helper.
                object? shortcut = Call(itemSetting, "CreateShortcut") ?? Call(itemSetting, "CreateShortcut", quantity);

                switch (action)
                {
                    case "add":
                        if (shortcut == null) throw new System.Exception("Could not create item shortcut.");
                        var added = Call(inv, "Add", shortcut, false, false, false);  // Add(IShortcut, showNotification, showConsole, markNewContent)
                        return $"OK: Added '{itemName}' x{quantity} to inventory (Add returned: {added}).";

                    case "remove":
                        if (shortcut == null) throw new System.Exception("Could not create item shortcut.");
                        Call(inv, "Remove", shortcut, quantity, false, false);
                        return $"OK: Removed '{itemName}' x{quantity} from inventory.";

                    case "count":
                        var c = Call(inv, "GetCount", shortcut!);
                        return $"'{itemName}' count: {c}";

                    default:
                        throw new System.Exception($"Unknown action '{action}'. Use one of: list, add, remove, count.");
                }
            });
        }
    }
}
