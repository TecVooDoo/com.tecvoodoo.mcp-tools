#if HAS_LUMEN
#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.Lumen.Editor
{
    [McpPluginToolType]
    public partial class Tool_Lumen
    {
        static readonly Type? LumenEffectPlayerType = FindType("DistantLands.Lumen.LumenEffectPlayer");

        static Type? FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        static UnityEngine.Component GetPlayer(string gameObjectName)
        {
            if (LumenEffectPlayerType == null)
                throw new Exception("DistantLands.Lumen.LumenEffectPlayer type not found in loaded assemblies.");
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new Exception($"GameObject '{gameObjectName}' not found.");
            var player = go.GetComponent(LumenEffectPlayerType);
            if (player == null)
                throw new Exception($"'{gameObjectName}' has no LumenEffectPlayer component.");
            return player;
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
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, Convert.ChangeType(value, field.FieldType)); return true; }
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, Convert.ChangeType(value, prop.PropertyType)); return true; }
            return false;
        }

        static bool SetEnum(object target, string name, string enumValue)
        {
            var type = target.GetType();
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, Enum.Parse(field.FieldType, enumValue, ignoreCase: true)); return true; }
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, Enum.Parse(prop.PropertyType, enumValue, ignoreCase: true)); return true; }
            return false;
        }

        static void SetColor(object target, string name, Color color)
        {
            var type = target.GetType();
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, color); return; }
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, color); return; }
            throw new Exception($"Color field/property '{name}' not found on {type.Name}.");
        }

        static object? Call(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null) throw new Exception($"Method '{methodName}' not found on {target.GetType().Name}.");
            return method.Invoke(target, args);
        }

        static Color ParseColor(string hex)
        {
            if (!hex.StartsWith("#")) hex = "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out var color))
                return color;
            throw new Exception($"Invalid hex color: '{hex}'. Use format '#RRGGBB' or '#RRGGBBAA'.");
        }

        static string FormatColor(object? c)
        {
            if (c is Color col) return $"#{ColorUtility.ToHtmlStringRGBA(col)}";
            return c?.ToString() ?? "null";
        }
    }
}
#endif
