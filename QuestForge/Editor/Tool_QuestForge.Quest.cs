#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
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
                var questTypeRef = FindType(QUEST_TYPE_NAME);
                if (questTypeRef == null)
                    throw new Exception($"Type '{QUEST_TYPE_NAME}' not found. Is QuestForge installed?");

                string directory = Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                var quest = ScriptableObject.CreateInstance(questTypeRef);

                Set(quest, "QuestID", questId);
                Set(quest, "questName", questName);
                Set(quest, "questDescription", description);

                // Parse quest type enum: Quest.QuestType
                var questTypeEnumType = questTypeRef.GetNestedType("QuestType");
                if (questTypeEnumType != null && Enum.TryParse(questTypeEnumType, questType, true, out var parsedType))
                    Set(quest, "questType", parsedType!);

                Set(quest, "isRepeatable", isRepeatable);

                AssetDatabase.CreateAsset(quest, assetPath);
                AssetDatabase.SaveAssets();

                string resultId = Get(quest, "QuestID")?.ToString() ?? questId;
                string resultName = Get(quest, "questName")?.ToString() ?? questName;
                string resultType = Get(quest, "questType")?.ToString() ?? questType;
                bool resultRepeatable = (bool)(Get(quest, "isRepeatable") ?? isRepeatable);

                return new CreateQuestResponse
                {
                    assetPath = assetPath,
                    questId = resultId,
                    questName = resultName,
                    questType = resultType,
                    isRepeatable = resultRepeatable
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
                var questTypeRef = FindType(QUEST_TYPE_NAME);
                if (questTypeRef == null)
                    throw new Exception($"Type '{QUEST_TYPE_NAME}' not found. Is QuestForge installed?");

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
                    var quest = AssetDatabase.LoadAssetAtPath(path, questTypeRef);
                    if (quest == null) continue;

                    count++;
                    string qId = Get(quest, "QuestID")?.ToString() ?? "?";
                    string qName = Get(quest, "questName")?.ToString() ?? "?";
                    string qType = Get(quest, "questType")?.ToString() ?? "?";
                    bool repeatable = (bool)(Get(quest, "isRepeatable") ?? false);

                    var objectives = Get(quest, "objectives") as IList;
                    int objCount = objectives != null ? objectives.Count : 0;

                    var prereqs = Get(quest, "prerequisiteQuestIDs") as IList;
                    int prereqCount = prereqs != null ? prereqs.Count : 0;

                    sb.AppendLine($"  [{qId}] {qName} | Type: {qType} | Objectives: {objCount} | Prerequisites: {prereqCount} | Repeatable: {repeatable}");
                    sb.AppendLine($"    Path: {path}");

                    if (objectives != null)
                    {
                        foreach (var obj in objectives)
                        {
                            if (obj == null) continue;
                            string objType = obj.GetType().Name;
                            string progressText = "";
                            try
                            {
                                progressText = Call(obj, "GetProgressText", (object)null!)?.ToString() ?? "";
                            }
                            catch { /* method may not exist or may fail with null */ }
                            sb.AppendLine($"    - {objType}: {progressText}");
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
