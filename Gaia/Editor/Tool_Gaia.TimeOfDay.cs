#if HAS_GAIA && GAIA_2023_PRO
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Gaia;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MCPTools.Gaia.Editor
{
    public partial class Tool_Gaia
    {
        [AiTool("gaia-time-of-day", Title = "Gaia / Time of Day")]
        [Description(@"Gets or sets the Procedural Worlds Sky time of day (GaiaAPI). Omit every value to only read.
In EDIT MODE a scripted time change does not render on its own -- the sun angle and fog/ambient curves only update on an editor repaint, which an MCP call does not cause (ENTRY-433: hour set to 7, sun still at the 12:00 angle). This tool ticks the atmosphere after every change, so the next screenshot shows the new time.
Measured PW Sky day: 06:00-18:00. By 19:00 the sun is ~15 degrees below the horizon (night: sun intensity 0, moon on), so there is no golden-hour frame at 19:00 -- use ~17:00 for low warm light.
Set cycleRunning=false to hold the time still for set dressing and screenshots. Needs PW Sky in the scene (gaia-sky action='add').")]
        public string TimeOfDay(
            [Description("Hour, 0-23.")] int? hour = null,
            [Description("Minute, 0-59.")] float? minute = null,
            [Description("true = the day/night cycle advances on its own; false = time holds still.")] bool? cycleRunning = null,
            [Description("Cycle speed; higher = faster day/night. Only matters while the cycle runs.")] float? timeScale = null,
            [Description("Sun rotation around the Y axis, in degrees (the sun's compass heading).")] float? sunRotation = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!GaiaUtils.CheckIfPWSkyExists())
                    throw new Exception("No Procedural Worlds Sky in the scene. Add it with gaia-sky action='add'.");
                if (hour.HasValue && (hour.Value < 0 || hour.Value > 23))
                    throw new Exception("hour must be 0-23.");
                if (minute.HasValue && (minute.Value < 0f || minute.Value >= 60f))
                    throw new Exception("minute must be 0-59.");
                if (timeScale.HasValue && timeScale.Value < 0f)
                    throw new Exception("timeScale must be >= 0.");

                StringBuilder sb = new StringBuilder();
                bool changed = hour.HasValue || minute.HasValue || cycleRunning.HasValue || timeScale.HasValue || sunRotation.HasValue;
                if (changed)
                {
                    string before = $"{GaiaAPI.GetTimeOfDayHour():D2}:{Mathf.FloorToInt(GaiaAPI.GetTimeOfDayMinute()):D2}";
                    if (hour.HasValue)
                        GaiaAPI.SetTimeOfDayHour(hour.Value);
                    if (minute.HasValue)
                        GaiaAPI.SetTimeOfDayMinute(minute.Value);
                    if (cycleRunning.HasValue)
                        GaiaAPI.SetTimeOfDayEnabled(cycleRunning.Value);
                    if (timeScale.HasValue)
                        GaiaAPI.SetTimeOfDayTimeScale(timeScale.Value);
                    if (sunRotation.HasValue && !GaiaAPI.SetTimeOfDaySunRotation(sunRotation.Value))
                        sb.AppendLine("** Sun rotation NOT set: no PW_VFX_Atmosphere instance is registered (it registers in OnEnable). **");

                    sb.AppendLine($"Time {before} -> {GaiaAPI.GetTimeOfDayHour():D2}:{Mathf.FloorToInt(GaiaAPI.GetTimeOfDayMinute()):D2}");
                    sb.AppendLine(TickAtmosphere());
                    if (!Application.isPlaying)
                        EditorSceneManager.MarkSceneDirty(PWSkyStandalone.Instance.gameObject.scene);

                    int h = GaiaAPI.GetTimeOfDayHour();
                    if (h >= 18 || h < 6)
                        sb.AppendLine("Note: PW Sky's day is 06:00-18:00 -- this is night (sun below the horizon, moon lit).");
                }
                AppendSkyReport(sb);
                return sb.ToString();
            });
        }
    }
}
#endif
