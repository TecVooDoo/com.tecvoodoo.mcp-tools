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
create-biome: instantiate BiomePreset 'name' (prototypes auto-assigned to all terrains); lists its spawners. IDEMPOTENT: an existing '<name> Biome' is listed, not re-created (the MCP client re-sends slow calls); replaceExisting=true rebuilds it.
spawn: run the ONE Spawner named exactly 'name'. REFUSES when every terrain is below sea level (Gaia would place nothing and report success).
RULES (ENTRY-431/436/446): ONE spawner per call -- a loop in one call placed 0 trees/details. TEXTURE spawners before tree/detail spawners.
Trees and details land on LATER editor ticks: check with gaia-query on a separate call. If a fresh world's first tree spawn shows 0, spawn it again.
Re-spawning replaces output. Spawning writes into the TerrainData asset.")]
        public string Spawn(
            [Description("'create-biome' or 'spawn'.")] string action,
            [Description("BiomePreset asset name (create-biome) or exact Spawner GameObject name (spawn).")] string name,
            [Description("spawn only: true = whole world (default), false = the spawner's local range around its position.")] bool allTerrains = true,
            [Description("create-biome only: delete an existing '<name> Biome' root and create it again. Default false.")] bool replaceExisting = false
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
                    return CreateBiome(name.Trim(), replaceExisting);
                if (a == "spawn")
                    return RunSpawner(name.Trim(), allTerrains);
                throw new Exception($"Unknown action '{action}'. Use: create-biome, spawn.");
            });
        }

        static string CreateBiome(string presetName, bool replaceExisting)
        {
            BiomePreset preset = FindBiomePreset(presetName);

            // BiomePreset.CreateBiome names its root "<preset> Biome" (BiomePreset.cs ~L173).
            string rootName = preset.name + " Biome";
            List<BiomeController> existing = new List<BiomeController>();
            foreach (BiomeController candidate in UnityEngine.Object.FindObjectsByType<BiomeController>(FindObjectsInactive.Include))
            {
                if (candidate.name == rootName)
                    existing.Add(candidate);
            }

            if (existing.Count > 0 && !replaceExisting)
            {
                StringBuilder already = new StringBuilder();
                already.AppendLine($"Biome '{rootName}' already exists ({existing.Count} root(s)) -- NOT created again (a re-sent call would otherwise duplicate every spawner). Pass replaceExisting=true to rebuild it.");
                if (existing.Count > 1)
                    already.AppendLine("** More than one root: spawning by name will refuse until the extras are deleted, or rebuild with replaceExisting=true. **");
                AppendSpawnerList(already, existing[0]);
                return already.ToString();
            }

            foreach (BiomeController old in existing)
                UnityEditor.Undo.DestroyObjectImmediate(old.gameObject);

            BiomeController controller = preset.CreateBiome(true);
            if (controller == null)
                throw new Exception($"BiomePreset '{presetName}' returned no BiomeController.");

            StringBuilder sb = new StringBuilder();
            string replaced = existing.Count > 0 ? $" (replaced {existing.Count} existing root(s))" : "";
            sb.AppendLine($"Created biome '{controller.name}' from '{presetName}'{replaced} with {controller.m_autoSpawners.Count} spawner(s). Nothing is spawned yet.");
            AppendSpawnerList(sb, controller);
            return sb.ToString();
        }

        static void AppendSpawnerList(StringBuilder sb, BiomeController controller)
        {
            sb.AppendLine("Spawn each with gaia-spawn action='spawn', ONE per call, texture spawners first:");
            foreach (AutoSpawner autoSpawner in controller.m_autoSpawners)
            {
                if (autoSpawner.spawner != null)
                    sb.AppendLine($"  '{autoSpawner.spawner.name}'");
            }
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
