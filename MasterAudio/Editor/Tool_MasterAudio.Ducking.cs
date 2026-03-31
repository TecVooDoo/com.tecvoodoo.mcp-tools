#if HAS_MASTERAUDIO
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using DarkTonic.MasterAudio;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_MasterAudio
    {
        [McpPluginTool("ma-configure-ducking", Title = "Master Audio / Configure Ducking")]
        [Description(@"Configure music ducking for a sound group.
Add a group to the duck list so music volume ducks when the group plays, or remove it.
duckedVolCut: volume reduction in dB when ducked (default -6).
unduckTime: seconds to restore music volume after sound ends (default 1.0).
riseVolStart: normalized position (0-1) of remaining sound when volume restore begins (default 0.5).")]
        public string ConfigureDucking(
            [Description("Sound group name to add/remove from duck list.")]
            string groupName,
            [Description("Action: add or remove.")]
            string action,
            [Description("Volume cut in dB when ducked. Default -6.")]
            float? duckedVolCut = null,
            [Description("Time in seconds to restore volume. Default 1.0.")]
            float? unduckTime = null,
            [Description("Normalized position (0-1) of sound remaining when restore begins. Default 0.5.")]
            float? riseVolStart = null
        )
        {
            if (string.IsNullOrEmpty(groupName))
                throw new ArgumentException("groupName cannot be null or empty.", nameof(groupName));
            if (string.IsNullOrEmpty(action))
                throw new ArgumentException("action cannot be null or empty.", nameof(action));

            return MainThread.Instance.Run(() =>
            {
                if (MasterAudio.SafeInstance == null)
                    throw new InvalidOperationException("MasterAudio instance not found in scene.");

                string act = action.ToLowerInvariant().Trim();

                switch (act)
                {
                    case "add":
                        float volCut = duckedVolCut ?? -6f;
                        float unduck = unduckTime ?? 1f;
                        float rise = riseVolStart ?? 0.5f;

                        MasterAudio.AddSoundGroupToDuckList(groupName, rise, volCut, unduck);
                        return $"OK: Group '{groupName}' added to duck list. VolCut={volCut:F1}dB UnduckTime={unduck:F2}s RiseStart={rise:F2}";

                    case "remove":
                        MasterAudio.RemoveSoundGroupFromDuckList(groupName);
                        return $"OK: Group '{groupName}' removed from duck list.";

                    default:
                        throw new ArgumentException(
                            $"Unknown action '{action}'. Valid: add, remove.",
                            nameof(action));
                }
            });
        }
    }
}
#endif
