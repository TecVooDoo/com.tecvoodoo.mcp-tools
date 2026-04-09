#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.RopeToolkit.Editor
{
    [McpPluginToolType]
    public partial class Tool_RopeToolkit
    {
        static readonly Type? RopeType = FindType("RopeToolkit.Rope");
        static readonly Type? RopeConnectionType = FindType("RopeToolkit.RopeConnection");
        static readonly Type? RopeConnectionTypeEnum = FindType("RopeToolkit.RopeConnectionType");

        static string FormatVector3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        static Component GetRope(string gameObjectName)
        {
            if (RopeType == null)
                throw new Exception("RopeToolkit.Rope type not found in loaded assemblies.");
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new Exception($"GameObject '{gameObjectName}' not found.");
            var rope = go.GetComponent(RopeType);
            if (rope == null)
                throw new Exception($"'{gameObjectName}' has no Rope component.");
            return rope;
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

        static object? Call(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null) throw new Exception($"Method '{methodName}' not found.");
            return method.Invoke(target, args);
        }

        /// <summary>Gets a struct property from target, returns it as a boxed object for field mutation.</summary>
        static object? GetStruct(object target, string name)
        {
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(target);
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(target);
            return null;
        }

        /// <summary>Sets a struct field on a boxed struct value.</summary>
        static bool SetStructField(object boxedStruct, string name, object value)
        {
            var type = boxedStruct.GetType();
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(boxedStruct, Convert.ChangeType(value, field.FieldType)); return true; }
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(boxedStruct, Convert.ChangeType(value, prop.PropertyType)); return true; }
            return false;
        }

        /// <summary>Writes a (potentially modified) boxed struct back to the target property.</summary>
        static void SetStruct(object target, string name, object boxedStruct)
        {
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, boxedStruct); return; }
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, boxedStruct); return; }
            throw new Exception($"Cannot write struct back to '{name}'.");
        }

        /// <summary>Reads a field from a boxed struct.</summary>
        static object? GetStructField(object boxedStruct, string name)
        {
            var type = boxedStruct.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(boxedStruct);
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(boxedStruct);
            return null;
        }
    }
}
