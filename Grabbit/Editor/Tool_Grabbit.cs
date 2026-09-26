#if HAS_GRABBIT
#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using com.IvanMurzak.McpPlugin;
using Grabbit2;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Grabbit.Editor
{
    /// <summary>
    /// Grabbit 2 (Jungle) tool group. Wraps the vendor's host-independent headless API
    /// (<see cref="GrabbitOps"/>, <see cref="GrabbitColliderBakeApi"/>) -- the three MCP adapters
    /// Grabbit ships target other MCP hosts and are inert under this one.
    /// </summary>
    [AiToolType]
    public partial class Tool_Grabbit
    {
        /// <summary>
        /// Resolves hierarchy paths / names to scene GameObjects, or falls back to the editor
        /// selection when none are given. Throws on any name that does not resolve.
        /// </summary>
        static List<GameObject> ResolveTargets(string[]? names, string role)
        {
            var result = new List<GameObject>();
            if (names == null || names.Length == 0)
            {
                result.AddRange(Selection.gameObjects);
                if (result.Count == 0)
                    throw new Exception($"No {role} given and nothing is selected in the editor.");
                return result;
            }

            foreach (string name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                    continue;
                var go = GameObject.Find(name.Trim());
                if (go == null)
                    throw new Exception($"Could not find {role} '{name}' (GameObject.Find matches active objects by name or hierarchy path).");
                result.Add(go);
            }

            if (result.Count == 0)
                throw new Exception($"No {role} resolved.");
            return result;
        }

        static string FormatRun(GrabbitRunResult r)
        {
            if (!r.Success)
                throw new Exception(r.Message);

            var sb = new StringBuilder();
            sb.AppendLine(r.Message);
            sb.AppendLine($"Status: {r.Status} | Steps: {r.StepsTaken} | Driven: {r.DrivenCount}/{r.RequestedCount}");
            foreach (var p in r.FinalPoses)
                sb.AppendLine($"  '{p.Name}' pos=({p.Position.x:F3}, {p.Position.y:F3}, {p.Position.z:F3}) rot=({p.EulerAngles.x:F1}, {p.EulerAngles.y:F1}, {p.EulerAngles.z:F1})");
            return sb.ToString();
        }

        static GrabbitRunOptions MakeOptions(int? maxSteps)
        {
            var options = new GrabbitRunOptions();
            if (maxSteps.HasValue && maxSteps.Value > 0)
                options.MaxSteps = maxSteps.Value;
            return options;
        }
    }
}
#endif
