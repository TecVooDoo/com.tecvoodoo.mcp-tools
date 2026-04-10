#if HAS_TIMEFLOW
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.Timeflow.Editor
{
    public partial class Tool_Timeflow
    {
        [McpPluginTool("timeflow-control", Title = "Timeflow / Playback Control")]
        [Description(@"Controls Timeflow playback on a GameObject.
action options:
  Play -- start playback from current position.
  PlayFromStart -- start playback from StartTime.
  PlayReverse -- play in reverse.
  Stop -- stop playback.
  Pause -- pause playback (keeps current time).
  SetTime -- jump to a specific time (requires time parameter).
time: target time in seconds (used with SetTime action, or to set before Play).
timeScale: set GlobalTimeScale (playback speed multiplier).
loop: enable or disable looping.
Works in both edit mode and play mode (Timeflow uses ExecuteInEditMode).")]
        public string ControlTimeflow(
            [Description("Name of the GameObject with the Timeflow component.")]
            string gameObjectName,
            [Description("Action: Play, PlayFromStart, PlayReverse, Stop, Pause, or SetTime.")]
            string action,
            [Description("Target time in seconds (for SetTime action).")] float? time = null,
            [Description("Playback speed multiplier.")] float? timeScale = null,
            [Description("Enable or disable looping.")] bool? loop = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                UnityEngine.Component tf = GetTimeflow(gameObjectName);
                StringBuilder result = new System.Text.StringBuilder();

                // Apply optional settings first
                if (timeScale.HasValue)
                {
                    Set(tf, "GlobalTimeScale", timeScale.Value);
                    result.AppendLine($"  GlobalTimeScale = {timeScale.Value}");
                }

                if (loop.HasValue)
                {
                    // LoopEnabled is a property
                    var prop = tf.GetType().GetProperty("LoopEnabled", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (prop != null && prop.CanWrite)
                        prop.SetValue(tf, loop.Value);
                    result.AppendLine($"  LoopEnabled = {loop.Value}");
                }

                // Execute action
                switch (action.ToLowerInvariant())
                {
                    case "play":
                        Call(tf, "Play");
                        result.AppendLine($"  Action: Play");
                        break;

                    case "playfromstart":
                        Call(tf, "Play", true);
                        result.AppendLine($"  Action: PlayFromStart");
                        break;

                    case "playreverse":
                        Call(tf, "PlayReverse", true);
                        result.AppendLine($"  Action: PlayReverse");
                        break;

                    case "stop":
                        Call(tf, "Stop");
                        result.AppendLine($"  Action: Stop");
                        break;

                    case "pause":
                        Call(tf, "Pause");
                        result.AppendLine($"  Action: Pause");
                        break;

                    case "settime":
                        if (!time.HasValue)
                            throw new Exception("SetTime action requires the 'time' parameter.");
                        Call(tf, "SetTime", time.Value);
                        result.AppendLine($"  Action: SetTime({time.Value}s)");
                        break;

                    default:
                        throw new Exception($"Unknown action '{action}'. Use: Play, PlayFromStart, PlayReverse, Stop, Pause, or SetTime.");
                }

                float currentTime = (float)(Get(tf, "CurrentTime") ?? 0f);
                bool isPlaying = (bool)(Get(tf, "IsPlaying") ?? false);
                result.AppendLine($"  State: time={currentTime:F3}s, playing={isPlaying}");

                UnityEditor.EditorUtility.SetDirty(tf);
                return $"OK: Timeflow on '{gameObjectName}':\n{result}";
            });
        }
    }
}
#endif
