#if HAS_DIALOGUE_SYSTEM
#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using PixelCrushers.DialogueSystem;

namespace MCPTools.DialogueSystem.Editor
{
    public partial class Tool_DialogueSystem
    {
        [McpPluginTool("ds-quest", Title = "Dialogue System / Quest")]
        [Description(@"Get or set quest and quest entry states in the Dialogue System.
Actions: 'get' (returns quest state and descriptions), 'set' (sets quest state),
'getentry' (gets a quest entry state), 'setentry' (sets a quest entry state),
'list' (lists all quests with their current states).
Quest states: 'unassigned', 'active', 'success', 'failure'.")]
        public string ManageQuest(
            [Description("Name of the quest (as defined in the dialogue database).")]
            string questName,
            [Description("Action: 'get', 'set', 'getentry', 'setentry', 'list'. Default 'get'.")]
            string? action = "get",
            [Description("Quest state for 'set' action: 'unassigned', 'active', 'success', 'failure'.")]
            string? state = null,
            [Description("Quest entry number for 'getentry'/'setentry' (1-based).")]
            int? entryNumber = null,
            [Description("Quest entry state for 'setentry': 'unassigned', 'active', 'success', 'failure'.")]
            string? entryState = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!DialogueManager.hasInstance)
                    return "ERROR: No DialogueManager instance found in the scene.";

                string actionLower = (action ?? "get").ToLowerInvariant();

                switch (actionLower)
                {
                    case "get":
                    {
                        QuestState currentState = QuestLog.GetQuestState(questName);
                        string description = QuestLog.GetQuestDescription(questName);
                        string successDesc = QuestLog.GetQuestDescription(questName, QuestState.Success);
                        string failureDesc = QuestLog.GetQuestDescription(questName, QuestState.Failure);
                        int entryCount = QuestLog.GetQuestEntryCount(questName);

                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"Quest: {questName}");
                        sb.AppendLine($"  State: {currentState}");
                        sb.AppendLine($"  Description: {description}");
                        sb.AppendLine($"  Success Desc: {successDesc}");
                        sb.AppendLine($"  Failure Desc: {failureDesc}");
                        sb.AppendLine($"  Entry Count: {entryCount}");

                        for (int i = 1; i <= entryCount; i++)
                        {
                            QuestState entryQState = QuestLog.GetQuestEntryState(questName, i);
                            string entryDesc = QuestLog.GetQuestEntry(questName, i);
                            sb.AppendLine($"  Entry {i}: [{entryQState}] {entryDesc}");
                        }

                        return sb.ToString();
                    }

                    case "set":
                    {
                        if (string.IsNullOrEmpty(state))
                            return "ERROR: 'state' parameter is required for 'set' action.";

                        QuestState newState = ParseQuestState(state);
                        QuestLog.SetQuestState(questName, newState);
                        return $"Set quest '{questName}' state to {newState}.";
                    }

                    case "getentry":
                    {
                        if (entryNumber == null)
                            return "ERROR: 'entryNumber' parameter is required for 'getentry' action.";

                        QuestState entryQState = QuestLog.GetQuestEntryState(questName, entryNumber.Value);
                        string entryDesc = QuestLog.GetQuestEntry(questName, entryNumber.Value);
                        return $"Quest '{questName}' Entry {entryNumber.Value}: [{entryQState}] {entryDesc}";
                    }

                    case "setentry":
                    {
                        if (entryNumber == null)
                            return "ERROR: 'entryNumber' parameter is required for 'setentry' action.";
                        if (string.IsNullOrEmpty(entryState))
                            return "ERROR: 'entryState' parameter is required for 'setentry' action.";

                        QuestState newEntryState = ParseQuestState(entryState);
                        QuestLog.SetQuestEntryState(questName, entryNumber.Value, newEntryState);
                        return $"Set quest '{questName}' entry {entryNumber.Value} state to {newEntryState}.";
                    }

                    case "list":
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("=== ALL QUESTS ===");

                        string[] allQuests = QuestLog.GetAllQuests(QuestState.Unassigned | QuestState.Active | QuestState.Success | QuestState.Failure);
                        if (allQuests == null || allQuests.Length == 0)
                        {
                            sb.AppendLine("  (no quests found)");
                        }
                        else
                        {
                            foreach (string quest in allQuests)
                            {
                                QuestState qs = QuestLog.GetQuestState(quest);
                                sb.AppendLine($"  [{qs}] {quest}");
                            }
                        }

                        return sb.ToString();
                    }

                    default:
                        return $"ERROR: Unknown action '{action}'. Valid actions: get, set, getentry, setentry, list.";
                }
            });
        }

        static QuestState ParseQuestState(string stateStr)
        {
            string lower = stateStr.ToLowerInvariant();
            switch (lower)
            {
                case "unassigned": return QuestState.Unassigned;
                case "active":     return QuestState.Active;
                case "success":    return QuestState.Success;
                case "failure":    return QuestState.Failure;
                default:           return QuestState.Unassigned;
            }
        }
    }
}
#endif
