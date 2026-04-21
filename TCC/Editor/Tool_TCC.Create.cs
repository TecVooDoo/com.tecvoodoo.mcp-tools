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
        [McpPluginTool("tcc-create", Title = "Technie Collider / Create")]
        [Description(@"Adds Technie Collider Creator setup to a GameObject and creates a PaintingData asset.
Opens the RigidColliderCreator window and calls GenerateAsset to create both PaintingData and HullData assets.
After creation, use tcc-add-hull to add hulls, then tcc-generate to bake colliders.
The GameObject MUST have a MeshFilter (or SkinnedMeshRenderer) with a sharedMesh.")]
        public string Create(
            [Description("Name of the GameObject to set up.")] string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var mesh = GetSourceMesh(go);
                var window = EnsureWindow();

                // Selection coupling — TCC API requires this
                var prevSelection = Selection.activeGameObject;
                try
                {
                    Selection.activeGameObject = go;
                    var paintingData = RigidColliderCreatorWindow.GenerateAsset(go, mesh);
                    if (paintingData == null)
                        throw new System.Exception("GenerateAsset returned null. PaintingData asset creation failed.");

                    return $"OK: TCC setup created on '{gameObjectName}'. PaintingData: {AssetDatabase.GetAssetPath(paintingData)}. Mesh: {mesh.name} ({mesh.triangles.Length / 3} tris). Use tcc-add-hull to add hulls.";
                }
                finally
                {
                    Selection.activeGameObject = prevSelection;
                }
            });
        }

        [McpPluginTool("tcc-add-hull", Title = "Technie Collider / Add Hull")]
        [Description(@"Adds a hull to an existing TCC setup.
hullType: Box, ConvexHull, Sphere, Face, FaceAsBox, Auto (VHACD), Capsule.
isChild: if true, generates collider on a child GameObject (default false = same GO).
isTrigger: if true, generated collider is a trigger.
materialName: optional PhysicsMaterial asset name.
paintAllFaces: if true, selects all triangles for this hull (required for Auto/ConvexHull workflows).
After adding, call tcc-generate to bake colliders.")]
        public string AddHull(
            [Description("Name of the GameObject with TCC setup.")] string gameObjectName,
            [Description("Hull type: Box, ConvexHull, Sphere, Face, FaceAsBox, Auto, Capsule.")] string hullType = "Auto",
            [Description("Generate collider on a child GameObject.")] bool isChild = false,
            [Description("Generated collider is a trigger.")] bool isTrigger = false,
            [Description("Optional PhysicsMaterial asset name.")] string? materialName = null,
            [Description("Select all faces for this hull (required for Auto/ConvexHull).")] bool paintAllFaces = true,
            [Description("Optional name override for this hull.")] string? hullName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var creator = GetCreator(gameObjectName);
                if (creator.paintingData == null)
                    throw new System.Exception($"'{gameObjectName}' has no PaintingData. Run tcc-create first.");

                var window = EnsureWindow();
                var prevSelection = Selection.activeGameObject;
                try
                {
                    Selection.activeGameObject = creator.gameObject;

                    var type = ParseHullType(hullType);
                    var mat = FindPhysicsMaterial(materialName);

                    var hull = creator.paintingData.AddHull(type, mat, isChild, isTrigger);
                    if (!string.IsNullOrEmpty(hullName))
                        hull.name = hullName!;

                    creator.paintingData.activeHull = creator.paintingData.hulls.Count - 1;

                    if (paintAllFaces)
                    {
                        // Need to refresh window state to point at this hull
                        try { window.SceneManipulator.PaintAllFaces(); }
                        catch (System.Exception ex)
                        {
                            return $"OK: Hull '{hull.name}' added (type={type}, isChild={isChild}, isTrigger={isTrigger}), but PaintAllFaces failed: {ex.Message}. You may need to paint via the Scene window.";
                        }
                    }

                    EditorUtility.SetDirty(creator.paintingData);
                    return $"OK: Hull '{hull.name}' added on '{gameObjectName}'. type={type} isChild={isChild} isTrigger={isTrigger} material={(mat != null ? mat.name : "none")} faces={hull.NumSelectedTriangles} totalHulls={creator.paintingData.hulls.Count}";
                }
                finally
                {
                    Selection.activeGameObject = prevSelection;
                }
            });
        }
    }
}
