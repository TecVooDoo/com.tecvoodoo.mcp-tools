#if HAS_MALBERS_QUESTFORGE
#nullable enable
using System.ComponentModel;
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
                Quest quest = AssetDatabase.LoadAssetAtPath<Quest>(questAssetPath);
                if (quest == null)
                    throw new System.Exception($"Quest not found at '{questAssetPath}'.");

                if (quest.objectives == null)
                    quest.objectives = new System.Collections.Generic.List<QuestObjective>();

                string typeLower = objectiveType.ToLower();
                QuestObjective objective;

                if (typeLower == "kill")
                {
                    var kill = new KillObjective();
                    kill.targetTag = targetId;
                    kill.requiredKills = requiredCount;
                    if (!string.IsNullOrEmpty(specificId))
                        kill.specificEnemyId = specificId;
                    objective = kill;
                }
                else if (typeLower == "collect")
                {
                    var collect = new CollectObjective();
                    collect.itemId = targetId;
                    collect.requiredAmount = requiredCount;
                    objective = collect;
                }
                else if (typeLower == "talkto")
                {
                    var talk = new TalkToObjective();
                    talk.npcId = targetId;
                    if (!string.IsNullOrEmpty(specificId))
                        talk.specificDialogueId = specificId;
                    objective = talk;
                }
                else if (typeLower == "gotolocation")
                {
                    var goTo = new GoToLocationObjective();
                    goTo.useManualValues = true;
                    goTo.manualLocationId = targetId;
                    goTo.manualTargetPosition = new Vector3(posX, posY, posZ);
                    goTo.manualRequiredDistance = requiredDistance;
                    objective = goTo;
                }
                else if (typeLower == "interact")
                {
                    var interact = new InteractObjective();
                    interact.interactableId = targetId;
                    interact.requiredInteractions = requiredCount;
                    objective = interact;
                }
                else
                {
                    throw new System.Exception($"Unknown objective type '{objectiveType}'. Use: Kill, Collect, TalkTo, GoToLocation, Interact.");
                }

                quest.objectives.Add(objective);

                EditorUtility.SetDirty(quest);
                AssetDatabase.SaveAssets();

                return new AddObjectiveResponse
                {
                    questAssetPath = questAssetPath,
                    questId = quest.QuestID,
                    objectiveType = objectiveType,
                    targetId = targetId,
                    requiredCount = requiredCount,
                    totalObjectives = quest.objectives.Count
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
#endif
