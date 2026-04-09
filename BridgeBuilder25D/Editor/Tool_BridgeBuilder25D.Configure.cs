#if HAS_BRIDGEBUILDER25D
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Kamgam.BridgeBuilder25D;
using UnityEditor;
using UnityEngine;

namespace MCPTools.BridgeBuilder25D.Editor
{
    public partial class Tool_BridgeBuilder25D
    {
        [McpPluginTool("bridge25d-configure", Title = "2.5D Bridge / Configure")]
        [Description(@"Configures physics, visuals, and damage on a Bridge25D component.
Only provided parameters are changed; others are left as-is.
Use 'bridge25d-query' first to see current values.
Call 'bridge25d-control' with action='recreate' after configuring to apply changes.")]
        public string Configure(
            [Description("Name of the Bridge25D GameObject.")] string gameObjectName,
            [Description("Mass per bridge plank.")] float? mass = null,
            [Description("Linear drag per plank (higher = less swing).")] float? linearDrag = null,
            [Description("Gravity scale per plank.")] float? gravityScale = null,
            [Description("Spring damping ratio [0-1].")] float? springDampingRatio = null,
            [Description("Spring frequency (higher = stiffer).")] float? springFrequency = null,
            [Description("Add spring joints every N parts. 0 = none. [0-5]")] int? addSpringJointsEvery = null,
            [Description("Damage threshold before break.")] float? damageTilBreak = null,
            [Description("Mass of broken parts.")] float? massBroken = null,
            [Description("Linear drag of broken parts.")] float? linearDragBroken = null,
            [Description("Layer index for broken parts.")] int? partLayerIfBroken = null,
            [Description("Scale of bridge part meshes (x,y,z as 'x,y,z' string).")] string? scale = null,
            [Description("Width variation between planks [0-0.7].")] float? widthVariation = null,
            [Description("Random X rotation per plank [0-10].")] float? randomRotationX = null,
            [Description("Random Y rotation per plank [0-10].")] float? randomRotationY = null,
            [Description("Wake physics on start (false = sleep until proximity).")] bool? startAwake = null,
            [Description("Recreate bridge on checkpoint reset.")] bool? recreateOnReset = null,
            [Description("Edge part outward offset along X.")] float? bridgeEdgePartOffset = null,
            [Description("Plank width in prefab (for spacing calculation).")] float? bridgePartWidthInPrefab = null,
            [Description("Plank depth in prefab (Z axis).")] float? bridgePartDepthInPrefab = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var bridge = GetBridge(gameObjectName);

                // Physics (per part)
                if (mass.HasValue) bridge.Mass = Mathf.Max(0.01f, mass.Value);
                if (linearDrag.HasValue) bridge.LinearDrag = Mathf.Max(0f, linearDrag.Value);
                if (gravityScale.HasValue) bridge.GravityScale = gravityScale.Value;
                if (springDampingRatio.HasValue) bridge.SpringDampingRatio = Mathf.Clamp01(springDampingRatio.Value);
                if (springFrequency.HasValue) bridge.SpringFrequency = Mathf.Max(0f, springFrequency.Value);
                if (addSpringJointsEvery.HasValue) bridge.AddSpringJointsEvery = Mathf.Clamp(addSpringJointsEvery.Value, 0, 5);

                // Damage
                if (damageTilBreak.HasValue) bridge.damageTilBreak = Mathf.Max(0f, damageTilBreak.Value);

                // Physics (broken)
                if (massBroken.HasValue) bridge.MassBroken = Mathf.Max(0.01f, massBroken.Value);
                if (linearDragBroken.HasValue) bridge.LinearDragBroken = Mathf.Max(0f, linearDragBroken.Value);
                if (partLayerIfBroken.HasValue) bridge.PartLayerIfBroken = partLayerIfBroken.Value;

                // Visuals
                if (scale != null)
                {
                    string[] parts = scale.Split(',');
                    if (parts.Length != 3)
                        throw new System.Exception("scale must be 'x,y,z' format (e.g. '1,1,1').");
                    bridge.Scale = new Vector3(
                        float.Parse(parts[0].Trim()),
                        float.Parse(parts[1].Trim()),
                        float.Parse(parts[2].Trim())
                    );
                }
                if (widthVariation.HasValue) bridge.WidthVariation = Mathf.Clamp(widthVariation.Value, 0f, 0.7f);
                if (randomRotationX.HasValue) bridge.RandomRotationX = Mathf.Clamp(randomRotationX.Value, 0f, 10f);
                if (randomRotationY.HasValue) bridge.RandomRotationY = Mathf.Clamp(randomRotationY.Value, 0f, 10f);

                // Behavior
                if (startAwake.HasValue) bridge.StartAwake = startAwake.Value;
                if (recreateOnReset.HasValue) bridge.RecreateOnReset = recreateOnReset.Value;
                if (bridgeEdgePartOffset.HasValue) bridge.BridgeEdgePartOffset = bridgeEdgePartOffset.Value;
                if (bridgePartWidthInPrefab.HasValue) bridge.BridgePartWidthInPrefab = Mathf.Max(0.01f, bridgePartWidthInPrefab.Value);
                if (bridgePartDepthInPrefab.HasValue) bridge.BridgePartDepthInPrefab = Mathf.Max(0.01f, bridgePartDepthInPrefab.Value);

                EditorUtility.SetDirty(bridge);

                return $"OK: Bridge25D on '{gameObjectName}' configured. Mass={bridge.Mass:F1} Drag={bridge.LinearDrag:F2} Spring={bridge.SpringFrequency:F2} DamageTilBreak={bridge.damageTilBreak:F1} Parts={bridge.Parts?.Count ?? 0}";
            });
        }

