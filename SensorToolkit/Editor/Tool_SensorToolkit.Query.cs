#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Micosmo.SensorToolkit;
using UnityEngine;

namespace MCPTools.SensorToolkit.Editor
{
    public partial class Tool_SensorToolkit
    {
        [McpPluginTool("sensor-query", Title = "SensorToolkit / Query Sensors")]
        [Description(@"Lists all sensor components on a GameObject and their configuration.
Reports sensor type, enabled state, pulse mode, shape, detection layers, and current detections (play mode).
Supports RangeSensor, RaySensor, LOSSensor, TriggerSensor, and SteeringSensor.")]
        public string QuerySensors(
            [Description("Name of the GameObject to inspect for sensor components.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"=== Sensors on '{go.name}' ===");

                Sensor[] sensors = go.GetComponents<Sensor>();
                SteeringSensor[] steeringSensors = go.GetComponents<SteeringSensor>();

                if (sensors.Length == 0 && steeringSensors.Length == 0)
                {
                    sb.AppendLine("  No sensor components found.");
                    return sb.ToString();
                }

                int index = 0;
                for (int i = 0; i < sensors.Length; i++)
                {
                    Sensor sensor = sensors[i];
                    index++;
                    sb.AppendLine($"\n[{index}] {sensor.GetType().Name}");
                    sb.AppendLine($"  Enabled: {sensor.enabled}");

                    if (sensor is IPulseRoutine pulsable)
                    {
                        sb.AppendLine($"  PulseMode: {pulsable.PulseMode}");
                        if (pulsable.PulseMode == PulseRoutine.Modes.FixedInterval)
                            sb.AppendLine($"  PulseInterval: {pulsable.PulseInterval:F2}s");
                    }

                    if (sensor is RangeSensor range)
                    {
                        sb.AppendLine($"  Shape: {range.Shape}");
                        if (range.Shape == RangeSensor.Shapes.Sphere)
                            sb.AppendLine($"  Radius: {range.Sphere.Radius:F2}");
                        else if (range.Shape == RangeSensor.Shapes.Box)
                            sb.AppendLine($"  HalfExtents: {FormatV3(range.Box.HalfExtents)}");
                        else if (range.Shape == RangeSensor.Shapes.Capsule)
                            sb.AppendLine($"  Capsule: radius={range.Capsule.Radius:F2}, height={range.Capsule.Height:F2}");
                        sb.AppendLine($"  DetectsOnLayers: {range.DetectsOnLayers.value}");
                        sb.AppendLine($"  DetectionMode: {range.DetectionMode}");
                        sb.AppendLine($"  IgnoreTriggerColliders: {range.IgnoreTriggerColliders}");
                        sb.AppendLine($"  TagFilter: {(range.EnableTagFilter ? "enabled" : "disabled")}");
                        if (range.EnableTagFilter && range.AllowedTags != null)
                            sb.AppendLine($"  AllowedTags: [{string.Join(", ", range.AllowedTags)}]");
                    }
                    else if (sensor is RaySensor ray)
                    {
                        sb.AppendLine($"  CastShape: {ray.Shape}");
                        sb.AppendLine($"  Length: {ray.Length:F2}");
                        sb.AppendLine($"  Direction: {FormatV3(ray.Direction)}");
                        sb.AppendLine($"  WorldSpace: {ray.WorldSpace}");
                        sb.AppendLine($"  DetectsOnLayers: {ray.DetectsOnLayers.value}");
                        sb.AppendLine($"  ObstructedByLayers: {ray.ObstructedByLayers.value}");
                        sb.AppendLine($"  DetectionMode: {ray.DetectionMode}");
                        sb.AppendLine($"  IgnoreTriggerColliders: {ray.IgnoreTriggerColliders}");
                        if (Application.isPlaying)
                            sb.AppendLine($"  IsObstructed: {ray.IsObstructed}");
                        sb.AppendLine($"  TagFilter: {(ray.EnableTagFilter ? "enabled" : "disabled")}");
                        if (ray.EnableTagFilter && ray.AllowedTags != null)
                            sb.AppendLine($"  AllowedTags: [{string.Join(", ", ray.AllowedTags)}]");
                    }
                    else if (sensor is LOSSensor los)
                    {
                        Sensor inputSensor = los.InputSensor;
                        sb.AppendLine($"  InputSensor: {(inputSensor != null ? inputSensor.gameObject.name : "none")}");
                        sb.AppendLine($"  BlocksLineOfSight: {los.BlocksLineOfSight.value}");
                        sb.AppendLine($"  NumberOfRays: {los.NumberOfRays}");
                        sb.AppendLine($"  MinimumVisibility: {los.MinimumVisibility:F2}");
                        sb.AppendLine($"  LimitDistance: {los.LimitDistance}");
                        if (los.LimitDistance)
                            sb.AppendLine($"  MaxDistance: {los.MaxDistance:F2}");
                        sb.AppendLine($"  LimitViewAngle: {los.LimitViewAngle}");
                        if (los.LimitViewAngle)
                        {
                            sb.AppendLine($"  MaxHorizAngle: {los.MaxHorizAngle:F1} deg");
                            sb.AppendLine($"  MaxVertAngle: {los.MaxVertAngle:F1} deg");
                        }
                        sb.AppendLine($"  IgnoreTriggerColliders: {los.IgnoreTriggerColliders}");
                    }
                    else if (sensor is TriggerSensor trigger)
                    {
                        sb.AppendLine($"  DetectionMode: {trigger.DetectionMode}");
                        sb.AppendLine($"  TagFilter: {(trigger.EnableTagFilter ? "enabled" : "disabled")}");
                        if (trigger.EnableTagFilter && trigger.AllowedTags != null)
                            sb.AppendLine($"  AllowedTags: [{string.Join(", ", trigger.AllowedTags)}]");
                    }

                    // Detection count in play mode
                    if (Application.isPlaying)
                    {
                        System.Collections.Generic.List<GameObject> detections = sensor.GetDetections();
                        sb.AppendLine($"  CurrentDetections: {detections.Count}");
                    }
                }

                // SteeringSensor does not inherit from Sensor
                for (int i = 0; i < steeringSensors.Length; i++)
                {
                    SteeringSensor steer = steeringSensors[i];
                    // Skip if already reported (SteeringSensor is BasePulsableSensor, not Sensor)
                    index++;
                    sb.AppendLine($"\n[{index}] SteeringSensor");
                    sb.AppendLine($"  Enabled: {steer.enabled}");
                    sb.AppendLine($"  PulseMode: {steer.PulseMode}");
                    if (steer.PulseMode == PulseRoutine.Modes.FixedInterval)
                        sb.AppendLine($"  PulseInterval: {steer.PulseInterval:F2}s");
                    sb.AppendLine($"  Resolution: {steer.Resolution}");
                    sb.AppendLine($"  IsSpherical: {steer.IsSpherical}");
                    sb.AppendLine($"  LocomotionMode: {steer.LocomotionMode}");
                    sb.AppendLine($"  SeekMode: {steer.Seek.SeekMode}");
                    sb.AppendLine($"  ArriveDistanceThreshold: {steer.Seek.ArriveDistanceThreshold:F2}");
                    sb.AppendLine($"  StoppingDistance: {steer.Seek.StoppingDistance:F2}");
                    if (Application.isPlaying)
                    {
                        sb.AppendLine($"  IsDestinationReached: {steer.IsDestinationReached}");
                        sb.AppendLine($"  SteeringVector: {FormatV3(steer.GetSteeringVector())}");
                    }
                }

                return sb.ToString();
            });
        }
    }
}
