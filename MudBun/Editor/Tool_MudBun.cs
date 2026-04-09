#if HAS_MUDBUN
#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.MudBun.Editor
{
    [McpPluginToolType]
    public partial class Tool_MudBun
    {
        static readonly Type? MudRendererBaseType = FindType("MudBun.MudRendererBase");
        static readonly Type? MudRendererType     = FindType("MudBun.MudRenderer");
        static readonly Type? MudSolidType        = FindType("MudBun.MudSolid");
        static readonly Type? MudSphereType       = FindType("MudBun.MudSphere");
        static readonly Type? MudBoxType          = FindType("MudBun.MudBox");
        static readonly Type? MudCylinderType     = FindType("MudBun.MudCylinder");
        static readonly Type? MudMaterialBaseType = FindType("MudBun.MudMaterialBase");

        static Type? FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
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

        static bool Set(object target, string name, object value)
        {
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, Convert.ChangeType(value, prop.PropertyType)); return true; }
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, Convert.ChangeType(value, field.FieldType)); return true; }
            return false;
        }

        static bool SetEnum(object target, string name, string enumValue)
        {
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                var parsed = Enum.Parse(prop.PropertyType, enumValue, ignoreCase: true);
                prop.SetValue(target, parsed);
                return true;
            }
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                var parsed = Enum.Parse(field.FieldType, enumValue, ignoreCase: true);
                field.SetValue(target, parsed);
                return true;
            }
            return false;
        }

        static object? Call(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null) throw new Exception($"Method '{methodName}' not found on {target.GetType().Name}.");
            return method.Invoke(target, args);
        }

        static Component GetRenderer(string gameObjectName)
        {
            if (MudRendererBaseType == null)
                throw new Exception("MudBun.MudRendererBase type not found in loaded assemblies.");
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new Exception($"GameObject '{gameObjectName}' not found.");
            var renderer = go.GetComponent(MudRendererBaseType);
            if (renderer == null)
                throw new Exception($"'{gameObjectName}' has no MudRenderer component.");
            return renderer;
        }

        static Component GetBrush(string gameObjectName)
        {
            if (MudSolidType == null)
                throw new Exception("MudBun.MudSolid type not found in loaded assemblies.");
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new Exception($"GameObject '{gameObjectName}' not found.");
            var brush = go.GetComponent(MudSolidType);
            if (brush == null)
                throw new Exception($"'{gameObjectName}' has no MudBun brush component (MudSolid).");
            return brush;
        }

        static object? GetBrushMaterial(Component brush)
        {
            // MudBun stores material data on the brush itself via properties
            return brush;
        }

        static Color ParseColor(string hex)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out var color))
                return color;
            throw new Exception($"Invalid hex color: '{hex}'. Use format '#RRGGBB' or '#RRGGBBAA'.");
        }

        static void SetColor(object target, string name, Color color)
        {
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, color); return; }
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, color); return; }
            throw new Exception($"Color property/field '{name}' not found on {type.Name}.");
        }

        static string FormatColor(object? c)
        {
            if (c is Color col) return $"#{ColorUtility.ToHtmlStringRGBA(col)}";
            return c?.ToString() ?? "null";
        }

        static string GetBrushTypeName(Component brush)
        {
            var typeName = brush.GetType().Name;
            return typeName;
        }

        static void MarkDirty(Component comp)
        {
            try
            {
                Call(comp, "MarkDirty");
            }
            catch
            {
                // Fallback: try via EditorUtility
                UnityEditor.EditorUtility.SetDirty(comp);
            }
        }
    }
}
#endif
