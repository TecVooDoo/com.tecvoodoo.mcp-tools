#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Heathen.UnityPhysics;
using Heathen.UnityPhysics.API;
using UnityEditor;
using UnityEngine;

namespace MCPTools.HeathenBallistics.Editor
{
    public partial class Tool_HeathenBallistics
    {
        [McpPluginTool("ballistic-calculate-solution", Title = "Heathen Ballistics / Calculate Aim Solution")]
        [Description(@"Calculates the launch angle(s) needed to hit a target position from a source position.
Returns 0, 1, or 2 solutions (low arc and high arc). Use the rotation to orient a launcher.
This is a pure math calculation -- no scene objects are modified.
fromX/Y/Z: world position of the projectile launch point.
toX/Y/Z: world position of the target.
speed: projectile launch speed in m/s.
gravity: downward gravity (default 9.81 -- positive = downward pull).")]
        public string CalculateSolution(
            [Description("Launch position X.")] float fromX,
            [Description("Launch position Y.")] float fromY,
            [Description("Launch position Z.")] float fromZ,
            [Description("Target position X.")] float toX,
            [Description("Target position Y.")] float toY,
            [Description("Target position Z.")] float toZ,
            [Description("Projectile speed in m/s.")] float speed,
            [Description("Gravity magnitude (default 9.81, downward).")] float gravity = 9.81f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var from = new Vector3(fromX, fromY, fromZ);
                var to   = new Vector3(toX, toY, toZ);

                int count = Ballistics.Solution(from, speed, to, gravity, out Quaternion lowAngle, out Quaternion highAngle);

                var sb = new StringBuilder();
                sb.AppendLine($"=== Ballistic Solution ===");
                sb.AppendLine($"from:    {FormatVector3(from)}");
                sb.AppendLine($"to:      {FormatVector3(to)}");
                sb.AppendLine($"speed:   {speed:F2} m/s");
                sb.AppendLine($"gravity: {gravity:F2}");
                sb.AppendLine($"solutions: {count}");

                if (count >= 1)
                {
                    var lowEuler = lowAngle.eulerAngles;
                    sb.AppendLine($"\nLow arc (flatter):  euler=({lowEuler.x:F2}, {lowEuler.y:F2}, {lowEuler.z:F2})");
                }
                if (count >= 2)
                {
                    var highEuler = highAngle.eulerAngles;
                    sb.AppendLine($"High arc (lobbed):  euler=({highEuler.x:F2}, {highEuler.y:F2}, {highEuler.z:F2})");
                }
                if (count == 0)
                    sb.AppendLine("No solution: target out of range for this speed. Increase speed or reduce distance.");

                return sb.ToString();
            });
        }

        [McpPluginTool("ballistic-visualize", Title = "Heathen Ballistics / Visualize Trajectory")]
        [Description(@"Adds (if missing) and configures a BallisticPathLineRender component for trajectory visualization.
Requires a LineRenderer on the same GameObject (adds one if missing).
gravityMode: None, Physics (use Physics.gravity), or Custom (use customGravityY).
Call Simulate() via script or set continuousRun=true to update every frame.
This tool sets up the visualization component -- it does not trigger a simulation run.")]
        public string ConfigureVisualize(
            [Description("Name of the GameObject to add BallisticPathLineRender to.")] string gameObjectName,
            [Description("Simulation step size. Smaller = more accurate [0.01-0.5].")] float? resolution = null,
            [Description("Max trajectory length in world units.")] float? maxLength = null,
            [Description("Max bounces to visualize.")] int? maxBounces = null,
            [Description("Velocity multiplier retained per bounce [0-1].")] float? bounceDamping = null,
            [Description("Gravity mode: None, Physics, or Custom.")] string? gravityMode = null,
            [Description("Custom gravity Y value (used when gravityMode=Custom).")] float? customGravityY = null,
            [Description("Update trajectory every frame (true) or once (false).")] bool? continuousRun = null,
            [Description("Run simulation immediately on component Start.")] bool? runOnStart = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);

                if (go.GetComponent<LineRenderer>() == null)
                    go.AddComponent<LineRenderer>();

                var viz = go.GetComponent<BallisticPathLineRender>();
                if (viz == null)
                    viz = go.AddComponent<BallisticPathLineRender>();

                if (resolution.HasValue) viz.resolution = Mathf.Clamp(resolution.Value, 0.001f, 0.5f);
                if (maxLength.HasValue) viz.maxLength = Mathf.Max(0.1f, maxLength.Value);
                if (maxBounces.HasValue) viz.maxBounces = Mathf.Max(0, maxBounces.Value);
                if (bounceDamping.HasValue) viz.bounceDamping = Mathf.Clamp01(bounceDamping.Value);
                if (continuousRun.HasValue) viz.continuousRun = continuousRun.Value;
                if (runOnStart.HasValue) viz.runOnStart = runOnStart.Value;

                if (gravityMode != null)
                {
                    if (!System.Enum.TryParse<GravityMode>(gravityMode, true, out var gmode))
                        throw new System.Exception($"Invalid gravityMode '{gravityMode}'. Valid: None, Physics, Custom");
                    viz.gravityMode = gmode;
                }
                if (customGravityY.HasValue)
                    viz.customGravity = new Vector3(viz.customGravity.x, customGravityY.Value, viz.customGravity.z);

                EditorUtility.SetDirty(viz);

                return $"OK: BallisticPathLineRender on '{gameObjectName}' configured. resolution={viz.resolution:F4} maxLength={viz.maxLength:F2} maxBounces={viz.maxBounces} gravityMode={viz.gravityMode}";
            });
        }
    }
}
