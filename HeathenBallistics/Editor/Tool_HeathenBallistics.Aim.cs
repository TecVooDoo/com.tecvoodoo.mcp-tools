#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Heathen.UnityPhysics;
using UnityEditor;
using UnityEngine;

namespace MCPTools.HeathenBallistics.Editor
{
    public partial class Tool_HeathenBallistics
    {
        [McpPluginTool("ballistic-configure-aim", Title = "Heathen Ballistics / Configure Aim")]
        [Description(@"Adds (if missing) and configures a BallisticAim component.
BallisticAim drives a dual-pivot turret or launcher to aim at a target position.
initialSpeed: projectile launch speed (m/s).
yPivotName / xPivotName: names of child GameObjects used as yaw and pitch pivots.
yLimitMin/Max: yaw rotation limits in degrees (default -180 to 180 = full rotation).
xLimitMin/Max: pitch rotation limits in degrees (default -180 to 180 = full rotation).
gravityY: vertical gravity acceleration (default -9.81).")]
        public string ConfigureAim(
            [Description("Name of the GameObject to add BallisticAim to.")] string gameObjectName,
            [Description("Projectile launch speed in m/s.")] float? initialSpeed = null,
            [Description("Name of the child GameObject used as the yaw (Y-axis) pivot.")] string? yPivotName = null,
            [Description("Name of the child GameObject used as the pitch (X-axis) pivot.")] string? xPivotName = null,
            [Description("Minimum yaw angle limit in degrees.")] float? yLimitMin = null,
            [Description("Maximum yaw angle limit in degrees.")] float? yLimitMax = null,
            [Description("Minimum pitch angle limit in degrees.")] float? xLimitMin = null,
            [Description("Maximum pitch angle limit in degrees.")] float? xLimitMax = null,
            [Description("Gravity Y acceleration (default -9.81).")] float? gravityY = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);

                var aim = go.GetComponent<BallisticAim>();
                if (aim == null)
                    aim = go.AddComponent<BallisticAim>();

                if (initialSpeed.HasValue) aim.initialSpeed = Mathf.Max(0.1f, initialSpeed.Value);

                if (yPivotName != null)
                {
                    var t = go.transform.Find(yPivotName);
                    if (t == null) throw new System.Exception($"Child '{yPivotName}' not found under '{gameObjectName}'.");
                    aim.yPivot = t;
                }
                if (xPivotName != null)
                {
                    var t = go.transform.Find(xPivotName);
                    if (t == null) throw new System.Exception($"Child '{xPivotName}' not found under '{gameObjectName}'.");
                    aim.xPivot = t;
                }

                if (yLimitMin.HasValue || yLimitMax.HasValue)
                    aim.yLimit = new Vector2(yLimitMin ?? aim.yLimit.x, yLimitMax ?? aim.yLimit.y);
                if (xLimitMin.HasValue || xLimitMax.HasValue)
                    aim.xLimit = new Vector2(xLimitMin ?? aim.xLimit.x, xLimitMax ?? aim.xLimit.y);

                if (gravityY.HasValue)
                    aim.constantAcceleration = new Vector3(aim.constantAcceleration.x, gravityY.Value, aim.constantAcceleration.z);

                EditorUtility.SetDirty(aim);

                return $"OK: BallisticAim on '{gameObjectName}' configured. speed={aim.initialSpeed:F2} yPivot={aim.yPivot?.name ?? "none"} xPivot={aim.xPivot?.name ?? "none"} yLimit={FormatVector2(aim.yLimit)} xLimit={FormatVector2(aim.xLimit)}";
            });
        }

        [McpPluginTool("ballistic-configure-trickshot", Title = "Heathen Ballistics / Configure TrickShot")]
        [Description(@"Adds (if missing) and configures a TrickShot component for bounce-trajectory launching.
TrickShot predicts bounce paths and spawns a projectile template to follow them.
speed: projectile speed in m/s.
radius: collision sphere radius for trajectory prediction.
bounces: number of bounces to simulate (0 = no bounces).
bounceDamping [0-1]: velocity retained per bounce (0 = stops dead, 1 = no damping).
distance: max trajectory length. If distanceIsTotalLength=false, this is per-bounce.
resolution: simulation step size (smaller = more accurate, more expensive, 0.01 recommended).
templateName: name of a GameObject with BallisticPathFollow to use as projectile template.")]
        public string ConfigureTrickShot(
            [Description("Name of the GameObject to add TrickShot to.")] string gameObjectName,
            [Description("Projectile speed in m/s.")] float? speed = null,
            [Description("Collision sphere radius for trajectory prediction.")] float? radius = null,
            [Description("Number of bounces to simulate (0 = no bounces).")] int? bounces = null,
            [Description("Velocity multiplier retained per bounce [0-1]. 1 = no damping.")] float? bounceDamping = null,
            [Description("Max trajectory length in world units.")] float? distance = null,
            [Description("If true, distance is total path length. If false, per-bounce.")] bool? distanceIsTotalLength = null,
            [Description("Trajectory simulation step size. Smaller = more accurate.")] float? resolution = null,
            [Description("Name of GameObject with BallisticPathFollow to use as projectile template.")] string? templateName = null,
            [Description("Gravity Y acceleration (default -9.81).")] float? gravityY = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);

                var shot = go.GetComponent<TrickShot>();
                if (shot == null)
                    shot = go.AddComponent<TrickShot>();

                if (speed.HasValue) shot.speed = Mathf.Max(0.1f, speed.Value);
                if (radius.HasValue) shot.radius = Mathf.Max(0.001f, radius.Value);
                if (bounces.HasValue) shot.bounces = Mathf.Max(0, bounces.Value);
                if (bounceDamping.HasValue) shot.bounceDamping = Mathf.Clamp01(bounceDamping.Value);
                if (distance.HasValue) shot.distance = Mathf.Max(0.1f, distance.Value);
                if (distanceIsTotalLength.HasValue) shot.distanceIsTotalLength = distanceIsTotalLength.Value;
                if (resolution.HasValue) shot.resolution = Mathf.Clamp(resolution.Value, 0.001f, 1f);
                if (gravityY.HasValue)
                    shot.constantAcceleration = new Vector3(shot.constantAcceleration.x, gravityY.Value, shot.constantAcceleration.z);

                if (templateName != null)
                {
                    var templateGO = GameObject.Find(templateName);
                    if (templateGO == null) throw new System.Exception($"Template GameObject '{templateName}' not found.");
                    var follow = templateGO.GetComponent<BallisticPathFollow>();
                    if (follow == null) throw new System.Exception($"'{templateName}' has no BallisticPathFollow component.");
                    shot.template = follow;
                }

                EditorUtility.SetDirty(shot);

                return $"OK: TrickShot on '{gameObjectName}' configured. speed={shot.speed:F2} bounces={shot.bounces} damping={shot.bounceDamping:F3} distance={shot.distance:F2} resolution={shot.resolution:F4}";
            });
        }
    }
}
