#nullable enable
using System;
using System.ComponentModel;
using System.Globalization;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_MasterAudio
    {
        [McpPluginTool("ma-play", Title = "Master Audio / Play Sound")]
        [Description(@"Play a sound by group name. Supports 2D and 3D positional audio.
Provide position as 'x,y,z' for 3D spatialized playback.")]
        public string Play(
            [Description("Sound group name to play.")]
            string groupName,
            [Description("Volume percentage 0-1. Default 1.0.")]
            float? volume = null,
            [Description("Pitch override. Null uses group default.")]
            float? pitch = null,
            [Description("Delay in seconds before playback. Default 0.")]
            float? delay = null,
            [Description("World position as 'x,y,z' for 3D sound. Omit for 2D.")]
            string? position = null
        )
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("groupName cannot be null or empty.", nameof(groupName));

            float vol = volume ?? 1f;
            float del = delay ?? 0f;

            return MainThread.Instance.Run(() =>
            {
                EnsureInstance();

                if (!string.IsNullOrEmpty(position))
                {
                    string[] parts = position.Split(',');
                    if (parts.Length != 3)
                        throw new ArgumentException("Position must be in 'x,y,z' format.", nameof(position));

                    float x = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
                    float y = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                    float z = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
                    Vector3 pos = new Vector3(x, y, z);

                    object? result = CallMA("PlaySound3DAtVector3AndForget", groupName, pos, vol, pitch, del);
                    bool played3D = result is bool b && b;

                    if (played3D)
                        return $"OK: Playing 3D sound '{groupName}' at ({x:F1},{y:F1},{z:F1}) vol={vol:F2} delay={del:F2}s";
                    else
                        return $"FAILED: Could not play '{groupName}'. Check group name exists and has available variations.";
                }
                else
                {
                    object? result = CallMA("PlaySoundAndForget", groupName, vol, pitch, del);
                    bool played2D = result is bool b && b;

                    if (played2D)
                        return $"OK: Playing 2D sound '{groupName}' vol={vol:F2} delay={del:F2}s";
                    else
                        return $"FAILED: Could not play '{groupName}'. Check group name exists and has available variations.";
                }
            });
        }
    }
}
