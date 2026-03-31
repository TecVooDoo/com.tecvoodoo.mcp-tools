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
        [McpPluginTool("ma-playlist", Title = "Master Audio / Playlist Control")]
        [Description(@"Control playlist playback: play, stop, next, previous, random, pause, unpause, mute, unmute, fade, or change.
For change action, provide playlistName.
For fade action, provide volume and optional fadeTime.
controllerName is optional -- omit to use the first/default PlaylistController.")]
        public string Playlist(
            [Description("Action: play, stop, next, previous, random, pause, unpause, mute, unmute, fade, change.")]
            string action,
            [Description("PlaylistController name. Null = first/default controller.")]
            string? controllerName = null,
            [Description("Playlist name for 'change' action.")]
            string? playlistName = null,
            [Description("Clip name for targeted playback.")]
            string? clipName = null,
            [Description("Target volume for fade action (0-1).")]
            float? volume = null,
            [Description("Fade duration in seconds.")]
            float? fadeTime = null
        )
        {
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("action cannot be null or empty.", nameof(action));

            return MainThread.Instance.Run(() =>
            {
                if (MasterAudio.SafeInstance == null)
                    throw new InvalidOperationException("MasterAudio instance not found in scene.");

                string act = action.ToLowerInvariant().Trim();
                bool hasController = !string.IsNullOrEmpty(controllerName);

                switch (act)
                {
                    case "play":
                        if (hasController)
                            MasterAudio.TriggerPlaylistClip(controllerName, clipName ?? string.Empty);
                        else
                            MasterAudio.TriggerPlaylistClip(clipName ?? string.Empty);
                        return $"OK: Playlist play triggered.{(clipName != null ? $" Clip: '{clipName}'" : "")}";

                    case "stop":
                        if (hasController)
                            MasterAudio.StopPlaylist(controllerName);
                        else
                            MasterAudio.StopPlaylist();
                        return $"OK: Playlist stopped.";

                    case "next":
                        if (hasController)
                            MasterAudio.TriggerNextPlaylistClip(controllerName);
                        else
                            MasterAudio.TriggerNextPlaylistClip();
                        return $"OK: Next playlist clip triggered.";

                    case "previous":
                        // TriggerPreviousPlaylistClip does not exist in this MA version.
                        // Fall back to random as closest alternative.
                        return $"WARNING: 'previous' not supported in this Master Audio version. Use 'next' or 'random'.";

                    case "random":
                        if (hasController)
                            MasterAudio.TriggerRandomPlaylistClip(controllerName);
                        else
                            MasterAudio.TriggerRandomPlaylistClip();
                        return $"OK: Random playlist clip triggered.";

                    case "pause":
                        if (hasController)
                            MasterAudio.PausePlaylist(controllerName);
                        else
                            MasterAudio.PausePlaylist();
                        return $"OK: Playlist paused.";

                    case "unpause":
                        if (hasController)
                            MasterAudio.UnpausePlaylist(controllerName);
                        else
                            MasterAudio.UnpausePlaylist();
                        return $"OK: Playlist unpaused.";

                    case "mute":
                        if (hasController)
                            MasterAudio.MutePlaylist(controllerName);
                        else
                            MasterAudio.MutePlaylist();
                        return $"OK: Playlist muted.";

                    case "unmute":
                        if (hasController)
                            MasterAudio.UnmutePlaylist(controllerName);
                        else
                            MasterAudio.UnmutePlaylist();
                        return $"OK: Playlist unmuted.";

                    case "fade":
                        float targetVol = volume ?? 0f;
                        float fadeDur = fadeTime ?? 1f;
                        if (hasController)
                            MasterAudio.FadePlaylistToVolume(controllerName, targetVol, fadeDur);
                        else
                            MasterAudio.FadePlaylistToVolume(targetVol, fadeDur);
                        return $"OK: Playlist fading to {targetVol:F2} over {fadeDur:F2}s.";

                    case "change":
                        if (string.IsNullOrEmpty(playlistName))
                            throw new ArgumentException("playlistName required for 'change' action.", nameof(playlistName));
                        if (hasController)
                            MasterAudio.ChangePlaylistByName(controllerName, playlistName);
                        else
                            MasterAudio.ChangePlaylistByName(playlistName);
                        return $"OK: Playlist changed to '{playlistName}'.";

                    default:
                        throw new ArgumentException(
                            $"Unknown action '{action}'. Valid: play, stop, next, previous, random, pause, unpause, mute, unmute, fade, change.",
                            nameof(action));
                }
            });
        }
    }
}
#endif
