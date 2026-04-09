#if HAS_LUMEN
#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.Lumen.Editor
{
    public partial class Tool_Lumen
    {
        [McpPluginTool("lumen-configure", Title = "Lumen / Configure Effect Player")]
        [Description(@"Configures a LumenEffectPlayer component on a GameObject.
All parameters are optional -- only provided values are changed.
scale: uniform scale modifier for all effects.
brightness: uniform brightness modifier for all effects.
colorHex: uniform color modifier (hex string like '#FF8800').
range: range multiplier for light layer effects.
fadingTime: transition duration in seconds.
updateFrequency: Always, OnChanges, or ViaScripting.
initBehavior: Immediate, FadeToTargetBrightness, FadeToTargetScale, FadeToTargetColor, SkipInitialization.
deinitBehavior: Immediate, FadeBrightnessToZero, FadeScaleToZero, FadeColorToBlack.
autoAssignSun: auto-assign brightest directional light as sun.
At runtime (play mode) you can use fadeToBrightness/fadeToScale/fadeToColorHex for smooth transitions.")]
        public string ConfigureLumen(
            [Description("Name of the GameObject with LumenEffectPlayer.")]
            string gameObjectName,
            [Description("Uniform scale modifier for all effects.")] float? scale = null,
            [Description("Uniform brightness modifier for all effects.")] float? brightness = null,
            [Description("Uniform color modifier (hex: '#RRGGBB').")] string? colorHex = null,
            [Description("Range multiplier for light layer effects.")] float? range = null,
            [Description("Transition duration in seconds.")] float? fadingTime = null,
            [Description("Update mode: Always, OnChanges, or ViaScripting.")] string? updateFrequency = null,
            [Description("Init behavior: Immediate, FadeToTargetBrightness, FadeToTargetScale, FadeToTargetColor, SkipInitialization.")] string? initBehavior = null,
            [Description("Deinit behavior: Immediate, FadeBrightnessToZero, FadeScaleToZero, FadeColorToBlack.")] string? deinitBehavior = null,
            [Description("Auto-assign brightest directional light as sun.")] bool? autoAssignSun = null,
            [Description("(Runtime) Fade brightness to this value over fadingTime.")] float? fadeToBrightness = null,
            [Description("(Runtime) Fade scale to this value over fadingTime.")] float? fadeToScale = null,
            [Description("(Runtime) Fade color to this hex value over fadingTime.")] string? fadeToColorHex = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                UnityEngine.Component player = GetPlayer(gameObjectName);
                StringBuilder changes = new StringBuilder();
                int changeCount = 0;

                if (scale.HasValue) { Set(player, "scale", scale.Value); changes.AppendLine($"  scale = {scale.Value}"); changeCount++; }
                if (brightness.HasValue) { Set(player, "brightness", brightness.Value); changes.AppendLine($"  brightness = {brightness.Value}"); changeCount++; }
                if (colorHex != null) { SetColor(player, "color", ParseColor(colorHex)); changes.AppendLine($"  color = {colorHex}"); changeCount++; }
                if (range.HasValue) { Set(player, "range", range.Value); changes.AppendLine($"  range = {range.Value}"); changeCount++; }
                if (fadingTime.HasValue) { Set(player, "fadingTime", fadingTime.Value); changes.AppendLine($"  fadingTime = {fadingTime.Value}"); changeCount++; }
                if (updateFrequency != null) { SetEnum(player, "updateFrequency", updateFrequency); changes.AppendLine($"  updateFrequency = {updateFrequency}"); changeCount++; }
                if (initBehavior != null) { SetEnum(player, "initializationBehavior", initBehavior); changes.AppendLine($"  initializationBehavior = {initBehavior}"); changeCount++; }
                if (deinitBehavior != null) { SetEnum(player, "deinitializationBehavior", deinitBehavior); changes.AppendLine($"  deinitializationBehavior = {deinitBehavior}"); changeCount++; }
                if (autoAssignSun.HasValue) { Set(player, "autoAssignSun", autoAssignSun.Value); changes.AppendLine($"  autoAssignSun = {autoAssignSun.Value}"); changeCount++; }

                // Runtime fade methods (play mode only)
                if (fadeToBrightness.HasValue || fadeToScale.HasValue || fadeToColorHex != null)
                {
                    if (!Application.isPlaying)
                    {
                        changes.AppendLine("  [Warning] Fade methods require play mode -- skipped.");
                    }
                    else
                    {
                        float fadeTime = fadingTime ?? (float)(Get(player, "fadingTime") ?? 1f);

                        if (fadeToBrightness.HasValue)
                        {
                            Call(player, "FadeBrightness", fadeToBrightness.Value, fadeTime);
                            changes.AppendLine($"  FadeBrightness({fadeToBrightness.Value}, {fadeTime}s)");
                            changeCount++;
                        }
                        if (fadeToScale.HasValue)
                        {
                            Call(player, "FadeScale", fadeToScale.Value, fadeTime);
                            changes.AppendLine($"  FadeScale({fadeToScale.Value}, {fadeTime}s)");
                            changeCount++;
                        }
                        if (fadeToColorHex != null)
                        {
                            Color fadeColor = ParseColor(fadeToColorHex);
                            Call(player, "FadeColor", fadeColor, fadeTime);
                            changes.AppendLine($"  FadeColor({fadeToColorHex}, {fadeTime}s)");
                            changeCount++;
                        }
                    }
                }

                if (changeCount > 0)
                    UnityEditor.EditorUtility.SetDirty(player);

                if (changeCount == 0)
                    return $"No changes applied to LumenEffectPlayer on '{gameObjectName}'.";

                return $"OK: LumenEffectPlayer on '{gameObjectName}' updated ({changeCount} change(s)):\n{changes}";
            });
        }
    }
}
#endif
