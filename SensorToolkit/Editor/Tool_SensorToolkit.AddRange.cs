#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Micosmo.SensorToolkit;
using UnityEditor;
using UnityEngine;

namespace MCPTools.SensorToolkit.Editor
{
    public partial class Tool_SensorToolkit
    {
        [McpPluginTool("sensor-add-range", Title = "SensorToolkit / Add Range or Ray Sensor")]
        [Description(@"Adds and configures a RangeSensor or RaySensor on a GameObject.
For RangeSensor: configures sphere shape with radius, detection layers, detection mode, pulse settings.
For RaySensor: configures ray length, direction, detection/obstruction layers, pulse settings.
Use 'sensor-query' afterward to verify the configuration.")]
        public string AddRangeOrRaySensor(
            [Description("Name of the target GameObject.")]
            string gameObjectName,
            [Description("Sensor type to add: 'Range' or 'Ray'.")]
            string sensorType,
            [Description("Sphere radius for RangeSensor. Default 5.")]
            float? radius = null,
            [Description("Ray length for RaySensor. Default 10.")]
            float? length = null,
            [Description("Layer mask for detection layers (integer bitmask). Null to keep default.")]
            int? detectsOnLayers = null,
            [Description("Pulse mode: 'Manual', 'FixedInterval', 'EachFrame'. Default 'EachFrame'.")]
            string? pulseMode = null,
            [Description("Seconds between pulses when using FixedInterval mode.")]
            float? pulseInterval = null,
            [Description("If true, sensor ignores trigger colliders.")]
            bool? ignoreTriggerColliders = null,
            [Description("Detection mode: 'Colliders', 'RigidBodies', 'Either'. Default 'Colliders'.")]
            string? detectionMode = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                StringBuilder sb = new StringBuilder();
                string sType = sensorType.Trim().ToLowerInvariant();

                if (sType == "range")
                {
                    RangeSensor sensor = GetOrAdd<RangeSensor>(go);

                    float r = radius ?? 5f;
                    sensor.SetSphereShape(r);

                    if (detectsOnLayers.HasValue)
                        sensor.DetectsOnLayers = detectsOnLayers.Value;

                    if (detectionMode != null)
                        sensor.DetectionMode = ParseEnum<DetectionModes>(detectionMode, DetectionModes.Colliders);

                    if (ignoreTriggerColliders.HasValue)
                        sensor.IgnoreTriggerColliders = ignoreTriggerColliders.Value;

                    if (pulseMode != null)
                        sensor.PulseMode = ParseEnum<PulseRoutine.Modes>(pulseMode, PulseRoutine.Modes.EachFrame);

                    if (pulseInterval.HasValue)
                        sensor.PulseInterval = pulseInterval.Value;

                    EditorUtility.SetDirty(go);

                    sb.AppendLine($"Added RangeSensor to '{go.name}'");
                    sb.AppendLine($"  Shape: Sphere, Radius: {r:F2}");
                    sb.AppendLine($"  DetectsOnLayers: {sensor.DetectsOnLayers.value}");
                    sb.AppendLine($"  DetectionMode: {sensor.DetectionMode}");
                    sb.AppendLine($"  PulseMode: {sensor.PulseMode}");
                    if (sensor.PulseMode == PulseRoutine.Modes.FixedInterval)
                        sb.AppendLine($"  PulseInterval: {sensor.PulseInterval:F2}s");
                    sb.AppendLine($"  IgnoreTriggerColliders: {sensor.IgnoreTriggerColliders}");
                }
                else if (sType == "ray")
                {
                    RaySensor sensor = GetOrAdd<RaySensor>(go);

                    if (length.HasValue)
                        sensor.Length = length.Value;

                    if (detectsOnLayers.HasValue)
                        sensor.DetectsOnLayers = detectsOnLayers.Value;

                    if (detectionMode != null)
                        sensor.DetectionMode = ParseEnum<DetectionModes>(detectionMode, DetectionModes.Colliders);

                    if (ignoreTriggerColliders.HasValue)
                        sensor.IgnoreTriggerColliders = ignoreTriggerColliders.Value;

                    if (pulseMode != null)
                        sensor.PulseMode = ParseEnum<PulseRoutine.Modes>(pulseMode, PulseRoutine.Modes.EachFrame);

                    if (pulseInterval.HasValue)
                        sensor.PulseInterval = pulseInterval.Value;

                    EditorUtility.SetDirty(go);

                    sb.AppendLine($"Added RaySensor to '{go.name}'");
                    sb.AppendLine($"  Length: {sensor.Length:F2}");
                    sb.AppendLine($"  Direction: {FormatV3(sensor.Direction)}");
                    sb.AppendLine($"  DetectsOnLayers: {sensor.DetectsOnLayers.value}");
                    sb.AppendLine($"  DetectionMode: {sensor.DetectionMode}");
                    sb.AppendLine($"  PulseMode: {sensor.PulseMode}");
                    if (sensor.PulseMode == PulseRoutine.Modes.FixedInterval)
                        sb.AppendLine($"  PulseInterval: {sensor.PulseInterval:F2}s");
                    sb.AppendLine($"  IgnoreTriggerColliders: {sensor.IgnoreTriggerColliders}");
                }
                else
                {
                    throw new System.Exception($"Invalid sensorType '{sensorType}'. Use 'Range' or 'Ray'.");
                }

                return sb.ToString();
            });
        }
    }
}
