#if HAS_GAIA
#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using com.IvanMurzak.McpPlugin;
using Gaia;
using UnityEngine;

namespace MCPTools.Gaia.Editor
{
    /// <summary>
    /// Gaia Pro VS (Procedural Worlds) tool group: wraps the public GaiaAPI / session API that
    /// Sandbox ENTRY-431 drove end-to-end through script-execute.
    /// </summary>
    /// <remarks>
    /// Measured rules this group encodes (ENTRY-431 / 436 / 446):
    /// world creation and tree/detail spawning finish on later editor ticks, so results are read
    /// on a SECOND call (gaia-query); stamping is synchronous; spawn ONE spawner per call (a batch
    /// loop places no trees or details); a spawn on terrain below sea level places nothing and
    /// reports success, so gaia-spawn refuses it.
    /// </remarks>
    [AiToolType]
    public partial class Tool_Gaia
    {
        /// <summary>Gaia-managed terrains in the open scenes, world-map preview terrains excluded.</summary>
        static List<Terrain> GetTerrains()
        {
            List<Terrain> result = new List<Terrain>();
            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                if (terrain == null || terrain.terrainData == null)
                    continue;
                if (TerrainHelper.IsWorldMapTerrain(terrain))
                    continue;
                result.Add(terrain);
            }
            return result;
        }

        static float WorldMinHeight(Terrain terrain)
        {
            return terrain.GetPosition().y + terrain.terrainData.bounds.min.y;
        }

        static float WorldMaxHeight(Terrain terrain)
        {
            return terrain.GetPosition().y + terrain.terrainData.bounds.max.y;
        }

        /// <summary>Exactly-named Spawner in the open scenes (ENTRY-436: name searches hit leftover spawners).</summary>
        static Spawner FindSpawner(string name)
        {
            Spawner[] spawners = UnityEngine.Object.FindObjectsByType<Spawner>(FindObjectsInactive.Include);
            Spawner? match = null;
            foreach (Spawner spawner in spawners)
            {
                if (spawner.name != name)
                    continue;
                if (match != null)
                    throw new Exception($"Two spawners are named '{name}'. Rename one -- this tool addresses spawners by exact name only.");
                match = spawner;
            }
            if (match == null)
                throw new Exception($"No spawner named exactly '{name}'. Use gaia-query to list spawners.");
            return match;
        }

        static BiomePreset FindBiomePreset(string name)
        {
            List<BiomePreset> presets = GaiaUtils.SearchProjectForBiomePresets(false);
            foreach (BiomePreset preset in presets)
            {
                if (preset != null && preset.name == name)
                    return preset;
            }
            throw new Exception($"No BiomePreset asset named '{name}'. Use gaia-query to list biome presets.");
        }

        static void AppendTerrainReport(StringBuilder sb, float seaLevel, bool includeDetailDensity = false)
        {
            List<Terrain> terrains = GetTerrains();
            sb.AppendLine($"Terrains: {terrains.Count} | Sea level: {seaLevel:F1} m");

            float requested = UnityEditor.SessionState.GetFloat(PendingSeaLevelKey, float.NaN);
            if (!float.IsNaN(requested) && Mathf.Abs(requested - seaLevel) > 0.01f)
                sb.AppendLine($"  ** gaia-create-world requested sea level {requested:F1} m but it is {seaLevel:F1} m -- not applied yet (creation still running, or a domain reload dropped the hook). If creation is done, run gaia-sea-level value={requested:F1}. **");

            foreach (Terrain terrain in terrains)
            {
                TerrainData data = terrain.terrainData;
                float min = WorldMinHeight(terrain);
                float max = WorldMaxHeight(terrain);
                string flag = max < seaLevel ? "  ** ENTIRELY BELOW SEA LEVEL -- spawns will place nothing **"
                    : min < seaLevel ? $"  ({PercentBelow(terrain, seaLevel):F0}% of the area below sea level -- spawns skip it)" : "";
                sb.AppendLine($"  '{terrain.name}' size=({data.size.x:F0}, {data.size.y:F0}, {data.size.z:F0}) height {min:F1}..{max:F1} m, " +
                              $"trees={data.treeInstanceCount}, treeProtos={data.treePrototypes.Length}, detailProtos={data.detailPrototypes.Length}, layers={data.terrainLayers.Length}{flag}");
                if (includeDetailDensity && data.detailPrototypes.Length > 0)
                    AppendDetailDensity(sb, data);
                if (!terrain.drawHeightmap)
                    sb.AppendLine("    ** drawHeightmap = false: the ground is INVISIBLE (a scripted spawn did this in ENTRY-431). Set Terrain.drawHeightmap back to true. **");
            }
        }

        /// <summary>
        /// Share of the terrain's area whose ground is below sea level, from a 64 x 64 sample grid.
        /// "Partly below" alone did not tell a SetDesign trial how far to lower the sea level (2026-10-10).
        /// </summary>
        static float PercentBelow(Terrain terrain, float seaLevel)
        {
            const int grid = 64;
            TerrainData data = terrain.terrainData;
            float originY = terrain.GetPosition().y;
            int below = 0;
            for (int z = 0; z < grid; z++)
            {
                for (int x = 0; x < grid; x++)
                {
                    float height = data.GetInterpolatedHeight((x + 0.5f) / grid, (z + 0.5f) / grid) + originY;
                    if (height < seaLevel)
                        below++;
                }
            }
            return 100f * below / (grid * grid);
        }

        /// <summary>Per-prototype detail density sums, so a grass/detail spawn can be verified (reads every detail layer once).</summary>
        static void AppendDetailDensity(StringBuilder sb, TerrainData data)
        {
            int res = data.detailResolution;
            long total = 0;
            StringBuilder perProto = new StringBuilder();
            for (int layer = 0; layer < data.detailPrototypes.Length; layer++)
            {
                int[,] cells = data.GetDetailLayer(0, 0, res, res, layer);
                long sum = 0;
                foreach (int density in cells)
                    sum += density;
                total += sum;
                if (sum > 0)
                    perProto.Append($" [{layer}]={sum}");
            }
            sb.AppendLine($"    detail density sum={total} (resolution {res}){(perProto.Length > 0 ? " per prototype:" + perProto : "")}");
        }
    }
}
#endif
