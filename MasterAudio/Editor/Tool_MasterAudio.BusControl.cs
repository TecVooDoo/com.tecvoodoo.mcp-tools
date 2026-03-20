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
        [McpPluginTool("ma-bus-control", Title = "Master Audio / Bus Control")]
        [Description(@"Control a bus: mute, unmute, solo, unsolo, pause, unpause, stop, fade, or pitch.
For fade action, provide volume and optional fadeTime.
For pitch action, provide pitch value.")]
        public string BusControl(
            [Description("Bus name.")]
            string busName,
            [Description("Action: mute, unmute, solo, unsolo, pause, unpause, stop, fade, pitch.")]
            string action,
            [Description("Target volume for fade action (0-1).")]
            float? volume = null,
            [Description("Fade duration in seconds. Default 1.0.")]
            float? fadeTime = null,
            [Description("Pitch value for pitch action.")]
            float? pitch = null
        )
        {
            if (string.IsNullOrEmpty(busName))
                throw new ArgumentException("busName cannot be null or empty.", nameof(busName));
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
                        MasterAudio.MuteBus(busName);
                        return $"OK: Bus '{busName}' muted.";

                    case "unmute":
                        MasterAudio.UnmuteBus(busName);
                        return $"OK: Bus '{busName}' unmuted.";

                    case "solo":
                        MasterAudio.SoloBus(busName);
                        return $"OK: Bus '{busName}' soloed.";

                    case "unsolo":
                        MasterAudio.UnsoloBus(busName);
                        return $"OK: Bus '{busName}' unsoloed.";

                    case "pause":
                        MasterAudio.PauseBus(busName);
                        return $"OK: Bus '{busName}' paused.";

                    case "unpause":
                        MasterAudio.UnpauseBus(busName);
                        return $"OK: Bus '{busName}' unpaused.";

                    case "stop":
                        MasterAudio.StopBus(busName);
                        return $"OK: Bus '{busName}' stopped.";

                    case "fade":
                        float targetVol = volume ?? 0f;
                        float fadeDur = fadeTime ?? 1f;
                        MasterAudio.FadeBusToVolume(busName, targetVol, fadeDur);
                        return $"OK: Bus '{busName}' fading to {targetVol:F2} over {fadeDur:F2}s.";

                    case "pitch":
                        if (!pitch.HasValue)
                            throw new ArgumentException("pitch parameter required for pitch action.", nameof(pitch));
                        MasterAudio.ChangeBusPitch(busName, pitch.Value);
                        return $"OK: Bus '{busName}' pitch set to {pitch.Value:F2}.";

                    default:
                        throw new ArgumentException(
                            $"Unknown action '{action}'. Valid: mute, unmute, solo, unsolo, pause, unpause, stop, fade, pitch.",
                            nameof(action));
                }
            });
        }
    }
}
#endif
