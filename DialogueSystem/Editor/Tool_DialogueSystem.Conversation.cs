#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.DialogueSystem.Editor
{
    public partial class Tool_DialogueSystem
    {
        [McpPluginTool("ds-conversation", Title = "Dialogue System / Conversation")]
        [Description(@"Start, stop, or check conversations.
Actions: 'start' (begins a conversation), 'stop' (stops current conversation),
'stopall' (stops all active conversations), 'check' (checks if conversation has valid entries).
For 'start' and 'check', provide the conversation title. Optionally specify actor and conversant
GameObjects by scene name, and an initial entry ID.")]
        public string ManageConversation(
            [Description("Action to perform: 'start', 'stop', 'stopall', 'check'.")]
            string action,
            [Description("Conversation title (required for 'start' and 'check').")]
            string? title = null,
            [Description("Scene GameObject name for the actor participant.")]
            string? actorName = null,
            [Description("Scene GameObject name for the conversant participant.")]
            string? conversantName = null,
            [Description("Initial dialogue entry ID. Default -1 starts from the beginning.")]
            int entryID = -1
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!HasDialogueManager())
                    return "ERROR: No DialogueManager instance found in the scene.";

                string actionLower = action.ToLowerInvariant();

                switch (actionLower)
                {
                    case "start":
                    {
                        if (string.IsNullOrEmpty(title))
                            return "ERROR: 'title' is required for 'start' action.";

                        Transform? actorTransform = FindTransformByName(actorName);
                        Transform? conversantTransform = FindTransformByName(conversantName);

                        if (!string.IsNullOrEmpty(actorName) && actorTransform == null)
                            return $"ERROR: Actor GameObject '{actorName}' not found in scene.";
                        if (!string.IsNullOrEmpty(conversantName) && conversantTransform == null)
                            return $"ERROR: Conversant GameObject '{conversantName}' not found in scene.";

                        CallStatic(DmType, "StartConversation", title, actorTransform!, conversantTransform!, entryID);
                        return $"Started conversation '{title}' (actor: {actorName ?? "(none)"}, conversant: {conversantName ?? "(none)"}, entryID: {entryID}).";
                    }

                    case "stop":
                    {
                        CallStatic(DmType, "StopConversation");
                        return "Stopped current conversation.";
                    }

                    case "stopall":
                    {
                        CallStatic(DmType, "StopAllConversations");
                        return "Stopped all active conversations.";
                    }

                    case "check":
                    {
                        if (string.IsNullOrEmpty(title))
                            return "ERROR: 'title' is required for 'check' action.";

                        Transform? actorTransform = FindTransformByName(actorName);
                        Transform? conversantTransform = FindTransformByName(conversantName);

                        var result = CallStatic(DmType, "ConversationHasValidEntry", title, actorTransform!, conversantTransform!);
                        bool hasValid = result is true;
                        return $"Conversation '{title}' has valid entry: {hasValid}";
                    }

                    default:
                        return $"ERROR: Unknown action '{action}'. Valid actions: start, stop, stopall, check.";
                }
            });
        }
    }
}
