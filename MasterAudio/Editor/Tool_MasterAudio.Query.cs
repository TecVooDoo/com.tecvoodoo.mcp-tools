#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_MasterAudio
    {
        [McpPluginTool("ma-query", Title = "Master Audio / Query")]
        [Description(@"Lists all sound groups, buses, playlists, master volume, mute state, and currently playing variations.
Use this to inspect the full Master Audio runtime state before making changes.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                EnsureInstance();

                Type maType = MasterAudioType;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== Master Audio State ===");

                object? masterVol = GetStatic("MasterVolumeLevel");
                object? mixerMuted = GetStatic("MixerMuted");
                object? playlistsMuted = GetStatic("PlaylistsMuted");

                sb.AppendLine($"  MasterVolume:    {masterVol:F2}");
                sb.AppendLine($"  MixerMuted:      {mixerMuted}");
                sb.AppendLine($"  PlaylistsMuted:  {playlistsMuted}");

                // Sound Groups
                object? groupNamesObj = GetStatic("RuntimeSoundGroupNames");
                string? noGroupName = GetStatic("NoGroupName") as string;

                if (groupNamesObj is IList groupNames)
                {
                    sb.AppendLine($"\n-- Sound Groups ({groupNames.Count}) --");
                    for (int i = 0; i < groupNames.Count; i++)
                    {
                        string? name = groupNames[i] as string;
                        if (string.IsNullOrEmpty(name) || name == noGroupName)
                            continue;

                        object? groupVol = CallMA("GetGroupVolume", name!);
                        object? isPlaying = CallMA("IsSoundGroupPlaying", name!);
                        sb.AppendLine($"  [{i}] {name} | Volume: {groupVol:F2} | Playing: {isPlaying}");
                    }
                }

                // Buses
                object? busNamesObj = GetStatic("RuntimeBusNames");
                if (busNamesObj is IList busNames)
                {
                    sb.AppendLine($"\n-- Buses ({busNames.Count}) --");
                    for (int i = 0; i < busNames.Count; i++)
                    {
                        string? busName = busNames[i] as string;
                        if (string.IsNullOrEmpty(busName) || busName == noGroupName)
                            continue;

                        sb.AppendLine($"  [{i}] {busName}");
                    }
                }

                // Playlists
                object? playlistsObj = GetStatic("MusicPlaylists");
                if (playlistsObj is IList playlists)
                {
                    sb.AppendLine($"\n-- Playlists ({playlists.Count}) --");
                    for (int i = 0; i < playlists.Count; i++)
                    {
                        object? pl = playlists[i];
                        if (pl == null) continue;

                        object? plName = Get(pl.GetType(), pl, "playlistName");
                        object? musicSettings = Get(pl.GetType(), pl, "MusicSettings");
                        int songCount = musicSettings is IList ms ? ms.Count : 0;
                        sb.AppendLine($"  [{i}] {plName} | Songs: {songCount}");
                    }
                }

                return sb.ToString();
            });
        }
    }
}
