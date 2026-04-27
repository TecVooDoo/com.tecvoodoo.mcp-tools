#nullable enable
using System.Collections;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.M3DText.Editor
{
    public partial class Tool_M3DText
    {
        [McpPluginTool("m3dt-query", Title = "Modular 3D Text / Query")]
        [Description(@"Reports Modular3DText components in the active scene.
If gameObjectName is provided, reports just that one with full config.
Otherwise lists every Modular3DText component with text + font + size.

Reports per-text: text content (truncated), font asset, material, FontSize (Vector3),
WordSpacing, Capitalize/LowerCase/AutoLetterSize flags, useModules + adding/deleting module counts,
characterObjectList count, autoFontSize, min/maxFontSize.")]
        public string Query(
            [Description("Optional GameObject name with Modular3DText. Omit to list all in scene.")]
            string? gameObjectName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();

                if (!string.IsNullOrEmpty(gameObjectName))
                {
                    var t = GetText(gameObjectName);
                    AppendDetailed(sb, t);
                    return sb.ToString();
                }

                var all = FindAllTexts();
                int count = 0;
                foreach (var t in all)
                {
                    count++;
                    AppendBrief(sb, t);
                }
                if (count == 0)
                {
                    sb.AppendLine("No Modular3DText components found in the active scene.");
                }
                else
                {
                    sb.Insert(0, $"Modular3DText components in scene ({count}):\n");
                }
                return sb.ToString();
            });
        }

        static void AppendBrief(StringBuilder sb, UnityEngine.Component t)
        {
            string text = (Get(t, "Text") as string) ?? "";
            if (text.Length > 60) text = text.Substring(0, 60) + "...";
            var font = Get(t, "Font") as UnityEngine.Object;
            var fontSize = Get(t, "FontSize");
            sb.AppendLine($"  - '{t.gameObject.name}' text=\"{text}\"  font={(font != null ? font.name : "(null)")}  size={(fontSize is Vector3 v ? FormatVec3(v) : fontSize?.ToString())}");
        }

        static void AppendDetailed(StringBuilder sb, UnityEngine.Component t)
        {
            sb.AppendLine($"=== Modular3DText on '{t.gameObject.name}' (scene='{t.gameObject.scene.name}') ===");
            string text = (Get(t, "Text") as string) ?? "";
            if (text.Length > 200) text = text.Substring(0, 200) + "...";
            sb.AppendLine($"  Text: \"{text}\"");

            var font = Get(t, "Font") as UnityEngine.Object;
            sb.AppendLine($"  Font:                {(font != null ? font.name : "(null)")}");
            var mat = Get(t, "Material") as Material;
            sb.AppendLine($"  Material:            {(mat != null ? mat.name : "(null)")}");
            var size = Get(t, "FontSize");
            sb.AppendLine($"  FontSize:            {(size is Vector3 v ? FormatVec3(v) : size?.ToString())}");
            sb.AppendLine($"  WordSpacing:         {Get(t, "WordSpacing")}");
            sb.AppendLine($"  Capitalize:          {Get(t, "Capitalize")}");
            sb.AppendLine($"  LowerCase:           {Get(t, "LowerCase")}");
            sb.AppendLine($"  AutoLetterSize:      {Get(t, "AutoLetterSize")}");
            sb.AppendLine($"  autoFontSize:        {Get(t, "autoFontSize")}");
            var minSize = Get(t, "minFontSize");
            var maxSize = Get(t, "maxFontSize");
            if (minSize is Vector3 mi && maxSize is Vector3 ma)
                sb.AppendLine($"  min/max FontSize:    {FormatVec3(mi)} / {FormatVec3(ma)}");

            sb.AppendLine($"  useModules:          {Get(t, "useModules")}");
            int addCount = (Get(t, "addingModules") is ICollection ac) ? ac.Count : 0;
            int delCount = (Get(t, "deletingModules") is ICollection dc) ? dc.Count : 0;
            sb.AppendLine($"  Modules: adding={addCount}  deleting={delCount}");
            sb.AppendLine($"  applyModuleOnNewCharacter: {Get(t, "applyModuleOnNewCharacter")}");
            sb.AppendLine($"  applyModulesOnStart:       {Get(t, "applyModulesOnStart")}");
            sb.AppendLine($"  applyModulesOnEnable:      {Get(t, "applyModulesOnEnable")}");

            int charCount = (Get(t, "characterObjectList") is ICollection cc) ? cc.Count : 0;
            sb.AppendLine($"  characterObjectList:       {charCount}");

            sb.AppendLine($"  combineMeshInEditor:       {Get(t, "combineMeshInEditor")}");
            sb.AppendLine($"  combineMeshDuringRuntime:  {Get(t, "combineMeshDuringRuntime")}");
            sb.AppendLine($"  hideLettersInPlayMode:     {Get(t, "hideLettersInHierarchyInPlayMode")}");
            sb.AppendLine($"  hideLettersInEditMode:     {Get(t, "hideLettersInHierarchyInEditMode")}");
        }
    }
}
