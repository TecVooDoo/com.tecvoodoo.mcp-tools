#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Unity.Cinemachine;
using UnityEngine;

namespace MCPTools.Cinemachine.Editor
{
    public partial class Tool_Cinemachine
    {
        [McpPluginTool("cm-query", Title = "Cinemachine / Query Camera")]
        [Description(@"Reads the full setup of a CinemachineCamera or CinemachineBrain.
For CinemachineCamera: reports Priority, Target, Lens settings (FOV, clip planes, Dutch),
and all pipeline components (CinemachineFollow, CinemachinePositionComposer,
CinemachineRotationComposer, CinemachineThirdPersonFollow, CinemachineBasicMultiChannelPerlin).
For CinemachineBrain: reports DefaultBlend, UpdateMethod, IgnoreTimeScale.")]
        public string QueryCamera(
            [Description("Name of the GameObject with CinemachineCamera or CinemachineBrain component.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var sb = new StringBuilder();

                var cam = go.GetComponent<CinemachineCamera>();
                var brain = go.GetComponent<CinemachineBrain>();

                if (cam != null)
                {
                    sb.AppendLine($"=== CinemachineCamera: {go.name} ===");
                    sb.AppendLine($"  IsLive:    {cam.IsLive}");
                    sb.AppendLine($"  Priority:  {cam.Priority.Value}");
                    sb.AppendLine($"  BlendHint: {cam.BlendHint}");

                    sb.AppendLine($"\n-- Target --");
                    sb.AppendLine($"  TrackingTarget: {(cam.Target.TrackingTarget != null ? cam.Target.TrackingTarget.name : "none")}");
                    sb.AppendLine($"  LookAtTarget:   {(cam.Target.LookAtTarget != null ? cam.Target.LookAtTarget.name : "none")}");

                    sb.AppendLine($"\n-- Lens --");
                    sb.AppendLine($"  FieldOfView:   {cam.Lens.FieldOfView:F2}");
                    sb.AppendLine($"  NearClipPlane: {cam.Lens.NearClipPlane:F3}");
                    sb.AppendLine($"  FarClipPlane:  {cam.Lens.FarClipPlane:F1}");
                    sb.AppendLine($"  Dutch:         {cam.Lens.Dutch:F2}");

                    var follow = go.GetComponent<CinemachineFollow>();
                    if (follow != null)
                    {
                        sb.AppendLine($"\n-- CinemachineFollow --");
                        sb.AppendLine($"  FollowOffset:    {FormatV3(follow.FollowOffset)}");
                        sb.AppendLine($"  BindingMode:     {follow.TrackerSettings.BindingMode}");
                        sb.AppendLine($"  PositionDamping: {FormatV3(follow.TrackerSettings.PositionDamping)}");
                        sb.AppendLine($"  RotationDamping: {FormatV3(follow.TrackerSettings.RotationDamping)}");
                    }

                    var tpf = go.GetComponent<CinemachineThirdPersonFollow>();
                    if (tpf != null)
                    {
                        sb.AppendLine($"\n-- CinemachineThirdPersonFollow --");
                        sb.AppendLine($"  Damping:        {FormatV3(tpf.Damping)}");
                        sb.AppendLine($"  ShoulderOffset: {FormatV3(tpf.ShoulderOffset)}");
                        sb.AppendLine($"  CameraSide:     {tpf.CameraSide:F2}");
                        sb.AppendLine($"  CameraDistance: {tpf.CameraDistance:F2}");
                    }

                    var pc = go.GetComponent<CinemachinePositionComposer>();
                    if (pc != null)
                    {
                        sb.AppendLine($"\n-- CinemachinePositionComposer --");
                        sb.AppendLine($"  CameraDistance: {pc.CameraDistance:F2}");
                        sb.AppendLine($"  Damping:        {FormatV3(pc.Damping)}");
                    }

                    var rc = go.GetComponent<CinemachineRotationComposer>();
                    if (rc != null)
                    {
                        sb.AppendLine($"\n-- CinemachineRotationComposer --");
                        sb.AppendLine($"  Damping: ({rc.Damping.x:F2}, {rc.Damping.y:F2})");
                    }

                    var noise = go.GetComponent<CinemachineBasicMultiChannelPerlin>();
                    if (noise != null)
                    {
                        sb.AppendLine($"\n-- CinemachineBasicMultiChannelPerlin --");
                        sb.AppendLine($"  AmplitudeGain:  {noise.AmplitudeGain:F3}");
                        sb.AppendLine($"  FrequencyGain:  {noise.FrequencyGain:F3}");
                        sb.AppendLine($"  NoiseProfile:   {(noise.NoiseProfile != null ? noise.NoiseProfile.name : "none")}");
                    }
                }

                if (brain != null)
                {
                    sb.AppendLine($"=== CinemachineBrain: {go.name} ===");
                    sb.AppendLine($"  UpdateMethod:   {brain.UpdateMethod}");
                    sb.AppendLine($"  IgnoreTimeScale:{brain.IgnoreTimeScale}");
                    sb.AppendLine($"  DefaultBlend:   {brain.DefaultBlend.Style} {brain.DefaultBlend.Time:F2}s");
                    sb.AppendLine($"  LiveCamera:     {(brain.ActiveVirtualCamera != null ? brain.ActiveVirtualCamera.Name : "none")}");
                }

                if (cam == null && brain == null)
                    throw new System.Exception($"'{gameObjectName}' has no CinemachineCamera or CinemachineBrain component.");

                return sb.ToString();
            });
        }
    }
}
