#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
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
                if (FullBodyBipedIKType == null)
                    throw new Exception("RootMotion.FinalIK.FullBodyBipedIK type not found in loaded assemblies.");
                if (BipedReferencesType == null)
                    throw new Exception("RootMotion.BipedReferences type not found in loaded assemblies.");

                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new Exception(error);
                if (go == null) throw new Exception("GameObject not found.");

                var fbbik = go.GetComponent(FullBodyBipedIKType);
                if (fbbik == null)
                    fbbik = Undo.AddComponent(go, FullBodyBipedIKType);

                if (autoDetect)
                {
                    // Create BipedReferences instance
                    var references = Activator.CreateInstance(BipedReferencesType)!;

                    // Find AutoDetectParams type and create instance
                    var autoDetectParamsType = BipedReferencesType.GetNestedType("AutoDetectParams");
                    object? autoDetectParams = null;
                    if (autoDetectParamsType != null)
                    {
                        // Constructor(bool legsParentInRoot, bool includeEyes)
                        var ctor = autoDetectParamsType.GetConstructor(new[] { typeof(bool), typeof(bool) });
                        if (ctor != null)
                            autoDetectParams = ctor.Invoke(new object[] { true, false });
                        else
                            autoDetectParams = Activator.CreateInstance(autoDetectParamsType);
                    }

                    // Call BipedReferences.AutoDetectReferences(ref references, transform, params)
                    var autoDetectMethod = BipedReferencesType.GetMethod("AutoDetectReferences",
                        BindingFlags.Public | BindingFlags.Static);
                    if (autoDetectMethod != null)
                    {
                        var args = new object?[] { references, go.transform, autoDetectParams };
                        autoDetectMethod.Invoke(null, args);
                        // ref parameter -- get updated value
                        references = args[0]!;
                    }

                    // Check isFilled
                    var isFilledProp = BipedReferencesType.GetProperty("isFilled", BindingFlags.Public | BindingFlags.Instance);
                    bool isFilled = isFilledProp != null && (bool)isFilledProp.GetValue(references)!;

                    if (!isFilled)
                    {
                        // Try via Animator humanoid
                        var animator = go.GetComponent<Animator>();
                        if (animator != null && animator.isHuman)
                        {
                            var assignMethod = BipedReferencesType.GetMethod("AssignHumanoidReferences",
                                BindingFlags.Public | BindingFlags.Static);
                            if (assignMethod != null)
                            {
                                var args2 = new object?[] { references, animator, autoDetectParams };
                                assignMethod.Invoke(null, args2);
                                references = args2[0]!;
                            }
                        }
                    }

                    isFilled = isFilledProp != null && (bool)isFilledProp.GetValue(references)!;
                    if (!isFilled)
                        throw new Exception("Could not auto-detect biped references. Assign manually.");

                    // Detect root node: IKSolverFullBodyBiped.DetectRootNodeBone(references)
                    Transform? rootNode = null;
                    if (IKSolverFBBType != null)
                    {
                        var detectMethod = IKSolverFBBType.GetMethod("DetectRootNodeBone",
                            BindingFlags.Public | BindingFlags.Static);
                        if (detectMethod != null)
                            rootNode = detectMethod.Invoke(null, new[] { references }) as Transform;
                    }

                    // fbbik.SetReferences(references, rootNode)
                    var setRefsMethod = fbbik.GetType().GetMethod("SetReferences",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (setRefsMethod != null)
                        setRefsMethod.Invoke(fbbik, new object?[] { references, rootNode });
                }

                // Check for errors
                string errorMsg = "";
                bool hasError = false;
                var refsErrorMethod = fbbik.GetType().GetMethod("ReferencesError",
                    BindingFlags.Public | BindingFlags.Instance);
                if (refsErrorMethod != null)
                {
                    var args = new object[] { errorMsg };
                    var result = refsErrorMethod.Invoke(fbbik, args);
                    if (result is bool b) hasError = b;
                    errorMsg = (string)args[0];
                }

                EditorUtility.SetDirty(go);

                // Read references.isFilled
                var refsField = Get(fbbik, "references");
                bool refsFilled = false;
                if (refsField != null)
                {
                    var isFProp = refsField.GetType().GetProperty("isFilled", BindingFlags.Public | BindingFlags.Instance);
                    if (isFProp != null)
                        refsFilled = (bool)isFProp.GetValue(refsField)!;
                }

                // Read solver.rootNode
                var solver = Get(fbbik, "solver");
                string rootNodeName = "none";
                if (solver != null)
                {
                    var rn = Get(solver, "rootNode");
                    if (rn is Transform rnT && rnT != null)
                        rootNodeName = rnT.name;
                }

                return new AddFBBIKResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    referencesDetected = refsFilled,
                    hasError = hasError,
                    errorMessage = errorMsg,
                    rootNode = rootNodeName
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
                if (FullBodyBipedIKType == null)
                    throw new Exception("RootMotion.FinalIK.FullBodyBipedIK type not found in loaded assemblies.");

                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new Exception(error);
                if (go == null) throw new Exception("GameObject not found.");

                var fbbik = go.GetComponent(FullBodyBipedIKType);
                if (fbbik == null)
                    throw new Exception($"'{go.name}' has no FullBodyBipedIK component.");

                var effector = GetEffectorReflection(fbbik, effectorName);

                if (position.HasValue)       Set(effector, "position", position.Value);
                if (positionWeight.HasValue)  Set(effector, "positionWeight", Mathf.Clamp01(positionWeight.Value));
                if (rotationWeight.HasValue)  Set(effector, "rotationWeight", Mathf.Clamp01(rotationWeight.Value));

                EditorUtility.SetDirty(go);

                var posVal = Get(effector, "position");
                string posStr = posVal is Vector3 pv ? FormatVector3(pv) : posVal?.ToString() ?? "";
                var pwVal = Get(effector, "positionWeight");
                var rwVal = Get(effector, "rotationWeight");

                return new SetEffectorResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    effectorName = effectorName,
                    position = posStr,
                    positionWeight = pwVal is float pw ? pw : 0f,
                    rotationWeight = rwVal is float rw ? rw : 0f
                };
            });
        }

        static object GetEffectorReflection(object fbbik, string name)
        {
            var solver = Get(fbbik, "solver")
                         ?? throw new Exception("Could not access fbbik.solver.");

            string fieldName = name.ToLower().Replace(" ", "") switch
            {
                "body"          => "bodyEffector",
                "lefthand"      => "leftHandEffector",
                "righthand"     => "rightHandEffector",
                "leftfoot"      => "leftFootEffector",
                "rightfoot"     => "rightFootEffector",
                "leftshoulder"  => "leftShoulderEffector",
                "rightshoulder" => "rightShoulderEffector",
                _ => throw new Exception($"Unknown effector '{name}'. Use: Body, LeftHand, RightHand, LeftFoot, RightFoot, LeftShoulder, RightShoulder.")
            };

            return Get(solver, fieldName)
                   ?? throw new Exception($"Effector '{fieldName}' not found on solver.");
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
