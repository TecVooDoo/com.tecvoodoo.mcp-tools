#nullable enable
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using DistantLands.Cozy;
using UnityEngine;

namespace MCPTools.Cozy.Editor
{
    public partial class Tool_Cozy
    {
        [McpPluginTool("cozy-query", Title = "Cozy / Query Weather Sphere")]
        [Description(@"Reports the runtime state of a CozyWeather sphere.
If gameObjectName is omitted, queries CozyWeather.instance (the first sphere in the scene).
Returns: cloud/sky/fog style, current weather profile, time-of-day, date, attached modules,
ecosystem snapshot (active weather profiles + weights), climate snapshot if present,
wind snapshot if present, and a count of biomes/systems registered.")]
        public string Query(
            [Description("Optional GameObject name with a CozyWeather component. Omit to use the active scene instance.")]
            string? gameObjectName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                CozyWeather cw = GetWeather(gameObjectName);
                var sb = new StringBuilder();

                sb.AppendLine($"=== CozyWeather on '{cw.gameObject.name}' (scene='{cw.gameObject.scene.name}') ===");
                sb.AppendLine($"  skyStyle:    {cw.skyStyle}");
                sb.AppendLine($"  cloudStyle:  {cw.cloudStyle}");
                sb.AppendLine($"  fogStyle:    {cw.fogStyle}");
                sb.AppendLine($"  lockToCamera: {cw.lockToCamera}  handleSceneLighting: {cw.handleSceneLighting}");

                // Time
                if (cw.timeModule != null)
                {
                    var t = cw.timeModule;
                    string time = t.currentTime != null ? t.currentTime.ToString() : "(null)";
                    sb.AppendLine($"  Time: {time}  day={t.currentDay}  year={t.currentYear}  yearPct={t.yearPercentage:F3}");
                    sb.AppendLine($"        modifiedDayPercentage={t.modifiedDayPercentage:F3}  daysPerYear={t.DaysPerYear}");
                    sb.AppendLine($"        perennialProfile: {(t.perennialProfile != null ? t.perennialProfile.name : "(none)")}");
                }
                else
                {
                    sb.AppendLine("  Time: (no CozyTimeModule attached)");
                }

                // Weather
                if (cw.weatherModule != null)
                {
                    var w = cw.weatherModule;
                    sb.AppendLine($"  Weather selection mode: {w.ecosystem?.weatherSelectionMode}");
                    sb.AppendLine($"  Current weather profile: {(w.ecosystem?.currentWeather != null ? w.ecosystem.currentWeather.name : "(none)")}");
                    if (w.currentWeatherProfiles != null && w.currentWeatherProfiles.Count > 0)
                    {
                        sb.AppendLine($"  Active weather profiles ({w.currentWeatherProfiles.Count}):");
                        foreach (var rel in w.currentWeatherProfiles
                                     .OrderByDescending(x => x.weight)
                                     .Take(8))
                        {
                            sb.AppendLine($"    - {(rel.profile != null ? rel.profile.name : "(null)")}  weight={rel.weight:F3}");
                        }
                    }

                    if (w.ecosystem?.currentForecast != null && w.ecosystem.currentForecast.Count > 0)
                    {
                        sb.AppendLine($"  Forecast ({w.ecosystem.currentForecast.Count}):");
                        foreach (var pat in w.ecosystem.currentForecast.Take(6))
                        {
                            sb.AppendLine($"    - {(pat.profile != null ? pat.profile.name : "(null)")}  start={(string)pat.startTime}  end={(string)pat.endTime}");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("  Weather: (no CozyWeatherModule attached)");
                }

                // Snapshot of cumulus/cirrus/etc -- the values being pushed to shaders
                sb.AppendLine($"  Cloud snapshot: cumulus={cw.cumulus:F2} cirrus={cw.cirrus:F2} altocumulus={cw.altocumulus:F2} nimbus={cw.nimbus:F2} fogDensity={cw.fogDensity:F2}");

                // Climate
                if (cw.climateModule != null)
                {
                    var c = cw.climateModule;
                    sb.AppendLine($"  Climate: temp={c.currentTemperature:F1}F  precip={c.currentPrecipitation:F2}  snow={c.snowAmount:F2}  groundwater={c.groundwaterAmount:F2}");
                    sb.AppendLine($"           profile={(c.climateProfile != null ? c.climateProfile.name : "(none)")}  controlMethod={c.controlMethod}");
                }

                // Wind
                if (cw.windModule != null)
                {
                    var wm = cw.windModule;
                    sb.AppendLine($"  Wind: speed={wm.windSpeed:F2}  changeSpeed={wm.windChangeSpeed:F2}  amount={wm.windAmount:F2}  gusting={wm.windGusting:F2}");
                    sb.AppendLine($"        useWindzone={wm.useWindzone}  useShaderWind={wm.useShaderWind}  override={wm.overrideWindDirection}  multiplier={wm.windMultiplier:F2}");
                    sb.AppendLine($"        speedKnots={wm.WindSpeedInKnots:F1}");
                }

                // Modules
                if (cw.modules != null && cw.modules.Count > 0)
                {
                    sb.AppendLine($"  Modules ({cw.modules.Count}):");
                    foreach (var m in cw.modules.Where(x => x != null))
                        sb.AppendLine($"    - {m.GetType().Name}  enabled={m.enabled}");
                }

                // Ecosystem (other systems / biomes)
                if (cw.systems != null)
                    sb.AppendLine($"  Registered systems (incl. self): {cw.systems.Count}");

                return sb.ToString();
            });
        }
    }
}
