#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Heathen.UnityPhysics;
using UnityEngine;

namespace MCPTools.HeathenPhysics.Editor
{
    public partial class Tool_HeathenPhysics
    {
        [McpPluginTool("hphys-query", Title = "Heathen Physics / Query Components")]
        [Description(@"Lists all Heathen Unity Physics components on a GameObject and their key settings.
Reports PhysicsData (hull, volume, mass), BuoyantBody (magnitude, mode, submergedRatio),
ForceEffectField (strength, radius, global), and ForceEffectReceiver (linear, angular, sensitivity).
Use this before configuring to understand the current physics setup.")]
        public string QueryPhysics(
            [Description("Name of the GameObject to inspect.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var sb = new StringBuilder();
                sb.AppendLine($"=== Heathen Physics: {go.name} ===");

                var data = go.GetComponent<PhysicsData>();
                if (data != null)
                {
                    sb.AppendLine($"\n-- PhysicsData --");
                    sb.AppendLine($"  volume:       {data.volume:F4}");
                    sb.AppendLine($"  area:         {data.area:F4}");
                    sb.AppendLine($"  xCrossSection:{data.xCrossSection:F4}");
                    sb.AppendLine($"  yCrossSection:{data.yCrossSection:F4}");
                    sb.AppendLine($"  zCrossSection:{data.zCrossSection:F4}");
                    sb.AppendLine($"  Mass:         {data.Mass:F3}  (from Rigidbody)");
                    sb.AppendLine($"  Density:      {data.Density:F4}");
                    sb.AppendLine($"  LinearSpeed:  {data.LinearSpeed:F3}");
                    sb.AppendLine($"  hasCustomHull:{data.customHullMesh != null}");
                }
                else
                {
                    sb.AppendLine("\n-- PhysicsData: NONE --");
                }

                var buoy = go.GetComponent<BuoyantBody>();
                if (buoy != null)
                {
                    sb.AppendLine($"\n-- BuoyantBody --");
                    sb.AppendLine($"  buoyantMagnitude: {FormatVector3(buoy.buoyantMagnitude)}");
                    sb.AppendLine($"  calculationMode:  {buoy.calculationMode}");
                    sb.AppendLine($"  submergedRatio:   {buoy.submergedRatio:F3}");
                    sb.AppendLine($"  IsFloating:       {buoy.IsFloating}");
                    sb.AppendLine($"  activeSurface:    {(buoy.activeSurface != null ? buoy.activeSurface.name : "none")}");
                }
                else
                {
                    sb.AppendLine("\n-- BuoyantBody: NONE --");
                }

                var field = go.GetComponent<ForceEffectField>();
                if (field != null)
                {
                    sb.AppendLine($"\n-- ForceEffectField --");
                    sb.AppendLine($"  strength:    {field.strength:F3}");
                    sb.AppendLine($"  radius:      {field.radius:F3}");
                    sb.AppendLine($"  IsGlobal:    {field.IsGlobal}");
                    sb.AppendLine($"  effects:     {field.forceEffects.Count} ForceEffect entries");
                }
                else
                {
                    sb.AppendLine("\n-- ForceEffectField: NONE --");
                }

                var receiver = go.GetComponent<ForceEffectReceiver>();
                if (receiver != null)
                {
                    sb.AppendLine($"\n-- ForceEffectReceiver --");
                    sb.AppendLine($"  useLinear:   {receiver.useLinear}");
                    sb.AppendLine($"  useAngular:  {receiver.useAngular}");
                    sb.AppendLine($"  sensitivity: {receiver.sensitivity:F3}");
                    sb.AppendLine($"  effectors:   {receiver.Effectors.Count}");
                }
                else
                {
                    sb.AppendLine("\n-- ForceEffectReceiver: NONE --");
                }

                return sb.ToString();
            });
        }
    }
}
