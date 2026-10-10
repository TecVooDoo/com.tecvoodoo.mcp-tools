#if HAS_PEGASUS
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using Component = UnityEngine.Component;

namespace MCPTools.Pegasus.Editor
{
    public partial class Tool_Pegasus
    {
        [AiTool("pegasus-control", Title = "Pegasus / Control")]
        [Description(@"Drives a Pegasus flythrough. 'manager' = its name (omit when only one exists).
action='scrub': move the target to 'percent' (0..1) of the path -- deterministic, edit mode or play mode; pair with screenshot-camera for set beauty shots.
action='sample': walk the path at 'samples' evenly spaced points (default 200) and report terrain clearance -- min / mean / count below ground (the ENTRY-435 measurement). Read-only: does not move the target.
action='start' | 'pause' | 'resume' | 'stop': play-mode playback (StartFlythrough / PauseFlythrough / ResumeFlythrough / StopFlythrough).
Returns the target's pose and the manager's state.")]
        public string Control(
            [Description("scrub | sample | start | pause | resume | stop.")] string action,
            [Description("Manager name. Omit when only one manager exists.")] string? manager = null,
            [Description("scrub: fraction of the path, 0..1.")] float percent = 0f,
            [Description("sample: number of samples. Default 200.")] int samples = 200
        )
        {
            return MainThread.Instance.Run(() =>
            {
                Component m = FindManager(manager);
                GameObject? target = GetField(m, "m_target") as GameObject;
                string a = string.IsNullOrWhiteSpace(action) ? "" : action.Trim().ToLowerInvariant();

                switch (a)
                {
                    case "scrub":
                    {
                        RequireTarget(target, m);
                        if (percent < 0f || percent > 1f)
                            throw new Exception("'percent' must be 0..1.");
                        Call(m, "MoveTargetTo", percent);
                        Transform t = target!.transform;
                        return $"Scrubbed '{m.gameObject.name}' to {percent:P1}: target '{target.name}' at ({t.position.x:F1}, {t.position.y:F1}, {t.position.z:F1}), " +
                               $"rotation ({t.eulerAngles.x:F1}, {t.eulerAngles.y:F1}, {t.eulerAngles.z:F1}){Clearance(t.position)}.";
                    }
                    case "sample":
                    {
                        RequireTarget(target, m);
                        return Sample(m, Math.Max(2, samples));
                    }
                    case "start":
                    case "pause":
                    case "resume":
                    case "stop":
                    {
                        if (!Application.isPlaying)
                            throw new Exception($"'{a}' is play-mode playback. In edit mode use action='scrub'.");
                        RequireTarget(target, m);
                        if (a == "start")
                            Call(m, "StartFlythrough", false);
                        else if (a == "pause")
                            Call(m, "PauseFlythrough");
                        else if (a == "resume")
                            Call(m, "ResumeFlythrough");
                        else
                            Call(m, "StopFlythrough");
                        return $"'{m.gameObject.name}': {a} -> state {GetField(m, "m_currentState")}, " +
                               $"{(float)GetField(m, "m_totalDistanceTravelled")!:F1} / {(float)GetField(m, "m_totalDistance")!:F1} m.";
                    }
                    default:
                        throw new Exception($"Unknown action '{action}'. Use: scrub, sample, start, pause, resume, stop.");
                }
            });
        }

        static void RequireTarget(GameObject? target, Component manager)
        {
            // ENTRY-435: with no target, CalculateTargetAtDistance silently returns (0,0,0) for every distance.
            if (target == null)
                throw new Exception($"Manager '{manager.gameObject.name}' has no target; sampling and scrubbing would silently return (0,0,0). Recreate it with pegasus-create.");
        }

        static string Clearance(Vector3 position)
        {
            float? ground = GroundHeight(position);
            return ground.HasValue ? $", {position.y - ground.Value:F2} m above terrain" : ", no terrain below";
        }

        static string Sample(Component manager, int samples)
        {
            float length = (float)GetField(manager, "m_totalDistance")!;
            if (length <= 0f)
                throw new Exception("Path length is 0 -- does the manager have at least 2 POIs?");

            int measured = 0, below = 0;
            float min = float.MaxValue, sum = 0f;
            Vector3 lowest = Vector3.zero;
            for (int i = 0; i < samples; i++)
            {
                float distance = length * i / samples;
                PoseAtDistance(manager, distance, out Vector3 position, out Quaternion _);
                float? ground = GroundHeight(position);
                if (!ground.HasValue)
                    continue;
                float clearance = position.y - ground.Value;
                measured++;
                sum += clearance;
                if (clearance < 0f)
                    below++;
                if (clearance < min)
                {
                    min = clearance;
                    lowest = position;
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"'{manager.gameObject.name}': {samples} samples over {length:F1} m, {measured} over terrain.");
            if (measured > 0)
            {
                sb.AppendLine($"Clearance: min {min:F2} m at ({lowest.x:F1}, {lowest.y:F1}, {lowest.z:F1}), mean {sum / measured:F2} m, below ground {below}/{measured}.");
                sb.AppendLine($"Configured minimum: {(float)GetField(manager, "m_minHeightAboveTerrain")!:F2} m.");
            }
            return sb.ToString();
        }
    }
}
#endif
