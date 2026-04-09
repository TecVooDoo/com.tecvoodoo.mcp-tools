#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;

namespace MCPTools.PWB.Editor
{
    [McpPluginToolType]
    public partial class Tool_PWB
    {
        const string PALETTE_MANAGER_TYPE = "PluginMaster.PaletteManager, Assembly-CSharp-Editor";
        const string MULTIBRUSH_TYPE      = "PluginMaster.MultibrushSettings, Assembly-CSharp-Editor";
        const string PALETTE_DATA_TYPE    = "PluginMaster.PaletteData, Assembly-CSharp-Editor";

        static Type? _paletteManagerType;
        static Type PaletteManagerType
        {
            get
            {
                if (_paletteManagerType == null)
                    _paletteManagerType = FindType(PALETTE_MANAGER_TYPE);
                if (_paletteManagerType == null)
                    throw new InvalidOperationException($"Type '{PALETTE_MANAGER_TYPE}' not found. Is Prefab World Builder installed?");
                return _paletteManagerType;
            }
        }

        static string FormatVector3(UnityEngine.Vector3 v)
            => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        static Type? FindType(string fullTypeName)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? type = asm.GetType(fullTypeName);
                if (type != null) return type;
            }
            // Try without assembly qualifier
            string shortName = fullTypeName.Contains(",") ? fullTypeName.Substring(0, fullTypeName.IndexOf(',')).Trim() : fullTypeName;
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? type = asm.GetType(shortName);
                if (type != null) return type;
            }
            return null;
        }

        static object? Get(object target, string name)
        {
            Type type = target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            PropertyInfo? prop = type.GetProperty(name, flags);
            if (prop != null) return prop.GetValue(target);
            FieldInfo? field = type.GetField(name, flags);
            if (field != null) return field.GetValue(target);
            return null;
        }

        static object? GetStatic(Type type, string name)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            PropertyInfo? prop = type.GetProperty(name, flags);
            if (prop != null) return prop.GetValue(null);
            FieldInfo? field = type.GetField(name, flags);
            if (field != null) return field.GetValue(null);
            return null;
        }

        static bool Set(object target, string name, object value)
        {
            Type type = target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            PropertyInfo? prop = type.GetProperty(name, flags);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, value); return true; }
            FieldInfo? field = type.GetField(name, flags);
            if (field != null) { field.SetValue(target, value); return true; }
            return false;
        }

        static object? Call(object target, string methodName, params object[] args)
        {
            Type type = target.GetType();
            Type[] argTypes = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
                argTypes[i] = args[i].GetType();
            MethodInfo? method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, argTypes, null);
            if (method == null)
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
                throw new Exception($"Method '{methodName}' not found on {type.Name}.");
            return method.Invoke(target, args);
        }

        static object? CallStatic(Type type, string methodName, params object[] args)
        {
            Type[] argTypes = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
                argTypes[i] = args[i].GetType();

            MethodInfo? method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, argTypes, null);
            if (method == null)
            {
                MethodInfo[] candidates = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (MethodInfo m in candidates)
                {
                    if (m.Name != methodName) continue;
                    ParameterInfo[] parms = m.GetParameters();
                    if (parms.Length < args.Length) continue;
                    int requiredCount = 0;
                    foreach (ParameterInfo p in parms)
                    {
                        if (!p.IsOptional) requiredCount++;
                    }
                    if (args.Length >= requiredCount && args.Length <= parms.Length)
                    {
                        method = m;
                        break;
                    }
                }
            }
            if (method == null)
                throw new Exception($"Static method '{methodName}' with {args.Length} arg(s) not found on {type.Name}.");

            ParameterInfo[] parameters = method.GetParameters();
            object[] fullArgs = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i < args.Length)
                    fullArgs[i] = args[i];
                else if (parameters[i].HasDefaultValue)
                    fullArgs[i] = parameters[i].DefaultValue!;
                else
                    fullArgs[i] = parameters[i].ParameterType.IsValueType
                        ? Activator.CreateInstance(parameters[i].ParameterType)!
                        : null!;
            }
            return method.Invoke(null, fullArgs);
        }
    }
}
