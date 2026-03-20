#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Unity.Cinemachine;
using Unity.Cinemachine.TargetTracking;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Cinemachine.Editor
{
    public partial class Tool_Cinemachine
    {
        [McpPluginTool("cm-configure-camera", Title = "Cinemachine / Configure Camera")]
        [Description(@"Configures a CinemachineCamera component.
priority: controls which camera is live when multiple cameras are active (higher wins).
fov: vertical field of view in degrees (default 60).
nearClip / farClip: camera clipping planes.
dutch: camera roll offset in degrees.
trackingTargetName / lookAtTargetName: assign named scene objects as targets.")]
        public string ConfigureCamera(
            [Description("Name of the GameObject with CinemachineCamera.")] string gameObjectName,
            [Description("Camera priority -- higher priority cameras take control.")] int? priority = null,
            [Description("Vertical field of view in degrees.")] float? fov = null,
            [Description("Near clip plane distance.")] float? nearClip = null,
            [Description("Far clip plane distance.")] float? farClip = null,
            [Description("Camera dutch (roll) in degrees.")] float? dutch = null,
            [Description("Name of the tracking (follow) target GameObject.")] string? trackingTargetName = null,
            [Description("Name of the look-at target GameObject.")] string? lookAtTargetName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var cam = go.GetComponent<CinemachineCamera>();
                if (cam == null) throw new System.Exception($"'{gameObjectName}' has no CinemachineCamera.");

                if (priority.HasValue) cam.Priority = new PrioritySettings { Value = priority.Value };

                if (fov.HasValue || nearClip.HasValue || farClip.HasValue || dutch.HasValue)
                {
                    LensSettings lens = cam.Lens;
                    if (fov.HasValue) lens.FieldOfView = Mathf.Clamp(fov.Value, 1f, 179f);
                    if (nearClip.HasValue) lens.NearClipPlane = Mathf.Max(0.001f, nearClip.Value);
                    if (farClip.HasValue) lens.FarClipPlane = Mathf.Max(lens.NearClipPlane + 0.1f, farClip.Value);
                    if (dutch.HasValue) lens.Dutch = dutch.Value;
                    cam.Lens = lens;
                }

                if (trackingTargetName != null)
                {
                    var t = GameObject.Find(trackingTargetName);
                    if (t == null) throw new System.Exception($"Tracking target '{trackingTargetName}' not found.");
                    cam.Target.TrackingTarget = t.transform;
                }
                if (lookAtTargetName != null)
                {
                    var t = GameObject.Find(lookAtTargetName);
                    if (t == null) throw new System.Exception($"LookAt target '{lookAtTargetName}' not found.");
                    cam.Target.LookAtTarget = t.transform;
                }

                EditorUtility.SetDirty(cam);
                return $"OK: CinemachineCamera '{gameObjectName}' configured. priority={cam.Priority.Value} fov={cam.Lens.FieldOfView:F1} near={cam.Lens.NearClipPlane:F3} far={cam.Lens.FarClipPlane:F0}";
            });
        }

        [McpPluginTool("cm-configure-follow", Title = "Cinemachine / Configure Follow")]
        [Description(@"Configures a CinemachineFollow or CinemachineThirdPersonFollow component.
CinemachineFollow: offset-based follow with damping (good for side-scrollers, top-down, overhead).
  followOffsetX/Y/Z: world offset from target.
  positionDampingX/Y/Z: position damping per axis.
  rotationDampingX/Y/Z: rotation damping per axis.
  bindingMode: WorldSpace, LockToTarget, LockToTargetNoRoll, LazyFollow, etc.
CinemachineThirdPersonFollow: shoulder rig follow (good for over-the-shoulder TPS).
  shoulderOffsetX/Y/Z: shoulder position relative to target.
  cameraSide: [0-1] where 0=left shoulder, 1=right shoulder.
  cameraDistance: distance behind the shoulder.")]
        public string ConfigureFollow(
            [Description("Name of the GameObject with the follow component.")] string gameObjectName,
            [Description("Follow offset X (CinemachineFollow only).")] float? followOffsetX = null,
            [Description("Follow offset Y (CinemachineFollow only).")] float? followOffsetY = null,
            [Description("Follow offset Z (CinemachineFollow only).")] float? followOffsetZ = null,
            [Description("Position damping X axis (CinemachineFollow only).")] float? positionDampingX = null,
            [Description("Position damping Y axis (CinemachineFollow only).")] float? positionDampingY = null,
            [Description("Position damping Z axis (CinemachineFollow only).")] float? positionDampingZ = null,
            [Description("Rotation damping X axis (CinemachineFollow only).")] float? rotationDampingX = null,
            [Description("Rotation damping Y axis (CinemachineFollow only).")] float? rotationDampingY = null,
            [Description("Rotation damping Z axis (CinemachineFollow only).")] float? rotationDampingZ = null,
            [Description("Binding mode: WorldSpace, LockToTarget, LockToTargetNoRoll, LockToTargetOnAssign, LockToTargetWithWorldUp, LazyFollow.")] string? bindingMode = null,
            [Description("Shoulder offset X (ThirdPersonFollow only).")] float? shoulderOffsetX = null,
            [Description("Shoulder offset Y (ThirdPersonFollow only).")] float? shoulderOffsetY = null,
            [Description("Shoulder offset Z (ThirdPersonFollow only).")] float? shoulderOffsetZ = null,
            [Description("Camera side [0-1]: 0=left, 1=right (ThirdPersonFollow only).")] float? cameraSide = null,
            [Description("Camera distance from shoulder (ThirdPersonFollow only).")] float? cameraDistance = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                bool dirty = false;

                var follow = go.GetComponent<CinemachineFollow>();
                if (follow != null)
                {
                    if (followOffsetX.HasValue || followOffsetY.HasValue || followOffsetZ.HasValue)
                        follow.FollowOffset = new Vector3(
                            followOffsetX ?? follow.FollowOffset.x,
                            followOffsetY ?? follow.FollowOffset.y,
                            followOffsetZ ?? follow.FollowOffset.z);

                    TrackerSettings ts = follow.TrackerSettings;
                    if (positionDampingX.HasValue || positionDampingY.HasValue || positionDampingZ.HasValue)
                        ts.PositionDamping = new Vector3(
                            positionDampingX ?? ts.PositionDamping.x,
                            positionDampingY ?? ts.PositionDamping.y,
                            positionDampingZ ?? ts.PositionDamping.z);
                    if (rotationDampingX.HasValue || rotationDampingY.HasValue || rotationDampingZ.HasValue)
                        ts.RotationDamping = new Vector3(
                            rotationDampingX ?? ts.RotationDamping.x,
                            rotationDampingY ?? ts.RotationDamping.y,
                            rotationDampingZ ?? ts.RotationDamping.z);
                    if (bindingMode != null)
                    {
                        if (!System.Enum.TryParse<BindingMode>(bindingMode, true, out BindingMode bm))
                            throw new System.Exception($"Invalid bindingMode '{bindingMode}'. Valid: WorldSpace, LockToTarget, LockToTargetNoRoll, LockToTargetOnAssign, LockToTargetWithWorldUp, LazyFollow");
                        ts.BindingMode = bm;
                    }
                    follow.TrackerSettings = ts;
                    EditorUtility.SetDirty(follow);
                    dirty = true;
                }

                var tpf = go.GetComponent<CinemachineThirdPersonFollow>();
                if (tpf != null)
                {
                    if (shoulderOffsetX.HasValue || shoulderOffsetY.HasValue || shoulderOffsetZ.HasValue)
                        tpf.ShoulderOffset = new Vector3(
                            shoulderOffsetX ?? tpf.ShoulderOffset.x,
                            shoulderOffsetY ?? tpf.ShoulderOffset.y,
                            shoulderOffsetZ ?? tpf.ShoulderOffset.z);
                    if (cameraSide.HasValue) tpf.CameraSide = Mathf.Clamp01(cameraSide.Value);
                    if (cameraDistance.HasValue) tpf.CameraDistance = Mathf.Max(0f, cameraDistance.Value);
                    EditorUtility.SetDirty(tpf);
                    dirty = true;
                }

                if (!dirty)
                    throw new System.Exception($"'{gameObjectName}' has no CinemachineFollow or CinemachineThirdPersonFollow component.");

                return $"OK: Follow configured on '{gameObjectName}'.";
            });
        }

        [McpPluginTool("cm-configure-noise", Title = "Cinemachine / Configure Noise")]
        [Description(@"Configures a CinemachineBasicMultiChannelPerlin component for camera shake.
amplitudeGain: overall shake strength (0 = no shake, 1 = default, >1 = stronger).
frequencyGain: shake speed multiplier (1 = default, higher = faster shaking).
noiseProfileName: name of a NoiseSettings asset to use as the noise profile.
  Common profiles: 'Handheld_tele_mild', '6D Shake', 'Handheld_normal_mild'.
  Leave null to keep the current profile.")]
        public string ConfigureNoise(
            [Description("Name of the GameObject with CinemachineBasicMultiChannelPerlin.")] string gameObjectName,
            [Description("Amplitude (shake strength) multiplier. 1=default, 0=no shake.")] float? amplitudeGain = null,
            [Description("Frequency (shake speed) multiplier. 1=default.")] float? frequencyGain = null,
            [Description("Name of a NoiseSettings asset to load (e.g. 'Handheld_tele_mild'). Searched in all Resources.")] string? noiseProfileName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var noise = go.GetComponent<CinemachineBasicMultiChannelPerlin>();
                if (noise == null) throw new System.Exception($"'{gameObjectName}' has no CinemachineBasicMultiChannelPerlin.");

                if (amplitudeGain.HasValue) noise.AmplitudeGain = Mathf.Max(0f, amplitudeGain.Value);
                if (frequencyGain.HasValue) noise.FrequencyGain = Mathf.Max(0f, frequencyGain.Value);

                if (noiseProfileName != null)
                {
                    NoiseSettings? profile = null;
                    string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:NoiseSettings {noiseProfileName}");
                    if (guids.Length > 0)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                        profile = UnityEditor.AssetDatabase.LoadAssetAtPath<NoiseSettings>(path);
                    }
                    if (profile == null) profile = Resources.Load<NoiseSettings>(noiseProfileName);
                    if (profile == null) throw new System.Exception($"NoiseSettings asset '{noiseProfileName}' not found. Search AssetDatabase and Resources both failed.");
                    noise.NoiseProfile = profile;
                }

                EditorUtility.SetDirty(noise);
                return $"OK: CinemachineBasicMultiChannelPerlin on '{gameObjectName}' configured. amplitude={noise.AmplitudeGain:F3} frequency={noise.FrequencyGain:F3} profile={noise.NoiseProfile?.name ?? "none"}";
            });
        }
    }
}
