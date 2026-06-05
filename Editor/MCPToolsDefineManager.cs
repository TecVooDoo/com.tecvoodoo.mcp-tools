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
    ///
    /// Two triggers:
    /// 1. [InitializeOnLoad] -- runs on every successful domain reload.
    /// 2. MCPToolsAssetPostprocessor -- runs when assets are deleted, BEFORE
    ///    recompilation. This breaks the chicken-and-egg cycle where stale defines
    ///    cause compilation errors that prevent the define manager from running.
    /// </summary>
    [InitializeOnLoad]
    static class MCPToolsDefineManager
    {
        // Assemblies that always exist -- cannot detect asset removal via assembly presence alone
        static readonly HashSet<string> WellKnownAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Assembly-CSharp",
            "Assembly-CSharp-Editor",
            "Assembly-CSharp-firstpass"
        };

        static readonly (string symbol, string detectType)[] Entries =
        {
            ("HAS_FLEXALON",     "Flexalon.FlexalonGridLayout, Flexalon"),
            ("HAS_PWB",          "PluginMaster.PaletteManager, Assembly-CSharp-Editor"),
            ("HAS_RAYFIRE",      "RayFire.RayfireRigid, RayFireAssembly"),
            ("HAS_MAGICACLOTH2", "MagicaCloth2.MagicaCloth, MagicaClothV2"),
            ("HAS_FINALIK",      "RootMotion.FinalIK.FullBodyBipedIK, Assembly-CSharp-firstpass"),
            ("HAS_ANIMANCER",    "Animancer.AnimancerComponent, Kybernetik.Animancer"),
            ("HAS_PLAYMAKER",    "HutongGames.PlayMaker.PlayMakerFSM, PlayMaker"),
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
            // 2.5D Kamgam additions
            ("HAS_TERRAIN25D",         "Kamgam.Terrain25DLib.Terrain25D, Terrain25D"),
            ("HAS_BRIDGEBUILDER25D",   "Kamgam.BridgeBuilder25D.Bridge25D, BridgeBuilder25DLib"),
            // VNPC additions
            ("HAS_NANINOVEL",          "Naninovel.Engine, Elringus.Naninovel.Runtime"),
            ("HAS_ADVENTURE_CREATOR",  "AC.KickStarter, AC"),
            ("HAS_TEXT_ANIMATOR",      "Febucci.TextAnimatorForUnity.TextAnimatorComponentBase, Febucci.TextAnimatorForUnity.Runtime"),
            ("HAS_INK",                "Ink.Runtime.Story, Ink-Libraries"),
            // TecVooDoo Session 1 additions
            ("HAS_DECAL_COLLIDER",     "DecalCollider.Runtime.DecalCollider, Assembly-CSharp"),
            ("HAS_TEXTURE_STUDIO",     "TextureStudio.CompositeMap, Assembly-CSharp"),
            ("HAS_BOINGKIT",           "BoingKit.BoingBones, BoingKit"),
            // TecVooDoo Session 2 additions
            ("HAS_JUICY_ACTIONS",      "MagicPigGames.JuicyActions.ActionExecutor, MagicPigGames.JuicyActions.Runtime"),
            ("HAS_MUDBUN",             "MudBun.MudRenderer, MudBun"),
            ("HAS_LUMEN",              "DistantLands.Lumen.LumenEffectPlayer, DistantLands.Lumen.Runtime"),
            ("HAS_TIMEFLOW",           "AxonGenesis.Timeflow, Timeflow"),
            // M3AnimatedSeries additions
            ("HAS_ULIPSYNC",           "uLipSync.uLipSync, uLipSync.Runtime"),
            // TecVooDoo Session 3 additions (Apr 21)
            ("HAS_TCC",                "Technie.PhysicsCreator.RigidColliderCreator, TechniePhysicsCreator"),
            ("HAS_MK_EDGE",            "MK.EdgeDetection.UniversalVolumeComponents.MKEdgeDetection, MKEdgeDetectionUniversalVolumeComponent"),
            ("HAS_ULTIMATE_TERRAIN",   "PampelGames.UltimateTerrain.UltimateTerrain, PG.UltimateTerrain"),
            ("HAS_PRESSE",             "FastStudios.Interactable, Assembly-CSharp"),
            ("HAS_RTW",                "RealTimeWeather.Managers.RealTimeWeatherManager, Assembly-CSharp"),
            // TecVooDoo Session 4 additions (Apr 25)
            ("HAS_COZY",               "DistantLands.Cozy.CozyWeather, DistantLands.Cozy.Runtime"),
            // TecVooDoo Session 5 additions (Apr 27)
            ("HAS_M3DT",               "TinyGiantStudio.Text.Modular3DText, Assembly-CSharp-firstpass"),
            ("HAS_ORK",                "GamingIsLove.ORKFramework.ORK, ORKFramework3"),
            ("HAS_CITYGEN3D",          "CityGen3D.Map, CityGen3D"),
            // TecVooDoo Session 10 additions (May 15)
            ("HAS_UMOTION_PRO",        "UMotionEditor.API.ClipEditor, UMotionEditor"),
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

        /// <summary>
        /// Checks all entries and adds/removes defines as needed.
        /// Public so MCPToolsAssetPostprocessor can call it during import.
        /// </summary>
        public static void UpdateDefines()
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

        /// <summary>
        /// Menu-rescan entry: enumerates current asmdef + DLL state from the project
        /// and removes defines whose target assembly is absent. Used by the manual
        /// "Rescan MCP Defines" menu item.
        /// </summary>
        public static void RemoveStaleDefines() => RemoveStaleDefines(null);

        /// <summary>
        /// Postprocessor-driven entry: when called with deletedAssetPaths from
        /// OnPostprocessAllAssets, extracts deleted asmdef + DLL filenames directly
        /// from that array as the AUTHORITATIVE signal of what just disappeared.
        /// Bypasses the AssetDatabase-rescan-stale-during-postprocessor-window race
        /// that previously caused stale HAS_* defines to persist after UPM-style
        /// asset removals (the failure mode behind the Session 11 Koreographer /
        /// UltimateTerrain / COZY / Behavior Designer / etc. bulk-removal cascade
        /// + Session 11 follow-on Animancer recovery via inline-mirror
        /// script-execute).
        ///
        /// Also fixes a DLL-based-detection bug: entries whose assembly is a DLL
        /// filename (HAS_DOTWEEN → DOTweenPro.dll, HAS_MAGICACLOTH2 →
        /// MagicaClothV2.dll, HAS_RAYFIRE → RayFireAssembly.dll, HAS_BROAUDIO →
        /// BroAudio.dll) were being spuriously flipped to "should remove" by the
        /// prior version because `presentAssemblies` (built from asmdef
        /// enumeration only) never contained DLL names. Now also enumerates
        /// `presentDlls` via filesystem scan so DLL-based detections only fire
        /// when the DLL is genuinely absent.
        /// </summary>
        public static void RemoveStaleDefines(string[]? deletedAssetPaths)
        {
            var target = EditorUserBuildSettings.selectedBuildTargetGroup;
            if (target == BuildTargetGroup.Unknown)
                target = BuildTargetGroup.Standalone;

            var namedTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(target);
            var current = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            var defines = new HashSet<string>(current.Split(';', StringSplitOptions.RemoveEmptyEntries));
            bool changed = false;

            // AUTHORITATIVE deletion signal (postprocessor path): extract deleted
            // asmdef + DLL filenames directly from the deletedAssets[] array.
            // Postprocessor passes this from OnPostprocessAllAssets, where Unity
            // has already committed the deletion list. Bypasses any
            // AssetDatabase-race during the deletion window.
            HashSet<string> deletedAsmdefNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            HashSet<string> deletedDllNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (deletedAssetPaths != null)
            {
                foreach (string path in deletedAssetPaths)
                {
                    if (path.EndsWith(".asmdef", StringComparison.OrdinalIgnoreCase))
                        deletedAsmdefNames.Add(System.IO.Path.GetFileNameWithoutExtension(path));
                    else if (path.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        deletedDllNames.Add(System.IO.Path.GetFileNameWithoutExtension(path));
                }
            }

            // CURRENT-STATE signal (fallback for menu rescan + DLL confirmation):
            // enumerate all asmdef filenames currently in the project.
            HashSet<string> presentAssemblies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] asmdefGuids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");
            foreach (string guid in asmdefGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                presentAssemblies.Add(System.IO.Path.GetFileNameWithoutExtension(path));
            }

            // DLL enumeration via filesystem (AssetDatabase doesn't surface .dll under
            // t:AssemblyDefinitionAsset). Required to correctly handle DLL-based
            // detection entries — without this, presentAssemblies.Contains("DOTweenPro")
            // is always false and HAS_DOTWEEN flickers off in every postprocessor run.
            HashSet<string> presentDlls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string root in new[] { "Assets", "Packages" })
            {
                if (!System.IO.Directory.Exists(root)) continue;
                try
                {
                    foreach (string dllPath in System.IO.Directory.EnumerateFiles(root, "*.dll", System.IO.SearchOption.AllDirectories))
                        presentDlls.Add(System.IO.Path.GetFileNameWithoutExtension(dllPath));
                }
                catch (System.IO.IOException) { /* ignore transient enumeration errors */ }
                catch (System.UnauthorizedAccessException) { /* ignore permission failures */ }
            }

            foreach (var (symbol, detectType) in Entries)
            {
                if (!defines.Contains(symbol)) continue;

                // Extract "TypeName, AssemblyName"
                string[] parts = detectType.Split(',');
                if (parts.Length < 2) continue;
                string typeName = parts[0].Trim();
                string assemblyName = parts[1].Trim();

                bool shouldRemove;
                if (WellKnownAssemblies.Contains(assemblyName))
                {
                    // Assembly-CSharp/Editor/firstpass always exist -- check if the
                    // asset's marker script is still present via AssetDatabase instead.
                    string className = typeName.Contains('.')
                        ? typeName.Substring(typeName.LastIndexOf('.') + 1)
                        : typeName;
                    shouldRemove = AssetDatabase.FindAssets(className + " t:MonoScript").Length == 0;
                }
                else if (deletedAsmdefNames.Contains(assemblyName) || deletedDllNames.Contains(assemblyName))
                {
                    // Postprocessor path: this assembly's asmdef or DLL was JUST deleted.
                    // Trust the deletedAssets[] signal absolutely; don't second-guess
                    // via AssetDatabase reads that could be stale during this window.
                    shouldRemove = true;
                }
                else
                {
                    // Fallback path: confirm the assembly is absent from the project's
                    // current state. Check BOTH asmdef names AND DLL filenames — without
                    // the DLL check, DLL-based-detection entries (HAS_DOTWEEN etc.) would
                    // spuriously flag for removal because presentAssemblies only holds
                    // asmdef names.
                    shouldRemove = !presentAssemblies.Contains(assemblyName) && !presentDlls.Contains(assemblyName);
                }

                if (shouldRemove)
                {
                    defines.Remove(symbol);
                    changed = true;
                }
            }

            if (changed)
            {
                var newDefines = string.Join(";", defines.OrderBy(d => d));
                PlayerSettings.SetScriptingDefineSymbols(namedTarget, newDefines);
            }
        }

        /// <summary>
        /// Manual rescan -- strips stale defines then re-detects all assets.
        /// Use if auto-detection missed a removal or domain reload got stuck.
        /// </summary>
        [MenuItem("Tools/TecVooDoo/Rescan MCP Defines")]
        public static void MenuRescanDefines()
        {
            RemoveStaleDefines();
            UpdateDefines();
        }
    }

    /// <summary>
    /// Catches asset deletions during import (BEFORE recompilation) and removes
    /// stale defines. This breaks the chicken-and-egg cycle where stale defines
    /// cause compilation errors that prevent MCPToolsDefineManager from running.
    /// </summary>
    class MCPToolsAssetPostprocessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (deletedAssets.Length == 0) return;

            // Check if any deleted asset looks like it could be an asmdef or
            // a folder containing scripts (heuristic: .cs, .asmdef, or folder deletion)
            bool relevantDeletion = false;
            foreach (string path in deletedAssets)
            {
                if (path.EndsWith(".asmdef") || path.EndsWith(".cs") || path.EndsWith(".dll"))
                {
                    relevantDeletion = true;
                    break;
                }
            }

            if (relevantDeletion)
            {
                // Pass deletedAssets through so RemoveStaleDefines treats the array
                // as the authoritative deletion signal instead of relying on
                // AssetDatabase reads that may be stale during the postprocessor
                // window. Closes the recurring stale-HAS_*-define failure mode
                // behind Session 11 (Koreographer/UltimateTerrain/COZY/Behavior
                // Designer bulk removal cascade) + follow-on Animancer recovery.
                MCPToolsDefineManager.RemoveStaleDefines(deletedAssets);
            }
        }
    }
}
