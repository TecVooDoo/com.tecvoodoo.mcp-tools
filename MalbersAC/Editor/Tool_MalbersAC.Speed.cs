#if HAS_MALBERS_AC
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using MalbersAnimations.Controller;
using UnityEditor;
using UnityEngine;

namespace MCPTools.MalbersAC.Editor
{
    public partial class Tool_MalbersAC
    {
        [McpPluginTool("ac-configure-speed", Title = "Malbers AC / Configure Speed")]
        [Description(@"Configures a speed entry within a speed set on an MAnimal component.
Speed sets control movement speeds for different states (e.g. Ground has Walk/Trot/Run).
Only provided parameters are changed; others are left as-is.
Use 'ac-query-animal' first to see available speed sets and their speeds.")]
        public ConfigureSpeedResponse ConfigureSpeed(
            [Description("Reference to the GameObject with MAnimal component.")]
            GameObjectRef targetRef,
            [Description("Speed set name (e.g. 'Ground', 'Swim', 'Fly'). Case-insensitive partial match.")]
            string speedSetName,
            [Description("Speed name within the set (e.g. 'Walk', 'Trot', 'Run'). Case-insensitive partial match.")]
            string speedName,
            [Description("Additive position speed (non-root-motion). Null to keep current.")]
            float? position = null,
            [Description("Position lerp smoothing. Null to keep current.")]
            float? lerpPosition = null,
            [Description("Additive rotation speed. Null to keep current.")]
            float? rotation = null,
            [Description("Animator speed multiplier. Null to keep current.")]
            float? animator = null,
            [Description("Animator lerp smoothing. Null to keep current.")]
            float? lerpAnimator = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var animal = go.GetComponent<MAnimal>();
                if (animal == null)
                    throw new System.Exception($"'{go.name}' has no MAnimal component.");

                MSpeedSet? foundSet = null;
                string setSearchLower = speedSetName.ToLower();

                if (animal.speedSets != null)
                {
                    foreach (var speedSet in animal.speedSets)
                    {
                        if (speedSet == null) continue;
                        if (speedSet.name.ToLower().Contains(setSearchLower))
                        {
                            foundSet = speedSet;
                            break;
                        }
                    }
                }

                if (foundSet == null)
                    throw new System.Exception($"Speed set '{speedSetName}' not found on '{go.name}'.");

                if (foundSet.Speeds == null || foundSet.Speeds.Count == 0)
                    throw new System.Exception($"Speed set '{foundSet.name}' has no speed entries.");

                int foundIndex = -1;
                string speedSearchLower = speedName.ToLower();

                for (int i = 0; i < foundSet.Speeds.Count; i++)
                {
                    if (foundSet.Speeds[i].name.ToLower().Contains(speedSearchLower))
                    {
                        foundIndex = i;
                        break;
                    }
                }

                if (foundIndex < 0)
                    throw new System.Exception($"Speed '{speedName}' not found in set '{foundSet.name}'.");

                MSpeed speed = foundSet.Speeds[foundIndex];

                if (position.HasValue) speed.position.Value = position.Value;
                if (lerpPosition.HasValue) speed.lerpPosition.Value = lerpPosition.Value;
                if (rotation.HasValue) speed.rotation.Value = rotation.Value;
                if (animator.HasValue) speed.animator.Value = animator.Value;
                if (lerpAnimator.HasValue) speed.lerpAnimator.Value = lerpAnimator.Value;

                foundSet.Speeds[foundIndex] = speed;

                EditorUtility.SetDirty(go);

                return new ConfigureSpeedResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    speedSetName = foundSet.name,
                    speedName = speed.name,
                    position = speed.position.Value,
                    lerpPosition = speed.lerpPosition.Value,
                    rotation = speed.rotation.Value,
                    animator = speed.animator.Value,
                    lerpAnimator = speed.lerpAnimator.Value
                };
            });
        }

        public class ConfigureSpeedResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Speed set name")] public string speedSetName = "";
            [Description("Speed entry name")] public string speedName = "";
            [Description("Position speed value")] public float position;
            [Description("Position lerp value")] public float lerpPosition;
            [Description("Rotation speed value")] public float rotation;
            [Description("Animator speed value")] public float animator;
            [Description("Animator lerp value")] public float lerpAnimator;
        }
    }
}
#endif
