#if HAS_FINALIK
#nullable enable
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using RootMotion.FinalIK;
using UnityEditor;
using UnityEngine;

namespace MCPTools.FinalIK.Editor
{
    public partial class Tool_FinalIK
    {
        [McpPluginTool("finalik-add-lookat", Title = "Final IK / Add LookAt IK")]
        [Description(@"Adds a LookAtIK component to a character. Makes the head and optionally spine bones
rotate to look at a target. Great for NPC gaze and attention systems.")]
        public AddIKResponse AddLookAtIK(
            [Description("Reference to the character root GameObject.")]
            GameObjectRef targetRef,
            [Description("World position of the look-at target. Default (0,0,5) in front.")]
            Vector3? targetPosition = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var lookat = go.GetComponent<LookAtIK>();
                if (lookat == null)
                    lookat = Undo.AddComponent<LookAtIK>(go);

                if (targetPosition.HasValue)
                    lookat.solver.IKPosition = targetPosition.Value;

                EditorUtility.SetDirty(go);

                return new AddIKResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    ikType = "LookAtIK",
                    targetPosition = FormatVector3(lookat.solver.IKPosition)
                };
            });
        }

        [McpPluginTool("finalik-add-aim", Title = "Final IK / Add Aim IK")]
        [Description(@"Adds an AimIK component. Rotates a chain of bones to aim at a target.
Useful for aiming weapons, pointing, or directing limbs toward a target.")]
        public AddIKResponse AddAimIK(
            [Description("Reference to the target GameObject.")]
            GameObjectRef targetRef,
            [Description("World position of the aim target. Default (0,0,5).")]
            Vector3? targetPosition = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var aim = go.GetComponent<AimIK>();
                if (aim == null)
                    aim = Undo.AddComponent<AimIK>(go);

                if (targetPosition.HasValue)
                    aim.solver.IKPosition = targetPosition.Value;

                EditorUtility.SetDirty(go);

                return new AddIKResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    ikType = "AimIK",
                    targetPosition = FormatVector3(aim.solver.IKPosition)
                };
            });
        }

        [McpPluginTool("finalik-list-ik", Title = "Final IK / List IK Components")]
        [Description("Lists all Final IK components in the current scene with their type and status.")]
        public ListIKResponse ListIK()
        {
            return MainThread.Instance.Run(() =>
            {
                var iks = Object.FindObjectsByType<IK>(FindObjectsSortMode.None);
                var lines = iks.Select(ik =>
                {
                    var typeName = ik.GetType().Name;
                    var solver = ik.GetIKSolver();
                    return $"[{ik.gameObject.GetInstanceID()}] {ik.gameObject.name} ({typeName}) enabled={ik.enabled}";
                }).ToArray();

                return new ListIKResponse
                {
                    count = iks.Length,
                    details = string.Join("\n", lines)
                };
            });
        }

        public class AddIKResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("IK type added")] public string ikType = "";
            [Description("Target position")] public string targetPosition = "";
        }

        public class ListIKResponse
        {
            [Description("Number of IK components found")] public int count;
            [Description("Details of each IK component")] public string details = "";
        }
    }
}
#endif
