#nullable enable
using System.Collections.Generic;
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
        [McpPluginTool("sensor-query-detections", Title = "SensorToolkit / Query Detections")]
        [Description(@"Queries runtime detections from sensors on a GameObject (play mode only).
Returns each detected object's name, distance, and signal strength.
Can filter by sensor type and tag, and sort by distance or signal strength.")]
        public string QueryDetections(
            [Description("Name of the GameObject with sensor components.")]
            string gameObjectName,
            [Description("Filter to a specific sensor type: 'Range', 'Ray', 'LOS', 'Trigger'. Null for first sensor found.")]
            string? sensorType = null,
            [Description("Sort results by 'distance' or 'strength'. Default 'distance'.")]
            string? sortBy = null,
            [Description("Filter detections to only objects with this tag. Null for all detections.")]
            string? tag = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!Application.isPlaying)
                    throw new System.Exception("sensor-query-detections requires play mode. Use sensor-query for edit-mode inspection.");

                GameObject go = FindGO(gameObjectName);
                StringBuilder sb = new StringBuilder();

                Sensor sensor = FindSensorOfType(go, sensorType);
                if (sensor == null)
                    throw new System.Exception($"No matching sensor found on '{go.name}'" +
                        (sensorType != null ? $" (filter: {sensorType})" : "") + ".");

                sb.AppendLine($"=== Detections from {sensor.GetType().Name} on '{go.name}' ===");

                string sort = (sortBy ?? "distance").Trim().ToLowerInvariant();
                List<Signal> signals;

                if (sort == "strength")
                {
                    signals = tag != null
                        ? sensor.GetSignalsBySignalStrength(tag)
                        : sensor.GetSignalsBySignalStrength();
                }
                else
                {
                    signals = tag != null
                        ? sensor.GetSignalsByDistance(tag)
                        : sensor.GetSignalsByDistance();
                }

                if (signals.Count == 0)
                {
                    sb.AppendLine("  No detections.");
                    return sb.ToString();
                }

                sb.AppendLine($"  Count: {signals.Count} (sorted by {sort})");
                if (tag != null)
                    sb.AppendLine($"  Tag filter: '{tag}'");

                for (int i = 0; i < signals.Count; i++)
                {
                    Signal signal = signals[i];
                    if (signal.Object == null) continue;

                    float distance = signal.DistanceTo(go.transform.position);
                    sb.AppendLine($"  [{i + 1}] {signal.Object.name}");
                    sb.AppendLine($"       Distance: {distance:F2}  Strength: {signal.Strength:F3}");
                    sb.AppendLine($"       Position: {FormatV3(signal.Object.transform.position)}");
                    sb.AppendLine($"       Tag: {signal.Object.tag}");
                }

                return sb.ToString();
            });
        }

        static Sensor FindSensorOfType(GameObject go, string? sensorType)
        {
            if (sensorType == null)
            {
                return go.GetComponent<Sensor>();
            }

            string filter = sensorType.Trim().ToLowerInvariant();
            Sensor[] sensors = go.GetComponents<Sensor>();

            for (int i = 0; i < sensors.Length; i++)
            {
                Sensor s = sensors[i];
                if (filter == "range" && s is RangeSensor) return s;
                if (filter == "ray" && s is RaySensor) return s;
                if (filter == "los" && s is LOSSensor) return s;
                if (filter == "trigger" && s is TriggerSensor) return s;
            }

            return null;
        }
    }
}
