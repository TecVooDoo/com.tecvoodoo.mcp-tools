#nullable enable
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
#if MCP_HAS_AIGD
using AIGD;
#else
using com.IvanMurzak.Unity.MCP.Runtime.Data;
#endif
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
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
                if (LookAtIKType == null)
                    throw new Exception("RootMotion.FinalIK.LookAtIK type not found in loaded assemblies.");

                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new Exception(error);
                if (go == null) throw new Exception("GameObject not found.");

                var lookat = go.GetComponent(LookAtIKType);
                if (lookat == null)
                    lookat = Undo.AddComponent(go, LookAtIKType);

                if (targetPosition.HasValue)
                {
                    var solver = Get(lookat, "solver");
                    if (solver != null)
                        Set(solver, "IKPosition", targetPosition.Value);
                }

                EditorUtility.SetDirty(go);

                var solverObj = Get(lookat, "solver");
                var ikPos = solverObj != null ? Get(solverObj, "IKPosition") : null;
                string tgtPos = ikPos is Vector3 v ? FormatVector3(v) : "(0.00, 0.00, 5.00)";

                return new AddIKResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    ikType = "LookAtIK",
                    targetPosition = tgtPos
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
                if (AimIKType == null)
                    throw new Exception("RootMotion.FinalIK.AimIK type not found in loaded assemblies.");

                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new Exception(error);
                if (go == null) throw new Exception("GameObject not found.");

                var aim = go.GetComponent(AimIKType);
                if (aim == null)
                    aim = Undo.AddComponent(go, AimIKType);

                if (targetPosition.HasValue)
                {
                    var solver = Get(aim, "solver");
                    if (solver != null)
                        Set(solver, "IKPosition", targetPosition.Value);
                }

                EditorUtility.SetDirty(go);

                var solverObj = Get(aim, "solver");
                var ikPos = solverObj != null ? Get(solverObj, "IKPosition") : null;
                string tgtPos = ikPos is Vector3 v ? FormatVector3(v) : "(0.00, 0.00, 5.00)";

                return new AddIKResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    ikType = "AimIK",
                    targetPosition = tgtPos
                };
            });
        }

        [McpPluginTool("finalik-list-ik", Title = "Final IK / List IK Components")]
        [Description("Lists all Final IK components in the current scene with their type and status.")]
        public ListIKResponse ListIK()
        {
            return MainThread.Instance.Run(() =>
            {
                if (IKType == null)
                    throw new Exception("RootMotion.FinalIK.IK type not found in loaded assemblies.");

                // FindObjectsByType<T>(FindObjectsSortMode) -- use reflection on Object
                var findMethod = typeof(UnityEngine.Object).GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .FirstOrDefault(m => m.Name == "FindObjectsByType"
                        && m.IsGenericMethod
                        && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType == typeof(FindObjectsSortMode));

                UnityEngine.Component[] iks;
                if (findMethod != null)
                {
                    var generic = findMethod.MakeGenericMethod(IKType);
                    var result = generic.Invoke(null, new object[] { FindObjectsSortMode.None });
                    iks = ((Array)result!).Cast<UnityEngine.Component>().ToArray();
                }
                else
                {
                    // Fallback for older Unity
#pragma warning disable CS0618
                    iks = UnityEngine.Object.FindObjectsOfType(IKType).Cast<UnityEngine.Component>().ToArray();
#pragma warning restore CS0618
                }

                var lines = iks.Select(ik =>
                {
                    var typeName = ik.GetType().Name;
                    var behaviour = ik as Behaviour;
                    var enabledStr = behaviour != null ? behaviour.enabled.ToString() : "?";
                    return $"[{ik.gameObject.GetInstanceID()}] {ik.gameObject.name} ({typeName}) enabled={enabledStr}";
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
