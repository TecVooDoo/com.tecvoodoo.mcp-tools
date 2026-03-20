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
        [McpPluginTool("sensor-add-los", Title = "SensorToolkit / Add LOS Sensor")]
        [Description(@"Adds and configures a LOSSensor (Line of Sight) on a GameObject.
The LOSSensor is a compound sensor that tests line of sight for detections from an input sensor.
It casts rays at detected objects and calculates the ratio that are unobstructed.
Configure blocking layers, number of rays, view angle limits, and minimum visibility threshold.")]
        public string AddLOSSensor(
            [Description("Name of the target GameObject.")]
            string gameObjectName,
            [Description("Name of the GameObject with the input sensor (e.g. a RangeSensor). If omitted, expects an input sensor on the same GameObject.")]
            string? inputSensorName = null,
            [Description("Layer mask for layers that block line of sight (integer bitmask).")]
            int? blocksLineOfSight = null,
            [Description("Number of rays to cast per object for visibility testing. Range 1-20. Default 1.")]
            int? numberOfRays = null,
            [Description("Maximum detection distance. Requires limitDistance to be true.")]
            float? maxDistance = null,
            [Description("Enable view angle limits. Default false.")]
            bool? limitViewAngle = null,
            [Description("Maximum horizontal view angle in degrees (0-180). Used when limitViewAngle is true.")]
            float? maxHorizAngle = null,
            [Description("Maximum vertical view angle in degrees (0-90). Used when limitViewAngle is true.")]
            float? maxVertAngle = null,
            [Description("Minimum visibility ratio (0-1) for an object to be considered detected. Default 0.5.")]
            float? minimumVisibility = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                StringBuilder sb = new StringBuilder();

                LOSSensor sensor = GetOrAdd<LOSSensor>(go);

                // Assign input sensor if specified
                if (inputSensorName != null)
                {
                    GameObject inputGO = FindGO(inputSensorName);
                    Sensor inputSensor = inputGO.GetComponent<Sensor>();
                    if (inputSensor == null)
                        throw new System.Exception($"'{inputSensorName}' has no Sensor component to use as input.");
                    sensor.InputSensor = inputSensor;
                }
                else
                {
                    // Try to find an input sensor on the same GameObject (first non-LOS sensor)
                    Sensor[] siblings = go.GetComponents<Sensor>();
                    for (int i = 0; i < siblings.Length; i++)
                    {
                        if (siblings[i] is LOSSensor) continue;
                        sensor.InputSensor = siblings[i];
                        break;
                    }
                }

                if (blocksLineOfSight.HasValue)
                    sensor.BlocksLineOfSight = blocksLineOfSight.Value;

                if (numberOfRays.HasValue)
                    sensor.NumberOfRays = Mathf.Clamp(numberOfRays.Value, 1, 20);

                if (maxDistance.HasValue)
                {
                    sensor.LimitDistance = true;
                    sensor.MaxDistance = maxDistance.Value;
                }

                if (limitViewAngle.HasValue)
                    sensor.LimitViewAngle = limitViewAngle.Value;

                if (maxHorizAngle.HasValue)
                    sensor.MaxHorizAngle = Mathf.Clamp(maxHorizAngle.Value, 0f, 180f);

                if (maxVertAngle.HasValue)
                    sensor.MaxVertAngle = Mathf.Clamp(maxVertAngle.Value, 0f, 90f);

                if (minimumVisibility.HasValue)
                    sensor.MinimumVisibility = Mathf.Clamp01(minimumVisibility.Value);

                EditorUtility.SetDirty(go);

                sb.AppendLine($"Added LOSSensor to '{go.name}'");
                Sensor assignedInput = sensor.InputSensor;
                sb.AppendLine($"  InputSensor: {(assignedInput != null ? assignedInput.gameObject.name + " (" + assignedInput.GetType().Name + ")" : "none")}");
                sb.AppendLine($"  BlocksLineOfSight: {sensor.BlocksLineOfSight.value}");
                sb.AppendLine($"  NumberOfRays: {sensor.NumberOfRays}");
                sb.AppendLine($"  MinimumVisibility: {sensor.MinimumVisibility:F2}");
                sb.AppendLine($"  LimitDistance: {sensor.LimitDistance}");
                if (sensor.LimitDistance)
                    sb.AppendLine($"  MaxDistance: {sensor.MaxDistance:F2}");
                sb.AppendLine($"  LimitViewAngle: {sensor.LimitViewAngle}");
                if (sensor.LimitViewAngle)
                {
                    sb.AppendLine($"  MaxHorizAngle: {sensor.MaxHorizAngle:F1} deg");
                    sb.AppendLine($"  MaxVertAngle: {sensor.MaxVertAngle:F1} deg");
                }

                return sb.ToString();
            });
        }
    }
}
