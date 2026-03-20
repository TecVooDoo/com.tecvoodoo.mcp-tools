#if HAS_DOTWEEN
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using UnityEditor;
using DG.Tweening;

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

            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                DOTweenAnimation[] anims = go.GetComponents<DOTweenAnimation>();

                if (anims.Length == 0)
                    throw new InvalidOperationException($"No DOTweenAnimation components found on '{gameObjectName}'.");

                int affected = 0;

                for (int i = 0; i < anims.Length; i++)
                {
                    DOTweenAnimation anim = anims[i];

                    // Filter by id if provided
                    if (!string.IsNullOrEmpty(id))
                    {
                        if (!string.Equals(anim.id, id, StringComparison.OrdinalIgnoreCase))
                            continue;
                    }

                    if (normalizedAction == "play")
                        anim.DOPlay();
                    else if (normalizedAction == "pause")
                        anim.DOPause();
                    else if (normalizedAction == "rewind")
                        anim.DORewind();
                    else if (normalizedAction == "restart")
                        anim.DORestart();
                    else if (normalizedAction == "complete")
                        anim.DOComplete();
                    else if (normalizedAction == "kill")
                        anim.DOKill();
                    else
                        throw new ArgumentException($"Invalid action '{action}'. Valid values: play, pause, rewind, restart, complete, kill.", nameof(action));

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
    }
}
#endif
