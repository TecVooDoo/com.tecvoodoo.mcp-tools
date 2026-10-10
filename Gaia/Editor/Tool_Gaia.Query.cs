#if HAS_GAIA
#nullable enable
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
        [AiTool("gaia-query", Title = "Gaia / Query", ReadOnlyHint = true)]
        [Description(@"Reports the Gaia world in the open scene (read-only): sea level, every terrain's size, world height range, tree count and prototype counts,
plus warnings for terrain below sea level and for invisible ground (drawHeightmap=false).
Also lists the scene's Spawners and, with includeBiomePresets, every BiomePreset asset in the project.
Gaia finishes world creation and tree/detail spawning on LATER editor ticks: call this on a separate call after gaia-create-world / gaia-spawn to read the result.")]
        public string Query(
            [Description("List the scene's Spawner GameObjects. Default true.")] bool includeSpawners = true,
            [Description("List BiomePreset assets in the project (an AssetDatabase search). Default false.")] bool includeBiomePresets = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                GaiaSessionManager? session = GaiaSessionManager.GetSessionManager(false, false);
                sb.AppendLine(session == null ? "Session manager: none in scene" : $"Session manager: '{session.name}', world creation running: {session.m_worldCreationRunning}");
                AppendTerrainReport(sb, GaiaAPI.GetSeaLevel());

                if (includeSpawners)
                {
                    Spawner[] spawners = Object.FindObjectsByType<Spawner>(FindObjectsInactive.Include);
                    sb.AppendLine($"Spawners: {spawners.Length}");
                    foreach (Spawner spawner in spawners)
                        sb.AppendLine($"  '{spawner.name}' (active={spawner.gameObject.activeInHierarchy})");
                }

                if (includeBiomePresets)
                {
                    List<BiomePreset> presets = GaiaUtils.SearchProjectForBiomePresets(false);
                    sb.AppendLine($"Biome presets: {presets.Count}");
                    foreach (BiomePreset preset in presets)
                    {
                        if (preset != null)
                            sb.AppendLine($"  '{preset.name}' ({preset.m_spawnerPresetList.Count} spawners)");
                    }
                }
                return sb.ToString();
            });
        }
    }
}
#endif
