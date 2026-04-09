#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    [McpPluginToolType]
    public partial class Tool_PMG
    {
        const string MUSIC_GENERATOR_TYPE_NAME   = "ProcGenMusic.MusicGenerator";
        const string GENERATOR_STATE_TYPE_NAME   = "ProcGenMusic.GeneratorState";
        const string CONFIGURATION_DATA_TYPE_NAME = "ProcGenMusic.ConfigurationData";
        const string SCALE_TYPE_NAME             = "ProcGenMusic.Scale";
        const string MODE_TYPE_NAME              = "ProcGenMusic.Mode";

        static Type? _genTypeCached;
        static Type MusicGeneratorType
        {
            get
            {
                if (_genTypeCached == null)
                    _genTypeCached = FindType(MUSIC_GENERATOR_TYPE_NAME);
                if (_genTypeCached == null)
                    throw new InvalidOperationException($"Type '{MUSIC_GENERATOR_TYPE_NAME}' not found. Is PMG installed?");
                return _genTypeCached;
            }
        }

        static Type? FindType(string fullTypeName)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? type = asm.GetType(fullTypeName);
                if (type != null) return type;
            }
            return null;
        }

        static object? Get(object target, string name)
        {
            Type type = target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            PropertyInfo? prop = type.GetProperty(name, flags);
            if (prop != null) return prop.GetValue(target);
            FieldInfo? field = type.GetField(name, flags);
            if (field != null) return field.GetValue(target);
            return null;
        }

        static bool Set(object target, string name, object value)
        {
            Type type = target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            PropertyInfo? prop = type.GetProperty(name, flags);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, value); return true; }
            FieldInfo? field = type.GetField(name, flags);
            if (field != null) { field.SetValue(target, value); return true; }
            return false;
        }

        static object? Call(object target, string methodName, params object[] args)
        {
            Type type = target.GetType();
            Type[] argTypes = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
                argTypes[i] = args[i].GetType();
            MethodInfo? method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, argTypes, null);
            if (method == null)
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
                throw new Exception($"Method '{methodName}' not found on {type.Name}.");
            return method.Invoke(target, args);
        }

        /// <summary>
        /// Find MusicGenerator in scene via reflection of FindFirstObjectByType.
        /// </summary>
        private static object? FindGenerator()
        {
            // UnityEngine.Object.FindFirstObjectByType<MusicGenerator>()
            // We call the generic method via reflection since we only have the Type at runtime.
            var findMethod = typeof(UnityEngine.Object).GetMethod(
                "FindFirstObjectByType",
                BindingFlags.Public | BindingFlags.Static,
                null,
                Type.EmptyTypes,
                null
            );
            if (findMethod == null)
            {
                // Fallback: FindObjectOfType
                var fallback = typeof(UnityEngine.Object).GetMethod(
                    "FindObjectOfType",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new[] { typeof(Type) },
                    null
                );
                return fallback?.Invoke(null, new object[] { MusicGeneratorType });
            }
            var generic = findMethod.MakeGenericMethod(MusicGeneratorType);
            return generic.Invoke(null, null);
        }

        [McpPluginTool("pmg-query", Title = "Procedural Music Generator / Query State")]
        [Description(@"Returns the current state of the Procedural Music Generator:
playing state, tempo (BPM), key (semitone steps from C), scale, mode, and AutoPlay setting.
Requires a MusicGenerator component in the active scene.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                var gen = FindGenerator();
                if (gen == null)
                    return "ERROR: MusicGenerator not found in scene.";

                var cfg = Get(gen, "ConfigurationData");
                if (cfg == null)
                    return "ERROR: Could not read ConfigurationData from MusicGenerator.";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== Procedural Music Generator ===");
                sb.AppendLine($"  State:    {Get(gen, "GeneratorState")}");
                sb.AppendLine($"  AutoPlay: {Get(gen, "AutoPlay")}");
                sb.AppendLine($"  Tempo:    {Get(cfg, "Tempo"):F1} BPM");
                sb.AppendLine($"  Key:      {Get(cfg, "KeySteps")} (semitones from C)");
                sb.AppendLine($"  Scale:    {Get(cfg, "Scale")}");
                sb.AppendLine($"  Mode:     {Get(cfg, "Mode")}");
                return sb.ToString();
            });
        }

        [McpPluginTool("pmg-play", Title = "Procedural Music Generator / Play")]
        [Description("Starts procedural music generation. Sets generator state to Playing.")]
        public string Play()
        {
            return MainThread.Instance.Run(() =>
            {
                var gen = FindGenerator();
                if (gen == null)
                    return "ERROR: MusicGenerator not found in scene.";

                var stateType = FindType(GENERATOR_STATE_TYPE_NAME);
                if (stateType == null)
                    return "ERROR: GeneratorState type not found.";

                var playingValue = Enum.Parse(stateType, "Playing");
                Call(gen, "SetState", playingValue);
                return $"OK: MusicGenerator state set to Playing";
            });
        }

        [McpPluginTool("pmg-stop", Title = "Procedural Music Generator / Stop")]
        [Description("Stops procedural music generation. Sets generator state to Stopped.")]
        public string Stop()
        {
            return MainThread.Instance.Run(() =>
            {
                var gen = FindGenerator();
                if (gen == null)
                    return "ERROR: MusicGenerator not found in scene.";

                var stateType = FindType(GENERATOR_STATE_TYPE_NAME);
                if (stateType == null)
                    return "ERROR: GeneratorState type not found.";

                var stoppedValue = Enum.Parse(stateType, "Stopped");
                Call(gen, "SetState", stoppedValue);
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
                var gen = FindGenerator();
                if (gen == null)
                    return "ERROR: MusicGenerator not found in scene.";

                var cfg = Get(gen, "ConfigurationData");
                if (cfg == null)
                    return "ERROR: Could not read ConfigurationData from MusicGenerator.";

                StringBuilder changes = new StringBuilder();

                if (tempo.HasValue)
                {
                    Set(cfg, "Tempo", tempo.Value);
                    changes.Append($" Tempo={tempo.Value:F1}BPM");
                }

                if (keySteps.HasValue)
                {
                    Set(cfg, "KeySteps", keySteps.Value);
                    changes.Append($" Key={keySteps.Value}");
                }

                if (!string.IsNullOrEmpty(scale))
                {
                    var scaleType = FindType(SCALE_TYPE_NAME);
                    if (scaleType == null)
                        throw new ArgumentException($"Scale enum type not found.");
                    if (!Enum.IsDefined(scaleType, scale!))
                    {
                        // Try case-insensitive
                        object? parsed = null;
                        try { parsed = Enum.Parse(scaleType, scale!, true); } catch { }
                        if (parsed == null)
                            throw new ArgumentException($"Unknown scale '{scale}'.");
                        Set(cfg, "Scale", parsed);
                        changes.Append($" Scale={parsed}");
                    }
                    else
                    {
                        var scaleVal = Enum.Parse(scaleType, scale!);
                        Set(cfg, "Scale", scaleVal);
                        changes.Append($" Scale={scaleVal}");
                    }
                }

                if (!string.IsNullOrEmpty(mode))
                {
                    var modeType = FindType(MODE_TYPE_NAME);
                    if (modeType == null)
                        throw new ArgumentException($"Mode enum type not found.");
                    object? modeVal = null;
                    try { modeVal = Enum.Parse(modeType, mode!, true); } catch { }
                    if (modeVal == null)
                        throw new ArgumentException($"Unknown mode '{mode}'.");
                    Set(cfg, "Mode", modeVal);
                    changes.Append($" Mode={modeVal}");
                }

                if (changes.Length == 0)
                    return "INFO: No parameters provided, nothing changed.";

                return $"OK: Applied:{changes}";
            });
        }
    }
}
