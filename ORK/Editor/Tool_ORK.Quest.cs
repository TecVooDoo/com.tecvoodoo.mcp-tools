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
        [McpPluginTool("ork-quest", Title = "ORK / Quest")]
        [Description(@"Reads or modifies the player's quest log.

action options:
  list   -- list all defined quests in the project (with active/finished status)
  add    -- add a quest to the player by name (optionally bypass requirements)
  remove -- remove a quest from the player by name
  has    -- check if a quest is in the player's log
  status -- get current status of a quest (active/finished/failed/inactive)

Requires Play mode for add/remove/has/status.")]
        public string Quest(
            [Description("'list' | 'add' | 'remove' | 'has' | 'status'")]
            string action,
            [Description("Quest name. Required for add/remove/has/status.")]
            string? questName = null,
            [Description("If true, ignore the quest's requirements when adding (default false).")]
            bool ignoreRequirements = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireORKInitialized();
                action = (action ?? "").Trim().ToLowerInvariant();

                if (action == "list")
                {
                    var sb = new StringBuilder();
                    var db = QuestsDB();
                    var arr = Get(db, "Settings") as IList;
                    sb.AppendLine($"ORK quests in project ({arr?.Count ?? 0}):");
                    if (arr == null) return sb.ToString();

                    var qh = QuestHandler();
                    foreach (var setting in arr)
                    {
                        if (setting == null) continue;
                        string name = Call(setting, "GetName") as string ?? setting.ToString();
                        bool inLog = qh != null && Call(qh, "HasQuest", setting) is bool hq && hq;
                        sb.AppendLine($"  - {name}{(inLog ? "  [in player log]" : "")}");
                    }
                    return sb.ToString();
                }

                if (string.IsNullOrEmpty(questName))
                    throw new System.Exception("questName is required for this action.");

                var qSetting = FindSettingByName(QuestsDB(), questName)
                               ?? throw new System.Exception($"Quest '{questName}' not found in ORK.Quests DB.");
                var handler = QuestHandler() ?? throw new System.Exception("QuestHandler not available (play mode required).");

                switch (action)
                {
                    case "add":
                        var added = Call(handler, "AddQuest", qSetting, ignoreRequirements, false, false);
                        return $"{(added is true ? "OK" : "WARN")}: AddQuest('{questName}') returned {added}.";

                    case "remove":
                        Call(handler, "RemoveQuest", qSetting, false);
                        return $"OK: RemoveQuest('{questName}') called.";

                    case "has":
                        var has = Call(handler, "HasQuest", qSetting);
                        return $"HasQuest('{questName}') = {has}";

                    case "status":
                        var quest = Call(handler, "GetQuest", qSetting);
                        if (quest == null) return $"Quest '{questName}' is not in the player log.";
                        var status = Get(quest, "Status") ?? Get(quest, "QuestStatus");
                        return $"Quest '{questName}' status: {(status != null ? status.ToString() : "(unknown)")}";

                    default:
                        throw new System.Exception($"Unknown action '{action}'. Use one of: list, add, remove, has, status.");
                }
            });
        }
    }
}
