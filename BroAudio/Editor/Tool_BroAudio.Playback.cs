#nullable enable
using System;
using System.ComponentModel;
using System.Globalization;
using Ami.BroAudio;
using Ami.BroAudio.Data;
using Ami.BroAudio.Runtime;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_BroAudio
    {
        /// <summary>
        /// Finds an AudioEntity by name from all loaded instances.
        /// </summary>
        private static AudioEntity? FindEntityByName(string entityName)
        {
            AudioEntity[] all = Resources.FindObjectsOfTypeAll<AudioEntity>();
            foreach (AudioEntity entity in all)
            {
                if (string.Equals(entity.Name, entityName, StringComparison.OrdinalIgnoreCase))
                    return entity;
            }
            return null;
        }

        [McpPluginTool("bro-play", Title = "Bro Audio / Play Sound")]
        [Description(@"Plays a sound by its entity name.
Use bro-query first to see all registered entity names.
Optionally pass position as 'x,y,z' for 3D spatialized playback.")]
        public string Play(
            [Description("Sound entity name to play (see bro-query for all names).")]
            string entityName,
            [Description("Fade-in duration in seconds. Default 0 (instant).")]
            float fadeIn = 0f,
            [Description("World position as 'x,y,z' for 3D playback. Omit for 2D.")]
            string? position = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                AudioEntity? entity = FindEntityByName(entityName);
                if (entity == null)
                    throw new ArgumentException($"No AudioEntity found with name '{entityName}'. Use bro-query to list available entities.", nameof(entityName));

                SoundID soundId = new SoundID(entity);

                if (!string.IsNullOrEmpty(position))
                {
                    string[] parts = position!.Split(',');
                    if (parts.Length != 3)
                        throw new ArgumentException("Position must be 'x,y,z'.", nameof(position));
                    float x = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
                    BroAudio.Play(soundId, new Vector3(x, y, z), fadeIn);
                    return $"OK: Playing '{entityName}' at ({x:F1},{y:F1},{z:F1}) fadeIn={fadeIn:F2}s";
                }

                if (fadeIn > 0f)
                    BroAudio.Play(soundId, fadeIn);
                else
                    BroAudio.Play(soundId);

                return $"OK: Playing '{entityName}' fadeIn={fadeIn:F2}s";
            });
        }

        [McpPluginTool("bro-stop", Title = "Bro Audio / Stop Sound")]
        [Description(@"Stops audio playback.
Stop by entity name (specific clip) or by audioType ('All', 'BGM', 'SFX', 'Ambience', 'Generic', 'UI').
Provide either entityName OR audioType, not both. If neither is provided, stops all audio.")]
        public string Stop(
            [Description("Sound entity name to stop. Omit to stop by type instead.")]
            string? entityName = null,
            [Description("BroAudioType to stop: All, BGM, SFX, Ambience, Generic, UI. Omit if using entityName.")]
            string? audioType = null,
            [Description("Fade-out duration in seconds. Default 0 (instant).")]
            float fadeOut = 0f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!string.IsNullOrEmpty(entityName))
                {
                    AudioEntity? entity = FindEntityByName(entityName!);
                    if (entity == null)
                        throw new ArgumentException($"No AudioEntity found with name '{entityName}'. Use bro-query to list available entities.", nameof(entityName));

                    SoundID soundId = new SoundID(entity);
                    if (fadeOut > 0f)
                        BroAudio.Stop(soundId, fadeOut);
                    else
                        BroAudio.Stop(soundId);
                    return $"OK: Stopped '{entityName}' fadeOut={fadeOut:F2}s";
                }

                BroAudioType type = BroAudioType.All;
                if (!string.IsNullOrEmpty(audioType))
                {
                    if (!Enum.TryParse<BroAudioType>(audioType, true, out type))
                        throw new ArgumentException(
                            $"Unknown audioType '{audioType}'. Valid: All, BGM, SFX, Ambience, Generic, UI.",
                            nameof(audioType));
                }

                if (fadeOut > 0f)
                    BroAudio.Stop(type, fadeOut);
                else
                    BroAudio.Stop(type);

                return $"OK: Stopped audioType={type} fadeOut={fadeOut:F2}s";
            });
        }

        [McpPluginTool("bro-volume", Title = "Bro Audio / Set Volume")]
        [Description(@"Sets volume for all audio, a specific audio type, or a specific sound entity.
Volume is 0.0 to 1.0. fadeTime is transition duration in seconds.
Provide entityName OR audioType to scope the change; omit both to set master volume.")]
        public string SetVolume(
            [Description("Target volume, 0.0 to 1.0.")]
            float volume,
            [Description("Transition duration in seconds. Default 0 (instant).")]
            float fadeTime = 0f,
            [Description("Sound entity name to target. Omit to use audioType instead.")]
            string? entityName = null,
            [Description("BroAudioType to target: All, BGM, SFX, Ambience, Generic, UI. Omit for master.")]
            string? audioType = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                volume = Mathf.Clamp01(volume);

                if (!string.IsNullOrEmpty(entityName))
                {
                    AudioEntity? entity = FindEntityByName(entityName!);
                    if (entity == null)
                        throw new ArgumentException($"No AudioEntity found with name '{entityName}'. Use bro-query to list available entities.", nameof(entityName));

                    SoundID soundId = new SoundID(entity);
                    BroAudio.SetVolume(soundId, volume, fadeTime);
                    return $"OK: '{entityName}' volume={volume:F2} fadeTime={fadeTime:F2}s";
                }

                BroAudioType type = BroAudioType.All;
                if (!string.IsNullOrEmpty(audioType))
                {
                    if (!Enum.TryParse<BroAudioType>(audioType, true, out type))
                        throw new ArgumentException(
                            $"Unknown audioType '{audioType}'. Valid: All, BGM, SFX, Ambience, Generic, UI.",
                            nameof(audioType));
                }

                BroAudio.SetVolume(type, volume, fadeTime);
                return $"OK: audioType={type} volume={volume:F2} fadeTime={fadeTime:F2}s";
            });
        }
    }
}
