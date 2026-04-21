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
        [McpPluginTool("tcc-bulk", Title = "Technie Collider / Bulk Configure")]
        [Description(@"Sets the same property on ALL hulls of a TCC setup at once.
Wraps RigidColliderCreator's SetAllTypes / SetAllMaterials / SetAllAsChild / SetAllAsTrigger.
Provide only the properties you want to change.
hullType: Box, ConvexHull, Sphere, Face, FaceAsBox, Auto, Capsule.
materialName: PhysicsMaterial asset name (use 'none' to clear).
After bulk changes, call tcc-generate to rebuild colliders.")]
        public string Bulk(
            [Description("Name of the GameObject with TCC setup.")] string gameObjectName,
            [Description("Apply this hull type to all hulls.")] string? hullType = null,
            [Description("Apply this PhysicsMaterial to all hulls (use 'none' to clear).")] string? materialName = null,
            [Description("Set isChildCollider on all hulls.")] bool? isChild = null,
            [Description("Set isTrigger on all hulls.")] bool? isTrigger = null,
            [Description("Set maxPlanes on all hulls (1-255).")] int? maxPlanes = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var creator = GetCreator(gameObjectName);
                if (creator.paintingData == null)
                    throw new System.Exception($"'{gameObjectName}' has no PaintingData.");

                int changes = 0;

                if (hullType != null)
                {
                    creator.SetAllTypes(ParseHullType(hullType));
                    changes++;
                }

                if (materialName != null)
                {
                    PhysicsMaterial? mat = null;
                    if (!string.Equals(materialName, "none", System.StringComparison.OrdinalIgnoreCase))
                    {
                        mat = FindPhysicsMaterial(materialName);
                        if (mat == null)
                            throw new System.Exception($"PhysicsMaterial '{materialName}' not found.");
                    }
                    creator.SetAllMaterials(mat!);
                    changes++;
                }

                if (isChild.HasValue)
                {
                    creator.SetAllAsChild(isChild.Value);
                    changes++;
                }

                if (isTrigger.HasValue)
                {
                    creator.SetAllAsTrigger(isTrigger.Value);
                    changes++;
                }

                if (maxPlanes.HasValue)
                {
                    int mp = Mathf.Clamp(maxPlanes.Value, 1, 255);
                    foreach (var h in creator.paintingData.hulls)
                        h.maxPlanes = mp;
                    changes++;
                }

                EditorUtility.SetDirty(creator.paintingData);
                return $"OK: Bulk update on '{gameObjectName}'. {changes} property/properties applied to {creator.paintingData.hulls.Count} hull(s).";
            });
        }
    }
}
