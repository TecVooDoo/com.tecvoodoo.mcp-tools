#if HAS_TERRAIN25D
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Kamgam.Terrain25DLib;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Terrain25D.Editor
{
    public partial class Tool_Terrain25D
    {
        [McpPluginTool("terrain25d-configure-mesh", Title = "2.5D Terrain / Configure Mesh Generator")]
        [Description(@"Configures the MeshGenerator on a Terrain25D GameObject.
Only provided parameters are changed; others are left as-is.
Front/Back bevel controls the 3D depth shape. Middle controls the flat connecting section.
Erosion adds organic edge variation. Snow adds vertex offset on top surfaces.
Use 'terrain25d-query' first to see current values.")]
        public string ConfigureMesh(
            [Description("Name of the root Terrain25D GameObject.")] string gameObjectName,
            [Description("Front bevel thickness along Z [0.2-30].")] float? frontBevelWidth = null,
            [Description("Front bevel inward shrink [0.1-30].")] float? frontBevelScale = null,
            [Description("Front bevel subdivisions [0-10]. 0 = no bevel.")] int? frontBevelDivisions = null,
            [Description("Front bevel type: Circular or Linear.")] string? frontBevelType = null,
            [Description("Close the front face.")] bool? frontClosed = null,
            [Description("Front middle section width.")] float? frontMiddleWidth = null,
            [Description("Back middle section width.")] float? backMiddleWidth = null,
            [Description("Front-project middle UVs (decal-like).")] bool? middleFrontProjectUVs = null,
            [Description("Middle UV scale if not front-projected.")] float? middleUVScale = null,
            [Description("Middle Z divisions [1+].")] int? middleZDivisions = null,
            [Description("Back bevel thickness along Z [0.2-30].")] float? backBevelWidth = null,
            [Description("Back bevel inward shrink [0.2-30].")] float? backBevelScale = null,
            [Description("Back bevel subdivisions [0-10]. 0 = no bevel.")] int? backBevelDivisions = null,
            [Description("Back bevel type: Circular or Linear.")] string? backBevelType = null,
            [Description("Close the back face.")] bool? backClosed = null,
            [Description("Average normals for smoother surface.")] bool? smoothNormals = null,
            [Description("Combine into one mesh.")] bool? combineMeshes = null,
            [Description("Mark mesh as static.")] bool? staticMesh = null,
            [Description("Mesh casts shadows.")] bool? castShadows = null,
            [Description("Enable erosion (organic edge variation).")] bool? erosion = null,
            [Description("Erosion strength [0.5-5].")] float? erosionStrength = null,
            [Description("Erosion segment length [0.2-1].")] float? erosionSegmentLength = null,
            [Description("Snow vertex offset thickness [0-2].")] float? snowThickness = null,
            [Description("Snow slope limit angle [0-90].")] float? snowSlopeLimit = null,
            [Description("Add 3D MeshCollider after generation.")] bool? add3DCollider = null,
            [Description("Remove 2D collider after generation.")] bool? remove2DCollider = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var mg = GetMeshGenerator(gameObjectName);

                // Front
                if (frontBevelWidth.HasValue) mg.FrontBevelWidth = Mathf.Clamp(frontBevelWidth.Value, 0.2f, 30f);
                if (frontBevelScale.HasValue) mg.FrontBevelScale = Mathf.Clamp(frontBevelScale.Value, 0.1f, 30f);
                if (frontBevelDivisions.HasValue) mg.FrontBevelDivisions = Mathf.Clamp(frontBevelDivisions.Value, 0, 10);
                if (frontBevelType != null)
                {
                    if (!System.Enum.TryParse<MeshGenerator.BevelType>(frontBevelType, true, out var ft))
                        throw new System.Exception($"Invalid frontBevelType '{frontBevelType}'. Valid: Circular, Linear");
                    mg.FrontBevelType = ft;
                }
                if (frontClosed.HasValue) mg.FrontClosed = frontClosed.Value;

                // Middle
                if (frontMiddleWidth.HasValue) mg.FrontMiddleWidth = frontMiddleWidth.Value;
                if (backMiddleWidth.HasValue) mg.BackMiddleWidth = backMiddleWidth.Value;
                if (middleFrontProjectUVs.HasValue) mg.MiddleFrontProjectUVs = middleFrontProjectUVs.Value;
                if (middleUVScale.HasValue) mg.MiddleUVScale = middleUVScale.Value;
                if (middleZDivisions.HasValue) mg.MiddleZDivisions = Mathf.Max(1, middleZDivisions.Value);

                // Back
                if (backBevelWidth.HasValue) mg.BackBevelWidth = Mathf.Clamp(backBevelWidth.Value, 0.2f, 30f);
                if (backBevelScale.HasValue) mg.BackBevelScale = Mathf.Clamp(backBevelScale.Value, 0.2f, 30f);
                if (backBevelDivisions.HasValue) mg.BackBevelDivisions = Mathf.Clamp(backBevelDivisions.Value, 0, 10);
                if (backBevelType != null)
                {
                    if (!System.Enum.TryParse<MeshGenerator.BevelType>(backBevelType, true, out var bt))
                        throw new System.Exception($"Invalid backBevelType '{backBevelType}'. Valid: Circular, Linear");
                    mg.BackBevelType = bt;
                }
                if (backClosed.HasValue) mg.BackClosed = backClosed.Value;

                // Mesh Generation
                if (smoothNormals.HasValue) mg.SmoothNormals = smoothNormals.Value;
                if (combineMeshes.HasValue) mg.CombineMeshes = combineMeshes.Value;
                if (staticMesh.HasValue) mg.StaticMesh = staticMesh.Value;
                if (castShadows.HasValue) mg.CastShadows = castShadows.Value;

                // Erosion
                if (erosion.HasValue) mg.Erosion = erosion.Value;
                if (erosionStrength.HasValue) mg.ErosionStrength = Mathf.Clamp(erosionStrength.Value, 0.5f, 5f);
                if (erosionSegmentLength.HasValue) mg.ErosionSegmentLength = Mathf.Clamp(erosionSegmentLength.Value, 0.2f, 1f);

                // Snow
                if (snowThickness.HasValue) mg.SnowThickness = Mathf.Clamp(snowThickness.Value, 0f, 2f);
                if (snowSlopeLimit.HasValue) mg.SnowSlopeLimit = Mathf.Clamp(snowSlopeLimit.Value, 0f, 90f);

                // 3D Collider
                if (add3DCollider.HasValue) mg.Add3DCollider = add3DCollider.Value;
                if (remove2DCollider.HasValue) mg.Remove2DCollider = remove2DCollider.Value;

                EditorUtility.SetDirty(mg);

                return $"OK: MeshGenerator on '{gameObjectName}' configured. Front={mg.FrontBevelWidth:F1}/{mg.FrontBevelDivisions}div Back={mg.BackBevelWidth:F1}/{mg.BackBevelDivisions}div Erosion={mg.Erosion} Snow={mg.SnowThickness:F2}";
            });
        }
    }
}
#endif
