#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEditor;
using UnityEngine;

namespace MCPTools.M3DText.Editor
{
    [McpPluginToolType]
    public partial class Tool_M3DText
    {
        // Cached types resolved via reflection. M3DText scripts compile into Assembly-CSharp-firstpass
        // because they live under Assets/Plugins/Tiny Giant Studio/.
        static readonly Type? Modular3DTextType   = FindType("TinyGiantStudio.Text.Modular3DText");
        static readonly Type? FontType            = FindType("TinyGiantStudio.Text.Font");
        static readonly Type? ModuleType          = FindType("TinyGiantStudio.Modules.Module");
        static readonly Type? ModuleContainerType = FindType("TinyGiantStudio.Modules.ModuleContainer");
        static readonly Type? VariableHolderType  = FindType("TinyGiantStudio.Modules.VariableHolder");
        static readonly Type? ButtonType          = FindType("TinyGiantStudio.Text.Button");
        static readonly Type? SliderType          = FindType("TinyGiantStudio.Text.Slider");
        static readonly Type? InputFieldType      = FindType("TinyGiantStudio.Text.InputField");
        static readonly Type? HorizontalSelectorType = FindType("TinyGiantStudio.Text.HorizontalSelector");
        static readonly Type? ListType            = FindType("TinyGiantStudio.Text.List");
        static readonly Type? ToggleType          = FindType("TinyGiantStudio.Text.Toggle");

        static Type? FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        static UnityEngine.Component GetText(string gameObjectName)
        {
            if (Modular3DTextType == null)
                throw new Exception("TinyGiantStudio.Text.Modular3DText type not found in loaded assemblies. Modular 3D Text may not be installed.");
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new Exception($"GameObject '{gameObjectName}' not found.");
            var t = go.GetComponent(Modular3DTextType);
            if (t == null)
                throw new Exception($"'{gameObjectName}' has no Modular3DText component.");
            return t;
        }

        static IEnumerable<UnityEngine.Component> FindAllTexts()
        {
            if (Modular3DTextType == null) yield break;
            var all = UnityEngine.Object.FindObjectsByType(Modular3DTextType, FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var obj in all)
                if (obj is UnityEngine.Component c) yield return c;
        }

        static object? Get(object target, string name)
        {
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(target);
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(target);
            return null;
        }

        static bool Set(object target, string name, object? value)
        {
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, value);
                return true;
            }
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, value);
                return true;
            }
            return false;
        }

        static UnityEngine.Object? FindFontByName(string nameOrPath)
        {
            if (FontType == null) return null;

            if (nameOrPath.Contains('/') || nameOrPath.EndsWith(".asset"))
            {
                var byPath = AssetDatabase.LoadAssetAtPath(nameOrPath, FontType) as UnityEngine.Object;
                if (byPath != null) return byPath;
            }

            string fileName = System.IO.Path.GetFileNameWithoutExtension(nameOrPath);
            string filter = $"t:{FontType.Name} {fileName}";
            var guids = AssetDatabase.FindAssets(filter);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, FontType) as UnityEngine.Object;
                if (asset == null) continue;
                if (string.Equals(asset.name, fileName, StringComparison.OrdinalIgnoreCase))
                    return asset;
            }
            // Fallback to first hit
            if (guids.Length > 0)
                return AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), FontType) as UnityEngine.Object;
            return null;
        }

        static List<UnityEngine.Object> ListAllFonts()
        {
            var list = new List<UnityEngine.Object>();
            if (FontType == null) return list;
            string filter = $"t:{FontType.Name}";
            foreach (var guid in AssetDatabase.FindAssets(filter))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(path, FontType) as UnityEngine.Object;
                if (asset != null) list.Add(asset);
            }
            return list;
        }

        static UnityEngine.Object? FindMaterial(string nameOrPath)
        {
            if (nameOrPath.Contains('/') || nameOrPath.EndsWith(".mat"))
            {
                var byPath = AssetDatabase.LoadAssetAtPath<Material>(nameOrPath);
                if (byPath != null) return byPath;
            }
            string fileName = System.IO.Path.GetFileNameWithoutExtension(nameOrPath);
            var guids = AssetDatabase.FindAssets($"t:Material {fileName}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;
                if (string.Equals(mat.name, fileName, StringComparison.OrdinalIgnoreCase))
                    return mat;
            }
            if (guids.Length > 0)
                return AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return null;
        }

        static string FormatVec3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";
    }
}
