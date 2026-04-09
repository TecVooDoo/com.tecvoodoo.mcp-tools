#nullable enable
using System.ComponentModel;
using System.Text;
using Animancer;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.Animancer.Editor
{
    public partial class Tool_Animancer
    {
        [McpPluginTool("animancer-query", Title = "Animancer / Query")]
        [Description(@"Reads the full AnimancerComponent state on a GameObject.
Reports: graph initialized, update mode, layer count, and for each layer:
current state (clip name, time, normalized time, speed, weight, isPlaying, isLooping),
all active states with their weights.
Also lists all registered state keys.")]
        public string Query(
            [Description("Name of the GameObject with AnimancerComponent.")] string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var ac = GetAnimancer(gameObjectName);
                var sb = new StringBuilder();

                sb.AppendLine($"AnimancerComponent on '{gameObjectName}':");
                sb.AppendLine($"  GraphInitialized: {ac.IsGraphInitialized}");

                if (!ac.IsGraphInitialized)
                {
                    sb.AppendLine("  (Graph not initialized — no animation data available)");
                    return sb.ToString();
                }

                sb.AppendLine($"  UpdateMode: {ac.UpdateMode}");

                var layers = ac.Layers;
                sb.AppendLine($"  Layers: {layers.Count}");

                for (int i = 0; i < layers.Count; i++)
                {
                    var layer = layers[i];
                    sb.AppendLine($"\n  Layer[{i}]: weight={layer.Weight:F2} additive={layer.IsAdditive}");

                    var current = layer.CurrentState;
                    if (current != null)
                    {
                        string clipName = current.Clip != null ? current.Clip.name : "(no clip)";
                        sb.AppendLine($"    CurrentState: \"{clipName}\"");
                        sb.AppendLine($"      time={current.Time:F3}s normalizedTime={current.NormalizedTime:F3} length={current.Length:F3}s");
                        sb.AppendLine($"      speed={current.Speed:F2} weight={current.Weight:F2} isPlaying={current.IsPlaying} isLooping={current.IsLooping}");
                    }
                    else
                    {
                        sb.AppendLine("    CurrentState: (none)");
                    }

                    // Active states
                    var activeStates = layer.ActiveStates;
                    if (activeStates.Count > 1 || (activeStates.Count == 1 && activeStates[0] != current))
                    {
                        sb.AppendLine($"    ActiveStates ({activeStates.Count}):");
                        foreach (var state in activeStates)
                        {
                            if (state == current) continue;
                            string name = state.Clip != null ? state.Clip.name : state.Key?.ToString() ?? "(unnamed)";
                            sb.AppendLine($"      \"{name}\" weight={state.Weight:F2} speed={state.Speed:F2} time={state.Time:F3}s");
                        }
                    }
                }

                // All registered states
                var states = ac.States;
                int stateCount = 0;
                var stateList = new StringBuilder();
                foreach (var state in states)
                {
                    stateCount++;
                    if (stateCount <= 20)
                    {
                        string name = state.Clip != null ? state.Clip.name : state.Key?.ToString() ?? "(unnamed)";
                        stateList.AppendLine($"    \"{name}\" playing={state.IsPlaying} weight={state.Weight:F2}");
                    }
                }
                if (stateCount > 0)
                {
                    sb.AppendLine($"\n  RegisteredStates ({stateCount}):");
                    sb.Append(stateList);
                    if (stateCount > 20) sb.AppendLine($"    ... +{stateCount - 20} more");
                }

                return sb.ToString();
            });
        }
    }
}
