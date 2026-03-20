#if HAS_MASTERAUDIO
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using DarkTonic.MasterAudio;

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
                if (MasterAudio.SafeInstance == null)
                    throw new InvalidOperationException("MasterAudio instance not found in scene.");

                string act = action.ToLowerInvariant().Trim();

                switch (act)
                {
                    case "mute":
                        MasterAudio.MuteGroup(groupName);
                        return $"OK: Group '{groupName}' muted.";

                    case "unmute":
                        MasterAudio.UnmuteGroup(groupName);
                        return $"OK: Group '{groupName}' unmuted.";

                    case "solo":
                        MasterAudio.SoloGroup(groupName);
                        return $"OK: Group '{groupName}' soloed.";

                    case "unsolo":
                        MasterAudio.UnsoloGroup(groupName);
                        return $"OK: Group '{groupName}' unsoloed.";

                    case "pause":
                        MasterAudio.PauseGroup(groupName);
                        return $"OK: Group '{groupName}' paused.";

                    case "unpause":
                        MasterAudio.UnpauseGroup(groupName);
                        return $"OK: Group '{groupName}' unpaused.";

                    case "stop":
                        MasterAudio.StopAllOfSound(groupName);
                        return $"OK: Group '{groupName}' stopped.";

                    case "fade":
                        float targetVol = volume ?? 0f;
                        float fadeDur = fadeTime ?? 1f;
                        MasterAudio.FadeSoundGroupToVolume(groupName, targetVol, fadeDur);
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
#endif
