#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Technie.PhysicsCreator;
using Technie.PhysicsCreator.Rigid;
using UnityEditor;
using UnityEngine;

namespace MCPTools.TCC.Editor
{
    public partial class Tool_TCC
    {
        [McpPluginTool("tcc-generate", Title = "Technie Collider / Generate")]
        [Description(@"Triggers GenerateColliders on a TCC setup. Bakes hulls into actual Collider components.
This kicks off a coroutine — for Auto hulls, VHACD runs in background and may take time (Low: ~1s, High: ~10s, Placebo: ~60s).
Use tcc-query afterwards to check IsGeneratingColliders status and final collider list.
Returns immediately; the generation completes asynchronously.")]
        public string Generate(
            [Description("Name of the GameObject with TCC setup.")] string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var creator = GetCreator(gameObjectName);
                if (creator.paintingData == null)
                    throw new System.Exception($"'{gameObjectName}' has no PaintingData.");
                if (creator.paintingData.hulls.Count == 0)
                    throw new System.Exception($"'{gameObjectName}' has no hulls. Use tcc-add-hull first.");

                var window = EnsureWindow();
                if (window.IsGeneratingColliders)
                    return $"WARN: '{gameObjectName}' is already generating colliders. Wait and call tcc-query to check status.";

                var prevSelection = Selection.activeGameObject;
                try
                {
                    Selection.activeGameObject = creator.gameObject;
                    window.GenerateColliders();
                    return $"OK: GenerateColliders triggered on '{gameObjectName}'. {creator.paintingData.hulls.Count} hull(s) queued. Preset={creator.paintingData.autoHullPreset}. Call tcc-query to check status.";
                }
                finally
                {
                    Selection.activeGameObject = prevSelection;
                }
            });
        }

        [McpPluginTool("tcc-configure-vhacd", Title = "Technie Collider / Configure VHACD")]
        [Description(@"Sets VHACD (Auto hull) parameters on a TCC PaintingData.
preset: Low (fast, rough), Medium (balanced, default), High (slow, precise), Placebo (very slow), Custom.
When preset is Custom, the other parameters take effect; otherwise preset values override them.")]
        public string ConfigureVhacd(
            [Description("Name of the GameObject with TCC setup.")] string gameObjectName,
            [Description("Preset: Low, Medium, High, Placebo, Custom.")] string? preset = null,
            [Description("Voxel resolution (10000-64000000).")] uint? resolution = null,
            [Description("Concavity threshold (0-1, lower = more decomposition).")] float? concavity = null,
            [Description("Plane downsampling (1-16).")] uint? planeDownsampling = null,
            [Description("Convex hull downsampling (1-16).")] uint? convexhullDownsampling = null,
            [Description("Min volume per hull (0-0.01).")] float? minVolumePerCH = null,
            [Description("Max convex hulls.")] uint? maxConvexHulls = null,
            [Description("Alpha bias (0-1).")] float? alpha = null,
            [Description("Beta bias (0-1).")] float? beta = null,
            [Description("PCA enable (0 or 1).")] uint? pca = null,
            [Description("Mode: 0=voxel, 1=tetrahedron.")] uint? mode = null,
            [Description("Max vertices per hull (4-1024).")] uint? maxNumVerticesPerCH = null,
            [Description("Project hull vertices to original mesh.")] bool? projectHullVertices = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var creator = GetCreator(gameObjectName);
                if (creator.paintingData == null)
                    throw new System.Exception($"'{gameObjectName}' has no PaintingData.");

                if (preset != null)
                    creator.paintingData.autoHullPreset = ParsePreset(preset);

                var p = creator.paintingData.vhacdParams;
                if (resolution.HasValue) p.resolution = resolution.Value;
                if (concavity.HasValue) p.concavity = concavity.Value;
                if (planeDownsampling.HasValue) p.planeDownsampling = planeDownsampling.Value;
                if (convexhullDownsampling.HasValue) p.convexhullDownsampling = convexhullDownsampling.Value;
                if (minVolumePerCH.HasValue) p.minVolumePerCH = minVolumePerCH.Value;
                if (maxConvexHulls.HasValue) p.maxConvexHulls = maxConvexHulls.Value;
                if (alpha.HasValue) p.alpha = alpha.Value;
                if (beta.HasValue) p.beta = beta.Value;
                if (pca.HasValue) p.pca = pca.Value;
                if (mode.HasValue) p.mode = mode.Value;
                if (maxNumVerticesPerCH.HasValue) p.maxNumVerticesPerCH = maxNumVerticesPerCH.Value;
                if (projectHullVertices.HasValue) p.projectHullVertices = projectHullVertices.Value;
                creator.paintingData.vhacdParams = p;

                EditorUtility.SetDirty(creator.paintingData);
                return $"OK: VHACD config updated on '{gameObjectName}'. preset={creator.paintingData.autoHullPreset} resolution={p.resolution} concavity={p.concavity} maxHulls={p.maxConvexHulls}";
            });
        }

        [McpPluginTool("tcc-delete-generated", Title = "Technie Collider / Delete Generated")]
        [Description(@"Removes all generated colliders and auto-child GameObjects from a TCC setup.
Leaves PaintingData and hull definitions intact — you can re-run tcc-generate to rebuild.")]
        public string DeleteGenerated(
            [Description("Name of the GameObject with TCC setup.")] string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var creator = GetCreator(gameObjectName);
                var window = EnsureWindow();
                var prevSelection = Selection.activeGameObject;
                try
                {
                    Selection.activeGameObject = creator.gameObject;
                    window.DeleteGenerated();
                    return $"OK: Generated colliders deleted from '{gameObjectName}'. PaintingData preserved.";
                }
                finally
                {
                    Selection.activeGameObject = prevSelection;
                }
            });
        }
    }
}
