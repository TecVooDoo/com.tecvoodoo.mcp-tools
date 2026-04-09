#if HAS_MUDBUN
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.MudBun.Editor
{
    public partial class Tool_MudBun
    {
        [McpPluginTool("mudbun-configure-brush", Title = "MudBun / Configure Brush")]
        [Description(@"Configures a MudBun brush (primitive) on a named GameObject.
The brush must be a child of a MudRenderer. Detects brush type automatically via reflection.
operator: Union, Subtract, Intersect, Dye, Pipe, Engrave.
Calls MarkDirty after changes to trigger mesh regeneration.")]
        public string ConfigureBrush(
            [Description("Name of the brush GameObject (child of a MudRenderer).")]
            string gameObjectName,
            [Description("Brush operator: Union, Subtract, Intersect, Dye, Pipe, Engrave.")]
            string? @operator = null,
            [Description("Blend factor for the brush. Higher = smoother blending with neighbors.")]
            float? blend = null,
            [Description("Radius for sphere or cylinder brushes.")]
            float? radius = null,
            [Description("Round factor for box or cylinder brushes.")]
            float? round = null,
            [Description("Brush color as hex string, e.g. '#FF0000'.")]
            string? colorHex = null,
            [Description("Brush emission color as hex string, e.g. '#FF8800'.")]
            string? emission = null,
            [Description("Brush metallic value [0-1].")]
            float? metallic = null,
            [Description("Brush smoothness value [0-1].")]
            float? smoothness = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var brush = GetBrush(gameObjectName);
                var brushType = GetBrushTypeName(brush);
                var changes = new System.Text.StringBuilder();

                if (@operator != null)
                {
                    SetEnum(brush, "Operator", @operator);
                    changes.Append($" Operator={@operator}");
                }
                if (blend.HasValue)
                {
                    Set(brush, "Blend", blend.Value);
                    changes.Append($" Blend={blend.Value:F3}");
                }

                // Type-specific parameters
                if (radius.HasValue)
                {
                    if (Set(brush, "Radius", radius.Value))
                        changes.Append($" Radius={radius.Value:F3}");
                    else
                        changes.Append($" [Radius not applicable to {brushType}]");
                }
                if (round.HasValue)
                {
                    if (Set(brush, "Round", round.Value))
                        changes.Append($" Round={round.Value:F3}");
                    else
                        changes.Append($" [Round not applicable to {brushType}]");
                }

                // Material properties on the brush
                if (colorHex != null)
                {
                    var color = ParseColor(colorHex);
                    SetColor(brush, "Color", color);
                    changes.Append($" Color={colorHex}");
                }
                if (emission != null)
                {
                    var emColor = ParseColor(emission);
                    SetColor(brush, "Emission", emColor);
                    changes.Append($" Emission={emission}");
                }
                if (metallic.HasValue)
                {
                    Set(brush, "Metallic", Mathf.Clamp01(metallic.Value));
                    changes.Append($" Metallic={metallic.Value:F2}");
                }
                if (smoothness.HasValue)
                {
                    Set(brush, "Smoothness", Mathf.Clamp01(smoothness.Value));
                    changes.Append($" Smoothness={smoothness.Value:F2}");
                }

                MarkDirty(brush);

                return changes.Length > 0
                    ? $"OK: Brush '{gameObjectName}' ({brushType}) updated:{changes}"
                    : $"OK: Brush '{gameObjectName}' ({brushType}) — no changes specified.";
            });
        }
    }
}
#endif
