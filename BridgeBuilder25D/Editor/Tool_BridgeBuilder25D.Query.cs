#if HAS_BRIDGEBUILDER25D
#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Kamgam.BridgeBuilder25D;
using UnityEngine;

namespace MCPTools.BridgeBuilder25D.Editor
{
    public partial class Tool_BridgeBuilder25D
    {
        [McpPluginTool("bridge25d-query", Title = "2.5D Bridge / Query")]
        [Description(@"Reads the full Bridge25D setup on a GameObject.
Reports physics settings (mass, drag, gravity, spring), damage threshold,
part count, edge parts, proximity trigger, and break state.
Use before configuring to understand current state.")]
        public string Query(
            [Description("Name of the Bridge25D GameObject.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var bridge = GetBridge(gameObjectName);
                var sb = new StringBuilder();

                sb.AppendLine($"=== Bridge25D: {bridge.name} ===");

                sb.AppendLine($"\n-- Creation --");
                sb.AppendLine($"  AutoCreate:       {bridge.AutoCreate}");
                sb.AppendLine($"  RecreateOnReset:  {bridge.RecreateOnReset}");
                sb.AppendLine($"  StartAwake:       {bridge.StartAwake}");
                sb.AppendLine($"  DamageTilBreak:   {bridge.damageTilBreak:F1}");

                sb.AppendLine($"\n-- Prefabs --");
                sb.AppendLine($"  BridgePartPrefab:     {(bridge.BridgePartPrefab != null ? bridge.BridgePartPrefab.name : "(none)")}");
                sb.AppendLine($"  BridgeEdgePartPrefab: {(bridge.BridgeEdgePartPrefab != null ? bridge.BridgeEdgePartPrefab.name : "(none)")}");
                sb.AppendLine($"  PartWidth:            {bridge.BridgePartWidthInPrefab:F2}");
                sb.AppendLine($"  PartDepth:            {bridge.BridgePartDepthInPrefab:F2}");

                sb.AppendLine($"\n-- Physics (per part) --");
                sb.AppendLine($"  Mass:              {bridge.Mass:F1}");
                sb.AppendLine($"  LinearDrag:        {bridge.LinearDrag:F2}");
                sb.AppendLine($"  GravityScale:      {bridge.GravityScale:F2}");
                sb.AppendLine($"  SpringDampingRatio:{bridge.SpringDampingRatio:F2}");
                sb.AppendLine($"  SpringFrequency:   {bridge.SpringFrequency:F2}");
                sb.AppendLine($"  AddSpringEvery:    {bridge.AddSpringJointsEvery}");

                sb.AppendLine($"\n-- Physics (if broken) --");
                sb.AppendLine($"  MassBroken:        {bridge.MassBroken:F1}");
                sb.AppendLine($"  LinearDragBroken:  {bridge.LinearDragBroken:F2}");
                sb.AppendLine($"  PartLayerIfBroken: {bridge.PartLayerIfBroken}");

                sb.AppendLine($"\n-- Visuals --");
                sb.AppendLine($"  Scale:             {bridge.Scale}");
                sb.AppendLine($"  WidthVariation:    {bridge.WidthVariation:F2}");
                sb.AppendLine($"  RandomRotationX:   {bridge.RandomRotationX:F1}");
                sb.AppendLine($"  RandomRotationY:   {bridge.RandomRotationY:F1}");
                sb.AppendLine($"  RandomMaterials:   {(bridge.RandomMaterials != null ? bridge.RandomMaterials.Length : 0)}");

                sb.AppendLine($"\n-- State --");
                sb.AppendLine($"  Parts:             {(bridge.Parts != null ? bridge.Parts.Count : 0)}");
                sb.AppendLine($"  EdgePart1:         {(bridge.EdgePart1 != null ? "present" : "none")}");
                sb.AppendLine($"  EdgePart2:         {(bridge.EdgePart2 != null ? "present" : "none")}");
                sb.AppendLine($"  ProximityTrigger:  {(bridge.ProximityTrigger != null ? "present" : "none")}");
                sb.AppendLine($"  EdgePartOffset:    {bridge.BridgeEdgePartOffset:F2}");

                return sb.ToString();
            });
        }
    }
}
#endif
