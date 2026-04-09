#if HAS_JUICY_ACTIONS
#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.JuicyActions.Editor
{
    [McpPluginToolType]
    public partial class Tool_JuicyActions
    {
        static readonly Type? ActionOnEventType   = FindType("MagicPigGames.JuicyActions.ActionOnEvent");
        static readonly Type? ActionExecutorType   = FindType("MagicPigGames.JuicyActions.ActionExecutor");

        static Component[] GetTriggers(string gameObjectName)
        {
            if (ActionOnEventType == null)
                throw new Exception("MagicPigGames.JuicyActions.ActionOnEvent type not found in loaded assemblies.");
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new Exception($"GameObject '{gameObjectName}' not found.");
            var triggers = go.GetComponents(ActionOnEventType);
            if (triggers.Length == 0)
                throw new Exception($"'{gameObjectName}' has no ActionOnEvent-derived trigger components.");
            return triggers;
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
            if (method == null) throw new Exception($"Method '{methodName}' not found on {target.GetType().Name}.");
            return method.Invoke(target, args);
        }
    }
}
#endif