        [McpPluginTool("bridge25d-control", Title = "2.5D Bridge / Control")]
        [Description(@"Controls bridge state: recreate, break, or toggle physics.
Actions: 'recreate' rebuilds the bridge from its spline, 'break' breaks the bridge
at a normalized position (0-1 from start to end), 'physics-on'/'physics-off' toggles
physics simulation on all parts, 'add-proximity' adds a proximity trigger,
'remove-proximity' removes the proximity trigger.")]
        public string Control(
            [Description("Name of the Bridge25D GameObject.")] string gameObjectName,
            [Description("Action: recreate, break, physics-on, physics-off, add-proximity, remove-proximity.")] string action,
            [Description("For 'break' action: normalized position [0-1] along bridge.")] float? breakPosition = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var bridge = GetBridge(gameObjectName);

                switch (action.ToLowerInvariant())
                {
                    case "recreate":
                        bridge.Recreate();
                        EditorUtility.SetDirty(bridge);
                        return $"OK: Bridge '{gameObjectName}' recreated. Parts={bridge.Parts?.Count ?? 0}";

                    case "break":
                        float t = breakPosition ?? 0.5f;
                        bridge.BreakAt(Mathf.Clamp01(t));
                        return $"OK: Bridge '{gameObjectName}' broken at t={t:F2}.";

                    case "physics-on":
                        bridge.SetPhysicsActive(true);
                        return $"OK: Physics activated on '{gameObjectName}'.";

                    case "physics-off":
                        bridge.SetPhysicsActive(false);
                        return $"OK: Physics deactivated on '{gameObjectName}'.";

                    case "add-proximity":
                        bridge.AddProximityTrigger();
                        EditorUtility.SetDirty(bridge);
                        return $"OK: Proximity trigger added to '{gameObjectName}'.";

                    case "remove-proximity":
                        bridge.RemoveProximityTrigger();
                        EditorUtility.SetDirty(bridge);
                        return $"OK: Proximity trigger removed from '{gameObjectName}'.";

                    default:
                        throw new System.Exception($"Unknown action '{action}'. Valid: recreate, break, physics-on, physics-off, add-proximity, remove-proximity");
                }
            });
        }
    }
}
#endif
