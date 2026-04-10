#if HAS_TIMEFLOW
#nullable enable
using System;
using System.Collections;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.Timeflow.Editor
{
    [McpPluginToolType]
    public partial class Tool_Timeflow
    {
        static readonly Type? TimeflowType = FindType("AxonGenesis.Timeflow");
        static readonly Type? TimeflowObjectType = FindType("AxonGenesis.TimeflowObject");
        static readonly Type? TimeflowBehaviorType = FindType("AxonGenesis.TimeflowBehavior");
        static readonly Type? TweenType = FindType("AxonGenesis.Tween");
        static readonly Type? TimeflowEventType = FindType("AxonGenesis.TimeflowEvent");

        static Type? FindType(string name)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        static UnityEngine.Component GetTimeflow(string gameObjectName)
        {
            if (TimeflowType == null)
                throw new Exception("AxonGenesis.Timeflow type not found in loaded assemblies.");
            GameObject? go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new Exception($"GameObject '{gameObjectName}' not found.");
            UnityEngine.Component? tf = go.GetComponent(TimeflowType);
            if (tf == null)
                throw new Exception($"'{gameObjectName}' has no Timeflow component.");
            return tf;
        }

        static UnityEngine.Component GetComponentOfType(string gameObjectName, Type? type, string typeName)
        {
            if (type == null)
                throw new Exception($"{typeName} type not found in loaded assemblies.");
            GameObject? go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new Exception($"GameObject '{gameObjectName}' not found.");
            UnityEngine.Component? comp = go.GetComponent(type);
            if (comp == null)
                throw new Exception($"'{gameObjectName}' has no {typeName} component.");
            return comp;
        }

        static object? Get(object target, string name)
        {
            Type type = target.GetType();
            PropertyInfo? prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(target);
            FieldInfo? field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(target);
            return null;
        }

        static bool Set(object target, string name, object value)
        {
            Type type = target.GetType();
            FieldInfo? field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, Convert.ChangeType(value, field.FieldType)); return true; }
            PropertyInfo? prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, Convert.ChangeType(value, prop.PropertyType)); return true; }
            return false;
        }

        static bool SetEnum(object target, string name, string enumValue)
        {
            Type type = target.GetType();
            FieldInfo? field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, Enum.Parse(field.FieldType, enumValue, ignoreCase: true)); return true; }
            PropertyInfo? prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, Enum.Parse(prop.PropertyType, enumValue, ignoreCase: true)); return true; }
            return false;
        }

        static object? Call(object target, string methodName, params object[] args)
        {
            MethodInfo? method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null) throw new Exception($"Method '{methodName}' not found on {target.GetType().Name}.");
            return method.Invoke(target, args);
        }

        static int GetCollectionCount(object? collection)
        {
            if (collection is ICollection col) return col.Count;
            if (collection is Array arr) return arr.Length;
            return 0;
        }
    }
}
#endif
