#if HAS_MASTERAUDIO
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using DarkTonic.MasterAudio;

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
                if (MasterAudio.SafeInstance == null)
                    throw new InvalidOperationException("MasterAudio instance not found in scene. Add a MasterAudio prefab first.");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== Master Audio State ===");
                sb.AppendLine($"  MasterVolume:    {MasterAudio.MasterVolumeLevel:F2}");
                sb.AppendLine($"  MixerMuted:      {MasterAudio.MixerMuted}");
                sb.AppendLine($"  PlaylistsMuted:  {MasterAudio.PlaylistsMuted}");

                // Sound Groups
                List<string> groupNames = MasterAudio.RuntimeSoundGroupNames;
                sb.AppendLine($"\n-- Sound Groups ({groupNames.Count}) --");
                for (int i = 0; i < groupNames.Count; i++)
                {
                    string name = groupNames[i];
                    if (string.IsNullOrEmpty(name) || name == MasterAudio.NoGroupName)
                        continue;

                    float groupVol = MasterAudio.GetGroupVolume(name);
                    bool isPlaying = MasterAudio.IsSoundGroupPlaying(name);
                    sb.AppendLine($"  [{i}] {name} | Volume: {groupVol:F2} | Playing: {isPlaying}");
                }

                // Buses
                List<string> busNames = MasterAudio.RuntimeBusNames;
                sb.AppendLine($"\n-- Buses ({busNames.Count}) --");
                for (int i = 0; i < busNames.Count; i++)
                {
                    string busName = busNames[i];
                    if (string.IsNullOrEmpty(busName) || busName == MasterAudio.NoGroupName)
                        continue;

                    GroupBus bus = MasterAudio.GetBusByName(busName);
                    if (bus != null)
                    {
                        sb.AppendLine($"  [{i}] {busName} | Volume: {bus.volume:F2} | Muted: {bus.isMuted} | Soloed: {bus.isSoloed} | ActiveVoices: {bus.ActiveVoices}");
                    }
                    else
                    {
                        sb.AppendLine($"  [{i}] {busName} | (bus reference not found)");
                    }
                }

                // Playlists
                List<MasterAudio.Playlist> playlists = MasterAudio.MusicPlaylists;
                sb.AppendLine($"\n-- Playlists ({playlists.Count}) --");
                for (int i = 0; i < playlists.Count; i++)
                {
                    MasterAudio.Playlist pl = playlists[i];
                    sb.AppendLine($"  [{i}] {pl.playlistName} | Songs: {pl.MusicSettings.Count}");
                }

                return sb.ToString();
            });
        }
    }
}
#endif
