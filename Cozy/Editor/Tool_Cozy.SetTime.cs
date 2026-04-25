#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using DistantLands.Cozy;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Cozy.Editor
{
    public partial class Tool_Cozy
    {
        [McpPluginTool("cozy-set-time", Title = "Cozy / Set Time of Day")]
        [Description(@"Sets time-of-day (and optionally day/year) on the scene's CozyTimeModule.

Mutually exclusive ways to set time -- pick ONE:
  - hour + minute (24h clock)
  - dayPercentage (0..1 where 0=midnight, 0.5=noon, 1=midnight)
  - skipHours / skipMinutes (relative skip via SkipTime, raises events along the way)
  - transitionToHour + transitionToMinute + transitionSeconds (smooth play-mode transition)

day / year set the calendar day and year fields directly.
freezeTimeInEdit toggles CozyWeather.FreezeUpdateInEditMode (handy for screenshots).")]
        public string SetTime(
            [Description("Optional GameObject name with CozyWeather. Omit to use the active scene instance.")]
            string? gameObjectName = null,
            [Description("Hour 0-23 (use with minute).")] int? hour = null,
            [Description("Minute 0-59 (use with hour).")] int? minute = null,
            [Description("Day percentage 0..1 (alternative to hour+minute).")] float? dayPercentage = null,
            [Description("Skip forward by N hours (uses SkipTime, raises events).")] float? skipHours = null,
            [Description("Skip forward by N minutes (in addition to skipHours).")] float? skipMinutes = null,
            [Description("Smooth transition target hour (play mode only).")] int? transitionToHour = null,
            [Description("Smooth transition target minute (play mode only).")] int? transitionToMinute = null,
            [Description("Duration of smooth transition in seconds (play mode only).")] float? transitionSeconds = null,
            [Description("Set the day-of-year (perennialProfile.daysPerYear).")] int? day = null,
            [Description("Set the year.")] int? year = null,
            [Description("Toggle CozyWeather.FreezeUpdateInEditMode (true = pause sun/sky update in editor).")] bool? freezeTimeInEdit = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                CozyWeather cw = GetWeather(gameObjectName);
                var t = cw.timeModule;
                if (t == null)
                    throw new System.Exception("CozyWeather has no CozyTimeModule attached. Add the Time Module first.");

                var changes = new StringBuilder();
                int changeCount = 0;

                if (hour.HasValue || minute.HasValue)
                {
                    int h = hour ?? t.currentTime.hours;
                    int m = minute ?? t.currentTime.minutes;
                    t.currentTime = new MeridiemTime(h, m, t.currentTime.seconds, t.currentTime.milliseconds);
                    changes.AppendLine($"  time = {h:D2}:{m:D2}");
                    changeCount++;
                }

                if (dayPercentage.HasValue)
                {
                    t.currentTime = Mathf.Clamp01(dayPercentage.Value);
                    changes.AppendLine($"  dayPercentage = {dayPercentage.Value:F4} ({(string)t.currentTime})");
                    changeCount++;
                }

                if (skipHours.HasValue || skipMinutes.HasValue)
                {
                    float skipFraction = ((skipHours ?? 0f) * 3600f + (skipMinutes ?? 0f) * 60f) / 86400f;
                    t.SkipTime((MeridiemTime)skipFraction);
                    changes.AppendLine($"  SkipTime(+{skipHours ?? 0}h{skipMinutes ?? 0}m) -> now {(string)t.currentTime}");
                    changeCount++;
                }

                if (transitionToHour.HasValue || transitionToMinute.HasValue || transitionSeconds.HasValue)
                {
                    if (!Application.isPlaying)
                    {
                        changes.AppendLine("  [Warning] TransitionTime requires play mode -- skipped.");
                    }
                    else
                    {
                        int h = transitionToHour ?? t.currentTime.hours;
                        int m = transitionToMinute ?? t.currentTime.minutes;
                        float duration = transitionSeconds ?? 5f;
                        float target = (h * 3600f + m * 60f) / 86400f;
                        t.TransitionTime(target, duration);
                        changes.AppendLine($"  TransitionTime -> {h:D2}:{m:D2} over {duration:F1}s");
                        changeCount++;
                    }
                }

                if (day.HasValue)
                {
                    t.currentDay = day.Value;
                    changes.AppendLine($"  currentDay = {day.Value}");
                    changeCount++;
                }

                if (year.HasValue)
                {
                    t.currentYear = year.Value;
                    changes.AppendLine($"  currentYear = {year.Value}");
                    changeCount++;
                }

                if (freezeTimeInEdit.HasValue)
                {
                    CozyWeather.FreezeUpdateInEditMode = freezeTimeInEdit.Value;
                    changes.AppendLine($"  FreezeUpdateInEditMode = {freezeTimeInEdit.Value}");
                    changeCount++;
                }

                if (changeCount == 0)
                    return $"No changes applied. Current time: {(string)t.currentTime}  day={t.currentDay}  year={t.currentYear}";

                EditorUtility.SetDirty(t);
                return $"OK: CozyTimeModule on '{cw.gameObject.name}' updated ({changeCount} change(s)):\n{changes}\nNow: {(string)t.currentTime}  day={t.currentDay}  year={t.currentYear}";
            });
        }
    }
}
