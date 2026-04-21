#nullable enable
using System.ComponentModel;
using System.Text;
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
        [McpPluginTool("tcc-query", Title = "Technie Collider / Query")]
        [Description(@"Reports TCC setup state on a GameObject.
Lists hulls (name, type, isChild, isTrigger, material, selected triangles, generated collider name),
PaintingData asset path, VHACD preset, custom params, and IsGeneratingColliders status.")]
        public string Query(
            [Description("Name of the GameObject with TCC setup.")] string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var creator = GetCreator(gameObjectName);
                var sb = new StringBuilder();

                sb.AppendLine($"RigidColliderCreator on '{gameObjectName}':");

                if (creator.paintingData == null)
                {
                    sb.AppendLine("  PaintingData: <none> — call tcc-create");
                    return sb.ToString();
                }

                string assetPath = AssetDatabase.GetAssetPath(creator.paintingData);
                sb.AppendLine($"  PaintingData: {assetPath}");
                sb.AppendLine($"  AutoHullPreset: {creator.paintingData.autoHullPreset}");

                if (creator.paintingData.autoHullPreset == AutoHullPreset.Custom)
                {
                    var p = creator.paintingData.vhacdParams;
                    sb.AppendLine($"  VHACD: resolution={p.resolution} concavity={p.concavity:F4} maxHulls={p.maxConvexHulls} alpha={p.alpha:F2} beta={p.beta:F2}");
                }

                sb.AppendLine($"  ActiveHullIndex: {creator.paintingData.activeHull}");

                // Window status
                var window = RigidColliderCreatorWindow.instance;
                if (window != null)
                    sb.AppendLine($"  IsGeneratingColliders: {window.IsGeneratingColliders}");
                else
                    sb.AppendLine($"  IsGeneratingColliders: <window not open>");

                // Hulls
                sb.AppendLine($"  Hulls ({creator.paintingData.hulls.Count}):");
                for (int i = 0; i < creator.paintingData.hulls.Count; i++)
                {
                    var h = creator.paintingData.hulls[i];
                    string matName = h.material != null ? h.material.name : "none";
                    sb.AppendLine($"    [{i}] '{h.name}' type={h.type} isChild={h.isChildCollider} isTrigger={h.isTrigger} material={matName} faces={h.NumSelectedTriangles} maxPlanes={h.maxPlanes}");
                    if (h.autoMeshes != null && h.autoMeshes.Length > 0)
                        sb.AppendLine($"        autoMeshes: {h.autoMeshes.Length} sub-hull(s) generated");
                }

                // Generated child colliders
                var children = creator.GetComponentsInChildren<RigidColliderCreatorChild>(true);
                if (children.Length > 0)
                {
                    sb.AppendLine($"  GeneratedChildren ({children.Length}):");
                    for (int i = 0; i < Mathf.Min(children.Length, 10); i++)
                    {
                        var col = children[i].GetComponent<Collider>();
                        sb.AppendLine($"    {children[i].gameObject.name} — {(col != null ? col.GetType().Name : "no collider")} {(children[i].isAutoHull ? "(auto)" : "")}");
                    }
                    if (children.Length > 10) sb.AppendLine($"    ... +{children.Length - 10} more");
                }

                // Same-GO colliders
                var ownColliders = creator.GetComponents<Collider>();
                if (ownColliders.Length > 0)
                {
                    sb.AppendLine($"  Self Colliders ({ownColliders.Length}):");
                    foreach (var c in ownColliders)
                        sb.AppendLine($"    {c.GetType().Name} isTrigger={c.isTrigger}");
                }

                return sb.ToString();
            });
        }
    }
}
