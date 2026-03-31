#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Melanchall.DryWetMidi.Multimedia;

namespace TecVooDoo.MCPTools.Editor
{
    [McpPluginToolType]
    public partial class Tool_DryWetMIDI
    {
        [McpPluginTool("midi-query-devices", Title = "DryWetMIDI / Query MIDI Devices")]
        [Description(@"Lists all available MIDI input and output devices on this machine.
Output devices can be used with Maestro (MidiStreamPlayer) or DryWetMIDI Playback.
Useful for verifying MIDI routing and available hardware/virtual devices.")]
        public string QueryDevices()
        {
            return MainThread.Instance.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== MIDI Devices ===");

                try
                {
                    ICollection<OutputDevice> outputs = OutputDevice.GetAll();
                    sb.AppendLine($"\n-- Output Devices ({outputs.Count}) --");
                    int idx = 0;
                    foreach (OutputDevice dev in outputs)
                    {
                        sb.AppendLine($"  [{idx}] {dev.Name}");
                        idx++;
                        dev.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  (output device query failed: {ex.Message})");
                }

                try
                {
                    ICollection<InputDevice> inputs = InputDevice.GetAll();
                    sb.AppendLine($"\n-- Input Devices ({inputs.Count}) --");
                    int idx = 0;
                    foreach (InputDevice dev in inputs)
                    {
                        sb.AppendLine($"  [{idx}] {dev.Name}");
                        idx++;
                        dev.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"  (input device query failed: {ex.Message})");
                }

                return sb.ToString();
            });
        }
    }
}
