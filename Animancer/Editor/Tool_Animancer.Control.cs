#nullable enable
using System.ComponentModel;
using Animancer;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Animancer.Editor
{
    public partial class Tool_Animancer
    {
        [McpPluginTool("animancer-play", Title = "Animancer / Play Animation")]
        [Description(@"Plays an animation on an AnimancerComponent by clip asset name.
fadeDuration: cross-fade duration in seconds (0 = instant switch).
layer: which layer to play on (default 0).
speed: playback speed (1 = normal, -1 = reverse, 0.5 = half speed).
startTime: normalized start time (0 = beginning, 0.5 = halfway, 1 = end). -1 to not set.
Requires play mode for runtime playback. In edit mode, sets up the state.")]
        public string Play(
            [Description("Name of the GameObject with AnimancerComponent.")] string gameObjectName,
            [Description("Name of the AnimationClip asset to play.")] string clipName,
            [Description("Cross-fade duration in seconds. 0 = instant.")] float fadeDuration = 0f,
            [Description("Layer index to play on.")] int layer = 0,
            [Description("Playback speed. 1 = normal, -1 = reverse.")] float? speed = null,
            [Description("Normalized start time [0-1]. -1 to not set.")] float startTime = -1f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var ac = GetAnimancer(gameObjectName);
                var clip = FindClip(clipName);

                AnimancerState state;
                if (fadeDuration > 0f)
                {
                    if (layer > 0)
                    {
                        while (ac.Layers.Count <= layer) ac.Layers.Count = layer + 1;
                        state = ac.Layers[layer].Play(clip, fadeDuration);
                    }
                    else
                    {
                        state = ac.Play(clip, fadeDuration);
                    }
                }
                else
                {
                    if (layer > 0)
                    {
                        while (ac.Layers.Count <= layer) ac.Layers.Count = layer + 1;
                        state = ac.Layers[layer].Play(clip);
                    }
                    else
                    {
                        state = ac.Play(clip);
                    }
                }

                if (speed.HasValue) state.Speed = speed.Value;
                if (startTime >= 0f) state.NormalizedTime = startTime;

                EditorUtility.SetDirty(ac);
                return $"OK: Playing '{clip.name}' on '{gameObjectName}' layer={layer} fade={fadeDuration:F2}s speed={state.Speed:F2} length={state.Length:F3}s";
            });
        }

        [McpPluginTool("animancer-stop", Title = "Animancer / Stop")]
        [Description(@"Stops animations on an AnimancerComponent.
clipName: stop a specific clip. Omit to stop all.")]
        public string Stop(
            [Description("Name of the GameObject with AnimancerComponent.")] string gameObjectName,
            [Description("Optional: specific clip name to stop. Omit to stop all.")] string? clipName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var ac = GetAnimancer(gameObjectName);

                if (clipName != null)
                {
                    var clip = FindClip(clipName);
                    var state = ac.Stop(clip);
                    return $"OK: Stopped '{clip.name}' on '{gameObjectName}'.";
                }
                else
                {
                    ac.Stop();
                    return $"OK: Stopped all animations on '{gameObjectName}'.";
                }
            });
        }

        [McpPluginTool("animancer-configure", Title = "Animancer / Configure State")]
        [Description(@"Modifies a currently registered animation state's properties.
Find the clip by name, then set speed, weight, time, or normalized time.
Useful for fine-tuning playback without restarting the animation.")]
        public string ConfigureState(
            [Description("Name of the GameObject with AnimancerComponent.")] string gameObjectName,
            [Description("Name of the AnimationClip to configure.")] string clipName,
            [Description("Playback speed.")] float? speed = null,
            [Description("Blend weight [0-1].")] float? weight = null,
            [Description("Playback time in seconds.")] float? time = null,
            [Description("Normalized playback time [0-1].")] float? normalizedTime = null,
            [Description("Is playing state.")] bool? isPlaying = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var ac = GetAnimancer(gameObjectName);
                var clip = FindClip(clipName);

                AnimancerState? state = null;
                foreach (var s in ac.States)
                {
                    if (s.Clip == clip)
                    {
                        state = s;
                        break;
                    }
                }

                if (state == null)
                    throw new System.Exception($"No registered state for clip '{clipName}' on '{gameObjectName}'. Play it first.");

                if (speed.HasValue) state.Speed = speed.Value;
                if (weight.HasValue) state.Weight = Mathf.Clamp01(weight.Value);
                if (time.HasValue) state.Time = time.Value;
                if (normalizedTime.HasValue) state.NormalizedTime = normalizedTime.Value;
                if (isPlaying.HasValue) state.IsPlaying = isPlaying.Value;

                EditorUtility.SetDirty(ac);
                return $"OK: State '{clip.name}' configured. speed={state.Speed:F2} weight={state.Weight:F2} time={state.Time:F3}s playing={state.IsPlaying}";
            });
        }
    }
}
