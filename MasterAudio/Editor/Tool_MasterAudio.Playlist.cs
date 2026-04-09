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
                EnsureInstance();

                string act = action.ToLowerInvariant().Trim();
                bool hasController = !string.IsNullOrEmpty(controllerName);

                switch (act)
                {
                    case "play":
                        if (hasController)
                            CallMA("TriggerPlaylistClip", controllerName!, clipName ?? string.Empty);
                        else
                            CallMA("TriggerPlaylistClip", clipName ?? string.Empty);
                        return $"OK: Playlist play triggered.{(clipName != null ? $" Clip: '{clipName}'" : "")}";

                    case "stop":
                        if (hasController)
                            CallMA("StopPlaylist", controllerName!);
                        else
                            CallMA("StopPlaylist");
                        return $"OK: Playlist stopped.";

                    case "next":
                        if (hasController)
                            CallMA("TriggerNextPlaylistClip", controllerName!);
                        else
                            CallMA("TriggerNextPlaylistClip");
                        return $"OK: Next playlist clip triggered.";

                    case "previous":
                        return $"WARNING: 'previous' not supported in this Master Audio version. Use 'next' or 'random'.";

                    case "random":
                        if (hasController)
                            CallMA("TriggerRandomPlaylistClip", controllerName!);
                        else
                            CallMA("TriggerRandomPlaylistClip");
                        return $"OK: Random playlist clip triggered.";

                    case "pause":
                        if (hasController)
                            CallMA("PausePlaylist", controllerName!);
                        else
                            CallMA("PausePlaylist");
                        return $"OK: Playlist paused.";

                    case "unpause":
                        if (hasController)
                            CallMA("UnpausePlaylist", controllerName!);
                        else
                            CallMA("UnpausePlaylist");
                        return $"OK: Playlist unpaused.";

                    case "mute":
                        if (hasController)
                            CallMA("MutePlaylist", controllerName!);
                        else
                            CallMA("MutePlaylist");
                        return $"OK: Playlist muted.";

                    case "unmute":
                        if (hasController)
                            CallMA("UnmutePlaylist", controllerName!);
                        else
                            CallMA("UnmutePlaylist");
                        return $"OK: Playlist unmuted.";

                    case "fade":
                        float targetVol = volume ?? 0f;
                        float fadeDur = fadeTime ?? 1f;
                        if (hasController)
                            CallMA("FadePlaylistToVolume", controllerName!, targetVol, fadeDur);
                        else
                            CallMA("FadePlaylistToVolume", targetVol, fadeDur);
                        return $"OK: Playlist fading to {targetVol:F2} over {fadeDur:F2}s.";

                    case "change":
                        if (string.IsNullOrEmpty(playlistName))
                            throw new ArgumentException("playlistName required for 'change' action.", nameof(playlistName));
                        if (hasController)
                            CallMA("ChangePlaylistByName", controllerName!, playlistName);
                        else
                            CallMA("ChangePlaylistByName", playlistName);
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
