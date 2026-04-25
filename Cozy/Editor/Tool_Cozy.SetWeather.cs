#nullable enable
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using DistantLands.Cozy;
using DistantLands.Cozy.Data;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Cozy.Editor
{
    public partial class Tool_Cozy
    {
        [McpPluginTool("cozy-set-weather", Title = "Cozy / Set Weather Profile")]
        [Description(@"Switches the active WeatherProfile on the scene's CozyWeatherModule.
Wraps CozyEcosystem.SetWeather(profile, transitionTime) and raises OnWeatherChange.

profile: WeatherProfile asset name (e.g. 'Partly Cloudy') or full asset path.
transitionTime: optional override of the ecosystem's weatherTransitionTime (seconds).
listProfiles: if true, returns the list of WeatherProfile assets in the project instead of switching.

In edit mode, the call sets `currentWeather` directly (UpdateEcosystem handles the swap on next tick).
In play mode, the call invokes the full SetWeather coroutine path with cross-fade.")]
        public string SetWeather(
            [Description("Optional GameObject name with CozyWeather. Omit to use the active scene instance.")]
            string? gameObjectName = null,
            [Description("WeatherProfile asset name or full path. Omit only when listProfiles=true.")]
            string? profile = null,
            [Description("Override the ecosystem's transition time (seconds). Optional.")]
            float? transitionTime = null,
            [Description("If true, returns the list of WeatherProfile assets instead of switching.")]
            bool listProfiles = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (listProfiles)
                {
                    var names = ListWeatherProfileNames();
                    var sb0 = new StringBuilder();
                    sb0.AppendLine($"WeatherProfile assets in project ({names.Count}):");
                    foreach (var n in names) sb0.AppendLine($"  - {n}");
                    return sb0.ToString();
                }

                if (string.IsNullOrEmpty(profile))
                    throw new System.Exception("'profile' is required (or pass listProfiles=true to enumerate available profiles).");

                CozyWeather cw = GetWeather(gameObjectName);
                if (cw.weatherModule == null)
                    throw new System.Exception("CozyWeather has no CozyWeatherModule attached. Add the Weather Module first.");
                if (cw.weatherModule.ecosystem == null)
                    throw new System.Exception("CozyWeatherModule has no ecosystem configured.");

                var target = FindWeatherProfile(profile);

                var sb = new StringBuilder();
                var prevName = cw.weatherModule.ecosystem.currentWeather != null ? cw.weatherModule.ecosystem.currentWeather.name : "(none)";

                if (Application.isPlaying)
                {
                    if (transitionTime.HasValue)
                        cw.weatherModule.ecosystem.SetWeather(target, transitionTime.Value);
                    else
                        cw.weatherModule.ecosystem.SetWeather(target);
                    sb.AppendLine($"OK: Weather coroutine started: '{prevName}' -> '{target.name}' over {(transitionTime ?? cw.weatherModule.ecosystem.weatherTransitionTime):F2}s");
                }
                else
                {
                    // Edit mode: SetWeather() needs StartCoroutine. UpdateEcosystem() picks up currentWeather changes
                    // and re-syncs the weightedWeatherProfiles list, so a direct assignment is the safe edit-mode path.
                    cw.weatherModule.ecosystem.currentWeather = target;
                    cw.weatherModule.ecosystem.weatherChangeCheck = target;
                    cw.weatherModule.ecosystem.weightedWeatherProfiles = new System.Collections.Generic.List<WeatherRelation>
                    {
                        new WeatherRelation { profile = target, weight = 1 }
                    };
                    cw.events?.RaiseOnWeatherChange();
                    EditorUtility.SetDirty(cw.weatherModule);
                    sb.AppendLine($"OK (edit mode): Weather set: '{prevName}' -> '{target.name}' (no cross-fade in edit mode)");
                }

                return sb.ToString();
            });
        }
    }
}
