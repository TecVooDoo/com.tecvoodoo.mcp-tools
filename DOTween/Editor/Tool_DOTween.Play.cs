#if HAS_DOTWEEN
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
// System.ComponentModel (imported for [Description]) also defines Component.
using Component = UnityEngine.Component;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_DOTween
    {
        [McpPluginTool("dotween-play", Title = "DOTween / Play Control")]
        [Description(@"Runtime control of DOTweenAnimation components on a GameObject.
Supports play, pause, rewind, restart, complete, and kill actions.
Optionally target a specific tween by id.")]
        public string PlayControl(
            [Description("Name of the GameObject with DOTweenAnimation components.")]
            string gameObjectName,
            [Description("Action to perform: play, pause, rewind, restart, complete, kill.")]
            string action,
            [Description("Optional tween id to target a specific animation. Null targets all on the GameObject.")]
            string? id = null
        )
        {
            if (string.IsNullOrEmpty(gameObjectName))
                throw new ArgumentException("gameObjectName cannot be null or empty.", nameof(gameObjectName));
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("action cannot be null or empty.", nameof(action));

            string normalizedAction = action.Trim().ToLowerInvariant();

            // Resolved up front so an invalid action always errors, rather than only when
            // the id filter happens to match at least one component.
            string methodName = ResolvePlayMethod(normalizedAction, action);

            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                Component[] anims = GetAnims(go);

                if (anims.Length == 0)
                    throw new InvalidOperationException($"No DOTweenAnimation components found on '{gameObjectName}'.");

                int affected = 0;

                for (int i = 0; i < anims.Length; i++)
                {
                    Component anim = anims[i];

                    // Filter by id if provided
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (!string.Equals(GetField<string>(anim, "id"), id, StringComparison.OrdinalIgnoreCase))
                            continue;
                    }

                    CallParameterless(anim, methodName);
                    affected++;
                }

                if (affected == 0)
                {
                    if (!string.IsNullOrEmpty(id))
                        return $"No DOTweenAnimation with id '{id}' found on '{gameObjectName}'.";
                    return $"No DOTweenAnimation components found on '{gameObjectName}'.";
                }

                string target = string.IsNullOrEmpty(id) ? "all" : $"id='{id}'";
                return $"OK: {normalizedAction} executed on {affected} DOTweenAnimation(s) ({target}) on '{gameObjectName}'.";
            });
        }

        static string ResolvePlayMethod(string normalizedAction, string rawAction)
        {
            switch (normalizedAction)
            {
                case "play":     return "DOPlay";
                case "pause":    return "DOPause";
                case "rewind":   return "DORewind";
                case "restart":  return "DORestart";
                case "complete": return "DOComplete";
                case "kill":     return "DOKill";
                default:
                    throw new ArgumentException($"Invalid action '{rawAction}'. Valid values: play, pause, rewind, restart, complete, kill.", "action");
            }
        }
    }
}
#endif
