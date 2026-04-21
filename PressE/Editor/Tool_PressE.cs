#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.PressE.Editor
{
    [McpPluginToolType]
    public partial class Tool_PressE
    {
        const string INTERACTABLE_TYPE = "FastStudios.Interactable";
        const string KEY_TYPE = "FastStudios.Key";
        const string MANAGER_TYPE = "FastStudios.InteractionManager";

        static GameObject FindGO(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) throw new Exception($"GameObject '{name}' not found.");
            return go;
        }

        static Type? FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        static Component? GetComponentByTypeName(GameObject go, string fullTypeName)
        {
            var t = FindType(fullTypeName);
            if (t == null) return null;
            return go.GetComponent(t);
        }

        static Component[] GetComponentsByTypeName(GameObject go, string fullTypeName)
        {
            var t = FindType(fullTypeName);
            if (t == null) return new Component[0];
            return go.GetComponents(t);
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
            if (field != null)
            {
                if (field.FieldType.IsEnum && value is string s)
                    field.SetValue(target, Enum.Parse(field.FieldType, s, true));
                else
                    field.SetValue(target, Convert.ChangeType(value, field.FieldType));
                return true;
            }
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                if (prop.PropertyType.IsEnum && value is string sp)
                    prop.SetValue(target, Enum.Parse(prop.PropertyType, sp, true));
                else
                    prop.SetValue(target, Convert.ChangeType(value, prop.PropertyType));
                return true;
            }
            return false;
        }

        static object? Call(object target, string methodName, params object[] args)
        {
            var type = target.GetType();
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null) throw new Exception($"Method '{methodName}' not found on {type.Name}.");
            return method.Invoke(target, args);
        }
    }
}
