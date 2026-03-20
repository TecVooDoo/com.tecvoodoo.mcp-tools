#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Cinemachine.Editor
{
    public partial class Tool_Cinemachine
    {
        [McpPluginTool("cm-configure-brain", Title = "Cinemachine / Configure Brain")]
        [Description(@"Configures the CinemachineBrain on a Camera GameObject.
defaultBlendStyle: transition style between cameras.
  Cut, EaseInOut, EaseIn, EaseOut, HardIn, HardOut, Linear.
defaultBlendTime: duration of the default transition in seconds.
updateMethod: when the brain updates -- SmartUpdate, FixedUpdate, LateUpdate.
ignoreTimeScale: if true, camera cuts always happen instantly regardless of Time.timeScale.")]
        public string ConfigureBrain(
            [Description("Name of the GameObject with CinemachineBrain (usually the main Camera).")] string gameObjectName,
            [Description("Default blend style: Cut, EaseInOut, EaseIn, EaseOut, HardIn, HardOut, Linear.")] string? defaultBlendStyle = null,
            [Description("Default blend duration in seconds.")] float? defaultBlendTime = null,
            [Description("Update method: SmartUpdate, FixedUpdate, LateUpdate.")] string? updateMethod = null,
            [Description("If true, ignore Time.timeScale for camera updates.")] bool? ignoreTimeScale = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var brain = go.GetComponent<CinemachineBrain>();
                if (brain == null) throw new System.Exception($"'{gameObjectName}' has no CinemachineBrain.");

                if (defaultBlendStyle != null || defaultBlendTime.HasValue)
                {
                    CinemachineBlendDefinition blend = brain.DefaultBlend;
                    if (defaultBlendStyle != null)
                    {
                        if (!System.Enum.TryParse<CinemachineBlendDefinition.Styles>(defaultBlendStyle, true, out CinemachineBlendDefinition.Styles style))
                            throw new System.Exception($"Invalid blend style '{defaultBlendStyle}'. Valid: Cut, EaseInOut, EaseIn, EaseOut, HardIn, HardOut, Linear");
                        blend.Style = style;
                    }
                    if (defaultBlendTime.HasValue) blend.Time = Mathf.Max(0f, defaultBlendTime.Value);
                    brain.DefaultBlend = blend;
                }

                if (updateMethod != null)
                {
                    if (!System.Enum.TryParse<CinemachineBrain.UpdateMethods>(updateMethod, true, out CinemachineBrain.UpdateMethods um))
                        throw new System.Exception($"Invalid updateMethod '{updateMethod}'. Valid: SmartUpdate, FixedUpdate, LateUpdate");
                    brain.UpdateMethod = um;
                }

                if (ignoreTimeScale.HasValue) brain.IgnoreTimeScale = ignoreTimeScale.Value;

                EditorUtility.SetDirty(brain);
                return $"OK: CinemachineBrain on '{gameObjectName}' configured. blend={brain.DefaultBlend.Style} {brain.DefaultBlend.Time:F2}s update={brain.UpdateMethod} ignoreTimeScale={brain.IgnoreTimeScale}";
            });
        }
    }
}
