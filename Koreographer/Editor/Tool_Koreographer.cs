#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using SonicBloom.Koreo;

namespace TecVooDoo.MCPTools.Editor
{
    [McpPluginToolType]
    public partial class Tool_Koreographer
    {
        [McpPluginTool("koreo-query", Title = "Koreographer / Query State")]
        [Description(@"Returns all loaded Koreography assets, their source clip names, track event IDs, and the current beat time.
Use this to discover available event IDs before registering callbacks in game code.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                Koreographer? koreo = Koreographer.Instance;
                if (koreo == null)
                    return "ERROR: Koreographer instance not found. Add a Koreographer component to the scene.";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== Koreographer State ===");

                int count = koreo.GetNumLoadedKoreography();
                sb.AppendLine($"  Loaded Koreographies: {count}");

                float beatTime = Koreographer.GetBeatTime();
                sb.AppendLine($"  Current Beat Time:    {beatTime:F3}");

                List<Koreography> allKoreo = new List<Koreography>();
                koreo.GetAllLoadedKoreography(allKoreo);

                for (int i = 0; i < allKoreo.Count; i++)
                {
                    Koreography k = allKoreo[i];
                    sb.AppendLine($"\n  [{i}] Koreography: {k.SourceClipName}");

                    string[] eventIDs = k.GetEventIDs();
                    sb.AppendLine($"      Tracks ({eventIDs.Length}):");
                    for (int j = 0; j < eventIDs.Length; j++)
                    {
                        sb.AppendLine($"        \"{eventIDs[j]}\"");
                    }
                }

                return sb.ToString();
            });
        }

        [McpPluginTool("koreo-beattime", Title = "Koreographer / Get Beat Time")]
        [Description(@"Returns the current beat time for a specific Koreography track (or global if trackName is omitted).
subdivision divides each beat: 1=quarter notes, 2=eighth, 4=sixteenth.
Useful for syncing visuals or game events to music in editor scripts.")]
        public string GetBeatTime(
            [Description("Source clip name of the Koreography to query. Omit for global beat time.")]
            string? trackName = null,
            [Description("Beat subdivision: 1=quarter, 2=eighth, 4=sixteenth. Default 1.")]
            int subdivision = 1
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (Koreographer.Instance == null)
                    return "ERROR: Koreographer instance not found.";

                float beatTime = Koreographer.GetBeatTime(trackName, subdivision);
                float beatDelta = Koreographer.GetBeatTimeDelta(trackName, subdivision);

                return $"BeatTime={beatTime:F4}  Delta={beatDelta:F6}  Track={(trackName ?? "global")}  Subdivision=1/{subdivision}";
            });
        }
    }
}
