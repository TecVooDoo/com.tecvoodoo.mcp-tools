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
        [McpPluginTool("mkedge-preset", Title = "MK Edge Detection / Apply Preset")]
        [Description(@"Applies a named preset of MK Edge Detection parameters to a target.
Presets: subtle-outline, comic, blueprint, sketch, ink-wash, souls-like, toon, noir.
Only works on the URP VolumeComponent variant.")]
        public string Preset(
            [Description("Target Volume GO or VolumeProfile asset name.")] string target,
            [Description("Preset name: subtle-outline, comic, blueprint, sketch, ink-wash, souls-like, toon, noir.")] string preset
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var (vc, feature) = FindMKEdge(target);
                if (vc == null)
                    throw new Exception("mkedge-preset only works on URP VolumeComponent targets. Use mkedge-configure for RendererFeatures.");

                string p = preset.ToLowerInvariant().Trim();
                switch (p)
                {
                    case "subtle-outline":
                        ApplyPreset(vc, lineSize: 0.5f, hardness: 0.5f, depthThr: (0.05f, 0.5f), normalThr: (0.1f, 1f), color: Color.black, inputs: InputData.Depth | InputData.Normal, kernel: LargeKernel.Sobel, sketch: false);
                        break;
                    case "comic":
                        ApplyPreset(vc, lineSize: 1.5f, hardness: 1f, depthThr: (0.1f, 1f), normalThr: (0.2f, 1f), color: Color.black, inputs: InputData.Depth | InputData.Normal, kernel: LargeKernel.Sobel, sketch: false);
                        break;
                    case "blueprint":
                        ApplyPreset(vc, lineSize: 1.2f, hardness: 0.9f, depthThr: (0.05f, 1f), normalThr: (0.1f, 1f), color: new Color(0.4f, 0.6f, 1f, 1f), inputs: InputData.Depth | InputData.Normal, kernel: LargeKernel.Scharr, sketch: false);
                        break;
                    case "sketch":
                        ApplyPreset(vc, lineSize: 1f, hardness: 0.7f, depthThr: (0.1f, 0.9f), normalThr: (0.15f, 1f), color: Color.black, inputs: InputData.Depth | InputData.Normal, kernel: LargeKernel.Sobel, sketch: true, sketchIntensity: 0.6f);
                        break;
                    case "ink-wash":
                        ApplyPreset(vc, lineSize: 2f, hardness: 0.6f, depthThr: (0.05f, 1f), normalThr: (0.1f, 1f), color: Color.black, inputs: InputData.Depth | InputData.Normal | InputData.SceneColor, kernel: LargeKernel.Prewitt, sketch: true, sketchIntensity: 0.4f);
                        break;
                    case "souls-like":
                        ApplyPreset(vc, lineSize: 1f, hardness: 1f, depthThr: (0.1f, 0.8f), normalThr: (0.2f, 1f), color: new Color(0.05f, 0.05f, 0.05f, 1f), inputs: InputData.Depth | InputData.Normal, kernel: LargeKernel.Sobel, sketch: false, enhance: true);
                        break;
                    case "toon":
                        ApplyPreset(vc, lineSize: 1.5f, hardness: 1f, depthThr: (0.05f, 1f), normalThr: (0.1f, 1f), color: Color.black, inputs: InputData.Depth | InputData.Normal, kernel: LargeKernel.Sobel, sketch: false);
                        break;
                    case "noir":
                        ApplyPreset(vc, lineSize: 1.8f, hardness: 1f, depthThr: (0.05f, 1f), normalThr: (0.1f, 1f), color: Color.black, inputs: InputData.Depth | InputData.Normal | InputData.SceneColor, kernel: LargeKernel.Sobel, sketch: false, enhance: true);
                        break;
                    default:
                        throw new Exception($"Unknown preset '{preset}'. Valid: subtle-outline, comic, blueprint, sketch, ink-wash, souls-like, toon, noir.");
                }

                EditorUtility.SetDirty(vc);
                return $"OK: Preset '{p}' applied to MK Edge Detection on '{target}'.";
            });
        }

        static void ApplyPreset(MKEdgeDetection vc, float lineSize, float hardness, (float min, float max) depthThr, (float min, float max) normalThr, Color color, InputData inputs, LargeKernel kernel, bool sketch, float sketchIntensity = 0.333f, bool enhance = false)
        {
            vc.globalLineSize.value = new RangeProperty(lineSize, vc.globalLineSize.value.minValue, vc.globalLineSize.value.maxValue);
            vc.lineHardness.value = new RangeProperty(hardness, vc.lineHardness.value.minValue, vc.lineHardness.value.maxValue);
            var dt = vc.depthThreshold.value;
            vc.depthThreshold.value = new MinMaxRangeProperty(depthThr.min, depthThr.max, dt.minLimit, dt.maxLimit);
            var nt = vc.normalThreshold.value;
            vc.normalThreshold.value = new MinMaxRangeProperty(normalThr.min, normalThr.max, nt.minLimit, nt.maxLimit);
            vc.lineColor.value = new ColorProperty(color.r, color.g, color.b, color.a, true, false);
            vc.inputData.value = new BitmaskProperty<InputData>(inputs);
            vc.largeKernel.value = new EnumProperty<LargeKernel>(kernel);
            vc.sketch.value = new BoolProperty(sketch);
            if (sketch)
                vc.sketchIntensity.value = new RangeProperty(sketchIntensity, vc.sketchIntensity.value.minValue, vc.sketchIntensity.value.maxValue);
            vc.enhanceDetails.value = new BoolProperty(enhance);
            vc.active = true;
        }
    }
}
