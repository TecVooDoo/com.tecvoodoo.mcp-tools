#if HAS_GAIA
#nullable enable
using System;
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
        [AiTool("gaia-sea-level", Title = "Gaia / Sea Level")]
        [Description(@"Gets or sets the Gaia sea level (metres), and reports each terrain's height range against it.
Spawn masks are sea-level relative: terrain below sea level gets NOTHING spawned and Gaia reports success anyway (ENTRY-431 -- a mis-entered value put sea level at 60 m over a 32.7 m terrain).
Omit 'value' to read. In edit mode the value is written to the scene's Gaia session; in play mode to the water system.")]
        public string SeaLevel(
            [Description("New sea level in metres. Omit to only read.")] float? value = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                if (value.HasValue)
                {
                    if (!Application.isPlaying && GaiaSessionManager.GetSessionManager(false, false) == null)
                        throw new Exception("No Gaia session manager in the scene, so there is nowhere to store a sea level. Create a world first.");
                    float before = GaiaAPI.GetSeaLevel();
                    GaiaAPI.SetSeaLevel(value.Value);
                    sb.AppendLine($"Sea level {before:F1} -> {GaiaAPI.GetSeaLevel():F1} m");
                }
                AppendTerrainReport(sb, GaiaAPI.GetSeaLevel());
                return sb.ToString();
            });
        }
    }
}
#endif
