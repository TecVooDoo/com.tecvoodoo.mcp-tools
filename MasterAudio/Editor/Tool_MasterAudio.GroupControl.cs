#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_MasterAudio
    {
        [McpPluginTool("ma-group-control", Title = "Master Audio / Group Control")]
        [Description(@"Control a sound group: mute, unmute, solo, unsolo, pause, unpause, stop, or fade.
For fade action, provide volume and optional fadeTime.")]
        public string GroupControl(
            [Description("Sound group name.")]
            string groupName,
            [Description("Action: mute, unmute, solo, unsolo, pause, unpause, stop, fade.")]
            string action,
            [Description("Target volume for fade action (0-1).")]
            float? volume = null,
            [Description("Fade duration in seconds. Default 1.0.")]
            float? fadeTime = null
        )
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("groupName cannot be null or empty.", nameof(groupName));
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("action cannot be null or empty.", nameof(action));

            return MainThread.Instance.Run(() =>
            {
                EnsureInstance();

                string act = action.ToLowerInvariant().Trim();

                switch (act)
                {
                    case "mute":
                        CallMA("MuteGroup", groupName);
                        return $"OK: Group '{groupName}' muted.";

                    case "unmute":
                        CallMA("UnmuteGroup", groupName);
                        return $"OK: Group '{groupName}' unmuted.";

                    case "solo":
                        CallMA("SoloGroup", groupName);
                        return $"OK: Group '{groupName}' soloed.";

                    case "unsolo":
                        CallMA("UnsoloGroup", groupName);
                        return $"OK: Group '{groupName}' unsoloed.";

                    case "pause":
                        CallMA("PauseSoundGroup", groupName);
                        return $"OK: Group '{groupName}' paused.";

                    case "unpause":
                        CallMA("UnpauseSoundGroup", groupName);
                        return $"OK: Group '{groupName}' unpaused.";

                    case "stop":
                        CallMA("StopAllOfSound", groupName);
                        return $"OK: Group '{groupName}' stopped.";

                    case "fade":
                        float targetVol = volume ?? 0f;
                        float fadeDur = fadeTime ?? 1f;
                        CallMA("FadeSoundGroupToVolume", groupName, targetVol, fadeDur);
                        return $"OK: Group '{groupName}' fading to {targetVol:F2} over {fadeDur:F2}s.";

                    default:
                        throw new ArgumentException(
                            $"Unknown action '{action}'. Valid: mute, unmute, solo, unsolo, pause, unpause, stop, fade.",
                            nameof(action));
                }
            });
        }
    }
}
