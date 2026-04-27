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
        [McpPluginTool("ork-database-query", Title = "ORK / Database Query")]
        [Description(@"Lists definitions from the ORK project database.

category options (case-insensitive):
  combatants -- ORK.Combatants (CombatantsSettings DB)
  items      -- ORK.Items (ItemsSettings DB)
  abilities  -- ORK.Abilities
  classes    -- ORK.Classes
  quests     -- ORK.Quests
  equipment  -- ORK.Equipment
  status     -- ORK.StatusValues
  all        -- counts only across all categories above

If filter is provided, returns entries whose name contains the filter (case-insensitive).
limit caps per-category list size (default 25).")]
        public string DatabaseQuery(
            [Description("'combatants' | 'items' | 'abilities' | 'classes' | 'quests' | 'equipment' | 'status' | 'all'")]
            string category,
            [Description("Optional name substring filter.")]
            string? filter = null,
            [Description("Per-category list cap (default 25).")]
            int limit = 25
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireORKInitialized();
                var sb = new StringBuilder();
                category = (category ?? "").Trim().ToLowerInvariant();

                if (category == "all")
                {
                    sb.AppendLine("ORK Database snapshot:");
                    sb.AppendLine($"  Combatants:    {CountOf(Combatants())}");
                    sb.AppendLine($"  Items:         {CountOf(Items())}");
                    sb.AppendLine($"  Abilities:     {CountOf(Abilities())}");
                    sb.AppendLine($"  Classes:       {CountOf(Classes())}");
                    sb.AppendLine($"  Quests:        {CountOf(QuestsDB())}");
                    sb.AppendLine($"  Equipment:     {CountOf(Equipment())}");
                    sb.AppendLine($"  StatusValues:  {CountOf(StatusValuesDB())}");
                    return sb.ToString();
                }

                object? db = category switch
                {
                    "combatants" => Combatants(),
                    "items"      => Items(),
                    "abilities"  => Abilities(),
                    "classes"    => Classes(),
                    "quests"     => QuestsDB(),
                    "equipment"  => Equipment(),
                    "status"     => StatusValuesDB(),
                    _ => null
                };
                if (db == null)
                    throw new System.Exception($"Unknown category '{category}'. Use one of: combatants, items, abilities, classes, quests, equipment, status, all.");

                var arr = Get(db, "Settings") as IList;
                int total = arr?.Count ?? 0;
                sb.AppendLine($"ORK {category} ({total} entries):");
                if (arr == null) return sb.ToString();

                int shown = 0;
                foreach (var s in arr)
                {
                    if (s == null) continue;
                    string name = Call(s, "GetName") as string ?? Get(s, "RenameableName") as string ?? Get(s, "Name") as string ?? s.ToString();
                    if (filter != null && !name.ToLowerInvariant().Contains(filter.ToLowerInvariant())) continue;
                    int id = Get(s, "ID") is int idVal ? idVal : -1;
                    sb.AppendLine($"  [{id}] {name}");
                    shown++;
                    if (shown >= limit) { sb.AppendLine($"  ... ({total - shown} more, raise limit to see them)"); break; }
                }
                return sb.ToString();
            });
        }
    }
}
