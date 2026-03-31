#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using MidiPlayerTK;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    [McpPluginToolType]
    public partial class Tool_Maestro
    {
        [McpPluginTool("maestro-query", Title = "Maestro / Query State")]
        [Description(@"Lists all MidiFilePlayer and MidiStreamPlayer instances in the scene.
Reports: GameObject name, MIDI file name, playing/paused state, tempo, loop setting.
Use this to discover player names before calling maestro-play or maestro-send-note.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== Maestro MIDI State ===");

                MidiFilePlayer[] filePlayers = Object.FindObjectsByType<MidiFilePlayer>(FindObjectsSortMode.None);
                sb.AppendLine($"\n-- MidiFilePlayer ({filePlayers.Length}) --");
                foreach (MidiFilePlayer fp in filePlayers)
                {
                    sb.AppendLine($"  GameObject: \"{fp.gameObject.name}\"");
                    sb.AppendLine($"    MidiName:  {fp.MPTK_MidiName ?? "(none)"}");
                    sb.AppendLine($"    IsPlaying: {fp.MPTK_IsPlaying}  IsPaused: {fp.MPTK_IsPaused}");
                    sb.AppendLine($"    Tempo:     {fp.MPTK_Tempo:F1} BPM  Speed: {fp.MPTK_Speed:F2}");
                    sb.AppendLine($"    Loop:      {fp.MPTK_Loop}");
                }

                MidiStreamPlayer[] streamPlayers = Object.FindObjectsByType<MidiStreamPlayer>(FindObjectsSortMode.None);
                sb.AppendLine($"\n-- MidiStreamPlayer ({streamPlayers.Length}) --");
                foreach (MidiStreamPlayer sp in streamPlayers)
                {
                    sb.AppendLine($"  GameObject: \"{sp.gameObject.name}\"");
                }

                return sb.ToString();
            });
        }

        [McpPluginTool("maestro-play", Title = "Maestro / Play MIDI File")]
        [Description(@"Starts playback on a MidiFilePlayer.
playerName: the GameObject name of the MidiFilePlayer to control.
midiName: optional MIDI file name to load before playing (must be in Maestro's MIDI database).
If midiName is omitted, plays the currently assigned MIDI.")]
        public string Play(
            [Description("GameObject name of the MidiFilePlayer to control.")]
            string playerName,
            [Description("MIDI file name to load (without extension). Omit to use currently assigned MIDI.")]
            string? midiName = null,
            [Description("Loop the MIDI file. Default false.")]
            bool loop = false,
            [Description("Playback speed multiplier. Default 1.0.")]
            float speed = 1.0f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                MidiFilePlayer? player = FindFilePlayer(playerName);
                if (player == null)
                    return $"ERROR: MidiFilePlayer '{playerName}' not found in scene.";

                if (!string.IsNullOrEmpty(midiName))
                    player.MPTK_MidiName = midiName!;

                player.MPTK_Loop = loop;
                player.MPTK_Speed = speed;
                player.MPTK_Play();

                return $"OK: Playing '{player.MPTK_MidiName}' on '{playerName}' loop={loop} speed={speed:F2}";
            });
        }

        [McpPluginTool("maestro-stop", Title = "Maestro / Stop MIDI Playback")]
        [Description("Stops MIDI playback on a MidiFilePlayer.")]
        public string StopPlay(
            [Description("GameObject name of the MidiFilePlayer to stop.")]
            string playerName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                MidiFilePlayer? player = FindFilePlayer(playerName);
                if (player == null)
                    return $"ERROR: MidiFilePlayer '{playerName}' not found in scene.";

                player.MPTK_Stop();
                return $"OK: Stopped '{playerName}'";
            });
        }

        [McpPluginTool("maestro-send-note", Title = "Maestro / Send Real-Time MIDI Note")]
        [Description(@"Sends a real-time MIDI NoteOn event to a MidiStreamPlayer.
note: MIDI note number 0-127 (60=Middle C, 69=A4).
channel: MIDI channel 0-15.
velocity: note velocity 0-127 (volume/intensity).
durationMs: note duration in milliseconds (-1 = hold until note-off).
Requires a MidiStreamPlayer in the scene.")]
        public string SendNote(
            [Description("GameObject name of the MidiStreamPlayer.")]
            string playerName,
            [Description("MIDI note number 0-127. 60=Middle C.")]
            int note,
            [Description("MIDI channel 0-15. Default 0.")]
            int channel = 0,
            [Description("Velocity 0-127. Default 100.")]
            int velocity = 100,
            [Description("Duration in milliseconds. -1 = sustain until stopped. Default 500.")]
            int durationMs = 500,
            [Description("General MIDI instrument preset 0-127. Default 0 (Acoustic Piano).")]
            int preset = 0
        )
        {
            return MainThread.Instance.Run(() =>
            {
                MidiStreamPlayer? player = FindStreamPlayer(playerName);
                if (player == null)
                    return $"ERROR: MidiStreamPlayer '{playerName}' not found in scene.";

                // Send PatchChange first if a non-default instrument is requested
                if (preset != 0)
                {
                    MPTKEvent patchEvent = new MPTKEvent()
                    {
                        Command = MPTKCommand.PatchChange,
                        Channel = Mathf.Clamp(channel, 0, 15),
                        Value   = Mathf.Clamp(preset, 0, 127),
                    };
                    player.MPTK_PlayEvent(patchEvent);
                }

                MPTKEvent noteEvent = new MPTKEvent()
                {
                    Command  = MPTKCommand.NoteOn,
                    Value    = Mathf.Clamp(note, 0, 127),
                    Channel  = Mathf.Clamp(channel, 0, 15),
                    Velocity = Mathf.Clamp(velocity, 0, 127),
                    Duration = durationMs,
                };

                player.MPTK_PlayEvent(noteEvent);
                return $"OK: NoteOn note={note} ch={channel} vel={velocity} dur={durationMs}ms preset={preset} on '{playerName}'";
            });
        }

        private static MidiFilePlayer? FindFilePlayer(string name)
        {
            MidiFilePlayer[] all = Object.FindObjectsByType<MidiFilePlayer>(FindObjectsSortMode.None);
            foreach (MidiFilePlayer p in all)
            {
                if (p.gameObject.name == name)
                    return p;
            }
            return null;
        }

        private static MidiStreamPlayer? FindStreamPlayer(string name)
        {
            MidiStreamPlayer[] all = Object.FindObjectsByType<MidiStreamPlayer>(FindObjectsSortMode.None);
            foreach (MidiStreamPlayer p in all)
            {
                if (p.gameObject.name == name)
                    return p;
            }
            return null;
        }
    }
}
