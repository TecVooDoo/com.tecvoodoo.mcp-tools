#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.M3DText.Editor
{
    public partial class Tool_M3DText
    {
        [McpPluginTool("m3dt-configure", Title = "Modular 3D Text / Configure")]
        [Description(@"Configures common properties on a Modular3DText component.
All parameters optional -- only provided values are applied.

font: Font asset name or path (e.g. 'Bagnard SDF' or full path). Triggers full rebuild.
material: Material asset name or path.
fontSize / fontSizeY / fontSizeZ: components of the FontSize Vector3 (default xyz=8,8,1).
wordSpacing: float spacing between words.
capitalize / lowerCase: text-case toggles. Capitalize wins if both true.
autoLetterSize: derive letter size from rendered area.
autoFontSize: enable adaptive font size; min/max via minFontSize / maxFontSize Vector3 (use minSize/maxSize args).
useModules / applyModulesOnStart / applyModulesOnEnable / applyModuleOnNewCharacter: module-system toggles.
combineMeshInEditor / combineMeshDuringRuntime: combined-mesh optimization toggles.
hideLettersInPlayMode / hideLettersInEditMode: hide character GameObjects in the Hierarchy.")]
        public string Configure(
            [Description("GameObject name with Modular3DText.")]
            string gameObjectName,
            [Description("Font asset name or path.")] string? font = null,
            [Description("Material asset name or path.")] string? material = null,
            [Description("FontSize x (also sets y,z if those omitted).")] float? fontSize = null,
            [Description("FontSize y (overrides shared fontSize).")] float? fontSizeY = null,
            [Description("FontSize z (overrides shared fontSize).")] float? fontSizeZ = null,
            [Description("Word spacing.")] float? wordSpacing = null,
            [Description("Capitalize all characters.")] bool? capitalize = null,
            [Description("Lower-case all characters.")] bool? lowerCase = null,
            [Description("Use rendered-area letter size instead of font-defined size.")] bool? autoLetterSize = null,
            [Description("Enable auto font sizing within min/max.")] bool? autoFontSize = null,
            [Description("Min font size (single value applied to xyz).")] float? minSize = null,
            [Description("Max font size (single value applied to xyz).")] float? maxSize = null,
            [Description("Enable module system (effects).")] bool? useModules = null,
            [Description("Apply adding modules on Start.")] bool? applyModulesOnStart = null,
            [Description("Apply adding modules on Enable.")] bool? applyModulesOnEnable = null,
            [Description("Apply modules to new characters as they spawn.")] bool? applyModuleOnNewCharacter = null,
            [Description("Combine character meshes in Editor.")] bool? combineMeshInEditor = null,
            [Description("Combine character meshes at runtime.")] bool? combineMeshDuringRuntime = null,
            [Description("Hide character GameObjects in Hierarchy during play mode.")] bool? hideLettersInPlayMode = null,
            [Description("Hide character GameObjects in Hierarchy during edit mode.")] bool? hideLettersInEditMode = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var t = GetText(gameObjectName);
                var changes = new StringBuilder();
                int changeCount = 0;

                if (font != null)
                {
                    var fontAsset = FindFontByName(font);
                    if (fontAsset == null) throw new System.Exception($"Font '{font}' not found in project.");
                    Set(t, "Font", fontAsset);
                    changes.AppendLine($"  Font = {fontAsset.name}");
                    changeCount++;
                }

                if (material != null)
                {
                    var matAsset = FindMaterial(material);
                    if (matAsset == null) throw new System.Exception($"Material '{material}' not found in project.");
                    Set(t, "Material", matAsset);
                    changes.AppendLine($"  Material = {matAsset.name}");
                    changeCount++;
                }

                if (fontSize.HasValue || fontSizeY.HasValue || fontSizeZ.HasValue)
                {
                    var current = Get(t, "FontSize") is Vector3 cv ? cv : new Vector3(8, 8, 1);
                    float x = fontSize ?? current.x;
                    float y = fontSizeY ?? (fontSize ?? current.y);
                    float z = fontSizeZ ?? (fontSize ?? current.z);
                    var v = new Vector3(x, y, z);
                    Set(t, "FontSize", v);
                    changes.AppendLine($"  FontSize = {FormatVec3(v)}");
                    changeCount++;
                }

                if (wordSpacing.HasValue)        { Set(t, "WordSpacing", wordSpacing.Value); changes.AppendLine($"  WordSpacing = {wordSpacing}"); changeCount++; }
                if (capitalize.HasValue)         { Set(t, "Capitalize", capitalize.Value); changes.AppendLine($"  Capitalize = {capitalize}"); changeCount++; }
                if (lowerCase.HasValue)          { Set(t, "LowerCase", lowerCase.Value); changes.AppendLine($"  LowerCase = {lowerCase}"); changeCount++; }
                if (autoLetterSize.HasValue)     { Set(t, "AutoLetterSize", autoLetterSize.Value); changes.AppendLine($"  AutoLetterSize = {autoLetterSize}"); changeCount++; }

                if (autoFontSize.HasValue)       { Set(t, "autoFontSize", autoFontSize.Value); changes.AppendLine($"  autoFontSize = {autoFontSize}"); changeCount++; }
                if (minSize.HasValue)
                {
                    var v = new Vector3(minSize.Value, minSize.Value, minSize.Value);
                    Set(t, "minFontSize", v);
                    changes.AppendLine($"  minFontSize = {FormatVec3(v)}");
                    changeCount++;
                }
                if (maxSize.HasValue)
                {
                    var v = new Vector3(maxSize.Value, maxSize.Value, maxSize.Value);
                    Set(t, "maxFontSize", v);
                    changes.AppendLine($"  maxFontSize = {FormatVec3(v)}");
                    changeCount++;
                }

                if (useModules.HasValue)             { Set(t, "useModules", useModules.Value); changes.AppendLine($"  useModules = {useModules}"); changeCount++; }
                if (applyModulesOnStart.HasValue)    { Set(t, "applyModulesOnStart", applyModulesOnStart.Value); changes.AppendLine($"  applyModulesOnStart = {applyModulesOnStart}"); changeCount++; }
                if (applyModulesOnEnable.HasValue)   { Set(t, "applyModulesOnEnable", applyModulesOnEnable.Value); changes.AppendLine($"  applyModulesOnEnable = {applyModulesOnEnable}"); changeCount++; }
                if (applyModuleOnNewCharacter.HasValue) { Set(t, "applyModuleOnNewCharacter", applyModuleOnNewCharacter.Value); changes.AppendLine($"  applyModuleOnNewCharacter = {applyModuleOnNewCharacter}"); changeCount++; }

                if (combineMeshInEditor.HasValue)    { Set(t, "combineMeshInEditor", combineMeshInEditor.Value); changes.AppendLine($"  combineMeshInEditor = {combineMeshInEditor}"); changeCount++; }
                if (combineMeshDuringRuntime.HasValue) { Set(t, "combineMeshDuringRuntime", combineMeshDuringRuntime.Value); changes.AppendLine($"  combineMeshDuringRuntime = {combineMeshDuringRuntime}"); changeCount++; }
                if (hideLettersInPlayMode.HasValue)  { Set(t, "hideLettersInHierarchyInPlayMode", hideLettersInPlayMode.Value); changes.AppendLine($"  hideLettersInPlayMode = {hideLettersInPlayMode}"); changeCount++; }
                if (hideLettersInEditMode.HasValue)  { Set(t, "hideLettersInHierarchyInEditMode", hideLettersInEditMode.Value); changes.AppendLine($"  hideLettersInEditMode = {hideLettersInEditMode}"); changeCount++; }

                if (changeCount == 0)
                    return $"No changes applied to '{gameObjectName}'.";

                EditorUtility.SetDirty(t);
                return $"OK: '{gameObjectName}' updated ({changeCount} change(s)):\n{changes}";
            });
        }
    }
}
