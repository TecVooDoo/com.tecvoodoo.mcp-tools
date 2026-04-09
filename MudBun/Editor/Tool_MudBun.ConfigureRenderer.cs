#if HAS_MUDBUN
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.MudBun.Editor
{
    public partial class Tool_MudBun
    {
        [McpPluginTool("mudbun-configure-renderer", Title = "MudBun / Configure Renderer")]
        [Description(@"Configures a MudRenderer component on a GameObject. All parameters optional except gameObjectName.
renderMode: FlatMesh, SmoothMesh, CircleSplats, QuadSplats, Decal.
meshingMode: MarchingCubes, DualQuads, SurfaceNets, DualContouring.
Calls MarkDirty after changes to trigger mesh regeneration.")]
        public string ConfigureRenderer(
            [Description("Name of the GameObject with the MudRenderer component.")]
            string gameObjectName,
            [Description("Render mode: FlatMesh, SmoothMesh, CircleSplats, QuadSplats, Decal.")]
            string? renderMode = null,
            [Description("Meshing mode: MarchingCubes, DualQuads, SurfaceNets, DualContouring.")]
            string? meshingMode = null,
            [Description("Voxel density for mesh generation. Higher = finer detail.")]
            float? voxelDensity = null,
            [Description("Maximum voxels in thousands. Controls memory/quality tradeoff.")]
            int? maxVoxelsK = null,
            [Description("Master color as hex string, e.g. '#FF0000'.")]
            string? masterColorHex = null,
            [Description("Master metallic value [0-1].")]
            float? masterMetallic = null,
            [Description("Master smoothness value [0-1].")]
            float? masterSmoothness = null,
            [Description("Surface shift offset. Positive = expand, negative = shrink.")]
            float? surfaceShift = null,
            [Description("Enable 2D mode for the renderer.")]
            bool? enable2dMode = null,
            [Description("Whether the renderer casts shadows.")]
            bool? castShadows = null,
            [Description("Whether the renderer receives shadows.")]
            bool? receiveShadows = null,
            [Description("Splat size for splat render modes (CircleSplats, QuadSplats).")]
            float? splatSize = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var renderer = GetRenderer(gameObjectName);
                var changes = new System.Text.StringBuilder();

                if (renderMode != null)
                {
                    SetEnum(renderer, "RenderMode", renderMode);
                    changes.Append($" RenderMode={renderMode}");
                }
                if (meshingMode != null)
                {
                    SetEnum(renderer, "MeshingMode", meshingMode);
                    changes.Append($" MeshingMode={meshingMode}");
                }
                if (voxelDensity.HasValue)
                {
                    Set(renderer, "VoxelDensity", voxelDensity.Value);
                    changes.Append($" VoxelDensity={voxelDensity.Value:F2}");
                }
                if (maxVoxelsK.HasValue)
                {
                    Set(renderer, "MaxVoxelsK", maxVoxelsK.Value);
                    changes.Append($" MaxVoxelsK={maxVoxelsK.Value}");
                }
                if (masterColorHex != null)
                {
                    var color = ParseColor(masterColorHex);
                    SetColor(renderer, "MasterColor", color);
                    changes.Append($" MasterColor={masterColorHex}");
                }
                if (masterMetallic.HasValue)
                {
                    Set(renderer, "MasterMetallic", Mathf.Clamp01(masterMetallic.Value));
                    changes.Append($" MasterMetallic={masterMetallic.Value:F2}");
                }
                if (masterSmoothness.HasValue)
                {
                    Set(renderer, "MasterSmoothness", Mathf.Clamp01(masterSmoothness.Value));
                    changes.Append($" MasterSmoothness={masterSmoothness.Value:F2}");
                }
                if (surfaceShift.HasValue)
                {
                    Set(renderer, "SurfaceShift", surfaceShift.Value);
                    changes.Append($" SurfaceShift={surfaceShift.Value:F3}");
                }
                if (enable2dMode.HasValue)
                {
                    Set(renderer, "Enable2dMode", enable2dMode.Value);
                    changes.Append($" Enable2dMode={enable2dMode.Value}");
                }
                if (castShadows.HasValue)
                {
                    Set(renderer, "CastShadows", castShadows.Value);
                    changes.Append($" CastShadows={castShadows.Value}");
                }
                if (receiveShadows.HasValue)
                {
                    Set(renderer, "ReceiveShadows", receiveShadows.Value);
                    changes.Append($" ReceiveShadows={receiveShadows.Value}");
                }
                if (splatSize.HasValue)
                {
                    Set(renderer, "SplatSize", splatSize.Value);
                    changes.Append($" SplatSize={splatSize.Value:F3}");
                }

                MarkDirty(renderer);

                return changes.Length > 0
                    ? $"OK: MudRenderer '{gameObjectName}' updated:{changes}"
                    : $"OK: MudRenderer '{gameObjectName}' — no changes specified.";
            });
        }
    }
}
#endif
