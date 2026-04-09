#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.TextureStudio.Editor
{
    public partial class Tool_TextureStudio
    {
        [McpPluginTool("texstudio-set-param", Title = "Texture Studio / Set Parameter")]
        [Description(@"Sets a parameter on a CompositeMap layer (cascades to children).
paramName values: Active, Angle, Bend, Blend, BlendAmount, BlendMode, FontSize, Map, Mask,
  OutlineWidth, OutlineCol, SpriteIndex, Tint, Text, TextAlign, TextCol, TextStyle, TextObj,
  UseMask, ReplaceColors, Position, Scale, Contrast, Saturation, ActiveCondition, Opacity, Blur, Hue.
After setting params, call texstudio-render to update the texture output.")]
        public string SetParam(
            [Description("Name of the CompositeMap asset.")] string assetName,
            [Description("Layer name to modify. Omit for all layers.")] string? layerName = null,
            [Description("Parameter name from ParamName enum (e.g. Tint, Opacity, Active, Hue).")] string? paramName = null,
            [Description("Float value (for Opacity, Angle, Hue, Saturation, Contrast, Blur, BlendAmount, FontSize).")] float? floatValue = null,
            [Description("Bool value (for Active, UseMask).")] bool? boolValue = null,
            [Description("Int value (for SpriteIndex, BlendMode).")] int? intValue = null,
            [Description("Color as hex (for Tint, TextCol, OutlineCol).")] string? colorHex = null,
            [Description("String value (for Text).")] string? stringValue = null,
            [Description("Texture asset name (for Map, Mask).")] string? textureName = null,
            [Description("Position X (for SetLayerTransform).")] float? posX = null,
            [Description("Position Y (for SetLayerTransform).")] float? posY = null,
            [Description("Scale X (for SetLayerTransform).")] float? scaleX = null,
            [Description("Scale Y (for SetLayerTransform).")] float? scaleY = null,
            [Description("Rotation angle in degrees (for SetLayerTransform).")] float? angle = null,
            [Description("Sprite asset name to assign (for SetSprite).")] string? spriteName = null,
            [Description("Save current state with this name.")] string? saveStateName = null,
            [Description("Restore state by name.")] string? loadStateName = null,
            [Description("Restore state by index.")] int? loadStateIndex = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var map = GetMap(assetName);
                int changes = 0;

                // State management
                if (saveStateName != null) { Call(map, "SaveState", saveStateName); changes++; }
                if (loadStateName != null) { Call(map, "SetState", loadStateName); changes++; }
                if (loadStateIndex.HasValue) { Call(map, "SetState", loadStateIndex.Value); changes++; }

                // Sprite assignment
                if (spriteName != null && layerName != null)
                {
                    string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite");
                    if (guids.Length == 0) throw new System.Exception($"Sprite '{spriteName}' not found.");
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite == null) throw new System.Exception($"Failed to load Sprite at '{path}'.");
                    Call(map, "SetSprite", layerName, sprite);
                    changes++;
                }

                // Transform
                if (layerName != null && (posX.HasValue || posY.HasValue || scaleX.HasValue || scaleY.HasValue || angle.HasValue))
                {
                    // Get current transform via reflection
                    Vector2 pos = Vector2.zero;
                    float ang = 0f;
                    Vector2 scl = Vector2.one;

                    // Use FindLayer + direct field access instead of ref params
                    var layer = Call(map, "FindLayer", layerName);
                    if (layer == null) throw new System.Exception($"Layer '{layerName}' not found.");

                    var posVal = Get(layer, "pos");
                    if (posVal is Vector3 p3) pos = new Vector2(p3.x, p3.y);
                    var sclVal = Get(layer, "scale");
                    if (sclVal is Vector2 s2) scl = s2;
                    var angVal = Get(layer, "angle");
                    if (angVal is float a) ang = a;

                    if (posX.HasValue) pos.x = posX.Value;
                    if (posY.HasValue) pos.y = posY.Value;
                    if (scaleX.HasValue) scl.x = scaleX.Value;
                    if (scaleY.HasValue) scl.y = scaleY.Value;
                    if (angle.HasValue) ang = angle.Value;

                    Call(map, "SetLayerTransform", layerName, pos, ang, scl);
                    changes++;
                }

                // SetParam by type
                if (paramName != null)
                {
                    object? target = map;
                    if (layerName != null)
                    {
                        target = Call(map, "FindLayer", layerName);
                        if (target == null) throw new System.Exception($"Layer '{layerName}' not found.");
                    }

                    if (floatValue.HasValue) { Call(target, "SetParam", paramName, floatValue.Value); changes++; }
                    else if (boolValue.HasValue) { Call(target, "SetParam", paramName, boolValue.Value); changes++; }
                    else if (intValue.HasValue) { Call(target, "SetParam", paramName, intValue.Value); changes++; }
                    else if (colorHex != null)
                    {
                        if (!ColorUtility.TryParseHtmlString($"#{colorHex}", out Color c))
                            throw new System.Exception($"Invalid hex color '{colorHex}'.");
                        Call(target, "SetParam", paramName, c);
                        changes++;
                    }
                    else if (stringValue != null) { Call(target, "SetParam", paramName, stringValue); changes++; }
                    else if (textureName != null)
                    {
                        string[] guids = AssetDatabase.FindAssets($"{textureName} t:Texture");
                        if (guids.Length == 0) throw new System.Exception($"Texture '{textureName}' not found.");
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        var tex = AssetDatabase.LoadAssetAtPath<Texture>(path);
                        if (tex == null) throw new System.Exception($"Failed to load Texture at '{path}'.");
                        Call(target, "SetParam", paramName, tex);
                        changes++;
                    }
                }

                EditorUtility.SetDirty(map);
                return $"OK: CompositeMap '{assetName}' updated. {changes} change(s) applied.";
            });
        }
    }
}
