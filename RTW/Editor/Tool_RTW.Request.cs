#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.RTW.Editor
{
    public partial class Tool_RTW
    {
        [McpPluginTool("rtw-request", Title = "Real Time Weather Pro / Request Weather")]
        [Description(@"Triggers a weather data fetch.
Provide either (city + country), (city + state), or (latitude + longitude).
Geo-coordinate request returns IEnumerator (coroutine) — kicked off via the manager's StartCoroutine.")]
        public string Request(
            [Description("City name.")] string? city = null,
            [Description("Country name (use with city).")] string? country = null,
            [Description("State name (use with city, US-style).")] string? state = null,
            [Description("Latitude (use with longitude).")] float? latitude = null,
            [Description("Longitude (use with latitude).")] float? longitude = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var mgr = GetManager();

                if (city != null && country != null)
                {
                    Call(mgr, "RequestWeatherByCityAndCountry", city, country);
                    return $"OK: Requested weather for {city}, {country}.";
                }

                if (city != null && state != null)
                {
                    Call(mgr, "RequestWeatherByCityAndState", city, state);
                    return $"OK: Requested weather for {city}, {state}.";
                }

                if (latitude.HasValue && longitude.HasValue)
                {
                    var enumerator = Call(mgr, "RequestWeatherByGeoCoordinates", latitude.Value, longitude.Value) as IEnumerator;
                    if (enumerator == null)
                        throw new Exception("RequestWeatherByGeoCoordinates did not return an IEnumerator.");

                    if (mgr is MonoBehaviour mb)
                        mb.StartCoroutine(enumerator);
                    else
                        throw new Exception("Manager is not a MonoBehaviour; cannot start coroutine.");

                    return $"OK: Requested weather for ({latitude.Value:F4}, {longitude.Value:F4}). Coroutine started.";
                }

                throw new Exception("Provide (city + country) OR (city + state) OR (latitude + longitude).");
            });
        }
    }
}
