#if HAS_FEEL
#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using MoreMountains.Feedbacks;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Feel.Editor
{
    public partial class Tool_Feel
    {
        [McpPluginTool("feel-query", Title = "Feel / Query MMF Player")]
        [Description(@"Reads the MMF_Player setup on a GameObject.
Reports player settings (intensity, timing, direction, channel) and
lists all feedbacks in the FeedbacksList (type, label, active, duration, chance).
Use before configuring to understand current state.")]
        public string QueryPlayer(
            [Description("Name of the GameObject with MMF_Player component.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var player = GetPlayer(gameObjectName);
                var sb = new StringBuilder();

                sb.AppendLine($"=== MMF_Player: {player.name} ===");
                sb.AppendLine($"\n-- Player Settings --");
                sb.AppendLine($"  FeedbacksIntensity:           {player.FeedbacksIntensity:F2}");
                sb.AppendLine($"  Direction:                    {player.Direction}");
                sb.AppendLine($"  CooldownDuration:             {player.CooldownDuration:F3}s");
                sb.AppendLine($"  InitialDelay:                 {player.InitialDelay:F3}s");
                sb.AppendLine($"  DurationMultiplier:           {player.DurationMultiplier:F2}");
                sb.AppendLine($"  CanPlay:                      {player.CanPlay}");
                sb.AppendLine($"  CanPlayWhileAlreadyPlaying:   {player.CanPlayWhileAlreadyPlaying}");
                sb.AppendLine($"  AutoPlayOnStart:              {player.AutoPlayOnStart}");
                sb.AppendLine($"  AutoPlayOnEnable:             {player.AutoPlayOnEnable}");
                sb.AppendLine($"  IsPlaying:                    {player.IsPlaying}");
                sb.AppendLine($"  ForceTimescaleMode:           {player.ForceTimescaleMode}");
                if (player.ForceTimescaleMode)
                    sb.AppendLine($"  ForcedTimescaleMode:          {player.ForcedTimescaleMode}");
                sb.AppendLine($"  OnlyPlayIfWithinRange:        {player.OnlyPlayIfWithinRange}");
                if (player.OnlyPlayIfWithinRange)
                    sb.AppendLine($"  RangeDistance:                {player.RangeDistance:F2}");
                sb.AppendLine($"  MMF_ChannelMode:              {player.MMF_ChannelMode}");
                sb.AppendLine($"  MMF_Channel:                  {player.MMF_Channel}");

                sb.AppendLine($"\n-- Feedbacks ({player.FeedbacksList.Count}) --");
                for (int i = 0; i < player.FeedbacksList.Count; i++)
                {
                    var fb = player.FeedbacksList[i];
                    if (fb == null) continue;
                    sb.AppendLine($"  [{i}] {fb.GetType().Name} | Label: '{fb.Label}' | Active: {fb.Active} | Duration: {fb.FeedbackDuration:F3}s | Chance: {fb.Chance:F0}%");
                }

                return sb.ToString();
            });
        }

        [McpPluginTool("feel-configure-player", Title = "Feel / Configure MMF Player")]
        [Description(@"Adds (if missing) and configures an MMF_Player component.
All parameters are optional -- only provided values are changed.
intensity [0-1+]: global multiplier on all feedback effects.
direction: TopToBottom or BottomToTop playback order.
cooldownDuration: seconds to lock out repeat plays after each play call.
initialDelay: seconds to wait before starting feedback sequence.
durationMultiplier: speed multiplier for all feedback durations.
autoPlayOnStart / autoPlayOnEnable: trigger automatically on scene start or object enable.
channel: integer channel ID for event-driven triggering via MMF_PlayerEvent.")]
        public string ConfigurePlayer(
            [Description("Name of the GameObject to add MMF_Player to.")] string gameObjectName,
            [Description("Global intensity multiplier [0-∞]. 1.0 = normal.")] float? intensity = null,
            [Description("Playback order: TopToBottom or BottomToTop.")] string? direction = null,
            [Description("Seconds to lock out repeat plays.")] float? cooldownDuration = null,
            [Description("Seconds to wait before starting sequence.")] float? initialDelay = null,
            [Description("Speed multiplier for all feedback durations.")] float? durationMultiplier = null,
            [Description("Allow a new play call while already playing.")] bool? canPlayWhileAlreadyPlaying = null,
            [Description("Auto-play on Start().")] bool? autoPlayOnStart = null,
            [Description("Auto-play on OnEnable().")] bool? autoPlayOnEnable = null,
            [Description("Integer channel ID for MMF_PlayerEvent triggering.")] int? channel = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = GameObject.Find(gameObjectName);
                if (go == null) throw new System.Exception($"GameObject '{gameObjectName}' not found.");

                var player = go.GetComponent<MMF_Player>();
                if (player == null)
                    player = go.AddComponent<MMF_Player>();

                if (intensity.HasValue) player.FeedbacksIntensity = Mathf.Max(0f, intensity.Value);
                if (direction != null)
                {
                    if (!System.Enum.TryParse<MMFeedbacks.Directions>(direction, true, out var dir))
                        throw new System.Exception($"Invalid direction '{direction}'. Valid: TopToBottom, BottomToTop");
                    player.Direction = dir;
                }
                if (cooldownDuration.HasValue) player.CooldownDuration = Mathf.Max(0f, cooldownDuration.Value);
                if (initialDelay.HasValue) player.InitialDelay = Mathf.Max(0f, initialDelay.Value);
                if (durationMultiplier.HasValue) player.DurationMultiplier = Mathf.Max(0.01f, durationMultiplier.Value);
                if (canPlayWhileAlreadyPlaying.HasValue) player.CanPlayWhileAlreadyPlaying = canPlayWhileAlreadyPlaying.Value;
                if (autoPlayOnStart.HasValue) player.AutoPlayOnStart = autoPlayOnStart.Value;
                if (autoPlayOnEnable.HasValue) player.AutoPlayOnEnable = autoPlayOnEnable.Value;
                if (channel.HasValue)
                {
                    player.MMF_ChannelMode = MMChannelModes.Int;
                    player.MMF_Channel = channel.Value;
                }

                EditorUtility.SetDirty(player);

                return $"OK: MMF_Player on '{gameObjectName}' configured. intensity={player.FeedbacksIntensity:F2} direction={player.Direction} feedbacks={player.FeedbacksList.Count} cooldown={player.CooldownDuration:F3}s";
            });
        }
    }
}
#endif
