#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.QuestForge.Editor
{
    public partial class Tool_QuestForge
    {
        [McpPluginTool("qf-add-objective", Title = "Quest Forge / Add Objective")]
        [Description(@"Adds an objective to an existing Quest ScriptableObject.
Objective types: Kill, Collect, TalkTo, GoToLocation, Interact.
Each type has specific parameters. Only parameters relevant to the chosen type are used.")]
        public AddObjectiveResponse AddObjective(
            [Description("Asset path of the Quest SO to modify (e.g. 'Assets/_Sandbox/_AQS/Data/Quests/Quest_FindJoey.asset').")]
            string questAssetPath,
            [Description("Objective type: 'Kill', 'Collect', 'TalkTo', 'GoToLocation', 'Interact'.")]
            string objectiveType,
            [Description("For Kill: enemy tag to kill (e.g. 'Enemy', 'Snake'). For Collect: item ID. For TalkTo: NPC ID. For GoToLocation: location ID. For Interact: interactable ID.")]
            string targetId,
            [Description("Required count (kills, items, interactions). Default 1.")]
            int requiredCount = 1,
            [Description("For Kill: specific enemy ID (optional, empty = any with tag). For TalkTo: specific dialogue ID (optional).")]
            string? specificId = null,
            [Description("For GoToLocation: target X position. Default 0.")]
            float posX = 0f,
            [Description("For GoToLocation: target Y position. Default 0.")]
            float posY = 0f,
            [Description("For GoToLocation: target Z position. Default 0.")]
            float posZ = 0f,
            [Description("For GoToLocation: required distance to complete. Default 5.")]
            float requiredDistance = 5f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var questType = FindType(QUEST_TYPE_NAME);
                if (questType == null)
                    throw new Exception($"Type '{QUEST_TYPE_NAME}' not found. Is QuestForge installed?");

                var quest = AssetDatabase.LoadAssetAtPath(questAssetPath, questType);
                if (quest == null)
                    throw new Exception($"Quest not found at '{questAssetPath}'.");

                // Ensure objectives list exists
                var objectives = Get(quest, "objectives") as IList;
                if (objectives == null)
                {
                    var objType = FindType(QUEST_OBJECTIVE_TYPE_NAME);
                    if (objType == null)
                        throw new Exception($"Type '{QUEST_OBJECTIVE_TYPE_NAME}' not found.");
                    var listType = typeof(List<>).MakeGenericType(objType);
                    objectives = (IList)Activator.CreateInstance(listType)!;
                    Set(quest, "objectives", objectives);
                }

                string typeLower = objectiveType.ToLower();
                object objective;

                if (typeLower == "kill")
                {
                    var killType = FindType(KILL_OBJECTIVE_TYPE_NAME);
                    if (killType == null) throw new Exception($"Type '{KILL_OBJECTIVE_TYPE_NAME}' not found.");
                    objective = Activator.CreateInstance(killType)!;
                    Set(objective, "targetTag", targetId);
                    Set(objective, "requiredKills", requiredCount);
                    if (!string.IsNullOrEmpty(specificId))
                        Set(objective, "specificEnemyId", specificId);
                }
                else if (typeLower == "collect")
                {
                    var collectType = FindType(COLLECT_OBJECTIVE_TYPE_NAME);
                    if (collectType == null) throw new Exception($"Type '{COLLECT_OBJECTIVE_TYPE_NAME}' not found.");
                    objective = Activator.CreateInstance(collectType)!;
                    Set(objective, "itemId", targetId);
                    Set(objective, "requiredAmount", requiredCount);
                }
                else if (typeLower == "talkto")
                {
                    var talkType = FindType(TALKTO_OBJECTIVE_TYPE_NAME);
                    if (talkType == null) throw new Exception($"Type '{TALKTO_OBJECTIVE_TYPE_NAME}' not found.");
                    objective = Activator.CreateInstance(talkType)!;
                    Set(objective, "npcId", targetId);
                    if (!string.IsNullOrEmpty(specificId))
                        Set(objective, "specificDialogueId", specificId);
                }
                else if (typeLower == "gotolocation")
                {
                    var goToType = FindType(GOTO_OBJECTIVE_TYPE_NAME);
                    if (goToType == null) throw new Exception($"Type '{GOTO_OBJECTIVE_TYPE_NAME}' not found.");
                    objective = Activator.CreateInstance(goToType)!;
                    Set(objective, "useManualValues", true);
                    Set(objective, "manualLocationId", targetId);
                    Set(objective, "manualTargetPosition", new Vector3(posX, posY, posZ));
                    Set(objective, "manualRequiredDistance", requiredDistance);
                }
                else if (typeLower == "interact")
                {
                    var interactType = FindType(INTERACT_OBJECTIVE_TYPE_NAME);
                    if (interactType == null) throw new Exception($"Type '{INTERACT_OBJECTIVE_TYPE_NAME}' not found.");
                    objective = Activator.CreateInstance(interactType)!;
                    Set(objective, "interactableId", targetId);
                    Set(objective, "requiredInteractions", requiredCount);
                }
                else
                {
                    throw new Exception($"Unknown objective type '{objectiveType}'. Use: Kill, Collect, TalkTo, GoToLocation, Interact.");
                }

                objectives.Add(objective);

                EditorUtility.SetDirty(quest);
                AssetDatabase.SaveAssets();

                string questId = Get(quest, "QuestID")?.ToString() ?? "";

                return new AddObjectiveResponse
                {
                    questAssetPath = questAssetPath,
                    questId = questId,
                    objectiveType = objectiveType,
                    targetId = targetId,
                    requiredCount = requiredCount,
                    totalObjectives = objectives.Count
                };
            });
        }

        public class AddObjectiveResponse
        {
            [Description("Asset path of the quest")] public string questAssetPath = "";
            [Description("Quest ID")] public string questId = "";
            [Description("Type of objective added")] public string objectiveType = "";
            [Description("Target ID for the objective")] public string targetId = "";
            [Description("Required count")] public int requiredCount;
            [Description("Total objectives on the quest")] public int totalObjectives;
        }
    }
}
