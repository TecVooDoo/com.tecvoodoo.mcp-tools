#if HAS_DOTWEEN
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using DG.Tweening;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_DOTween
    {
        [McpPluginTool("dotween-global", Title = "DOTween / Global Control")]
        [Description(@"Global DOTween control. Affects ALL active tweens in the scene.
Actions: killall, pauseall, playall, completeall, rewindall.")]
        public string GlobalControl(
            [Description("Global action: killall, pauseall, playall, completeall, rewindall.")]
            string action
        )
        {
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("action cannot be null or empty.", nameof(action));

            string normalizedAction = action.Trim().ToLowerInvariant();

            return MainThread.Instance.Run(() =>
            {
                if (normalizedAction == "killall")
                {
                    int killed = DOTween.KillAll();
                    return $"OK: DOTween.KillAll() -- {killed} tween(s) killed.";
                }
                else if (normalizedAction == "pauseall")
                {
                    int paused = DOTween.PauseAll();
                    return $"OK: DOTween.PauseAll() -- {paused} tween(s) paused.";
                }
                else if (normalizedAction == "playall")
                {
                    int played = DOTween.PlayAll();
                    return $"OK: DOTween.PlayAll() -- {played} tween(s) playing.";
                }
                else if (normalizedAction == "completeall")
                {
                    int completed = DOTween.CompleteAll();
                    return $"OK: DOTween.CompleteAll() -- {completed} tween(s) completed.";
                }
                else if (normalizedAction == "rewindall")
                {
                    int rewound = DOTween.RewindAll();
                    return $"OK: DOTween.RewindAll() -- {rewound} tween(s) rewound.";
                }
                else
                {
                    throw new ArgumentException($"Invalid action '{action}'. Valid values: killall, pauseall, playall, completeall, rewindall.", nameof(action));
                }
            });
        }
    }
}
#endif
