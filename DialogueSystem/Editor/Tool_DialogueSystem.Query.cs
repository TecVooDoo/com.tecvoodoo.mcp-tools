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

                if (!DialogueManager.hasInstance)
                {
                    return "ERROR: No DialogueManager instance found in the scene.";
                }

                DialogueDatabase db = DialogueManager.masterDatabase;
                if (db == null)
                {
                    return "ERROR: DialogueManager has no masterDatabase assigned.";
                }

                // Active conversation state
                sb.AppendLine("=== ACTIVE CONVERSATION STATE ===");
                sb.AppendLine($"  IsConversationActive: {DialogueManager.isConversationActive}");
                sb.AppendLine($"  LastConversationStarted: {DialogueManager.lastConversationStarted}");
                string actorName = DialogueManager.currentActor != null ? DialogueManager.currentActor.name : "(none)";
                string conversantName = DialogueManager.currentConversant != null ? DialogueManager.currentConversant.name : "(none)";
                sb.AppendLine($"  CurrentActor: {actorName}");
                sb.AppendLine($"  CurrentConversant: {conversantName}");

                // Conversations
                sb.AppendLine("\n=== CONVERSATIONS ===");
                if (db.conversations != null)
                {
                    int convCount = db.conversations.Count;
                    sb.AppendLine($"  Total: {convCount}");
                    int limit = System.Math.Min(convCount, 50);
                    for (int i = 0; i < limit; i++)
                    {
                        Conversation conv = db.conversations[i];
                        int entryCount = conv.dialogueEntries != null ? conv.dialogueEntries.Count : 0;
                        sb.AppendLine($"  [{conv.id}] \"{conv.Title}\" | ActorID: {conv.ActorID} | ConversantID: {conv.ConversantID} | Entries: {entryCount}");
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
                if (db.actors != null)
                {
                    int actorCount = db.actors.Count;
                    sb.AppendLine($"  Total: {actorCount}");
                    int limit = System.Math.Min(actorCount, 50);
                    for (int i = 0; i < limit; i++)
                    {
                        Actor actor = db.actors[i];
                        sb.AppendLine($"  [{actor.id}] \"{actor.Name}\" | IsPlayer: {actor.IsPlayer}");
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
                if (db.variables != null)
                {
                    int varCount = db.variables.Count;
                    sb.AppendLine($"  Total: {varCount}");
                    int limit = System.Math.Min(varCount, 50);
                    for (int i = 0; i < limit; i++)
                    {
                        Variable variable = db.variables[i];
                        string initVal = variable.InitialValue;
                        string varType = variable.Type.ToString();
                        sb.AppendLine($"  [{variable.id}] \"{variable.Name}\" | Type: {varType} | InitialValue: {initVal}");
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
                if (db.items != null)
                {
                    int questCount = 0;
                    foreach (Item item in db.items)
                    {
                        if (!item.IsItem)
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
#endif
