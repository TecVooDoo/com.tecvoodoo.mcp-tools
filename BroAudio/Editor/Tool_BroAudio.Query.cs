#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using Ami.BroAudio;
using Ami.BroAudio.Data;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_BroAudio
    {
        [McpPluginTool("bro-query", Title = "Bro Audio / Query State")]
        [Description(@"Lists all registered sound entities, their names, and audio types.
Also shows master volume per audio type.
Use this to discover entity names before calling bro-play or bro-stop.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== Bro Audio State ===");

                AudioEntity[] entities = Resources.FindObjectsOfTypeAll<AudioEntity>();

                if (entities.Length > 0)
                {
                    sb.AppendLine($"\n-- Sound Entities ({entities.Length}) --");
                    foreach (AudioEntity entity in entities)
                    {
                        SoundID soundId = new SoundID(entity);
                        bool isPlaying = BroAudio.HasAnyPlayingInstances(soundId);
                        sb.AppendLine($"  Name={entity.Name,-30}  Playing={isPlaying,-5}");
                    }
                }
                else
                {
                    sb.AppendLine("  (no sound entities found)");
                }

                // Volume per type
                sb.AppendLine("\n-- Volume by Audio Type --");
                foreach (BroAudioType audioType in Enum.GetValues(typeof(BroAudioType)))
                {
                    if (audioType == BroAudioType.None) continue;
                    sb.AppendLine($"  {audioType}");
                }

                return sb.ToString();
            });
        }
    }
}
