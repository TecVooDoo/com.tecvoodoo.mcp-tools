#if HAS_GAIA
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Gaia;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Gaia.Editor
{
    public partial class Tool_Gaia
    {
        [AiTool("gaia-stamp", Title = "Gaia / Stamp")]
        [Description(@"Stamps a heightmap image onto the terrain through Gaia's stamper (GaiaAPI.GaiaStamp). Synchronous -- the terrain is changed when this returns (unlike spawning).
'stampPath' = project path of the stamp texture (e.g. 'Assets/Procedural Worlds/Packages - Install/Stamps/Mountains/2k Mountain 1.exr').
Placement defaults to the first Gaia terrain, centred and covering it (Gaia's own Fit To Terrain values): width = 100 (% of one terrain tile's size), height = 5 (vertical scale), y = terrain base.
operation: RaiseHeight (default) | LowerHeight | BlendHeight | SetHeight | AddHeight | SubtractHeight | MixHeight.
Edit mode only. Run gaia-sea-level afterwards: a stamp can leave terrain below sea level, where spawns place nothing.
Safe outside Gaia-built scenes: the stamper's editor-update hook is removed in the same call (a scripted stamp otherwise left an endless per-frame exception that dropped MCP, ENTRY-438).")]
        public string Stamp(
            [Description("Project path of the stamp texture.")] string stampPath,
            [Description("Feature operation. Default RaiseHeight.")] string? operation = null,
            [Description("World X of the stamp centre. Omit to centre on the first terrain.")] float? x = null,
            [Description("World Z of the stamp centre. Omit to centre on the first terrain.")] float? z = null,
            [Description("Stamp base height (world Y). Omit for the terrain's base.")] float? y = null,
            [Description("Width as a percentage of one terrain tile's size. Default 100.")] float width = 100f,
            [Description("Vertical scale. Default 5.")] float height = 5f,
            [Description("Rotation about Y in degrees. Default 0.")] float rotation = 0f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (Application.isPlaying)
                    throw new Exception("Gaia stamping is edit-mode only.");

                Texture2D? texture = AssetDatabase.LoadAssetAtPath<Texture2D>(stampPath);
                if (texture == null)
                    throw new Exception($"No Texture2D at '{stampPath}'.");

                GaiaConstants.FeatureOperation featureOperation = GaiaConstants.FeatureOperation.RaiseHeight;
                if (!string.IsNullOrWhiteSpace(operation) && !Enum.TryParse(operation!.Trim(), true, out featureOperation))
                    throw new Exception($"Unknown operation '{operation}'. Use: {string.Join(", ", Enum.GetNames(typeof(GaiaConstants.FeatureOperation)))}.");

                System.Collections.Generic.List<Terrain> terrains = GetTerrains();
                if (terrains.Count == 0)
                    throw new Exception("No Gaia terrain in the scene -- the stamper needs a terrain under it. Run gaia-create-world first.");
                Terrain target = terrains[0];
                Bounds bounds = new Bounds();
                TerrainHelper.GetTerrainBounds(target, ref bounds);

                StamperSettings settings = ScriptableObject.CreateInstance<StamperSettings>();
                settings.m_x = x ?? bounds.center.x;
                settings.m_z = z ?? bounds.center.z;
                settings.m_y = y ?? target.GetPosition().y;
                settings.m_width = width;
                settings.m_height = height;
                settings.m_rotation = rotation;
                settings.m_operation = featureOperation;
                // Mirrors Gaia's own APITest.StampButton: the stamp image rides in the first image mask.
                settings.m_imageMasks = new ImageMask[] { new ImageMask { m_operation = ImageMaskOperation.ImageMask, ImageMaskTexture = texture } };

                float maxBefore = WorldMaxHeight(target);
                try
                {
                    GaiaAPI.GaiaStamp(settings, true);
                }
                finally
                {
                    // UWF_Unity v1.33 / ENTRY-438: Stamper.Stamp() ends with StartEditorUpdates(), and ~10 s later
                    // that callback's cleanup (AfterSessionPlaybackCleanup) throws in a scene Gaia did not build
                    // BEFORE it unsubscribes -- an endless per-frame exception that dropped MCP until a restart.
                    // Unhook every stamper in this same call. Cost: Gaia's temporary "Session Stamper" is left in place.
                    // (Spawner is safe: its EditorUpdate unsubscribes before the cleanup, so it cannot loop.)
                    foreach (Stamper stamper in UnityEngine.Object.FindObjectsByType<Stamper>(FindObjectsInactive.Include))
                        stamper.StopEditorUpdates();
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Stamped '{texture.name}' ({featureOperation}) at ({settings.m_x:F1}, {settings.m_y:F1}, {settings.m_z:F1}), width {width}%, height {height}, rotation {rotation}.");
                sb.AppendLine($"'{target.name}' max height {maxBefore:F1} -> {WorldMaxHeight(target):F1} m");
                AppendTerrainReport(sb, GaiaAPI.GetSeaLevel());
                return sb.ToString();
            });
        }
    }
}
#endif
