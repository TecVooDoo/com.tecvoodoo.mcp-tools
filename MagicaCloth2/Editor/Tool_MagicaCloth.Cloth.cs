#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
#if MCP_HAS_AIGD
using AIGD;
#else
using com.IvanMurzak.Unity.MCP.Runtime.Data;
#endif
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using MagicaCloth2;
using UnityEditor;
using UnityEngine;

namespace MCPTools.MagicaCloth2.Editor
{
    public partial class Tool_MagicaCloth
    {
        [McpPluginTool("magica-add-bone-cloth", Title = "Magica Cloth / Add Bone Cloth")]
        [Description(@"Adds a MagicaCloth component configured as BoneCloth to a GameObject.
BoneCloth simulates cloth using bone transforms — ideal for capes, hair, tails, skirts.
Specify root bones that define the cloth chain.")]
        public AddClothResponse AddBoneCloth(
            [Description("Reference to the target GameObject.")]
            GameObjectRef targetRef,
            [Description("Names of root bone transforms (children of the target or its hierarchy). At least one required.")]
            string[] rootBoneNames,
            [Description("Gravity strength (0-10). Default 5.")]
            float gravity = 5f,
            [Description("Damping (air resistance, 0-1). Default 0.05.")]
            float damping = 0.05f,
            [Description("Particle radius for collision. Default 0.02.")]
            float radius = 0.02f,
            [Description("Blend weight between simulation and animation pose (0-1). Default 1 (full simulation).")]
            float blendWeight = 1f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var cloth = go.GetComponent<global::MagicaCloth2.MagicaCloth>();
                if (cloth == null)
                    cloth = Undo.AddComponent<global::MagicaCloth2.MagicaCloth>(go);

                var sd = cloth.SerializeData;
                sd.clothType = ClothProcess.ClothType.BoneCloth;
                sd.gravity = Mathf.Clamp(gravity, 0f, 10f);
                sd.blendWeight = Mathf.Clamp01(blendWeight);

                // Find root bones
                var allTransforms = go.GetComponentsInChildren<Transform>(true);
                sd.rootBones.Clear();
                var foundBones = new List<string>();
                foreach (var boneName in rootBoneNames)
                {
                    var bone = allTransforms.FirstOrDefault(t => t.name == boneName);
                    if (bone != null)
                    {
                        sd.rootBones.Add(bone);
                        foundBones.Add(boneName);
                    }
                }

                if (sd.rootBones.Count == 0)
                    throw new System.Exception($"No root bones found matching: {string.Join(", ", rootBoneNames)}");

                EditorUtility.SetDirty(go);

                return new AddClothResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    clothType = "BoneCloth",
                    rootBonesFound = string.Join(", ", foundBones),
                    gravity = sd.gravity
                };
            });
        }

        [McpPluginTool("magica-add-mesh-cloth", Title = "Magica Cloth / Add Mesh Cloth")]
        [Description(@"Adds a MagicaCloth component configured as MeshCloth to a GameObject.
MeshCloth simulates cloth on mesh vertices — ideal for flags, curtains, tablecloths.
The GameObject should have a Renderer (MeshRenderer or SkinnedMeshRenderer).")]
        public AddClothResponse AddMeshCloth(
            [Description("Reference to the target GameObject with a Renderer.")]
            GameObjectRef targetRef,
            [Description("Gravity strength (0-10). Default 5.")]
            float gravity = 5f,
            [Description("Damping (air resistance, 0-1). Default 0.05.")]
            float damping = 0.05f,
            [Description("Particle radius for collision. Default 0.02.")]
            float radius = 0.02f,
            [Description("Blend weight (0-1). Default 1.")]
            float blendWeight = 1f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var renderer = go.GetComponent<Renderer>();
                if (renderer == null)
                    throw new System.Exception($"'{go.name}' has no Renderer component.");

                var cloth = go.GetComponent<global::MagicaCloth2.MagicaCloth>();
                if (cloth == null)
                    cloth = Undo.AddComponent<global::MagicaCloth2.MagicaCloth>(go);

                var sd = cloth.SerializeData;
                sd.clothType = ClothProcess.ClothType.MeshCloth;
                sd.gravity = Mathf.Clamp(gravity, 0f, 10f);
                sd.blendWeight = Mathf.Clamp01(blendWeight);
                sd.sourceRenderers.Clear();
                sd.sourceRenderers.Add(renderer);

                EditorUtility.SetDirty(go);

                return new AddClothResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    clothType = "MeshCloth",
                    rootBonesFound = renderer.GetType().Name,
                    gravity = sd.gravity
                };
            });
        }

        [McpPluginTool("magica-list-cloth", Title = "Magica Cloth / List Cloth in Scene")]
        [Description("Lists all MagicaCloth components in the current scene with their type and configuration.")]
        public ListClothResponse ListCloth()
        {
            return MainThread.Instance.Run(() =>
            {
                var cloths = Object.FindObjectsByType<global::MagicaCloth2.MagicaCloth>(FindObjectsSortMode.None);
                var lines = cloths.Select(c =>
                {
                    var sd = c.SerializeData;
                    return $"[{c.gameObject.GetInstanceID()}] {c.gameObject.name} " +
                           $"(Type:{sd.clothType}, Gravity:{sd.gravity:F1}, Blend:{sd.blendWeight:F2})";
                }).ToArray();

                return new ListClothResponse
                {
                    count = cloths.Length,
                    details = string.Join("\n", lines)
                };
            });
        }

        public class AddClothResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Cloth type configured")] public string clothType = "";
            [Description("Root bones or renderer found")] public string rootBonesFound = "";
            [Description("Gravity setting")] public float gravity;
        }

        public class ListClothResponse
        {
            [Description("Number of MagicaCloth components found")] public int count;
            [Description("Details of each cloth component")] public string details = "";
        }
    }
}
