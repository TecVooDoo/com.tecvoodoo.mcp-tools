#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.DecalCollider.Editor
{
    [McpPluginToolType]
    public partial class Tool_DecalCollider
    {
        const string DC_TYPE = "DecalCollider.Runtime.DecalCollider";

        static GameObject FindGO(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) throw new Exception($"GameObject '{name}' not found.");
            return go;
        }

        static Component GetDecal(string gameObjectName)
        {
            var go = FindGO(gameObjectName);
            var type = FindType(DC_TYPE);
            if (type == null) throw new Exception("DecalCollider type not found.");
            var dc = go.GetComponent(type);
            if (dc == null) throw new Exception($"'{gameObjectName}' has no DecalCollider component.");
            return dc;
        }

        static Type? FindType(string fullTypeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullTypeName);
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

        static void SetEnum(object target, string fieldName, string enumValue)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null) throw new Exception($"Field '{fieldName}' not found.");
            var val = Enum.Parse(field.FieldType, enumValue, true);
            field.SetValue(target, val);
        }

        static object? CallMethod(object target, string methodName, params object[] args)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null) throw new Exception($"Method '{methodName}' not found on {target.GetType().Name}.");
            return method.Invoke(target, args);
        }
    }
}
