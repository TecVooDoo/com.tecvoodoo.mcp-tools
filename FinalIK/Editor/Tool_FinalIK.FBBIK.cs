#if HAS_FINALIK
#nullable enable
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using RootMotion;
using RootMotion.FinalIK;
using UnityEditor;
using UnityEngine;

namespace MCPTools.FinalIK.Editor
{
    public partial class Tool_FinalIK
    {
        [McpPluginTool("finalik-add-fbbik", Title = "Final IK / Add Full Body Biped IK")]
        [Description(@"Adds a FullBodyBipedIK component to a character GameObject and auto-detects biped bone references.
The character must have an Animator with a humanoid avatar, or a standard biped bone hierarchy.
This is the main IK component for bipeds — controls hands, feet, body, and shoulders.")]
        public AddFBBIKResponse AddFullBodyBipedIK(
            [Description("Reference to the character root GameObject.")]
            GameObjectRef targetRef,
            [Description("Auto-detect biped references from the skeleton. Default true.")]
            bool autoDetect = true
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var fbbik = go.GetComponent<FullBodyBipedIK>();
                if (fbbik == null)
                    fbbik = Undo.AddComponent<FullBodyBipedIK>(go);

                if (autoDetect)
                {
                    var references = new BipedReferences();
                    BipedReferences.AutoDetectReferences(ref references, go.transform,
                        new BipedReferences.AutoDetectParams(true, false));

                    if (!references.isFilled)
                    {
                        // Try via Animator humanoid
                        var animator = go.GetComponent<Animator>();
                        if (animator != null && animator.isHuman)
                        {
                            BipedReferences.AssignHumanoidReferences(ref references, animator,
                                new BipedReferences.AutoDetectParams(true, false));
                        }
                    }

                    if (!references.isFilled)
                        throw new System.Exception("Could not auto-detect biped references. Assign manually.");

                    var rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(references);
                    fbbik.SetReferences(references, rootNode);
                }

                string errorMsg = "";
                bool hasError = fbbik.ReferencesError(ref errorMsg);

                EditorUtility.SetDirty(go);

                return new AddFBBIKResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    referencesDetected = fbbik.references.isFilled,
                    hasError = hasError,
                    errorMessage = errorMsg,
                    rootNode = fbbik.solver.rootNode != null ? fbbik.solver.rootNode.name : "none"
                };
            });
        }

        [McpPluginTool("finalik-set-effector", Title = "Final IK / Set FBBIK Effector")]
        [Description(@"Sets an effector target position and/or weight on a FullBodyBipedIK component.
Effectors: 'Body', 'LeftHand', 'RightHand', 'LeftFoot', 'RightFoot', 'LeftShoulder', 'RightShoulder'.
Set positionWeight to 1 to fully control the effector, 0 to release it.")]
        public SetEffectorResponse SetEffector(
            [Description("Reference to the character with FullBodyBipedIK.")]
            GameObjectRef targetRef,
            [Description("Effector name: 'Body', 'LeftHand', 'RightHand', 'LeftFoot', 'RightFoot', 'LeftShoulder', 'RightShoulder'.")]
            string effectorName,
            [Description("World position for the effector target. Null to keep current.")]
            Vector3? position = null,
            [Description("Position weight (0-1). 1 = full IK control, 0 = no IK. Null to keep current.")]
            float? positionWeight = null,
            [Description("Rotation weight (0-1). Null to keep current.")]
            float? rotationWeight = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var fbbik = go.GetComponent<FullBodyBipedIK>();
                if (fbbik == null)
                    throw new System.Exception($"'{go.name}' has no FullBodyBipedIK component.");

                IKEffector effector = GetEffector(fbbik, effectorName);

                if (position.HasValue) effector.position = position.Value;
                if (positionWeight.HasValue) effector.positionWeight = Mathf.Clamp01(positionWeight.Value);
                if (rotationWeight.HasValue) effector.rotationWeight = Mathf.Clamp01(rotationWeight.Value);

                EditorUtility.SetDirty(go);

                return new SetEffectorResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    effectorName = effectorName,
                    position = FormatVector3(effector.position),
                    positionWeight = effector.positionWeight,
                    rotationWeight = effector.rotationWeight
                };
            });
        }

        static IKEffector GetEffector(FullBodyBipedIK fbbik, string name)
        {
            switch (name.ToLower().Replace(" ", ""))
            {
                case "body":           return fbbik.solver.bodyEffector;
                case "lefthand":       return fbbik.solver.leftHandEffector;
                case "righthand":      return fbbik.solver.rightHandEffector;
                case "leftfoot":       return fbbik.solver.leftFootEffector;
                case "rightfoot":      return fbbik.solver.rightFootEffector;
                case "leftshoulder":   return fbbik.solver.leftShoulderEffector;
                case "rightshoulder":  return fbbik.solver.rightShoulderEffector;
                default: throw new System.Exception($"Unknown effector '{name}'. Use: Body, LeftHand, RightHand, LeftFoot, RightFoot, LeftShoulder, RightShoulder.");
            }
        }

        public class AddFBBIKResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Biped references detected")] public bool referencesDetected;
            [Description("Setup has errors")] public bool hasError;
            [Description("Error message if any")] public string errorMessage = "";
            [Description("Root node bone name")] public string rootNode = "";
        }

        public class SetEffectorResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Effector name")] public string effectorName = "";
            [Description("Effector position")] public string position = "";
            [Description("Position weight")] public float positionWeight;
            [Description("Rotation weight")] public float rotationWeight;
        }
    }
}
#endif
