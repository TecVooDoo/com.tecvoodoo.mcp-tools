#if HAS_GAIA
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Gaia;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Gaia.Editor
{
    public partial class Tool_Gaia
    {
        // SessionState survives domain reloads (the static event subscription below does not),
        // so gaia-query can still flag a requested sea level that was never applied.
        const string PendingSeaLevelKey = "TMCP.Gaia.RequestedSeaLevel";

        [AiTool("gaia-create-world", Title = "Gaia / Create World")]
        [Description(@"Creates a Gaia terrain world (GaiaAPI.CreateGaiaWorld) with xTiles x zTiles tiles of tileSize metres, tileHeight metres of height range, and a sea level.
Edit mode only. REFUSES when Gaia terrains already exist unless replaceExisting=true -- the MCP client may re-send a slow call, and a second run would rebuild the world.
The terrain is created ASYNCHRONOUSLY: this returns before it exists. Read it with gaia-query on a later call.
Gaia itself ignores the requested sea level and derives one from the tile height when creation finishes; this tool re-applies 'seaLevel' at that moment (Gaia's OnWorldCreated event). gaia-query reports any mismatch.
Side effect measured in ENTRY-431: it adds the active scene to EditorBuildSettings.")]
        public string CreateWorld(
            [Description("Tile edge length in metres (Gaia default 1024).")] int tileSize = 1024,
            [Description("Tile height range in metres (Gaia default 1000).")] float tileHeight = 1000f,
            [Description("Tiles along X. Default 1.")] int xTiles = 1,
            [Description("Tiles along Z. Default 1.")] int zTiles = 1,
            [Description("Sea level in metres (Gaia default 50).")] int seaLevel = 50,
            [Description("Create even though Gaia terrains already exist. Default false.")] bool replaceExisting = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (Application.isPlaying)
                    throw new Exception("Gaia world creation is edit-mode only.");
                if (tileSize < 32 || tileHeight <= 0f || xTiles < 1 || zTiles < 1)
                    throw new Exception("tileSize must be >= 32, tileHeight > 0, xTiles and zTiles >= 1.");

                GaiaSessionManager? session = GaiaSessionManager.GetSessionManager(false, false);
                if (session != null && session.m_worldCreationRunning)
                    throw new Exception("A Gaia world creation is already running. Wait, then check with gaia-query.");

                int existing = GetTerrains().Count;
                if (existing > 0 && !replaceExisting)
                    throw new Exception($"{existing} terrain(s) already exist. Pass replaceExisting=true to create anyway.");

                WorldCreationSettings settings = ScriptableObject.CreateInstance<WorldCreationSettings>();
                settings.m_tileSize = tileSize;
                settings.m_tileHeight = tileHeight;
                settings.m_xTiles = xTiles;
                settings.m_zTiles = zTiles;
                settings.m_seaLevel = seaLevel;

                // Measured SetDesign 2026-10-10: requested 50, got 6.3. GaiaSessionManager overwrites the session
                // sea level from the tile height near the end of its creation coroutine (~L4466) and never reads
                // m_seaLevel, so re-apply it when Gaia announces the world is done.
                SessionState.SetFloat(PendingSeaLevelKey, seaLevel);
                UnsubscribeSeaLevelHandlers();
                GaiaSessionManager.OnWorldCreated += ApplyPendingSeaLevel;
                GaiaSessionManager.OnWorldCreationCancelled += CancelPendingSeaLevel;

                GaiaAPI.CreateGaiaWorld(settings, true);

                return $"World creation started: {xTiles} x {zTiles} tile(s) of {tileSize} m, height range {tileHeight} m, sea level {seaLevel} m (applied when creation finishes). " +
                       "It completes on later editor ticks -- call gaia-query on a separate call to confirm the terrain and the sea level.";
            });
        }

        static void UnsubscribeSeaLevelHandlers()
        {
            GaiaSessionManager.OnWorldCreated -= ApplyPendingSeaLevel;
            GaiaSessionManager.OnWorldCreationCancelled -= CancelPendingSeaLevel;
        }

        static void ApplyPendingSeaLevel()
        {
            // Unsubscribe FIRST, so a throw below can never leave a handler firing again (UWF_Unity v1.33).
            UnsubscribeSeaLevelHandlers();
            float requested = SessionState.GetFloat(PendingSeaLevelKey, float.NaN);
            if (float.IsNaN(requested))
                return;
            try
            {
                GaiaAPI.SetSeaLevel(requested);
                SessionState.EraseFloat(PendingSeaLevelKey);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TMCP Gaia] Could not apply the requested sea level {requested} m after world creation: {ex.Message}. Run gaia-sea-level value={requested}.");
            }
        }

        static void CancelPendingSeaLevel()
        {
            UnsubscribeSeaLevelHandlers();
            SessionState.EraseFloat(PendingSeaLevelKey);
        }
    }
}
#endif
