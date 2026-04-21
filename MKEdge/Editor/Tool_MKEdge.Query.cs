#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using MK.EdgeDetection;
using MK.EdgeDetection.UniversalVolumeComponents;
using MK.EdgeDetection.UniversalRendererFeatures;
using UnityEngine;

namespace MCPTools.MKEdge.Editor
{
    public partial class Tool_MKEdge
    {
        [McpPluginTool("mkedge-query", Title = "MK Edge Detection / Query")]
        [Description(@"Reports the full state of an MK Edge Detection effect.
target: name of a Volume GameObject, a VolumeProfile asset, or a UniversalRendererData asset with the feature.
Reports all 36 parameters + active/enabled state + variant (Volume vs RendererFeature).")]
        public string Query(
            [Description("Volume GO name, VolumeProfile asset name, or UniversalRendererData asset name.")] string target
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var (vc, feature) = FindMKEdge(target);
                var sb = new StringBuilder();

                if (vc != null)
                {
                    sb.AppendLine($"MK Edge Detection (URP Volume) on '{target}':");
                    sb.AppendLine($"  Active: {vc.active}");
                    AppendParams(sb, vc);
                }
                else if (feature != null)
                {
                    sb.AppendLine($"MK Edge Detection (URP Renderer Feature) on '{target}':");
                    sb.AppendLine($"  IsActive: {feature.isActive}");
                    sb.AppendLine($"  WorkMode: {feature.workMode}");
                    AppendFeatureParams(sb, feature);
                }

                return sb.ToString();
            });
        }

        static void AppendParams(StringBuilder sb, MKEdgeDetection vc)
        {
            sb.AppendLine($"  Precision: {vc.precision.value.value}");
            sb.AppendLine($"  Kernels: large={vc.largeKernel.value.value} medium={vc.mediumKernel.value.value} small={vc.smallKernel.value.value}");
            sb.AppendLine($"  InputData: {vc.inputData.value.value}");
            sb.AppendLine($"  LineHardness: {vc.lineHardness.value.value:F3}");
            sb.AppendLine($"  Fade: {vc.fade.value.value}");
            sb.AppendLine($"  GlobalLineSize: {vc.globalLineSize.value.value:F3} (range {vc.globalLineSize.value.minValue}-{vc.globalLineSize.value.maxValue})");
            sb.AppendLine($"  DepthLineSize: {vc.depthLineSize.value.value:F3}");
            sb.AppendLine($"  NormalLineSize: {vc.normalLineSize.value.value:F3}");
            sb.AppendLine($"  SceneColorLineSize: {vc.sceneColorLineSize.value.value:F3}");
            sb.AppendLine($"  DepthThreshold: {vc.depthThreshold.value.minValue:F3}-{vc.depthThreshold.value.maxValue:F3}");
            sb.AppendLine($"  NormalThreshold: {vc.normalThreshold.value.minValue:F3}-{vc.normalThreshold.value.maxValue:F3}");
            sb.AppendLine($"  SceneColorThreshold: {vc.sceneColorThreshold.value.minValue:F3}-{vc.sceneColorThreshold.value.maxValue:F3}");
            sb.AppendLine($"  DepthFade: near={vc.depthNearFade.value.value:F1} far={vc.depthFarFade.value.value:F1}");
            sb.AppendLine($"  NormalFade: near={vc.normalNearFade.value.value:F1} far={vc.normalsFarFade.value.value:F1}");
            sb.AppendLine($"  SceneColorFade: near={vc.sceneColorNearFade.value.value:F1} far={vc.sceneColorFarFade.value.value:F1}");
            sb.AppendLine($"  LineColor: {(Color)vc.lineColor.value}");
            sb.AppendLine($"  OverlayColor: {(Color)vc.overlayColor.value}");
            sb.AppendLine($"  Sketch: {vc.sketch.value.value} intensity={vc.sketchIntensity.value.value:F3} freq={vc.sketchFrequency.value.value:F1}");
            sb.AppendLine($"  EnhanceDetails: {vc.enhanceDetails.value.value}");
            sb.AppendLine($"  VisualizeEdges: {vc.visualizeEdges.value.value}");
            sb.AppendLine($"  Workflow: {vc.workflow.value.value}");
        }

        static void AppendFeatureParams(StringBuilder sb, MKEdgeDetectionRendererFeature f)
        {
            sb.AppendLine($"  Precision: {f.precision.value}");
            sb.AppendLine($"  Kernels: large={f.largeKernel.value} medium={f.mediumKernel.value} small={f.smallKernel.value}");
            sb.AppendLine($"  InputData: {f.inputData.value}");
            sb.AppendLine($"  GlobalLineSize: {f.globalLineSize.value:F3}");
            sb.AppendLine($"  LineColor: {(Color)f.lineColor}");
            sb.AppendLine($"  Sketch: {f.sketch.value}");
        }
    }
}
