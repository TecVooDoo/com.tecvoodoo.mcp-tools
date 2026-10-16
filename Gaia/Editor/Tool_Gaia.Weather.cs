#if HAS_GAIA && GAIA_2023_PRO
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Gaia;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Gaia.Editor
{
    public partial class Tool_Gaia
    {
        // A weather start deferred until the previous weather has wound down (ENTRY-433). Static: play mode only,
        // and a domain reload ends play mode's state anyway.
        static string? s_pendingWeather;
        static int s_pendingAfterFrame;
        static double s_pendingDeadline;
        const double WeatherSwitchTimeoutSeconds = 10.0;

        [AiTool("gaia-weather", Title = "Gaia / Weather")]
        [Description(@"Controls Procedural Worlds Sky weather: action='status' (default, read-only), 'rain', 'snow' or 'clear'.
Weather VFX are PLAY-MODE ONLY unless the weather object's RunInEditor is on (ENTRY-433); in edit mode this tool refuses rain/snow and says so.
SWITCHING rain <-> snow is done in TWO STAGES: the vendor's same-frame stop+start STRANDS the old emitter (measured: rain kept falling from a clear sky 20+ s after StopWeatherRain(); StartWeatherSnow()). This tool stops the current weather now and starts the new one on a later frame, once the old one reports finished (or after 10 s). The switch is therefore NOT done when the call returns -- check with action='status' on a later call.
'status' lists the weather particle systems that are emitting and flags a STRANDED emitter (particles emitting while no weather is active).
'instant' sets whether starts/stops are instant or fade over the weather fade duration.
Needs PW Sky in the scene (gaia-sky action='add').")]
        public string Weather(
            [Description("'status' (default), 'rain', 'snow' or 'clear'.")] string action = "status",
            [Description("Instant start/stop instead of fading. Omit to keep the current setting (PW default: true).")] bool? instant = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                ProceduralWorldsGlobalWeather? weather = FindWeather();
                if (weather == null)
                    throw new Exception("No Gaia Weather object (ProceduralWorldsGlobalWeather) in the scene. Add PW Sky with gaia-sky action='add'.");

                StringBuilder sb = new StringBuilder();
                if (instant.HasValue)
                    weather.m_instantStartStop = instant.Value;

                string verb = action.Trim().ToLowerInvariant();
                switch (verb)
                {
                    case "status":
                        break;
                    case "clear":
                        CancelPendingWeather();
                        if (weather.IsRaining)
                            weather.StopRain();
                        if (weather.IsSnowing)
                            weather.StopSnow();
                        sb.AppendLine("Weather cleared (rain and snow stopped). Emitters wind down over the next frames; confirm with action='status'.");
                        break;
                    case "rain":
                    case "snow":
                        StartWeather(sb, weather, verb);
                        break;
                    default:
                        throw new Exception($"Unknown action '{action}'. Use 'status', 'rain', 'snow' or 'clear'.");
                }

                if (s_pendingWeather != null)
                    sb.AppendLine($"Pending: '{s_pendingWeather}' starts once the previous weather has finished (timeout {WeatherSwitchTimeoutSeconds:F0} s).");
                AppendSkyReport(sb);
                AppendWeatherEmitters(sb, weather);
                return sb.ToString();
            });
        }

        static void StartWeather(StringBuilder sb, ProceduralWorldsGlobalWeather weather, string target)
        {
            bool rain = target == "rain";
            if (!Application.isPlaying && !weather.RunInEditor)
                throw new Exception("Weather VFX run in PLAY MODE only (the weather object's RunInEditor is off). Enter play mode, then call again.");
            if (weather.m_disableWeatherFX)
                throw new Exception("Weather FX are disabled on the weather object (m_disableWeatherFX). Enable them first.");
            if (rain ? !weather.EnableRain : !weather.EnableSnow)
                throw new Exception($"{(rain ? "Rain" : "Snow")} is disabled on the weather object (Enable{(rain ? "Rain" : "Snow")} = false); the vendor start would do nothing.");

            if (rain ? weather.IsRaining : weather.IsSnowing)
            {
                CancelPendingWeather();
                sb.AppendLine($"Already {(rain ? "raining" : "snowing")} -- nothing to do.");
                return;
            }
            if (s_pendingWeather == target)
            {
                sb.AppendLine($"'{target}' is already pending (a repeated call) -- nothing new scheduled.");
                return;
            }

            bool otherActive = rain ? weather.IsSnowing : weather.IsRaining;
            if (!otherActive)
            {
                CancelPendingWeather();
                if (rain)
                    weather.PlayRain();
                else
                    weather.PlaySnow();
                sb.AppendLine($"{(rain ? "Rain" : "Snow")} started.");
                return;
            }

            // Stage 1 now: stop the other weather. Stage 2 on a later frame (OnPendingWeatherUpdate).
            if (rain)
                weather.StopSnow();
            else
                weather.StopRain();
            s_pendingWeather = target;
            s_pendingAfterFrame = Time.frameCount + 1;
            s_pendingDeadline = EditorApplication.timeSinceStartup + WeatherSwitchTimeoutSeconds;
            EditorApplication.update -= OnPendingWeatherUpdate;
            EditorApplication.update += OnPendingWeatherUpdate;
            sb.AppendLine($"Stopped {(rain ? "snow" : "rain")}; {target} is scheduled for a later frame (a same-frame switch strands the old emitter).");
        }

        static void OnPendingWeatherUpdate()
        {
            string? target = s_pendingWeather;
            ProceduralWorldsGlobalWeather? weather = FindWeather();
            if (target == null || weather == null || (!Application.isPlaying && !weather.RunInEditor))
            {
                CancelPendingWeather();
                return;
            }
            if (Time.frameCount <= s_pendingAfterFrame)
                return;

            bool rain = target == "rain";
            bool previousFinished = rain ? weather.IsSnowingFinished : weather.IsRainingFinished;
            bool timedOut = EditorApplication.timeSinceStartup >= s_pendingDeadline;
            if (!previousFinished && !timedOut)
                return;

            // Unsubscribe FIRST, so a throw below can never leave the handler firing again (UWF_Unity v1.33).
            CancelPendingWeather();
            try
            {
                if (rain)
                    weather.PlayRain();
                else
                    weather.PlaySnow();
                if (timedOut && !previousFinished)
                    Debug.LogWarning($"[TMCP Gaia] Started {target} after the {WeatherSwitchTimeoutSeconds:F0} s timeout; the previous weather never reported finished. Check gaia-weather status for a stranded emitter.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TMCP Gaia] Deferred {target} start failed: {ex.Message}");
            }
        }

        static void CancelPendingWeather()
        {
            EditorApplication.update -= OnPendingWeatherUpdate;
            s_pendingWeather = null;
        }

        static void AppendWeatherEmitters(StringBuilder sb, ProceduralWorldsGlobalWeather weather)
        {
            sb.AppendLine($"Finished flags: rain={weather.IsRainingFinished} snow={weather.IsSnowingFinished}");
            // The emitters are referenced by field: the rain VFX follows the camera, so it need not sit under the weather object.
            bool rainStranded = AppendEmitter(sb, "rain", weather.m_rainVFX, weather.IsRaining);
            bool snowStranded = AppendEmitter(sb, "snow", weather.m_snowParticles, weather.IsSnowing);
            snowStranded |= AppendEmitter(sb, "snow VFX", weather.m_snowVFX, weather.IsSnowing);
            if ((rainStranded || snowStranded) && s_pendingWeather == null)
                sb.AppendLine($"  ** STRANDED EMITTER: {(rainStranded ? "rain" : "snow")} is emitting although that weather is not active (ENTRY-433's same-frame switch bug). Run action='clear' and check again a moment later; if it persists, stop and restart play mode. **");
        }

        /// <summary>Reports one weather emitter; returns true when it emits while its weather is off.</summary>
        static bool AppendEmitter(StringBuilder sb, string label, ParticleSystem? system, bool weatherActive)
        {
            if (system == null)
            {
                sb.AppendLine($"  {label} emitter: not assigned");
                return false;
            }
            sb.AppendLine($"  {label} emitter '{HierarchyPath(system.transform)}' emitting={system.isEmitting} particles={system.particleCount}");
            return system.isEmitting && !weatherActive;
        }
    }
}
#endif
