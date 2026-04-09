#if HAS_JUICY_ACTIONS
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.JuicyActions.Editor
{
    public partial class Tool_JuicyActions
    {
        [McpPluginTool("juicy-play", Title = "Juicy Actions / Play Trigger")]
        [Description(@"Triggers execution of a Juicy Actions trigger on a GameObject at runtime.
Finds all ActionOnEvent-derived triggers on the GameObject and fires the one at the
specified index. Only works in play mode -- returns an error if the editor is not playing.")]
        public string PlayTrigger(
            [Description("Name of the GameObject with Juicy Actions trigger components.")]
            string gameObjectName,
            [Description("Zero-based index of which trigger to fire (default 0).")]
            int triggerIndex = 0
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!Application.isPlaying)
                    throw new Exception("juicy-play requires Play mode. Enter play mode first.");

                var triggers = GetTriggers(gameObjectName);

                if (triggerIndex < 0 || triggerIndex >= triggers.Length)
                    throw new Exception($"triggerIndex {triggerIndex} out of range. '{gameObjectName}' has {triggers.Length} trigger(s) (0-{triggers.Length - 1}).");

                var trigger = triggers[triggerIndex];
                var typeName = trigger.GetType().Name;

                // Try PlayActions() first, then Execute() as fallback
                var playMethod = trigger.GetType().GetMethod("PlayActions",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (playMethod != null)
                {
                    playMethod.Invoke(trigger, null);
                    return $"OK: Triggered PlayActions() on [{triggerIndex}] {typeName} on '{gameObjectName}'.";
                }

                var executeMethod = trigger.GetType().GetMethod("Execute",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (executeMethod != null)
                {
                    executeMethod.Invoke(trigger, null);
                    return $"OK: Triggered Execute() on [{triggerIndex}] {typeName} on '{gameObjectName}'.";
                }

                throw new Exception($"Neither PlayActions() nor Execute() found on {typeName}. Check available methods on this trigger type.");
            });
        }
    }
}
#endif
