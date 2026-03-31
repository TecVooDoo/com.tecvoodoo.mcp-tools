#if HAS_PMG
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using ProcGenMusic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TecVooDoo.MCPTools.Editor
{
    [McpPluginToolType]
    public partial class Tool_PMG
    {
        private static MusicGenerator? FindGenerator()
        {
            return UnityEngine.Object.FindFirstObjectByType<MusicGenerator>();
        }

        [McpPluginTool("pmg-query", Title = "Procedural Music Generator / Query State")]
        [Description(@"Returns the current state of the Procedural Music Generator:
playing state, tempo (BPM), key (semitone steps from C), scale, mode, and AutoPlay setting.
Requires a MusicGenerator component in the active scene.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                MusicGenerator? gen = FindGenerator();
                if (gen == null)
                    return "ERROR: MusicGenerator not found in scene.";

                ConfigurationData cfg = gen.ConfigurationData;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== Procedural Music Generator ===");
                sb.AppendLine($"  State:    {gen.GeneratorState}");
                sb.AppendLine($"  AutoPlay: {gen.AutoPlay}");
                sb.AppendLine($"  Tempo:    {cfg.Tempo:F1} BPM");
                sb.AppendLine($"  Key:      {cfg.KeySteps} (semitones from C)");
                sb.AppendLine($"  Scale:    {cfg.Scale}");
                sb.AppendLine($"  Mode:     {cfg.Mode}");
                return sb.ToString();
            });
        }

        [McpPluginTool("pmg-play", Title = "Procedural Music Generator / Play")]
        [Description("Starts procedural music generation. Sets generator state to Playing.")]
        public string Play()
        {
            return MainThread.Instance.Run(() =>
            {
                MusicGenerator? gen = FindGenerator();
                if (gen == null)
                    return "ERROR: MusicGenerator not found in scene.";

                gen.SetState(GeneratorState.Playing);
                return $"OK: MusicGenerator state set to Playing";
            });
        }

        [McpPluginTool("pmg-stop", Title = "Procedural Music Generator / Stop")]
        [Description("Stops procedural music generation. Sets generator state to Stopped.")]
        public string Stop()
        {
            return MainThread.Instance.Run(() =>
            {
                MusicGenerator? gen = FindGenerator();
                if (gen == null)
                    return "ERROR: MusicGenerator not found in scene.";

                gen.SetState(GeneratorState.Stopped);
                return "OK: MusicGenerator state set to Stopped";
            });
        }

        [McpPluginTool("pmg-configure", Title = "Procedural Music Generator / Configure")]
        [Description(@"Configures the Procedural Music Generator at runtime.
Provide any combination of: tempo (BPM), keySteps (semitones 0-11), scale (Major/Minor/Chromatic/etc), mode (Ionian/Dorian/Phrygian/Lydian/Mixolydian/Aeolian/Locrian).
Only parameters you provide will be changed. Changes take effect on the next measure.")]
        public string Configure(
            [Description("Tempo in BPM. Typical range: 60-180.")]
            float? tempo = null,
            [Description("Key as semitone steps from C (0=C, 1=C#, 2=D, ... 11=B).")]
            int? keySteps = null,
            [Description("Scale name: Major, Minor, Chromatic, MajorPentatonic, MinorPentatonic, Blues, HarmonicMinor, etc.")]
            string? scale = null,
            [Description("Mode name: Ionian, Dorian, Phrygian, Lydian, Mixolydian, Aeolian, Locrian.")]
            string? mode = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                MusicGenerator? gen = FindGenerator();
                if (gen == null)
                    return "ERROR: MusicGenerator not found in scene.";

                ConfigurationData cfg = gen.ConfigurationData;
                StringBuilder changes = new StringBuilder();

                if (tempo.HasValue)
                {
                    cfg.Tempo = tempo.Value;
                    changes.Append($" Tempo={tempo.Value:F1}BPM");
                }

                if (keySteps.HasValue)
                {
                    cfg.KeySteps = keySteps.Value;
                    changes.Append($" Key={keySteps.Value}");
                }

                if (!string.IsNullOrEmpty(scale))
                {
                    if (!Enum.TryParse<Scale>(scale, true, out Scale scaleVal))
                        throw new ArgumentException($"Unknown scale '{scale}'.", nameof(scale));
                    cfg.Scale = scaleVal;
                    changes.Append($" Scale={scaleVal}");
                }

                if (!string.IsNullOrEmpty(mode))
                {
                    if (!Enum.TryParse<Mode>(mode, true, out Mode modeVal))
                        throw new ArgumentException($"Unknown mode '{mode}'.", nameof(mode));
                    cfg.Mode = modeVal;
                    changes.Append($" Mode={modeVal}");
                }

                if (changes.Length == 0)
                    return "INFO: No parameters provided, nothing changed.";

                return $"OK: Applied:{changes}";
            });
        }
    }
}
#endif
