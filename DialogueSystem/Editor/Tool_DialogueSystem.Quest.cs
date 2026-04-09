#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

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
                if (!HasDialogueManager())
                    return "ERROR: No DialogueManager instance found in the scene.";

                string actionLower = (action ?? "get").ToLowerInvariant();

                switch (actionLower)
                {
                    case "get":
                    {
                        var currentState = CallStatic(QuestLogType, "GetQuestState", questName);
                        var description = CallStatic(QuestLogType, "GetQuestDescription", questName);

                        // GetQuestDescription with QuestState param
                        var successState = ParseQuestStateEnum("success");
                        var failureState = ParseQuestStateEnum("failure");
                        var successDesc = CallStaticExplicit(QuestLogType, "GetQuestDescription",
                            new Type[] { typeof(string), QuestStateType! }, questName, successState!);
                        var failureDesc = CallStaticExplicit(QuestLogType, "GetQuestDescription",
                            new Type[] { typeof(string), QuestStateType! }, questName, failureState!);

                        var entryCount = CallStatic(QuestLogType, "GetQuestEntryCount", questName);
                        int entryCountInt = entryCount is int ec ? ec : 0;

                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine($"Quest: {questName}");
                        sb.AppendLine($"  State: {currentState}");
                        sb.AppendLine($"  Description: {description}");
                        sb.AppendLine($"  Success Desc: {successDesc}");
                        sb.AppendLine($"  Failure Desc: {failureDesc}");
                        sb.AppendLine($"  Entry Count: {entryCountInt}");

                        for (int i = 1; i <= entryCountInt; i++)
                        {
                            var entryQState = CallStatic(QuestLogType, "GetQuestEntryState", questName, i);
                            var entryDesc = CallStatic(QuestLogType, "GetQuestEntry", questName, i);
                            sb.AppendLine($"  Entry {i}: [{entryQState}] {entryDesc}");
                        }

                        return sb.ToString();
                    }

                    case "set":
                    {
                        if (string.IsNullOrEmpty(state))
                            return "ERROR: 'state' parameter is required for 'set' action.";

                        var newState = ParseQuestStateEnum(state);
                        if (newState == null)
                            return "ERROR: Could not resolve QuestState type.";

                        CallStaticExplicit(QuestLogType, "SetQuestState",
                            new Type[] { typeof(string), QuestStateType! }, questName, newState);
                        return $"Set quest '{questName}' state to {newState}.";
                    }

                    case "getentry":
                    {
                        if (entryNumber == null)
                            return "ERROR: 'entryNumber' parameter is required for 'getentry' action.";

                        var entryQState = CallStatic(QuestLogType, "GetQuestEntryState", questName, entryNumber.Value);
                        var entryDesc = CallStatic(QuestLogType, "GetQuestEntry", questName, entryNumber.Value);
                        return $"Quest '{questName}' Entry {entryNumber.Value}: [{entryQState}] {entryDesc}";
                    }

                    case "setentry":
                    {
                        if (entryNumber == null)
                            return "ERROR: 'entryNumber' parameter is required for 'setentry' action.";
                        if (string.IsNullOrEmpty(entryState))
                            return "ERROR: 'entryState' parameter is required for 'setentry' action.";

                        var newEntryState = ParseQuestStateEnum(entryState);
                        if (newEntryState == null)
                            return "ERROR: Could not resolve QuestState type.";

                        CallStaticExplicit(QuestLogType, "SetQuestEntryState",
                            new Type[] { typeof(string), typeof(int), QuestStateType! },
                            questName, entryNumber.Value, newEntryState);
                        return $"Set quest '{questName}' entry {entryNumber.Value} state to {newEntryState}.";
                    }

                    case "list":
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("=== ALL QUESTS ===");

                        var allFlags = AllQuestStateFlags();
                        if (allFlags == null)
                            return "ERROR: Could not resolve QuestState type.";

                        var allQuests = CallStaticExplicit(QuestLogType, "GetAllQuests",
                            new Type[] { QuestStateType! }, allFlags) as string[];

                        if (allQuests == null || allQuests.Length == 0)
                        {
                            sb.AppendLine("  (no quests found)");
                        }
                        else
                        {
                            foreach (string quest in allQuests)
                            {
                                var qs = CallStatic(QuestLogType, "GetQuestState", quest);
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
    }
}
