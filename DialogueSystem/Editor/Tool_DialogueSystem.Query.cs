#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.DialogueSystem.Editor
{
    public partial class Tool_DialogueSystem
    {
        [McpPluginTool("ds-query", Title = "Dialogue System / Query Database")]
        [Description(@"Queries the Dialogue System database contents and active conversation state.
Lists conversations (title, actor, conversant, entry count), actors (name, isPlayer),
variables (name, initial value, type), quest count, and active conversation info.
Output is limited to 50 items per category. Requires DialogueManager in the scene.")]
        public string QueryDatabase()
        {
            return MainThread.Instance.Run(() =>
            {
                StringBuilder sb = new StringBuilder();

                if (!HasDialogueManager())
                    return "ERROR: No DialogueManager instance found in the scene.";

                object? db = GetMasterDatabase();
                if (db == null)
                    return "ERROR: DialogueManager has no masterDatabase assigned.";

                // Active conversation state
                sb.AppendLine("=== ACTIVE CONVERSATION STATE ===");
                bool isActive = GetStatic(DmType, "isConversationActive") is true;
                string lastConv = GetStatic(DmType, "lastConversationStarted")?.ToString() ?? "(none)";
                sb.AppendLine($"  IsConversationActive: {isActive}");
                sb.AppendLine($"  LastConversationStarted: {lastConv}");

                var currentActor = GetStatic(DmType, "currentActor") as Transform;
                var currentConversant = GetStatic(DmType, "currentConversant") as Transform;
                string actorName = currentActor != null ? currentActor.name : "(none)";
                string conversantName = currentConversant != null ? currentConversant.name : "(none)";
                sb.AppendLine($"  CurrentActor: {actorName}");
                sb.AppendLine($"  CurrentConversant: {conversantName}");

                // Conversations
                sb.AppendLine("\n=== CONVERSATIONS ===");
                var conversations = Get(db, "conversations") as IList;
                if (conversations != null)
                {
                    int convCount = conversations.Count;
                    sb.AppendLine($"  Total: {convCount}");
                    int limit = Math.Min(convCount, 50);
                    for (int i = 0; i < limit; i++)
                    {
                        object conv = conversations[i]!;
                        int id = (int)(Get(conv, "id") ?? 0);
                        string title = Get(conv, "Title")?.ToString() ?? "(unknown)";
                        int actorID = (int)(Get(conv, "ActorID") ?? 0);
                        int conversantID = (int)(Get(conv, "ConversantID") ?? 0);
                        var entries = Get(conv, "dialogueEntries") as IList;
                        int entryCount = entries?.Count ?? 0;
                        sb.AppendLine($"  [{id}] \"{title}\" | ActorID: {actorID} | ConversantID: {conversantID} | Entries: {entryCount}");
                    }
                    if (convCount > 50)
                        sb.AppendLine($"  ... and {convCount - 50} more conversations.");
                }
                else
                {
                    sb.AppendLine("  (none)");
                }

                // Actors
                sb.AppendLine("\n=== ACTORS ===");
                var actors = Get(db, "actors") as IList;
                if (actors != null)
                {
                    int actorCount = actors.Count;
                    sb.AppendLine($"  Total: {actorCount}");
                    int limit = Math.Min(actorCount, 50);
                    for (int i = 0; i < limit; i++)
                    {
                        object actor = actors[i]!;
                        int id = (int)(Get(actor, "id") ?? 0);
                        string name = Get(actor, "Name")?.ToString() ?? "(unknown)";
                        bool isPlayer = Get(actor, "IsPlayer") is true;
                        sb.AppendLine($"  [{id}] \"{name}\" | IsPlayer: {isPlayer}");
                    }
                    if (actorCount > 50)
                        sb.AppendLine($"  ... and {actorCount - 50} more actors.");
                }
                else
                {
                    sb.AppendLine("  (none)");
                }

                // Variables
                sb.AppendLine("\n=== VARIABLES ===");
                var variables = Get(db, "variables") as IList;
                if (variables != null)
                {
                    int varCount = variables.Count;
                    sb.AppendLine($"  Total: {varCount}");
                    int limit = Math.Min(varCount, 50);
                    for (int i = 0; i < limit; i++)
                    {
                        object variable = variables[i]!;
                        int id = (int)(Get(variable, "id") ?? 0);
                        string name = Get(variable, "Name")?.ToString() ?? "(unknown)";
                        string initVal = Get(variable, "InitialValue")?.ToString() ?? "";
                        string varType = Get(variable, "Type")?.ToString() ?? "(unknown)";
                        sb.AppendLine($"  [{id}] \"{name}\" | Type: {varType} | InitialValue: {initVal}");
                    }
                    if (varCount > 50)
                        sb.AppendLine($"  ... and {varCount - 50} more variables.");
                }
                else
                {
                    sb.AppendLine("  (none)");
                }

                // Quests (items where !IsItem)
                sb.AppendLine("\n=== QUESTS ===");
                var items = Get(db, "items") as IList;
                if (items != null)
                {
                    int questCount = 0;
                    foreach (object? item in items)
                    {
                        if (item != null && Get(item, "IsItem") is false)
                            questCount++;
                    }
                    sb.AppendLine($"  Quest count: {questCount}");
                }
                else
                {
                    sb.AppendLine("  (none)");
                }

                return sb.ToString();
            });
        }
    }
}
