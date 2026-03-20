#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using DamageNumbersPro;
using UnityEditor;
using UnityEngine;

namespace MCPTools.DamageNumbersPro.Editor
{
    public partial class Tool_DamageNumbersPro
    {
        [McpPluginTool("dnp-query", Title = "Damage Numbers Pro / Query Settings")]
        [Description(@"Reads the DamageNumber (DamageNumberMesh or DamageNumberGUI) configuration.
Reports display settings (lifetime, number, texts, color), animation settings
(fade in/out, movement mode), performance settings (pooling, updateDelay), and
spam control settings (grouping, combination).")]
        public string QueryDamageNumber(
            [Description("Name of the GameObject with DamageNumber component.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var dn = GetDN(gameObjectName);
                var sb = new StringBuilder();

                sb.AppendLine($"=== DamageNumber: {dn.name} ({dn.GetType().Name}) ===");
                sb.AppendLine($"\n-- Display --");
                sb.AppendLine($"  lifetime:        {dn.lifetime:F2}s");
                sb.AppendLine($"  permanent:       {dn.permanent}");
                sb.AppendLine($"  unscaledTime:    {dn.unscaledTime}");
                sb.AppendLine($"  enable3DGame:    {dn.enable3DGame}");
                sb.AppendLine($"  faceCameraView:  {dn.faceCameraView}");
                sb.AppendLine($"  enableNumber:    {dn.enableNumber}");
                sb.AppendLine($"  number:          {dn.number}");
                sb.AppendLine($"  enableLeftText:  {dn.enableLeftText} '{dn.leftText}'");
                sb.AppendLine($"  enableRightText: {dn.enableRightText} '{dn.rightText}'");
                sb.AppendLine($"  enableTopText:   {dn.enableTopText} '{dn.topText}'");
                sb.AppendLine($"  enableBottomText:{dn.enableBottomText} '{dn.bottomText}'");

                sb.AppendLine($"\n-- Fade In --");
                sb.AppendLine($"  durationFadeIn:      {dn.durationFadeIn:F3}s");
                sb.AppendLine($"  enableOffsetFadeIn:  {dn.enableOffsetFadeIn} offset=({dn.offsetFadeIn.x:F2},{dn.offsetFadeIn.y:F2})");
                sb.AppendLine($"  enableScaleFadeIn:   {dn.enableScaleFadeIn} scale=({dn.scaleFadeIn.x:F2},{dn.scaleFadeIn.y:F2})");

                sb.AppendLine($"\n-- Fade Out --");
                sb.AppendLine($"  durationFadeOut:     {dn.durationFadeOut:F3}s");
                sb.AppendLine($"  enableOffsetFadeOut: {dn.enableOffsetFadeOut} offset=({dn.offsetFadeOut.x:F2},{dn.offsetFadeOut.y:F2})");

                sb.AppendLine($"\n-- Movement --");
                sb.AppendLine($"  enableLerp:     {dn.enableLerp}");
                sb.AppendLine($"  enableVelocity: {dn.enableVelocity}");
                sb.AppendLine($"  enableShaking:  {dn.enableShaking}");
                sb.AppendLine($"  enableFollowing:{dn.enableFollowing}");

                sb.AppendLine($"\n-- Spam Control --");
                sb.AppendLine($"  spamGroup:          '{dn.spamGroup}'");
                sb.AppendLine($"  enableCombination:  {dn.enableCombination}");
                sb.AppendLine($"  enablePush:         {dn.enablePush}");
                sb.AppendLine($"  enableDestruction:  {dn.enableDestruction}");

                sb.AppendLine($"\n-- Performance --");
                sb.AppendLine($"  enablePooling:   {dn.enablePooling}");
                sb.AppendLine($"  poolSize:        {dn.poolSize}");
                sb.AppendLine($"  updateDelay:     {dn.updateDelay:F4}s");

                return sb.ToString();
            });
        }

        [McpPluginTool("dnp-configure-display", Title = "Damage Numbers Pro / Configure Display")]
        [Description(@"Configures display settings on a DamageNumber component.
lifetime: seconds before the number fades out and despawns (ignored if permanent=true).
permanent: if true, number persists until manually faded out.
enableNumber: show/hide the numeric value.
leftText/rightText: prefix/suffix text alongside the number (e.g. '-' or ' HP').
topText/bottomText: text above/below the number.
enable3DGame: enable 3D-specific positioning (face camera, render through walls).
faceCameraView: rotate to always face the main camera.
colorR/G/B/A: set the number display color (0-1 range).")]
        public string ConfigureDisplay(
            [Description("Name of the GameObject with DamageNumber component.")] string gameObjectName,
            [Description("Seconds before fade-out (ignored if permanent=true).")] float? lifetime = null,
            [Description("If true, number never fades out automatically.")] bool? permanent = null,
            [Description("Ignore Time.timeScale (useful during slow-motion).")] bool? unscaledTime = null,
            [Description("Show/hide the numeric value.")] bool? enableNumber = null,
            [Description("Enable 3D mode (face camera, depth rendering).")] bool? enable3DGame = null,
            [Description("Always rotate to face the camera.")] bool? faceCameraView = null,
            [Description("Enable prefix text on the left of the number.")] bool? enableLeftText = null,
            [Description("Prefix text string (e.g. '-' for damage, '+' for heal).")] string? leftText = null,
            [Description("Enable suffix text on the right of the number.")] bool? enableRightText = null,
            [Description("Suffix text string (e.g. ' HP', ' DMG').")] string? rightText = null,
            [Description("Enable text above the number.")] bool? enableTopText = null,
            [Description("Text to show above the number.")] string? topText = null,
            [Description("Enable text below the number.")] bool? enableBottomText = null,
            [Description("Text to show below the number.")] string? bottomText = null,
            [Description("Color red channel [0-1].")] float? colorR = null,
            [Description("Color green channel [0-1].")] float? colorG = null,
            [Description("Color blue channel [0-1].")] float? colorB = null,
            [Description("Color alpha channel [0-1].")] float? colorA = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var dn = GetDN(gameObjectName);

                if (lifetime.HasValue) dn.lifetime = Mathf.Max(0f, lifetime.Value);
                if (permanent.HasValue) dn.permanent = permanent.Value;
                if (unscaledTime.HasValue) dn.unscaledTime = unscaledTime.Value;
                if (enableNumber.HasValue) dn.enableNumber = enableNumber.Value;
                if (enable3DGame.HasValue) dn.enable3DGame = enable3DGame.Value;
                if (faceCameraView.HasValue) dn.faceCameraView = faceCameraView.Value;
                if (enableLeftText.HasValue) dn.enableLeftText = enableLeftText.Value;
                if (leftText != null) dn.leftText = leftText;
                if (enableRightText.HasValue) dn.enableRightText = enableRightText.Value;
                if (rightText != null) dn.rightText = rightText;
                if (enableTopText.HasValue) dn.enableTopText = enableTopText.Value;
                if (topText != null) dn.topText = topText;
                if (enableBottomText.HasValue) dn.enableBottomText = enableBottomText.Value;
                if (bottomText != null) dn.bottomText = bottomText;

                if (colorR.HasValue || colorG.HasValue || colorB.HasValue || colorA.HasValue)
                {
                    var col = new Color(
                        colorR ?? dn.numberSettings.color.r,
                        colorG ?? dn.numberSettings.color.g,
                        colorB ?? dn.numberSettings.color.b,
                        colorA ?? dn.numberSettings.color.a
                    );
                    dn.SetColor(col);
                }

                EditorUtility.SetDirty(dn);

                return $"OK: DamageNumber '{gameObjectName}' display configured. lifetime={dn.lifetime:F2}s permanent={dn.permanent} enableNumber={dn.enableNumber} leftText='{dn.leftText}' rightText='{dn.rightText}'";
            });
        }

        [McpPluginTool("dnp-configure-animation", Title = "Damage Numbers Pro / Configure Animation")]
        [Description(@"Configures fade and movement animation on a DamageNumber component.
Fade In/Out: control timing and whether position/scale is animated during fade.
Movement modes (mutually exclusive -- enable only one):
  enableLerp: smooth eased movement to a target offset position (default).
  enableVelocity: physics-based movement with drag and gravity.
  enableShaking: idle shake (no net movement).
lerpOffsetX/Y: destination offset during lerp movement (world units, local space).
velocityX/Y: initial velocity for velocity mode.
fadeInDuration/fadeOutDuration: timing for opacity transitions.
fadeInOffsetX/Y: position offset at start of fade-in (popup animates FROM this offset).
fadeOutOffsetX/Y: position offset at end of fade-out (popup animates TO this offset).")]
        public string ConfigureAnimation(
            [Description("Name of the GameObject with DamageNumber component.")] string gameObjectName,
            [Description("Fade-in duration in seconds.")] float? fadeInDuration = null,
            [Description("Fade-out duration in seconds.")] float? fadeOutDuration = null,
            [Description("Enable position offset during fade-in.")] bool? enableOffsetFadeIn = null,
            [Description("Fade-in start offset X (number animates from this X position).")] float? fadeInOffsetX = null,
            [Description("Fade-in start offset Y (number animates from this Y position).")] float? fadeInOffsetY = null,
            [Description("Enable position offset during fade-out.")] bool? enableOffsetFadeOut = null,
            [Description("Fade-out end offset X.")] float? fadeOutOffsetX = null,
            [Description("Fade-out end offset Y.")] float? fadeOutOffsetY = null,
            [Description("Enable scale animation during fade-in (pops in from scaled size).")] bool? enableScaleFadeIn = null,
            [Description("Enable smooth lerp movement (default movement mode).")] bool? enableLerp = null,
            [Description("Enable physics-based velocity movement.")] bool? enableVelocity = null,
            [Description("Enable idle shaking.")] bool? enableShaking = null,
            [Description("Enable random rotation on spawn.")] bool? enableStartRotation = null,
            [Description("Min spawn rotation in degrees.")] float? minRotation = null,
            [Description("Max spawn rotation in degrees.")] float? maxRotation = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var dn = GetDN(gameObjectName);

                if (fadeInDuration.HasValue) dn.durationFadeIn = Mathf.Max(0f, fadeInDuration.Value);
                if (fadeOutDuration.HasValue) dn.durationFadeOut = Mathf.Max(0f, fadeOutDuration.Value);
                if (enableOffsetFadeIn.HasValue) dn.enableOffsetFadeIn = enableOffsetFadeIn.Value;
                if (fadeInOffsetX.HasValue || fadeInOffsetY.HasValue)
                    dn.offsetFadeIn = new Vector2(fadeInOffsetX ?? dn.offsetFadeIn.x, fadeInOffsetY ?? dn.offsetFadeIn.y);
                if (enableOffsetFadeOut.HasValue) dn.enableOffsetFadeOut = enableOffsetFadeOut.Value;
                if (fadeOutOffsetX.HasValue || fadeOutOffsetY.HasValue)
                    dn.offsetFadeOut = new Vector2(fadeOutOffsetX ?? dn.offsetFadeOut.x, fadeOutOffsetY ?? dn.offsetFadeOut.y);
                if (enableScaleFadeIn.HasValue) dn.enableScaleFadeIn = enableScaleFadeIn.Value;
                if (enableLerp.HasValue) dn.enableLerp = enableLerp.Value;
                if (enableVelocity.HasValue) dn.enableVelocity = enableVelocity.Value;
                if (enableShaking.HasValue) dn.enableShaking = enableShaking.Value;
                if (enableStartRotation.HasValue) dn.enableStartRotation = enableStartRotation.Value;
                if (minRotation.HasValue) dn.minRotation = minRotation.Value;
                if (maxRotation.HasValue) dn.maxRotation = maxRotation.Value;

                EditorUtility.SetDirty(dn);

                return $"OK: DamageNumber '{gameObjectName}' animation configured. fadeIn={dn.durationFadeIn:F3}s fadeOut={dn.durationFadeOut:F3}s lerp={dn.enableLerp} velocity={dn.enableVelocity} shaking={dn.enableShaking}";
            });
        }

        [McpPluginTool("dnp-configure-performance", Title = "Damage Numbers Pro / Configure Performance")]
        [Description(@"Configures pooling and spam control on a DamageNumber component.
enablePooling: recycle instances instead of destroy/instantiate (highly recommended for frequent spawning).
poolSize: max instances kept in pool. Set based on max concurrent numbers expected.
updateDelay: seconds between position/animation updates (default 0.0125 = 80fps updates). Increase to reduce CPU cost.
spamGroup: string identifier for grouping nearby numbers (prevents overlap). Numbers with same group combine.
enableCombination: merge nearby numbers with same spamGroup into one accumulating number.
enablePush: push other numbers away from newly spawned ones.")]
        public string ConfigurePerformance(
            [Description("Name of the GameObject with DamageNumber component.")] string gameObjectName,
            [Description("Enable object pooling for this damage number type.")] bool? enablePooling = null,
            [Description("Max pool size (instances kept for reuse).")] int? poolSize = null,
            [Description("Update interval in seconds (default 0.0125 = 80fps). Higher = less CPU.")] float? updateDelay = null,
            [Description("Spam group identifier. Numbers with same group can combine.")] string? spamGroup = null,
            [Description("Merge nearby same-group numbers into one accumulating number.")] bool? enableCombination = null,
            [Description("Push other numbers away when spawning.")] bool? enablePush = null,
            [Description("Destroy nearby same-group numbers when spawning.")] bool? enableDestruction = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var dn = GetDN(gameObjectName);

                if (enablePooling.HasValue) dn.enablePooling = enablePooling.Value;
                if (poolSize.HasValue) dn.poolSize = Mathf.Max(1, poolSize.Value);
                if (updateDelay.HasValue) dn.updateDelay = Mathf.Max(0.001f, updateDelay.Value);
                if (spamGroup != null) dn.spamGroup = spamGroup;
                if (enableCombination.HasValue) dn.enableCombination = enableCombination.Value;
                if (enablePush.HasValue) dn.enablePush = enablePush.Value;
                if (enableDestruction.HasValue) dn.enableDestruction = enableDestruction.Value;

                EditorUtility.SetDirty(dn);

                return $"OK: DamageNumber '{gameObjectName}' performance configured. pooling={dn.enablePooling} poolSize={dn.poolSize} updateDelay={dn.updateDelay:F4}s group='{dn.spamGroup}'";
            });
        }
    }
}
