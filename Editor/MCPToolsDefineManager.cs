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
            // Audio additions (AudioProject)
            ("HAS_BROAUDIO",           "Ami.BroAudio.BroAudio, BroAudio"),
            ("HAS_KOREOGRAPHER",       "SonicBloom.Koreo.Koreographer, SonicBloom.Koreo"),
            ("HAS_PMG",                "ProcGenMusic.MusicGenerator, Assembly-CSharp"),
            ("HAS_MAESTRO",            "MidiPlayerTK.MidiFilePlayer, MidiPlayer.Run"),
            ("HAS_DRYWETMIDI",         "Melanchall.DryWetMidi.Core.MidiFile, Melanchall.DryWetMidi"),
            ("HAS_FMOD",               "FMODUnity.RuntimeManager, FMODUnity"),
            ("HAS_CHUNITY",            "ChuckMainInstance, Assembly-CSharp"),
            // VNPC additions
            ("HAS_NANINOVEL",          "Naninovel.Engine, Elringus.Naninovel.Runtime"),
            ("HAS_ADVENTURE_CREATOR",  "AC.KickStarter, AC"),
            ("HAS_TEXT_ANIMATOR",      "Febucci.TextAnimatorForUnity.TextAnimatorComponentBase, Febucci.TextAnimatorForUnity.Runtime"),
            ("HAS_INK",                "Ink.Runtime.Story, Ink-Libraries"),
        };

        static MCPToolsDefineManager()
        {
            UpdateDefines();
        }

        static Type? FindType(string assemblyQualifiedName)
        {
            // Type.GetType() only searches the calling assembly + mscorlib.
            // We need to search ALL loaded assemblies for types in Assembly-CSharp,
            // third-party DLLs, and UPM packages.
            Type? t = Type.GetType(assemblyQualifiedName);
            if (t != null) return t;

            // Parse "TypeName, AssemblyName" and search all loaded assemblies
            string[] parts = assemblyQualifiedName.Split(',');
            if (parts.Length < 2) return null;

            string typeName = parts[0].Trim();
            string assemblyName = parts[1].Trim();

            // First pass: match the specified assembly name exactly
            foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                string asmSimpleName = asm.GetName().Name ?? "";
                if (string.Equals(asmSimpleName, assemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    t = asm.GetType(typeName);
                    if (t != null) return t;
                }
            }

            // Second pass: type may live in a different assembly than expected
            // (e.g. Assets/Plugins/ compiles to Assembly-CSharp-firstpass instead of Assembly-CSharp)
            foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(typeName);
                if (t != null) return t;
            }

            return null;
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
                bool assetPresent = FindType(detectType) != null;
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
