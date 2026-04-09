#if HAS_TERRAIN25D
#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Kamgam.Terrain25DLib;
using UnityEngine;

namespace MCPTools.Terrain25D.Editor
{
    public partial class Tool_Terrain25D
    {
        [McpPluginTool("terrain25d-query", Title = "2.5D Terrain / Query")]
        [Description(@"Reads the full 2.5D Terrain setup on a GameObject.
Reports MeshGenerator settings (bevel, middle, erosion, snow, mesh properties),
Collider2DGenerator presence, FoliageGenerator presence, and SplineController state.
Use before configuring to understand current state.")]
        public string Query(
            [Description("Name of the root Terrain25D GameObject.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var terrain = GetTerrain(gameObjectName);
                var sb = new StringBuilder();

                sb.AppendLine($"=== Terrain25D: {terrain.name} ===");

                // SplineController
                var sc = terrain.SplineController;
                sb.AppendLine($"\n-- SplineController --");
                if (sc != null)
                    sb.AppendLine($"  SplineCount: {sc.SplineCount}");
                else
                    sb.AppendLine("  (not present)");

                // MeshGenerator
                var mg = terrain.MeshGenerator;
                sb.AppendLine($"\n-- MeshGenerator --");
                if (mg != null)
                {
                    sb.AppendLine($"  Front: BevelWidth={mg.FrontBevelWidth:F1} BevelScale={mg.FrontBevelScale:F1} Divisions={mg.FrontBevelDivisions} Type={mg.FrontBevelType} Closed={mg.FrontClosed}");
                    sb.AppendLine($"  Middle: FrontWidth={mg.FrontMiddleWidth:F1} BackWidth={mg.BackMiddleWidth:F1} FrontProjectUVs={mg.MiddleFrontProjectUVs} UVScale={mg.MiddleUVScale:F1} ZDivisions={mg.MiddleZDivisions}");
                    sb.AppendLine($"  Back: BevelWidth={mg.BackBevelWidth:F1} BevelScale={mg.BackBevelScale:F1} Divisions={mg.BackBevelDivisions} Type={mg.BackBevelType} Closed={mg.BackClosed}");
                    sb.AppendLine($"  Mesh: SmoothNormals={mg.SmoothNormals} CombineMeshes={mg.CombineMeshes} Static={mg.StaticMesh} Shadows={mg.CastShadows} Compression={mg.MeshCompression}");
                    sb.AppendLine($"  Snow: Thickness={mg.SnowThickness:F2} SlopeLimit={mg.SnowSlopeLimit:F1}");
                    sb.AppendLine($"  Erosion: Enabled={mg.Erosion} Strength={mg.ErosionStrength:F2} SegmentLength={mg.ErosionSegmentLength:F2}");
                    sb.AppendLine($"  3D Collider: Add3D={mg.Add3DCollider} Remove2D={mg.Remove2DCollider}");
                    sb.AppendLine($"  Material: {(mg.Material != null ? mg.Material.name : "(none)")}");
                }
                else
                    sb.AppendLine("  (not present)");

                // Collider2DGenerator
                var cg = terrain.Collider2DGenerator;
                sb.AppendLine($"\n-- Collider2DGenerator: {(cg != null ? "present" : "not present")} --");

                // FoliageGenerator
                var fg = terrain.FoliageGenerator;
                sb.AppendLine($"\n-- FoliageGenerator --");
                if (fg != null)
                {
                    sb.AppendLine($"  Range: Start={fg.Start:F0}% End={fg.End:F0}%");
                    sb.AppendLine($"  RayStartOffsetY: {fg.RayStartOffsetY:F1}");
                    sb.AppendLine($"  GeneratorSets: {(fg.GeneratorSets != null ? fg.GeneratorSets.Length : 0)}");
                }
                else
                    sb.AppendLine("  (not present)");

                return sb.ToString();
            });
        }
    }
}
#endif
