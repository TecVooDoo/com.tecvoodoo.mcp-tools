#if HAS_GAIA
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Gaia;
using UnityEngine;

namespace MCPTools.Gaia.Editor
{
    public partial class Tool_Gaia
    {
        [AiTool("gaia-spawn", Title = "Gaia / Spawn")]
        [Description(@"Gaia biome spawning, ONE step per call.
action='create-biome': instantiate the BiomePreset named 'name' (BiomePreset.CreateBiome, prototypes auto-assigned to all terrains); returns its spawner names.
action='spawn': run the ONE Spawner GameObject named exactly 'name' (Spawner.Spawn). REFUSES when every terrain is below sea level -- Gaia would place nothing and report success.
RULES (measured, ENTRY-431/436/446): spawn ONE spawner per call -- looping several in one call placed 0 trees and 0 details. Run TEXTURE spawners before tree and detail spawners (their rules mask on terrain textures).
Trees and details land on LATER editor ticks: read the result with gaia-query on a separate call. On a freshly API-created world the first tree spawn can place 0 -- spawn that spawner again.
Re-spawning a spawner replaces its output rather than duplicating it. Spawning writes into the TerrainData asset.")]
        public string Spawn(
            [Description("'create-biome' or 'spawn'.")] string action,
            [Description("BiomePreset asset name (create-biome) or exact Spawner GameObject name (spawn).")] string name,
            [Description("spawn only: true = whole world (default), false = the spawner's local range around its position.")] bool allTerrains = true
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (Application.isPlaying)
                    throw new Exception("Gaia spawning is edit-mode only.");
                if (string.IsNullOrWhiteSpace(name))
                    throw new Exception("'name' is required.");

                string a = string.IsNullOrWhiteSpace(action) ? "" : action.Trim().ToLowerInvariant();
                if (a == "create-biome")
                    return CreateBiome(name.Trim());
                if (a == "spawn")
                    return RunSpawner(name.Trim(), allTerrains);
                throw new Exception($"Unknown action '{action}'. Use: create-biome, spawn.");
            });
        }

        static string CreateBiome(string presetName)
        {
            BiomePreset preset = FindBiomePreset(presetName);
            BiomeController controller = preset.CreateBiome(true);
            if (controller == null)
                throw new Exception($"BiomePreset '{presetName}' returned no BiomeController.");

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Created biome '{controller.name}' from '{presetName}' with {controller.m_autoSpawners.Count} spawner(s). Nothing is spawned yet.");
            sb.AppendLine("Spawn each with gaia-spawn action='spawn', ONE per call, texture spawners first:");
            foreach (AutoSpawner autoSpawner in controller.m_autoSpawners)
            {
                if (autoSpawner.spawner != null)
                    sb.AppendLine($"  '{autoSpawner.spawner.name}'");
            }
            return sb.ToString();
        }

        static string RunSpawner(string spawnerName, bool allTerrains)
        {
            Spawner spawner = FindSpawner(spawnerName);
            float seaLevel = GaiaAPI.GetSeaLevel();

            List<Terrain> terrains = GetTerrains();
            if (terrains.Count == 0)
                throw new Exception("No Gaia terrain in the scene to spawn on.");

            bool anyAboveSea = false;
            foreach (Terrain terrain in terrains)
            {
                if (WorldMaxHeight(terrain) >= seaLevel)
                {
                    anyAboveSea = true;
                    break;
                }
            }
            if (!anyAboveSea)
            {
                StringBuilder refusal = new StringBuilder();
                refusal.AppendLine($"REFUSED: every terrain is below the sea level ({seaLevel:F1} m), so Gaia would place nothing and report success.");
                refusal.AppendLine("Lower it with gaia-sea-level, or raise the terrain with gaia-stamp, then spawn again.");
                AppendTerrainReport(refusal, seaLevel);
                throw new Exception(refusal.ToString());
            }

            spawner.Spawn(allTerrains);

            return $"Spawn started for '{spawner.name}' ({(allTerrains ? "all terrains" : "local range")}, sea level {seaLevel:F1} m). " +
                   "GameObject and texture output is placed now; trees and details land on later editor ticks -- read them with gaia-query on a separate call. " +
                   "If a tree spawner shows 0 trees on a fresh world, spawn it once more.";
        }
    }
}
#endif
