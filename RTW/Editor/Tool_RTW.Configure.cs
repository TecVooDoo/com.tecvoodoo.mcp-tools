#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.RTW.Editor
{
    public partial class Tool_RTW
    {
        [McpPluginTool("rtw-configure", Title = "Real Time Weather Pro / Configure")]
        [Description(@"Sets RealTimeWeatherManager configuration.
weatherSystem: None, Enviro, Tenkoku, Atmos, Expanse, EasySky.
waterSystem: None, KWS, Crest.
weatherRequestMode: None, RtwMode, TomorrowMode, OpenWeatherMapMode.
maritimeRequestMode: None, MetoceanMode, StormglassMode, TomorrowMode.
activateWeather: name of the system to activate (Enviro/Tenkoku/Atmos/Crest/KWS) — calls ActivateXSimulation.
deactivateWeather: same options + 'All' to call DeactivateAllWeather.
deactivateWater: 'All' to call DeactivateAllWater.")]
        public string Configure(
            [Description("Latitude (degrees).")] float? latitude = null,
            [Description("Longitude (degrees).")] float? longitude = null,
            [Description("Active weather system name.")] string? weatherSystem = null,
            [Description("Active water system name.")] string? waterSystem = null,
            [Description("Weather data request mode.")] string? weatherRequestMode = null,
            [Description("Maritime data request mode.")] string? maritimeRequestMode = null,
            [Description("Enable auto-weather updates.")] bool? isAutoWeatherEnabled = null,
            [Description("Auto-update interval (seconds).")] int? autoWeatherUpdateRate = null,
            [Description("Activate a sim: Enviro, Tenkoku, Atmos, Crest, KWS.")] string? activateWeather = null,
            [Description("Deactivate a sim: Enviro, Tenkoku, Atmos, Crest, KWS, or All.")] string? deactivateWeather = null,
            [Description("Deactivate water: All.")] string? deactivateWater = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var mgr = GetManager();
                var sb = new StringBuilder();
                int changes = 0;

                if (latitude.HasValue) { Set(mgr, "Latitude", latitude.Value); changes++; }
                if (longitude.HasValue) { Set(mgr, "Longitude", longitude.Value); changes++; }
                if (weatherSystem != null) { SetEnum(mgr, "SelectedWeatherSystem", weatherSystem); changes++; }
                if (waterSystem != null) { SetEnum(mgr, "SelectedWaterSystem", waterSystem); changes++; }
                if (weatherRequestMode != null) { SetEnum(mgr, "WeatherDataRequestMode", weatherRequestMode); changes++; }
                if (maritimeRequestMode != null) { SetEnum(mgr, "MaritimeDataRequestMode", maritimeRequestMode); changes++; }
                if (isAutoWeatherEnabled.HasValue) { Set(mgr, "IsAutoWeatherEnabled", isAutoWeatherEnabled.Value); changes++; }
                if (autoWeatherUpdateRate.HasValue) { Set(mgr, "AutoWeatherUpdateRate", autoWeatherUpdateRate.Value); changes++; }

                if (activateWeather != null)
                {
                    string m = $"Activate{activateWeather}Simulation";
                    Call(mgr, m);
                    sb.AppendLine($"  Called {m}()");
                    changes++;
                }

                if (deactivateWeather != null)
                {
                    string m = string.Equals(deactivateWeather, "All", System.StringComparison.OrdinalIgnoreCase)
                        ? "DeactivateAllWeather"
                        : $"Deactivate{deactivateWeather}Simulation";
                    Call(mgr, m);
                    sb.AppendLine($"  Called {m}()");
                    changes++;
                }

                if (deactivateWater != null)
                {
                    if (string.Equals(deactivateWater, "All", System.StringComparison.OrdinalIgnoreCase))
                    {
                        Call(mgr, "DeactivateAllWater");
                        sb.AppendLine("  Called DeactivateAllWater()");
                        changes++;
                    }
                }

                if (mgr is UnityEngine.Object uo) EditorUtility.SetDirty(uo);
                return $"OK: RealTimeWeatherManager configured. {changes} change(s) applied.\n{sb}";
            });
        }
    }
}
