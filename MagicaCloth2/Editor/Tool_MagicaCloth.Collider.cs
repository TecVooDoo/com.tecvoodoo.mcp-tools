#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using MagicaCloth2;
using UnityEditor;
using UnityEngine;

namespace MCPTools.MagicaCloth2.Editor
{
    public partial class Tool_MagicaCloth
    {
        [McpPluginTool("magica-add-sphere-collider", Title = "Magica Cloth / Add Sphere Collider")]
        [Description(@"Adds a MagicaSphereCollider to a GameObject (typically a bone).
Magica colliders prevent cloth from penetrating character body parts.
Common usage: add to hand, head, chest, or leg bones.")]
        public AddColliderResponse AddSphereCollider(
            [Description("Reference to the target GameObject (usually a bone).")]
            GameObjectRef targetRef,
            [Description("Radius of the sphere collider. Default 0.1.")]
            float radius = 0.1f,
            [Description("Local center offset. Default (0,0,0).")]
            Vector3? center = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var col = go.GetComponent<MagicaSphereCollider>();
                if (col == null)
                    col = Undo.AddComponent<MagicaSphereCollider>(go);

                col.SetSize(radius);
                if (center.HasValue) col.center = center.Value;

                EditorUtility.SetDirty(go);

                return new AddColliderResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    colliderType = "Sphere",
                    size = FormatVector3(col.GetSize())
                };
            });
        }

        [McpPluginTool("magica-add-capsule-collider", Title = "Magica Cloth / Add Capsule Collider")]
        [Description(@"Adds a MagicaCapsuleCollider to a GameObject (typically a bone).
Capsule colliders are best for limbs (arms, legs, torso).
Direction sets the capsule axis: X, Y, or Z.")]
        public AddColliderResponse AddCapsuleCollider(
            [Description("Reference to the target GameObject (usually a bone).")]
            GameObjectRef targetRef,
            [Description("Start radius of the capsule. Default 0.05.")]
            float startRadius = 0.05f,
            [Description("End radius of the capsule. Default 0.05.")]
            float endRadius = 0.05f,
            [Description("Length of the capsule. Default 0.2.")]
            float length = 0.2f,
            [Description("Capsule direction axis: 'X', 'Y', or 'Z'. Default 'X'.")]
            string direction = "X",
            [Description("Local center offset. Default (0,0,0).")]
            Vector3? center = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var col = go.GetComponent<MagicaCapsuleCollider>();
                if (col == null)
                    col = Undo.AddComponent<MagicaCapsuleCollider>(go);

                col.SetSize(startRadius, endRadius, length);
                if (center.HasValue) col.center = center.Value;

                switch (direction.ToUpper())
                {
                    case "Y": col.direction = MagicaCapsuleCollider.Direction.Y; break;
                    case "Z": col.direction = MagicaCapsuleCollider.Direction.Z; break;
                    default:  col.direction = MagicaCapsuleCollider.Direction.X; break;
                }

                EditorUtility.SetDirty(go);

                return new AddColliderResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    colliderType = "Capsule",
                    size = FormatVector3(col.GetSize())
                };
            });
        }

        public class AddColliderResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Collider type added")] public string colliderType = "";
            [Description("Collider size")] public string size = "";
        }
    }
}
