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
        [McpPluginTool("sensor-configure-steering", Title = "SensorToolkit / Configure Steering Sensor")]
        [Description(@"Configures a SteeringSensor component on a GameObject.
Sets seek mode (Position, Direction, Wander, Stop), seek target, arrive/stopping distances,
resolution, locomotion mode, and spherical mode.
The SteeringSensor implements context-based steering for AI navigation.")]
        public string ConfigureSteering(
            [Description("Name of the GameObject with a SteeringSensor component.")]
            string gameObjectName,
            [Description("Seek mode: 'Position', 'Direction', 'Wander', 'Stop'. Null to keep current.")]
            string? seekMode = null,
            [Description("Name of a GameObject to seek toward (sets SeekMode to Position). Null to skip.")]
            string? seekTargetName = null,
            [Description("Distance threshold to consider destination reached. Null to keep current.")]
            float? arriveDistanceThreshold = null,
            [Description("Distance at which the agent begins slowing down. Null to keep current.")]
            float? stoppingDistance = null,
            [Description("Direction bucket count for steering grid resolution. Null to keep current.")]
            int? resolution = null,
            [Description("Locomotion mode: 'None', 'RigidBodyFlying', 'RigidBodyCharacter', 'UnityCharacterController'. Null to keep current.")]
            string? locomotionMode = null,
            [Description("Use spherical (3D) steering vectors instead of planar. Null to keep current.")]
            bool? isSpherical = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                SteeringSensor sensor = go.GetComponent<SteeringSensor>();
                if (sensor == null)
                    throw new System.Exception($"'{go.name}' has no SteeringSensor component.");

                StringBuilder sb = new StringBuilder();

                // Handle seek target first (overrides seekMode to Position)
                if (seekTargetName != null)
                {
                    GameObject target = FindGO(seekTargetName);
                    sensor.ArriveTo(target.transform);
                    sb.AppendLine($"Set seek target to '{target.name}'");
                }
                else if (seekMode != null)
                {
                    SeekMode mode = ParseEnum<SeekMode>(seekMode, SeekMode.Stop);
                    sensor.Seek.SeekMode = mode;
                }

                if (arriveDistanceThreshold.HasValue)
                    sensor.Seek.ArriveDistanceThreshold = arriveDistanceThreshold.Value;

                if (stoppingDistance.HasValue)
                    sensor.Seek.StoppingDistance = stoppingDistance.Value;

                if (resolution.HasValue)
                    sensor.Resolution = resolution.Value;

                if (locomotionMode != null)
                    sensor.LocomotionMode = ParseEnum<LocomotionMode>(locomotionMode, LocomotionMode.None);

                if (isSpherical.HasValue)
                    sensor.IsSpherical = isSpherical.Value;

                EditorUtility.SetDirty(go);

                sb.AppendLine($"Configured SteeringSensor on '{go.name}'");
                sb.AppendLine($"  SeekMode: {sensor.Seek.SeekMode}");
                sb.AppendLine($"  ArriveDistanceThreshold: {sensor.Seek.ArriveDistanceThreshold:F2}");
                sb.AppendLine($"  StoppingDistance: {sensor.Seek.StoppingDistance:F2}");
                sb.AppendLine($"  Resolution: {sensor.Resolution}");
                sb.AppendLine($"  IsSpherical: {sensor.IsSpherical}");
                sb.AppendLine($"  LocomotionMode: {sensor.LocomotionMode}");
                sb.AppendLine($"  PulseMode: {sensor.PulseMode}");

                return sb.ToString();
            });
        }
    }
}
