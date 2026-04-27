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
        [McpPluginTool("ork-query-combatant", Title = "ORK / Query Combatant")]
        [Description(@"Reports runtime state of a Combatant.

If combatantName is omitted, lists every combatant in the player's ActiveGroup with summary
(name, level, class, isDead).

If combatantName is provided, returns deep status: name, level, class, all StatusValue
base/current values, group, isDead, inventory item count, equipment slot count.

Requires Play mode (ORK runtime data is only initialized in play mode).")]
        public string QueryCombatant(
            [Description("Optional combatant name (case-insensitive). Omit to list group.")]
            string? combatantName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireORK();
                var sb = new StringBuilder();

                if (string.IsNullOrEmpty(combatantName))
                {
                    var members = GetActiveGroupCombatants();
                    sb.AppendLine($"ORK ActiveGroup combatants ({members.Count}):");
                    foreach (var c in members)
                    {
                        string name = Call(c, "GetName") as string ?? c.ToString();
                        int level = Get(c, "Level") is int l ? l : 0;
                        var status = Get(c, "Status");
                        bool dead = status != null && Get(status, "IsDead") is bool d && d;
                        var cls = Get(c, "Class");
                        sb.AppendLine($"  - {name}  level={level}  class={(cls != null ? Call(cls, "GetName") as string ?? cls.ToString() : "(none)")}  dead={dead}");
                    }
                    if (members.Count == 0)
                        sb.AppendLine("  (empty -- runtime not initialized? Make sure ORK is in play mode and a player group is active.)");
                    return sb.ToString();
                }

                var combatant = FindCombatantByName(combatantName)
                                ?? throw new System.Exception($"Combatant '{combatantName}' not found in ActiveGroup or current battle.");

                string nm = Call(combatant, "GetName") as string ?? combatant.ToString();
                int lvl = Get(combatant, "Level") is int lev ? lev : 0;
                var st = Get(combatant, "Status");
                var cls2 = Get(combatant, "Class");
                var inv = Get(combatant, "Inventory");
                var eq = Get(combatant, "Equipment");
                var grp = Get(combatant, "Group");

                sb.AppendLine($"=== Combatant '{nm}' ===");
                sb.AppendLine($"  Level: {lvl}");
                sb.AppendLine($"  Class: {(cls2 != null ? Call(cls2, "GetName") as string ?? cls2.ToString() : "(none)")}");
                sb.AppendLine($"  Group: {(grp != null ? grp.ToString() : "(none)")}");
                if (st != null)
                {
                    bool dead = Get(st, "IsDead") is bool dd && dd;
                    sb.AppendLine($"  IsDead: {dead}");

                    // CombatantStatus has indexer this[int] over StatusValue and a Get(StatusValueSetting) accessor.
                    // Easiest enumeration: iterate ORK.StatusValues DB and call status.Get on each.
                    var svDB = StatusValuesDB();
                    var svArr = Get(svDB, "Settings") as IList;
                    if (svArr != null)
                    {
                        sb.AppendLine($"  Status values ({svArr.Count}):");
                        foreach (var setting in svArr)
                        {
                            if (setting == null) continue;
                            var sv = Call(st, "Get", setting);
                            if (sv == null) continue;
                            string svName = Call(setting, "GetName") as string ?? setting.ToString();
                            int baseVal = Call(sv, "GetBaseValue") is int bv ? bv : 0;
                            int dispVal = Call(sv, "GetDisplayValue") is int dv ? dv : baseVal;
                            int dispMax = Call(sv, "GetDisplayMaxValue") is int dx ? dx : 0;
                            sb.AppendLine($"    {svName}: {dispVal}/{dispMax} (base {baseVal})");
                        }
                    }
                }
                int invCount = inv != null && Get(inv, "Count") is int ic ? ic : 0;
                sb.AppendLine($"  Inventory items: {invCount}");
                if (eq != null) sb.AppendLine($"  Equipment: {eq}");

                return sb.ToString();
            });
        }
    }
}
