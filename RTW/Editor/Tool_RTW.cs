#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.RTW.Editor
{
    [McpPluginToolType]
    public partial class Tool_RTW
    {
        const string MGR_TYPE = "RealTimeWeather.Managers.RealTimeWeatherManager";

        static Type? FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        static object GetManager()
        {
            var type = FindType(MGR_TYPE);
            if (type == null) throw new Exception("RealTimeWeatherManager type not found.");
            var prop = type.GetProperty("instance", BindingFlags.Public | BindingFlags.Static);
            if (prop == null) throw new Exception("RealTimeWeatherManager.instance property not found.");
            var inst = prop.GetValue(null);
            if (inst == null) throw new Exception("No RealTimeWeatherManager in scene. Add the component first.");
            return inst;
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
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, Convert.ChangeType(value, prop.PropertyType));
                return true;
            }
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, Convert.ChangeType(value, field.FieldType));
                return true;
            }
            return false;
        }

        static bool SetEnum(object target, string name, string enumValue)
        {
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            Type? enumType = prop?.PropertyType ?? type.GetField(name, BindingFlags.Public | BindingFlags.Instance)?.FieldType;
            if (enumType == null || !enumType.IsEnum) return false;
            var val = Enum.Parse(enumType, enumValue, true);
            if (prop != null) { prop.SetValue(target, val); return true; }
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, val); return true; }
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
