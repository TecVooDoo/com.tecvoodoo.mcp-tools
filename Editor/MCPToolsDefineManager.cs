#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace MCPTools.Editor
{
    /// <summary>
    /// Auto-detects installed assets and manages scripting define symbols
    /// so that MCP tool assemblies compile only when their target asset is present.
    /// </summary>
    [InitializeOnLoad]
    static class MCPToolsDefineManager
    {
        static readonly (string symbol, string detectType)[] Entries =
        {
            ("HAS_FLEXALON",     "Flexalon.FlexalonGridLayout, Flexalon"),
            ("HAS_PWB",          "PluginMaster.PaletteManager, Assembly-CSharp-Editor"),
            ("HAS_RAYFIRE",      "RayFire.RayfireRigid, RayFireAssembly"),
            ("HAS_MAGICACLOTH2", "MagicaCloth2.MagicaCloth, MagicaClothV2"),
            ("HAS_FINALIK",      "RootMotion.FinalIK.FullBodyBipedIK, Assembly-CSharp-firstpass"),
            ("HAS_ANIMANCER",    "Animancer.AnimancerComponent, Kybernetik.Animancer"),
            ("HAS_PLAYMAKER",    "HutongGames.PlayMaker.PlayMakerFSM, PlayMaker"),
            ("HAS_ASSETINVENTORY", "AssetInventory.AssetSearch, AssetInventory.Editor"),
            ("HAS_MALBERS_AC",     "MalbersAnimations.Controller.MAnimal, MalbersAnimations"),
            ("HAS_MALBERS_QUESTFORGE", "MalbersAnimations.QuestForge.QuestManager, Assembly-CSharp"),
            ("HAS_RETARGETPRO",    "KINEMATION.RetargetPro.Runtime.RetargetProComponent, RetargetPro.Runtime"),
            // Session 2 additions
            ("HAS_ROPE_TOOLKIT",       "RopeToolkit.Rope, Assembly-CSharp"),
            ("HAS_HEATHEN_PHYSICS",    "Heathen.UnityPhysics.PhysicsData, Heathen.UnityPhysics"),
            ("HAS_HEATHEN_BALLISTICS", "Heathen.UnityPhysics.BallisticAim, Heathen.Ballistics"),
            ("HAS_FEEL",               "MoreMountains.Feedbacks.MMF_Player, MoreMountains.Tools"),
            ("HAS_DAMAGE_NUMBERS_PRO", "DamageNumbersPro.DamageNumberMesh, DamageNumbersPro"),
            // Session 3 additions
            ("HAS_CINEMACHINE",        "Unity.Cinemachine.CinemachineCamera, Unity.Cinemachine"),
            ("HAS_ANIMATION_RIGGING",  "UnityEngine.Animations.Rigging.TwoBoneIKConstraint, Unity.Animation.Rigging"),
            ("HAS_ALINE",              "Drawing.MonoBehaviourGizmos, ALINE"),
            // Session 4 additions
            ("HAS_MASTERAUDIO",        "DarkTonic.MasterAudio.MasterAudio, Assembly-CSharp"),
            ("HAS_ASTAR",              "AstarPath, AstarPathfindingProject"),
            ("HAS_DIALOGUE_SYSTEM",    "PixelCrushers.DialogueSystem.DialogueManager, Assembly-CSharp"),
            ("HAS_SENSORTOOLKIT",      "Micosmo.SensorToolkit.RangeSensor, Micosmo.SensorToolkit"),
            ("HAS_UCC",                "Opsive.UltimateCharacterController.Character.UltimateCharacterLocomotion, Opsive.UltimateCharacterController"),
            ("HAS_BEHAVIOR_DESIGNER",  "Opsive.BehaviorDesigner.Runtime.BehaviorTree, Opsive.BehaviorDesigner.Runtime"),
            ("HAS_DOTWEEN",            "DG.Tweening.DOTweenAnimation, DOTweenPro"),
            ("HAS_UNITY_ENTITIES",     "Unity.Entities.SceneSectionComponent, Unity.Entities.Hybrid"),
            ("HAS_UNITY_PHYSICS",      "Unity.Physics.Authoring.PhysicsStepAuthoring, Unity.Physics.Hybrid"),
        };

        static MCPToolsDefineManager()
        {
            UpdateDefines();
        }

        static void UpdateDefines()
        {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (target == BuildTargetGroup.Unknown)
                target = BuildTargetGroup.Standalone;

            var namedTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(target);
            var current = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            var defines = new HashSet<string>(current.Split(';', StringSplitOptions.RemoveEmptyEntries));
            bool changed = false;

            foreach (var (symbol, detectType) in Entries)
            {
                bool assetPresent = Type.GetType(detectType) != null;
                if (assetPresent && defines.Add(symbol))
                    changed = true;
                else if (!assetPresent && defines.Remove(symbol))
                    changed = true;
            }

            if (changed)
            {
                var newDefines = string.Join(";", defines.OrderBy(d => d));
                PlayerSettings.SetScriptingDefineSymbols(namedTarget, newDefines);
            }
        }
    }
}
