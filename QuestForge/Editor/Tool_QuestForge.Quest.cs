#if HAS_MALBERS_QUESTFORGE
#nullable enable
using System.ComponentModel;
using System.IO;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using MalbersAnimations.QuestForge;
using UnityEditor;
using UnityEngine;

namespace MCPTools.QuestForge.Editor
{
    public partial class Tool_QuestForge
    {
        [McpPluginTool("qf-create-quest", Title = "Quest Forge / Create Quest")]
        [Description(@"Creates a new Quest ScriptableObject asset.
The quest is saved to the specified path and can have objectives added via 'qf-add-objective'.
Quest types: Main, Side, Daily, Repeatable.")]
        public CreateQuestResponse CreateQuest(
            [Description("Asset path for the new quest SO (e.g. 'Assets/_Sandbox/_AQS/Data/Quests/Quest_FindJoey.asset').")]
            string assetPath,
            [Description("Unique quest ID string (e.g. 'find_joey_01'). Must be unique across all quests.")]
            string questId,
            [Description("Display name of the quest.")]
            string questName,
            [Description("Quest description text.")]
            string description,
            [Description("Quest type: 'Main', 'Side', 'Daily', 'Repeatable'. Default 'Side'.")]
            string questType = "Side",
            [Description("Whether the quest can be repeated after completion. Default false.")]
            bool isRepeatable = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string directory = Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                Quest quest = ScriptableObject.CreateInstance<Quest>();
                quest.QuestID = questId;
                quest.questName = questName;
                quest.questDescription = description;

                if (System.Enum.TryParse<Quest.QuestType>(questType, true, out var parsedType))
                    quest.questType = parsedType;

                quest.isRepeatable = isRepeatable;

                AssetDatabase.CreateAsset(quest, assetPath);
                AssetDatabase.SaveAssets();

                return new CreateQuestResponse
                {
                    assetPath = assetPath,
                    questId = quest.QuestID,
                    questName = quest.questName,
                    questType = quest.questType.ToString(),
                    isRepeatable = quest.isRepeatable
                };
            });
        }

        [McpPluginTool("qf-query-quests", Title = "Quest Forge / Query Quests")]
        [Description(@"Lists all Quest ScriptableObject assets in the project.
Shows quest ID, name, type, objective count, and prerequisite count for each quest.
Use to get an overview of all quests in the project.")]
        public QueryQuestsResponse QueryQuests(
            [Description("Folder path to search (e.g. 'Assets/_Sandbox/_AQS/Data/Quests'). Null to search entire project.")]
            string? searchFolder = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string[] guids;
                if (!string.IsNullOrEmpty(searchFolder))
                    guids = AssetDatabase.FindAssets("t:Quest", new[] { searchFolder });
                else
                    guids = AssetDatabase.FindAssets("t:Quest");

                var sb = new StringBuilder();
                int count = 0;

                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    Quest quest = AssetDatabase.LoadAssetAtPath<Quest>(path);
                    if (quest == null) continue;

                    count++;
                    int objCount = quest.objectives != null ? quest.objectives.Count : 0;
                    int prereqCount = quest.prerequisiteQuestIDs != null ? quest.prerequisiteQuestIDs.Count : 0;

                    sb.AppendLine($"  [{quest.QuestID}] {quest.questName} | Type: {quest.questType} | Objectives: {objCount} | Prerequisites: {prereqCount} | Repeatable: {quest.isRepeatable}");
                    sb.AppendLine($"    Path: {path}");

                    if (quest.objectives != null)
                    {
                        foreach (var obj in quest.objectives)
                        {
                            if (obj == null) continue;
                            string objType = obj.GetType().Name;
                            sb.AppendLine($"    - {objType}: {obj.GetProgressText(null)}");
                        }
                    }
                }

                return new QueryQuestsResponse
                {
                    questCount = count,
                    details = sb.ToString()
                };
            });
        }

        public class CreateQuestResponse
        {
            [Description("Asset path where quest was saved")] public string assetPath = "";
            [Description("Quest ID")] public string questId = "";
            [Description("Quest display name")] public string questName = "";
            [Description("Quest type")] public string questType = "";
            [Description("Whether quest is repeatable")] public bool isRepeatable;
        }

        public class QueryQuestsResponse
        {
            [Description("Number of quests found")] public int questCount;
            [Description("Detailed quest listing")] public string details = "";
        }
    }
}
#endif
