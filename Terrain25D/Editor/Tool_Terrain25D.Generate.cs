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
        [McpPluginTool("terrain25d-generate", Title = "2.5D Terrain / Generate Mesh")]
        [Description(@"Triggers mesh generation on a Terrain25D's MeshGenerator.
Also optionally generates 2D colliders and foliage.
Call after configuring mesh parameters with 'terrain25d-configure-mesh'.")]
        public string Generate(
            [Description("Name of the root Terrain25D GameObject.")] string gameObjectName,
            [Description("Also generate 2D polygon colliders. Default: true.")] bool? generateColliders = null,
            [Description("Also generate foliage (if FoliageGenerator present). Default: false.")] bool? generateFoliage = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var terrain = GetTerrain(gameObjectName);
                var mg = terrain.MeshGenerator;
                if (mg == null)
                    throw new System.Exception($"'{gameObjectName}' has no MeshGenerator child. Add one first.");

                var sc = terrain.SplineController;
                if (sc == null)
                    throw new System.Exception($"'{gameObjectName}' has no SplineController child. Add one first.");

                if (sc.SplineCount == 0)
                    throw new System.Exception($"SplineController on '{gameObjectName}' has no splines. Add spline points first.");

                // Generate mesh
                mg.GenerateMesh();
                EditorUtility.SetDirty(mg);

                int result = 1;
                string details = "mesh";

                // Generate colliders
                bool doColliders = generateColliders ?? true;
                if (doColliders)
                {
                    var cg = terrain.Collider2DGenerator;
                    if (cg != null)
                    {
                        cg.GenerateColliders();
                        EditorUtility.SetDirty(cg);
                        result++;
                        details += "+colliders";
                    }
                }

                // Generate foliage
                bool doFoliage = generateFoliage ?? false;
                if (doFoliage)
                {
                    var fg = terrain.FoliageGenerator;
                    if (fg != null)
                    {
                        // FoliageGenerator.Generate() is editor-only
                        var method = fg.GetType().GetMethod("Generate",
                            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (method != null)
                        {
                            method.Invoke(fg, null);
                            EditorUtility.SetDirty(fg);
                            result++;
                            details += "+foliage";
                        }
                    }
                }

                return $"OK: Generated {details} on '{gameObjectName}'. Splines={sc.SplineCount}";
            });
        }
    }
}
#endif
