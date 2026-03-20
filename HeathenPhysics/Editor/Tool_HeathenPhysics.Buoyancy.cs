#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Heathen.UnityPhysics;
using UnityEditor;
using UnityEngine;

namespace MCPTools.HeathenPhysics.Editor
{
    public partial class Tool_HeathenPhysics
    {
        [McpPluginTool("hphys-configure-buoyancy", Title = "Heathen Physics / Configure Buoyancy")]
        [Description(@"Adds (if missing) and configures a BuoyantBody component.
BuoyantBody requires a PhysicsData component on the same GameObject.
buoyantMagnitudeY: the upward buoyancy force (default -19.6 = 2x gravity, opposes sinking).
calculationMode: Fast (best perf), Simple (moderate), Detailed (most accurate).
Assign the water surface via activeSurfaceObjectName -- the named GameObject must have a SurfaceTool component.")]
        public string ConfigureBuoyancy(
            [Description("Name of the GameObject (must already have PhysicsData).")] string gameObjectName,
            [Description("Buoyancy upward force Y component (default -19.6 = 2x gravity counter). Typically negative.")] float? buoyantMagnitudeY = null,
            [Description("Buoyancy calculation mode: Fast, Simple, or Detailed.")] string? calculationMode = null,
            [Description("Name of the GameObject with a SurfaceTool component (water surface). Optional.")] string? activeSurfaceObjectName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);

                var data = go.GetComponent<PhysicsData>();
                if (data == null)
                    throw new System.Exception($"'{gameObjectName}' needs a PhysicsData component before adding BuoyantBody. Add PhysicsData first with hphys-configure-physics-data.");

                var buoy = go.GetComponent<BuoyantBody>();
                if (buoy == null)
                    buoy = go.AddComponent<BuoyantBody>();

                if (buoyantMagnitudeY.HasValue)
                    buoy.buoyantMagnitude = new Vector3(buoy.buoyantMagnitude.x, buoyantMagnitudeY.Value, buoy.buoyantMagnitude.z);

                if (calculationMode != null)
                {
                    if (!System.Enum.TryParse<CalculationMode>(calculationMode, true, out var mode))
                        throw new System.Exception($"Invalid calculationMode '{calculationMode}'. Valid: Fast, Simple, Detailed");
                    buoy.calculationMode = mode;
                }

                if (activeSurfaceObjectName != null)
                {
                    var surfaceGO = GameObject.Find(activeSurfaceObjectName);
                    if (surfaceGO == null)
                        throw new System.Exception($"Surface GameObject '{activeSurfaceObjectName}' not found.");
                    var surface = surfaceGO.GetComponent<SurfaceTool>();
                    if (surface == null)
                        throw new System.Exception($"'{activeSurfaceObjectName}' has no SurfaceTool component.");
                    buoy.activeSurface = surface;
                }

                EditorUtility.SetDirty(buoy);

                return $"OK: BuoyantBody on '{gameObjectName}' configured. buoyantMagnitude={FormatVector3(buoy.buoyantMagnitude)} mode={buoy.calculationMode} surface={buoy.activeSurface?.name ?? "none"}";
            });
        }

        [McpPluginTool("hphys-configure-physics-data", Title = "Heathen Physics / Configure PhysicsData")]
        [Description(@"Adds (if missing) and configures a PhysicsData component.
PhysicsData is the base component for all Heathen physics -- add it first.
volume and cross-section values drive buoyancy and drag calculations.
If customHullMeshName is provided, assigns a mesh by that name from MeshFilter children.
Call without arguments to simply add PhysicsData to a GameObject.")]
        public string ConfigurePhysicsData(
            [Description("Name of the GameObject to add PhysicsData to.")] string gameObjectName,
            [Description("Manual volume override (world units cubed). Leave null to auto-calculate.")] float? volume = null,
            [Description("Manual surface area override. Leave null to auto-calculate.")] float? area = null,
            [Description("Cross-section along X axis (for drag). Leave null to auto-calculate.")] float? xCrossSection = null,
            [Description("Cross-section along Y axis (for drag). Leave null to auto-calculate.")] float? yCrossSection = null,
            [Description("Cross-section along Z axis (for drag). Leave null to auto-calculate.")] float? zCrossSection = null,
            [Description("Enable debug visualization.")] bool? debug = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);

                var data = go.GetComponent<PhysicsData>();
                if (data == null)
                    data = go.AddComponent<PhysicsData>();

                if (volume.HasValue) data.volume = volume.Value;
                if (area.HasValue) data.area = area.Value;
                if (xCrossSection.HasValue) data.xCrossSection = xCrossSection.Value;
                if (yCrossSection.HasValue) data.yCrossSection = yCrossSection.Value;
                if (zCrossSection.HasValue) data.zCrossSection = zCrossSection.Value;
                if (debug.HasValue) data.debug = debug.Value;

                EditorUtility.SetDirty(data);

                return $"OK: PhysicsData on '{gameObjectName}' configured. volume={data.volume:F4} area={data.area:F4} x/y/zCrossSection={data.xCrossSection:F4}/{data.yCrossSection:F4}/{data.zCrossSection:F4}";
            });
        }
    }
}
