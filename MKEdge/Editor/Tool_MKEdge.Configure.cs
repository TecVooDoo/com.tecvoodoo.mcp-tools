#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using MK.EdgeDetection;
using MK.EdgeDetection.PostProcessing.Generic;
using MK.EdgeDetection.UniversalVolumeComponents;
using UnityEditor;
using UnityEngine;

namespace MCPTools.MKEdge.Editor
{
    public partial class Tool_MKEdge
    {
        [McpPluginTool("mkedge-configure", Title = "MK Edge Detection / Configure")]
        [Description(@"Sets parameters on an MK Edge Detection effect (URP Volume or RendererFeature).
target: Volume GO name, VolumeProfile asset name, or UniversalRendererData name.
Only provided parameters change. Color values use hex format (FF0000).
precision: High, Medium, Low.
largeKernel: Sobel, Scharr, Prewitt.
mediumKernel: RobertsCrossDiagonal, RobertsCrossAxis.
smallKernel: HalfCrossHorizontal, HalfCrossVertical.
inputData: Depth, Normal, SceneColor (combine with |, e.g. 'Depth|Normal').
workflow: Generic, Selective.
For RendererFeature only: workMode (PostProcessVolumes, Global).")]
        public string Configure(
            [Description("Target Volume GO, VolumeProfile, or UniversalRendererData name.")] string target,
            [Description("precision: High/Medium/Low.")] string? precision = null,
            [Description("largeKernel: Sobel/Scharr/Prewitt.")] string? largeKernel = null,
            [Description("mediumKernel: RobertsCrossDiagonal/RobertsCrossAxis.")] string? mediumKernel = null,
            [Description("smallKernel: HalfCrossHorizontal/HalfCrossVertical.")] string? smallKernel = null,
            [Description("inputData flags: Depth, Normal, SceneColor (combine with |).")] string? inputData = null,
            [Description("workflow: Generic/Selective.")] string? workflow = null,
            [Description("workMode (RendererFeature only): PostProcessVolumes/Global.")] string? workMode = null,
            [Description("Global line size [0-3].")] float? globalLineSize = null,
            [Description("Depth line size [0-2].")] float? depthLineSize = null,
            [Description("Normal line size [0-2].")] float? normalLineSize = null,
            [Description("Scene color line size [0-2].")] float? sceneColorLineSize = null,
            [Description("Line hardness [0-1].")] float? lineHardness = null,
            [Description("Enable fade.")] bool? fade = null,
            [Description("Line size match factor [0-1].")] float? lineSizeMatchFactor = null,
            [Description("Depth threshold min [0-1].")] float? depthThresholdMin = null,
            [Description("Depth threshold max [0-1].")] float? depthThresholdMax = null,
            [Description("Normal threshold min [0-1].")] float? normalThresholdMin = null,
            [Description("Normal threshold max [0-1].")] float? normalThresholdMax = null,
            [Description("Scene color threshold min [0-1].")] float? sceneColorThresholdMin = null,
            [Description("Scene color threshold max [0-1].")] float? sceneColorThresholdMax = null,
            [Description("Depth near fade.")] float? depthNearFade = null,
            [Description("Depth far fade.")] float? depthFarFade = null,
            [Description("Normal near fade.")] float? normalNearFade = null,
            [Description("Normal far fade.")] float? normalsFarFade = null,
            [Description("Scene color near fade.")] float? sceneColorNearFade = null,
            [Description("Scene color far fade.")] float? sceneColorFarFade = null,
            [Description("Line color (hex, e.g. 000000).")] string? lineColorHex = null,
            [Description("Overlay color (hex, e.g. FFFFFF).")] string? overlayColorHex = null,
            [Description("Enable sketch overlay.")] bool? sketch = null,
            [Description("Sketch intensity [0-1].")] float? sketchIntensity = null,
            [Description("Sketch frequency.")] float? sketchFrequency = null,
            [Description("Enhance details (sub-pixel sharpening).")] bool? enhanceDetails = null,
            [Description("Visualize edges (debug).")] bool? visualizeEdges = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var (vc, feature) = FindMKEdge(target);
                int changes = 0;

                if (vc != null)
                {
                    if (precision != null) { vc.precision.value = new EnumProperty<Precision>(ParseEnum<Precision>(precision)); changes++; }
                    if (largeKernel != null) { vc.largeKernel.value = new EnumProperty<LargeKernel>(ParseEnum<LargeKernel>(largeKernel)); changes++; }
                    if (mediumKernel != null) { vc.mediumKernel.value = new EnumProperty<MediumKernel>(ParseEnum<MediumKernel>(mediumKernel)); changes++; }
                    if (smallKernel != null) { vc.smallKernel.value = new EnumProperty<SmallKernel>(ParseEnum<SmallKernel>(smallKernel)); changes++; }
                    if (inputData != null) { vc.inputData.value = new BitmaskProperty<InputData>(ParseInputData(inputData)); changes++; }
                    if (workflow != null) { vc.workflow.value = new EnumProperty<Workflow>(ParseEnum<Workflow>(workflow)); changes++; }

                    if (globalLineSize.HasValue) { vc.globalLineSize.value = new RangeProperty(globalLineSize.Value, vc.globalLineSize.value.minValue, vc.globalLineSize.value.maxValue); changes++; }
                    if (depthLineSize.HasValue) { vc.depthLineSize.value = new RangeProperty(depthLineSize.Value, vc.depthLineSize.value.minValue, vc.depthLineSize.value.maxValue); changes++; }
                    if (normalLineSize.HasValue) { vc.normalLineSize.value = new RangeProperty(normalLineSize.Value, vc.normalLineSize.value.minValue, vc.normalLineSize.value.maxValue); changes++; }
                    if (sceneColorLineSize.HasValue) { vc.sceneColorLineSize.value = new RangeProperty(sceneColorLineSize.Value, vc.sceneColorLineSize.value.minValue, vc.sceneColorLineSize.value.maxValue); changes++; }
                    if (lineHardness.HasValue) { vc.lineHardness.value = new RangeProperty(lineHardness.Value, vc.lineHardness.value.minValue, vc.lineHardness.value.maxValue); changes++; }
                    if (lineSizeMatchFactor.HasValue) { vc.lineSizeMatchFactor.value = new RangeProperty(lineSizeMatchFactor.Value, vc.lineSizeMatchFactor.value.minValue, vc.lineSizeMatchFactor.value.maxValue); changes++; }
                    if (fade.HasValue) { vc.fade.value = new BoolProperty(fade.Value); changes++; }

                    if (depthThresholdMin.HasValue || depthThresholdMax.HasValue) { var t = vc.depthThreshold.value; vc.depthThreshold.value = new MinMaxRangeProperty(depthThresholdMin ?? t.minValue, depthThresholdMax ?? t.maxValue, t.minLimit, t.maxLimit); changes++; }
                    if (normalThresholdMin.HasValue || normalThresholdMax.HasValue) { var t = vc.normalThreshold.value; vc.normalThreshold.value = new MinMaxRangeProperty(normalThresholdMin ?? t.minValue, normalThresholdMax ?? t.maxValue, t.minLimit, t.maxLimit); changes++; }
                    if (sceneColorThresholdMin.HasValue || sceneColorThresholdMax.HasValue) { var t = vc.sceneColorThreshold.value; vc.sceneColorThreshold.value = new MinMaxRangeProperty(sceneColorThresholdMin ?? t.minValue, sceneColorThresholdMax ?? t.maxValue, t.minLimit, t.maxLimit); changes++; }

                    if (depthNearFade.HasValue) { vc.depthNearFade.value = new AbsFloatProperty(depthNearFade.Value); changes++; }
                    if (depthFarFade.HasValue) { vc.depthFarFade.value = new AbsFloatProperty(depthFarFade.Value); changes++; }
                    if (normalNearFade.HasValue) { vc.normalNearFade.value = new AbsFloatProperty(normalNearFade.Value); changes++; }
                    if (normalsFarFade.HasValue) { vc.normalsFarFade.value = new AbsFloatProperty(normalsFarFade.Value); changes++; }
                    if (sceneColorNearFade.HasValue) { vc.sceneColorNearFade.value = new AbsFloatProperty(sceneColorNearFade.Value); changes++; }
                    if (sceneColorFarFade.HasValue) { vc.sceneColorFarFade.value = new AbsFloatProperty(sceneColorFarFade.Value); changes++; }

                    if (lineColorHex != null) { var c = ParseHex(lineColorHex); vc.lineColor.value = new ColorProperty(c.r, c.g, c.b, c.a, true, false); changes++; }
                    if (overlayColorHex != null) { var c = ParseHex(overlayColorHex); vc.overlayColor.value = new ColorProperty(c.r, c.g, c.b, c.a, true, false); changes++; }

                    if (sketch.HasValue) { vc.sketch.value = new BoolProperty(sketch.Value); changes++; }
                    if (sketchIntensity.HasValue) { vc.sketchIntensity.value = new RangeProperty(sketchIntensity.Value, vc.sketchIntensity.value.minValue, vc.sketchIntensity.value.maxValue); changes++; }
                    if (sketchFrequency.HasValue) { vc.sketchFrequency.value = new FloatProperty(sketchFrequency.Value); changes++; }

                    if (enhanceDetails.HasValue) { vc.enhanceDetails.value = new BoolProperty(enhanceDetails.Value); changes++; }
                    if (visualizeEdges.HasValue) { vc.visualizeEdges.value = new BoolProperty(visualizeEdges.Value); changes++; }

                    EditorUtility.SetDirty(vc);
                }
                else if (feature != null)
                {
                    if (workMode != null) { feature.workMode = ParseWorkMode(workMode); changes++; }
                    if (precision != null) { feature.precision = new EnumProperty<Precision>(ParseEnum<Precision>(precision)); changes++; }
                    if (largeKernel != null) { feature.largeKernel = new EnumProperty<LargeKernel>(ParseEnum<LargeKernel>(largeKernel)); changes++; }
                    if (mediumKernel != null) { feature.mediumKernel = new EnumProperty<MediumKernel>(ParseEnum<MediumKernel>(mediumKernel)); changes++; }
                    if (smallKernel != null) { feature.smallKernel = new EnumProperty<SmallKernel>(ParseEnum<SmallKernel>(smallKernel)); changes++; }
                    if (inputData != null) { feature.inputData = new BitmaskProperty<InputData>(ParseInputData(inputData)); changes++; }
                    if (globalLineSize.HasValue) { feature.globalLineSize = new RangeProperty(globalLineSize.Value, feature.globalLineSize.minValue, feature.globalLineSize.maxValue); changes++; }
                    if (lineHardness.HasValue) { feature.lineHardness = new RangeProperty(lineHardness.Value, feature.lineHardness.minValue, feature.lineHardness.maxValue); changes++; }
                    if (lineColorHex != null) { var c = ParseHex(lineColorHex); feature.lineColor = new ColorProperty(c.r, c.g, c.b, c.a, true, false); changes++; }
                    if (sketch.HasValue) { feature.sketch = new BoolProperty(sketch.Value); changes++; }
                    if (visualizeEdges.HasValue) { feature.visualizeEdges = new BoolProperty(visualizeEdges.Value); changes++; }
                }

                return $"OK: MK Edge Detection on '{target}' updated. {changes} parameter(s) applied.";
            });
        }

        static T ParseEnum<T>(string s) where T : struct, Enum =>
            (T)Enum.Parse(typeof(T), s, true);

        static InputData ParseInputData(string s)
        {
            // Allows "Depth|Normal" or "Depth, Normal"
            string normalized = s.Replace('|', ',');
            return (InputData)Enum.Parse(typeof(InputData), normalized, true);
        }

        static MK.EdgeDetection.UniversalRendererFeatures.MKEdgeDetectionRendererFeature.WorkMode ParseWorkMode(string s) =>
            (MK.EdgeDetection.UniversalRendererFeatures.MKEdgeDetectionRendererFeature.WorkMode)Enum.Parse(
                typeof(MK.EdgeDetection.UniversalRendererFeatures.MKEdgeDetectionRendererFeature.WorkMode), s, true);

        static Color ParseHex(string hex)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out Color c)) return c;
            throw new Exception($"Invalid hex color '{hex}'.");
        }
    }
}
