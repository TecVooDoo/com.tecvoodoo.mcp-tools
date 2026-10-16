#if HAS_GAIA && GAIA_2023_PRO
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Gaia;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MCPTools.Gaia.Editor
{
    /// <summary>
    /// Procedural Worlds Sky (ENTRY-433): add / remove / status, plus the helpers the time-of-day and
    /// weather tools share. The whole PW Sky API is behind GAIA_2023_PRO (a Standalone-only define),
    /// so these tools exist only while that define is live.
    /// </summary>
    public partial class Tool_Gaia
    {
        const string SkyRestoreKeyPrefix = "TMCP.Gaia.PWSkyRestore:";

        /// <summary>What GRC_PWSky.AddToScene overwrites and RemoveFromScene never puts back (ENTRY-433).</summary>
        [Serializable]
        class SkyRestoreRecord
        {
            public List<string> disabledLightPaths = new List<string>();
            public string skyboxPath = "";
            public string skyboxName = "";
            public bool skyboxWasNull;
            public List<string> windZonePaths = new List<string>();
            public bool fog;
            public FogMode fogMode;
            public float fogDensity;
            public Color fogColor;
            public float fogStart;
            public float fogEnd;
            public AmbientMode ambientMode;
            public Color ambientSky;
            public Color ambientEquator;
            public Color ambientGround;
            public float ambientIntensity;
        }

        [AiTool("gaia-sky", Title = "Gaia / PW Sky")]
        [Description(@"Adds, removes or reports Procedural Worlds Sky (the Gaia Pro sky: time of day, clouds, rain/snow).
action='status' (default) -- read-only report: sky present, time of day, sun/moon, skybox, fog, weather flags.
action='add' -- GRC_PWSky.AddToScene. The vendor call DISABLES EVERY DIRECTIONAL LIGHT in the scene and swaps the skybox and fog (ENTRY-433); this tool first records the lights it is about to switch off, the skybox and the fog/ambient settings (EditorPrefs, per scene path) so 'remove' can put them back, then ticks the atmosphere so the first frame is not drowned in fog. REFUSES when PW Sky is already present (a re-sent call would otherwise wipe the record), and when a Gaia 'Lighting' object holding other children exists, unless force=true -- AddToScene deletes that object first.
action='remove' -- GRC_PWSky.RemoveFromScene, which deletes the whole Gaia 'Lighting' object and restores NOTHING (measured: zero active lights, terrain trees stop rendering, PW skybox and fog left behind, scene not dirtied). This tool re-enables the recorded lights, restores skybox/fog/ambient, deletes the 'PW Wind Zone' the add created, and marks the scene dirty. REFUSES when 'Lighting' holds children that PW Sky did not create, unless force=true. A Gaia 'Lighting' object that existed (even empty) before the add is gone afterwards -- the vendor add replaces it. Gaia logs ONE MissingReferenceException from PW_VFX_Clouds.EditorUpdateCloudPosition right after a remove (vendor, one-shot, harmless).
Not available on HDRP (the vendor add returns early). Edit mode only for add/remove. Save the scene afterwards.")]
        public string Sky(
            [Description("'status' (default), 'add' or 'remove'.")] string action = "status",
            [Description("add: proceed although a Gaia 'Lighting' object with other children exists (it will be deleted). remove: proceed although 'Lighting' holds children PW Sky did not create (they will be deleted). Default false.")] bool force = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                switch (action.Trim().ToLowerInvariant())
                {
                    case "status":
                        break;
                    case "add":
                        AddSky(sb, force);
                        break;
                    case "remove":
                        RemoveSky(sb, force);
                        break;
                    default:
                        throw new Exception($"Unknown action '{action}'. Use 'status', 'add' or 'remove'.");
                }
                AppendSkyReport(sb);
                return sb.ToString();
            });
        }

        static void AddSky(StringBuilder sb, bool force)
        {
            if (Application.isPlaying)
                throw new Exception("gaia-sky add is edit-mode only.");
            if (GaiaUtils.CheckIfPWSkyExists())
            {
                sb.AppendLine("PW Sky is ALREADY in the scene -- nothing added (a repeat add would delete and rebuild it and lose the restore record). Use action='remove' first to rebuild.");
                return;
            }

            GameObject? lighting = GaiaUtils.GetLightingObject(false);
            if (lighting != null && lighting.transform.childCount > 0 && !force)
                throw new Exception($"A Gaia '{lighting.name}' object with {lighting.transform.childCount} child(ren) already exists, and AddToScene DELETES it before building. Move anything you want to keep out of it, or pass force=true.");

            Scene scene = SceneManager.GetActiveScene();
            SkyRestoreRecord record = CaptureSkyRestoreRecord();
            EditorPrefs.SetString(SkyRestoreKey(scene), JsonUtility.ToJson(record));

            GaiaRuntimeComponent component = CreatePWSkyComponent();
            try
            {
                component.Initialize();
                component.AddToScene();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(component);
            }

            if (!GaiaUtils.CheckIfPWSkyExists())
                throw new Exception("AddToScene returned but no PW Sky exists. The vendor add is a no-op on HDRP, and needs its 'PW Sky Prefabs.asset'.");

            // Re-apply the add's 16:00 through the GaiaAPI setters, as gaia-time-of-day does. SetDesign S46 measured that the
            // atmosphere tick never writes the sun intensity (2.49 survived two LateUpdates and later frames); only a
            // time set brought it to the 16:00 curve's 1.60, even with the time unchanged.
            GaiaAPI.SetTimeOfDayHour(GaiaAPI.GetTimeOfDayHour());
            GaiaAPI.SetTimeOfDayMinute(GaiaAPI.GetTimeOfDayMinute());
            EditorSceneManager.MarkSceneDirty(scene);
            sb.AppendLine($"PW Sky added. Recorded for restore: {record.disabledLightPaths.Count} directional light(s) the add switched off" +
                          (record.disabledLightPaths.Count > 0 ? $" ({string.Join(", ", record.disabledLightPaths)})" : "") +
                          $", skybox '{(record.skyboxWasNull ? "<none>" : record.skyboxPath.Length > 0 ? $"{record.skyboxName} ({record.skyboxPath})" : "<scene material, NOT restorable>")}', fog {(record.fog ? "on" : "off")}.");
            sb.AppendLine(TickAtmosphere());
        }

        static void RemoveSky(StringBuilder sb, bool force)
        {
            if (Application.isPlaying)
                throw new Exception("gaia-sky remove is edit-mode only.");

            GameObject? lighting = GaiaUtils.GetLightingObject(false);
            if (lighting != null && !force)
            {
                List<string> foreign = ForeignLightingChildren(lighting);
                if (foreign.Count > 0)
                    throw new Exception($"'{lighting.name}' holds children PW Sky did not create: {string.Join(", ", foreign)}. RemoveFromScene deletes the WHOLE object. Move them out first, or pass force=true.");
            }

            bool hadSky = GaiaUtils.CheckIfPWSkyExists() || lighting != null;
            GaiaRuntimeComponent component = CreatePWSkyComponent();
            try
            {
                component.RemoveFromScene();
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(component);
            }

            Scene scene = SceneManager.GetActiveScene();
            string json = EditorPrefs.GetString(SkyRestoreKey(scene), "");
            if (json.Length > 0)
            {
                SkyRestoreRecord record = JsonUtility.FromJson<SkyRestoreRecord>(json);
                ApplySkyRestoreRecord(sb, scene, record);
                EditorPrefs.DeleteKey(SkyRestoreKey(scene));
            }
            else
            {
                sb.AppendLine(hadSky
                    ? "No restore record for this scene (PW Sky was not added by gaia-sky, or the scene was renamed). Lights, skybox and fog were NOT restored -- see the report below and fix them by hand."
                    : "No restore record and no PW Sky: nothing to do. If you just removed it, this is likely a RE-SENT call and the first run already restored the scene -- check the report below.");
            }

            EditorSceneManager.MarkSceneDirty(scene);
            sb.AppendLine(hadSky ? "PW Sky removed." : "No PW Sky found; nothing removed.");
        }

        static string SkyRestoreKey(Scene scene)
        {
            return SkyRestoreKeyPrefix + (scene.path.Length > 0 ? scene.path : scene.name);
        }

        static SkyRestoreRecord CaptureSkyRestoreRecord()
        {
            SkyRestoreRecord record = new SkyRestoreRecord();
            // AddToScene deactivates every directional light it finds (active ones; inactive ones stay inactive).
            foreach (Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude))
            {
                if (light.type == LightType.Directional && light.gameObject.activeInHierarchy)
                    record.disabledLightPaths.Add(HierarchyPath(light.transform));
            }

            Material? skybox = RenderSettings.skybox;
            record.skyboxWasNull = skybox == null;
            record.skyboxPath = skybox != null ? AssetDatabase.GetAssetPath(skybox) : "";
            record.skyboxName = skybox != null ? skybox.name : "";
            foreach (WindZone zone in UnityEngine.Object.FindObjectsByType<WindZone>(FindObjectsInactive.Include))
                record.windZonePaths.Add(HierarchyPath(zone.transform));
            record.fog = RenderSettings.fog;
            record.fogMode = RenderSettings.fogMode;
            record.fogDensity = RenderSettings.fogDensity;
            record.fogColor = RenderSettings.fogColor;
            record.fogStart = RenderSettings.fogStartDistance;
            record.fogEnd = RenderSettings.fogEndDistance;
            record.ambientMode = RenderSettings.ambientMode;
            record.ambientSky = RenderSettings.ambientSkyColor;
            record.ambientEquator = RenderSettings.ambientEquatorColor;
            record.ambientGround = RenderSettings.ambientGroundColor;
            record.ambientIntensity = RenderSettings.ambientIntensity;
            return record;
        }

        static void ApplySkyRestoreRecord(StringBuilder sb, Scene scene, SkyRestoreRecord record)
        {
            bool skyboxRestored = true;
            int restored = 0;
            foreach (string path in record.disabledLightPaths)
            {
                GameObject? go = FindByHierarchyPath(scene, path);
                if (go == null)
                {
                    sb.AppendLine($"  ** Recorded light '{path}' not found -- renamed or deleted since the add. Re-enable your sun by hand. **");
                    continue;
                }
                go.SetActive(true);
                restored++;
            }
            sb.AppendLine($"Re-enabled {restored}/{record.disabledLightPaths.Count} directional light(s).");

            if (record.skyboxWasNull)
            {
                RenderSettings.skybox = null;
            }
            else if (record.skyboxPath.Length > 0)
            {
                Material? original = LoadSkybox(record.skyboxPath, record.skyboxName);
                if (original != null)
                    RenderSettings.skybox = original;
                else
                    sb.AppendLine($"  ** Original skybox '{record.skyboxName}' ({record.skyboxPath}) no longer loads; the PW skybox instance is still assigned. **");
                skyboxRestored = original != null;
            }
            else
            {
                sb.AppendLine("  ** The original skybox was a scene-only material (no asset path) and cannot be restored; the PW skybox instance is still assigned. **");
                skyboxRestored = false;
            }

            RenderSettings.fog = record.fog;
            RenderSettings.fogMode = record.fogMode;
            RenderSettings.fogDensity = record.fogDensity;
            RenderSettings.fogColor = record.fogColor;
            RenderSettings.fogStartDistance = record.fogStart;
            RenderSettings.fogEndDistance = record.fogEnd;
            RenderSettings.ambientMode = record.ambientMode;
            RenderSettings.ambientSkyColor = record.ambientSky;
            RenderSettings.ambientEquatorColor = record.ambientEquator;
            RenderSettings.ambientGroundColor = record.ambientGround;
            RenderSettings.ambientIntensity = record.ambientIntensity;
            sb.AppendLine(skyboxRestored ? "Restored skybox, fog and ambient to their pre-add values." : "Restored fog and ambient to their pre-add values (NOT the skybox -- see above).");

            // The add leaves a 'PW Wind Zone' that RemoveFromScene does not delete (SetDesign S46).
            foreach (WindZone zone in UnityEngine.Object.FindObjectsByType<WindZone>(FindObjectsInactive.Include))
            {
                string path = HierarchyPath(zone.transform);
                if (zone.name.StartsWith("PW Wind Zone") && !record.windZonePaths.Contains(path))
                {
                    UnityEngine.Object.DestroyImmediate(zone.gameObject);
                    sb.AppendLine($"Deleted '{path}' (created by the add).");
                }
            }
        }

        /// <summary>
        /// Loads a recorded skybox. Built-in materials share the pseudo-path 'Resources/unity_builtin_extra', which
        /// LoadAssetAtPath cannot load (SetDesign S46: Default-Skybox was never restored), so those go by name.
        /// </summary>
        static Material? LoadSkybox(string path, string name)
        {
            if (path == "Resources/unity_builtin_extra")
                return name.Length > 0 ? AssetDatabase.GetBuiltinExtraResource<Material>(name + ".mat") : null;
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Material material && (name.Length == 0 || material.name == name))
                    return material;
            }
            return null;
        }

        /// <summary>GRC_PWSky lives in Assembly-CSharp (no asmdef), so it is found through its GaiaCore base type.</summary>
        static GaiaRuntimeComponent CreatePWSkyComponent()
        {
            foreach (Type type in TypeCache.GetTypesDerivedFrom<GaiaRuntimeComponent>())
            {
                if (type.Name == "GRC_PWSky" && !type.IsAbstract)
                    return (GaiaRuntimeComponent)ScriptableObject.CreateInstance(type);
            }
            throw new Exception("Gaia.GRC_PWSky not found. Install Procedural Worlds Sky from the Gaia Setup window.");
        }

        /// <summary>Children of the Gaia Lighting object whose names match none of the PW Sky prefabs.</summary>
        static List<string> ForeignLightingChildren(GameObject lighting)
        {
            HashSet<string> expected = new HashSet<string>();
            foreach (Type type in TypeCache.GetTypesDerivedFrom<GaiaRuntimeComponent>())
            {
                if (type.Name != "GRC_PWSky")
                    continue;
                MethodInfo? getPrefabs = type.GetMethod("GetSkyPrefabs", BindingFlags.Public | BindingFlags.Static);
                object? prefabs = getPrefabs?.Invoke(null, null);
                if (prefabs == null)
                    break;
                foreach (FieldInfo field in prefabs.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (field.FieldType == typeof(GameObject) && field.GetValue(prefabs) is GameObject prefab && prefab != null)
                        expected.Add(prefab.name);
                }
            }

            List<string> foreign = new List<string>();
            foreach (Transform child in lighting.transform)
            {
                // AddToScene strips "(Clone)" from most children but not the URP post-processing one (SetDesign S46).
                string name = child.name.EndsWith("(Clone)") ? child.name.Substring(0, child.name.Length - "(Clone)".Length).TrimEnd() : child.name;
                if (!expected.Contains(name))
                    foreign.Add(child.name);
            }
            return foreign;
        }

        static PW_VFX_Atmosphere? FindAtmosphere()
        {
            // Instance is a static set in OnEnable only; look the component up when it is unset.
            PW_VFX_Atmosphere? atmosphere = PW_VFX_Atmosphere.Instance;
            if (atmosphere == null)
                atmosphere = UnityEngine.Object.FindAnyObjectByType<PW_VFX_Atmosphere>();
            return atmosphere;
        }

        static ProceduralWorldsGlobalWeather? FindWeather()
        {
            ProceduralWorldsGlobalWeather? weather = ProceduralWorldsGlobalWeather.Instance;
            if (weather == null)
                weather = UnityEngine.Object.FindAnyObjectByType<ProceduralWorldsGlobalWeather>();
            return weather;
        }

        /// <summary>
        /// Edit-mode tick (ENTRY-433): the sun angle is set in PW_VFX_Atmosphere.LateUpdate (private) and fog/ambient
        /// in UpdateSystem, and neither runs from an MCP call -- QueuePlayerLoopUpdate did not deliver one. LateUpdate
        /// calls UpdateSystem itself in edit mode. Play mode ticks on its own.
        /// </summary>
        static string TickAtmosphere()
        {
            if (Application.isPlaying)
                return "Play mode: the atmosphere updates on its own each frame.";
            PW_VFX_Atmosphere? atmosphere = FindAtmosphere();
            if (atmosphere == null)
                return "** No PW_VFX_Atmosphere in the scene -- nothing ticked. **";

            MethodInfo? lateUpdate = typeof(PW_VFX_Atmosphere).GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (lateUpdate == null)
                return "** PW_VFX_Atmosphere.LateUpdate not found (Gaia changed) -- the sun angle was NOT applied in edit mode. **";
            lateUpdate.Invoke(atmosphere, null);
            SceneView.RepaintAll();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            return "Atmosphere ticked (sun angle, fog and ambient applied).";
        }

        static void AppendSkyReport(StringBuilder sb)
        {
            bool sky = GaiaUtils.CheckIfPWSkyExists();
            sb.AppendLine($"PW Sky: {(sky ? "present" : "not in scene")} | play mode: {Application.isPlaying}");
            if (sky)
            {
                GaiaTimeOfDay tod = PWSkyStandalone.Instance.m_gaiaTimeOfDay;
                sb.AppendLine($"Time of day: {tod.m_todHour:D2}:{Mathf.FloorToInt(tod.m_todMinutes):D2} | cycle running: {tod.m_todEnabled} | time scale: {tod.m_todDayTimeScale:F2} | sun rotation (Y): {GaiaAPI.GetTimeOfDaySunRotation():F1}");
            }

            ProceduralWorldsGlobalWeather? weather = FindWeather();
            if (weather != null)
            {
                AppendLight(sb, "Sun", weather.m_sunLight);
                AppendLight(sb, "Moon", weather.m_moonLight);
                sb.AppendLine($"Weather: raining={weather.IsRaining} snowing={weather.IsSnowing} | FX disabled={weather.m_disableWeatherFX} | run in editor={weather.RunInEditor} | instant transitions={weather.m_instantStartStop}");
            }

            int activeDirectional = 0;
            foreach (Light light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsInactive.Exclude))
            {
                if (light.type == LightType.Directional && light.isActiveAndEnabled)
                    activeDirectional++;
            }
            sb.AppendLine($"Active directional lights: {activeDirectional}{(activeDirectional == 0 ? "  ** NONE -- terrain trees stop rendering and the terrain looks flat (ENTRY-433) **" : "")}");

            Material? skybox = RenderSettings.skybox;
            sb.AppendLine($"Skybox: {(skybox == null ? "<none>" : $"'{skybox.name}' shader '{skybox.shader.name}'")} | fog: {(RenderSettings.fog ? $"on, {RenderSettings.fogMode}, density {RenderSettings.fogDensity:F5}" : "off")} | ambient: {RenderSettings.ambientMode}");
        }

        static void AppendLight(StringBuilder sb, string label, Light? light)
        {
            if (light == null)
            {
                sb.AppendLine($"{label}: none");
                return;
            }
            Vector3 euler = light.transform.eulerAngles;
            sb.AppendLine($"{label}: '{light.name}' active={light.isActiveAndEnabled} intensity {light.intensity:F2} rotation ({euler.x:F1}, {euler.y:F1}, {euler.z:F1})");
        }

        static string HierarchyPath(Transform transform)
        {
            string path = transform.name;
            Transform? parent = transform.parent;
            while (parent != null)
            {
                path = parent.name + "/" + path;
                parent = parent.parent;
            }
            return path;
        }

        /// <summary>Finds a GameObject by hierarchy path, inactive ones included (GameObject.Find skips them).</summary>
        static GameObject? FindByHierarchyPath(Scene scene, string path)
        {
            int slash = path.IndexOf('/');
            string rootName = slash < 0 ? path : path.Substring(0, slash);
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                if (root.name != rootName)
                    continue;
                if (slash < 0)
                    return root;
                Transform? found = root.transform.Find(path.Substring(slash + 1));
                if (found != null)
                    return found.gameObject;
            }
            return null;
        }
    }
}
#endif
