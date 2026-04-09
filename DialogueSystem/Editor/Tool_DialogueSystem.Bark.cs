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
        [McpPluginTool("ds-bark", Title = "Dialogue System / Bark")]
        [Description(@"Triggers a character bark (short dialogue line above a character's head).
Either provide a conversationTitle to bark from a conversation, or barkText for raw text.
If both are provided, barkText takes priority. Requires a speaker GameObject in the scene.")]
        public string TriggerBark(
            [Description("Scene GameObject name for the speaker character.")]
            string speakerName,
            [Description("Conversation title to bark from (picks a random valid entry, or use entryID).")]
            string? conversationTitle = null,
            [Description("Scene GameObject name for the listener character.")]
            string? listenerName = null,
            [Description("Raw text string to bark (alternative to conversation-based bark).")]
            string? barkText = null,
            [Description("Specific dialogue entry ID to bark from the conversation.")]
            int? entryID = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!HasDialogueManager())
                    return "ERROR: No DialogueManager instance found in the scene.";

                Transform? speakerTransform = FindTransformByName(speakerName);
                if (speakerTransform == null)
                    return $"ERROR: Speaker GameObject '{speakerName}' not found in scene.";

                Transform? listenerTransform = FindTransformByName(listenerName);
                if (!string.IsNullOrEmpty(listenerName) && listenerTransform == null)
                    return $"ERROR: Listener GameObject '{listenerName}' not found in scene.";

                // Raw text bark takes priority
                if (!string.IsNullOrEmpty(barkText))
                {
                    CallStatic(DmType, "BarkString", barkText, speakerTransform, listenerTransform!);
                    return $"Barked raw text from '{speakerName}': \"{barkText}\"";
                }

                // Conversation-based bark
                if (!string.IsNullOrEmpty(conversationTitle))
                {
                    if (entryID.HasValue)
                    {
                        CallStatic(DmType, "Bark", conversationTitle, speakerTransform, listenerTransform!, entryID.Value);
                        return $"Barked from '{conversationTitle}' entry {entryID.Value} via '{speakerName}'.";
                    }
                    else
                    {
                        CallStatic(DmType, "Bark", conversationTitle, speakerTransform, listenerTransform!);
                        return $"Barked from '{conversationTitle}' via '{speakerName}'.";
                    }
                }

                return "ERROR: Provide either 'barkText' or 'conversationTitle'.";
            });
        }
    }
}
