#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MCPTools.AnimationRigging.Editor
{
    public partial class Tool_AnimationRigging
    {
        [McpPluginTool("rig-query", Title = "Animation Rigging / Query Rig Setup")]
        [Description(@"Lists all Animation Rigging components on a GameObject and its hierarchy.
Reports: RigBuilder (layers, active state), Rig (weight), and all constraints
(TwoBoneIKConstraint, MultiAimConstraint, MultiParentConstraint, ChainIKConstraint)
with their current weights and key configuration.
Use gameObjectName for the Animator/RigBuilder root, or a specific Rig/constraint object.")]
        public string QueryRig(
            [Description("Name of the GameObject to inspect (Animator root, Rig, or constraint).")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var sb = new StringBuilder();
                sb.AppendLine($"=== Animation Rigging: {go.name} ===");

                var rigBuilder = go.GetComponent<RigBuilder>();
                if (rigBuilder != null)
                {
                    sb.AppendLine($"\n-- RigBuilder --");
                    sb.AppendLine($"  layers: {rigBuilder.layers.Count}");
                    for (int i = 0; i < rigBuilder.layers.Count; i++)
                    {
                        RigLayer layer = rigBuilder.layers[i];
                        sb.AppendLine($"  [{i}] {(layer.rig != null ? layer.rig.name : "null")} active={layer.active} weight={layer.rig?.weight:F3}");
                    }
                }

                var rig = go.GetComponent<Rig>();
                if (rig != null)
                {
                    sb.AppendLine($"\n-- Rig --");
                    sb.AppendLine($"  weight: {rig.weight:F3}");
                }

                TwoBoneIKConstraint[] twoBoneIKs = go.GetComponentsInChildren<TwoBoneIKConstraint>(true);
                if (twoBoneIKs.Length > 0)
                {
                    sb.AppendLine($"\n-- TwoBoneIKConstraint ({twoBoneIKs.Length}) --");
                    foreach (TwoBoneIKConstraint c in twoBoneIKs)
                    {
                        sb.AppendLine($"  {c.name}: weight={c.weight:F3} root={c.data.root?.name ?? "none"} mid={c.data.mid?.name ?? "none"} tip={c.data.tip?.name ?? "none"} target={c.data.target?.name ?? "none"} hint={c.data.hint?.name ?? "none"}");
                        sb.AppendLine($"    posWeight={c.data.targetPositionWeight:F3} rotWeight={c.data.targetRotationWeight:F3} hintWeight={c.data.hintWeight:F3}");
                    }
                }

                MultiAimConstraint[] aims = go.GetComponentsInChildren<MultiAimConstraint>(true);
                if (aims.Length > 0)
                {
                    sb.AppendLine($"\n-- MultiAimConstraint ({aims.Length}) --");
                    foreach (MultiAimConstraint c in aims)
                    {
                        sb.AppendLine($"  {c.name}: weight={c.weight:F3} constrained={c.data.constrainedObject?.name ?? "none"} sources={c.data.sourceObjects.Count}");
                    }
                }

                MultiParentConstraint[] parents = go.GetComponentsInChildren<MultiParentConstraint>(true);
                if (parents.Length > 0)
                {
                    sb.AppendLine($"\n-- MultiParentConstraint ({parents.Length}) --");
                    foreach (MultiParentConstraint c in parents)
                    {
                        sb.AppendLine($"  {c.name}: weight={c.weight:F3} constrained={c.data.constrainedObject?.name ?? "none"} sources={c.data.sourceObjects.Count}");
                    }
                }

                ChainIKConstraint[] chains = go.GetComponentsInChildren<ChainIKConstraint>(true);
                if (chains.Length > 0)
                {
                    sb.AppendLine($"\n-- ChainIKConstraint ({chains.Length}) --");
                    foreach (ChainIKConstraint c in chains)
                    {
                        sb.AppendLine($"  {c.name}: weight={c.weight:F3} root={c.data.root?.name ?? "none"} tip={c.data.tip?.name ?? "none"} target={c.data.target?.name ?? "none"}");
                        sb.AppendLine($"    chainRotWeight={c.data.chainRotationWeight:F3} tipRotWeight={c.data.tipRotationWeight:F3} iterations={c.data.maxIterations}");
                    }
                }

                return sb.ToString();
            });
        }
    }
}
