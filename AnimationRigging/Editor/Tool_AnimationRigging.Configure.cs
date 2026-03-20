#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MCPTools.AnimationRigging.Editor
{
    public partial class Tool_AnimationRigging
    {
        [McpPluginTool("rig-configure-twoboneik", Title = "Animation Rigging / Configure Two Bone IK")]
        [Description(@"Configures a TwoBoneIKConstraint component.
Requires a TwoBoneIKConstraint already on the named object (add via GameObject menu or gameobject-component-add first).
rootBoneName / midBoneName / tipBoneName: the three bones in the IK chain (e.g. UpperArm, Forearm, Hand).
targetName: the IK target transform the tip bone tries to reach.
hintName: optional pole vector hint for elbow/knee direction.
targetPositionWeight [0-1]: how strongly the IK drives toward targetName position.
targetRotationWeight [0-1]: how strongly the IK drives toward targetName rotation.
hintWeight [0-1]: how strongly the pole vector hint influences the mid-joint direction.")]
        public string ConfigureTwoBoneIK(
            [Description("Name of the GameObject with TwoBoneIKConstraint.")] string gameObjectName,
            [Description("Name of the root bone (uppermost in the chain, e.g. UpperArm).")] string? rootBoneName = null,
            [Description("Name of the mid bone (e.g. Forearm or LowerLeg).")] string? midBoneName = null,
            [Description("Name of the tip bone (e.g. Hand or Foot).")] string? tipBoneName = null,
            [Description("Name of the IK target GameObject.")] string? targetName = null,
            [Description("Name of the IK hint (pole vector) GameObject. Optional.")] string? hintName = null,
            [Description("Position weight [0-1]. 1 = tip fully reaches target position.")] float? targetPositionWeight = null,
            [Description("Rotation weight [0-1]. 1 = tip fully matches target rotation.")] float? targetRotationWeight = null,
            [Description("Hint (pole vector) weight [0-1].")] float? hintWeight = null,
            [Description("Constraint blend weight [0-1].")] float? weight = null,
            [Description("Maintain current offset between target and tip on enable.")] bool? maintainPositionOffset = null,
            [Description("Maintain current rotation offset between target and tip on enable.")] bool? maintainRotationOffset = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var c = go.GetComponent<TwoBoneIKConstraint>();
                if (c == null) throw new System.Exception($"'{gameObjectName}' has no TwoBoneIKConstraint. Add it first with gameobject-component-add.");

                TwoBoneIKConstraintData data = c.data;
                if (rootBoneName != null) data.root = FindTransform(rootBoneName);
                if (midBoneName != null) data.mid = FindTransform(midBoneName);
                if (tipBoneName != null) data.tip = FindTransform(tipBoneName);
                if (targetName != null) data.target = FindTransform(targetName);
                if (hintName != null) data.hint = FindTransform(hintName);
                if (targetPositionWeight.HasValue) data.targetPositionWeight = targetPositionWeight.Value;
                if (targetRotationWeight.HasValue) data.targetRotationWeight = targetRotationWeight.Value;
                if (hintWeight.HasValue) data.hintWeight = hintWeight.Value;
                if (maintainPositionOffset.HasValue) data.maintainTargetPositionOffset = maintainPositionOffset.Value;
                if (maintainRotationOffset.HasValue) data.maintainTargetRotationOffset = maintainRotationOffset.Value;
                c.data = data;

                if (weight.HasValue) c.weight = Mathf.Clamp01(weight.Value);

                EditorUtility.SetDirty(c);
                return $"OK: TwoBoneIKConstraint on '{gameObjectName}' configured. root={c.data.root?.name ?? "none"} mid={c.data.mid?.name ?? "none"} tip={c.data.tip?.name ?? "none"} target={c.data.target?.name ?? "none"} posW={c.data.targetPositionWeight:F3} rotW={c.data.targetRotationWeight:F3} hintW={c.data.hintWeight:F3} weight={c.weight:F3}";
            });
        }

        [McpPluginTool("rig-configure-aim", Title = "Animation Rigging / Configure Multi Aim Constraint")]
        [Description(@"Configures a MultiAimConstraint component.
MultiAimConstraint makes a bone (e.g. Head, Spine) aim toward one or more source transforms.
constrainedObjectName: the bone/transform to be aimed.
sourceNames: comma-separated list of source object names (e.g. 'Target1,Target2').
sourceWeights: comma-separated weights matching sourceNames (e.g. '1.0,0.5').
If sources already exist, existing entries are preserved and new ones are appended.")]
        public string ConfigureAim(
            [Description("Name of the GameObject with MultiAimConstraint.")] string gameObjectName,
            [Description("Name of the bone/transform to be aimed (e.g. Head).")] string? constrainedObjectName = null,
            [Description("Comma-separated names of aim source GameObjects.")] string? sourceNames = null,
            [Description("Comma-separated weights for each source [0-1] (must match source count).")] string? sourceWeights = null,
            [Description("Constraint blend weight [0-1].")] float? weight = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var c = go.GetComponent<MultiAimConstraint>();
                if (c == null) throw new System.Exception($"'{gameObjectName}' has no MultiAimConstraint. Add it first with gameobject-component-add.");

                MultiAimConstraintData data = c.data;
                if (constrainedObjectName != null) data.constrainedObject = FindTransform(constrainedObjectName);

                if (sourceNames != null)
                {
                    string[] names = sourceNames.Split(',');
                    float[] weights = new float[names.Length];
                    if (sourceWeights != null)
                    {
                        string[] wStrs = sourceWeights.Split(',');
                        for (int i = 0; i < weights.Length && i < wStrs.Length; i++)
                        {
                            if (float.TryParse(wStrs[i].Trim(), out float w))
                                weights[i] = Mathf.Clamp01(w);
                            else
                                weights[i] = 1f;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < weights.Length; i++) weights[i] = 1f;
                    }

                    WeightedTransformArray sources = data.sourceObjects;
                    for (int i = 0; i < names.Length; i++)
                    {
                        Transform t = FindTransform(names[i].Trim());
                        sources.Add(new WeightedTransform(t, weights[i]));
                    }
                    data.sourceObjects = sources;
                }

                c.data = data;
                if (weight.HasValue) c.weight = Mathf.Clamp01(weight.Value);

                EditorUtility.SetDirty(c);
                return $"OK: MultiAimConstraint on '{gameObjectName}' configured. constrained={c.data.constrainedObject?.name ?? "none"} sources={c.data.sourceObjects.Count} weight={c.weight:F3}";
            });
        }

        [McpPluginTool("rig-configure-weights", Title = "Animation Rigging / Configure Rig Weights")]
        [Description(@"Sets weight on a Rig or constraint component.
Use this to blend a whole rig in/out (Rig component) or a single constraint in/out.
weight [0-1]: 0 = constraint has no effect, 1 = full effect.
Typical workflow: weight=0 to disable IK during a cutscene, weight=1 to re-enable.
Also can enable/disable a RigBuilder layer by index.")]
        public string ConfigureWeights(
            [Description("Name of the GameObject with Rig or constraint component.")] string gameObjectName,
            [Description("Weight for Rig or any constraint on this object [0-1].")] float? weight = null,
            [Description("RigBuilder layer index to enable/disable (-1 to skip).")] int rigLayerIndex = -1,
            [Description("Enable (true) or disable (false) the specified RigBuilder layer.")] bool? rigLayerActive = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                bool changed = false;

                var rig = go.GetComponent<Rig>();
                if (rig != null && weight.HasValue)
                {
                    rig.weight = Mathf.Clamp01(weight.Value);
                    EditorUtility.SetDirty(rig);
                    changed = true;
                }

                if (weight.HasValue)
                {
                    float w = Mathf.Clamp01(weight.Value);
                    foreach (IRigConstraint c in go.GetComponents<IRigConstraint>())
                    {
                        c.weight = w;
                        EditorUtility.SetDirty(c as Object);
                        changed = true;
                    }
                }

                var rigBuilder = go.GetComponent<RigBuilder>();
                if (rigBuilder != null && rigLayerIndex >= 0 && rigLayerActive.HasValue)
                {
                    if (rigLayerIndex < rigBuilder.layers.Count)
                    {
                        RigLayer layer = rigBuilder.layers[rigLayerIndex];
                        layer.active = rigLayerActive.Value;
                        rigBuilder.layers[rigLayerIndex] = layer;
                        EditorUtility.SetDirty(rigBuilder);
                        changed = true;
                    }
                }

                if (!changed)
                    throw new System.Exception($"'{gameObjectName}' has no Rig, constraint, or RigBuilder to configure.");

                return $"OK: Weights configured on '{gameObjectName}'.{(weight.HasValue ? $" weight={weight.Value:F3}" : "")}";
            });
        }
    }
}
