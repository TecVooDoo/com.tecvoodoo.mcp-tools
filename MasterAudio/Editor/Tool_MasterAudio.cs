#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;

namespace TecVooDoo.MCPTools.Editor
{
    [McpPluginToolType]
    public partial class Tool_MasterAudio
    {
        const string MA_TYPE_NAME = "DarkTonic.MasterAudio.MasterAudio";

        static Type? _maTypeCached;

        /// <summary>
        /// Get the MasterAudio type, cached after first lookup.
        /// </summary>
        static Type MasterAudioType
        {
            get
            {
                if (_maTypeCached == null)
                    _maTypeCached = FindType(MA_TYPE_NAME);
                if (_maTypeCached == null)
                    throw new InvalidOperationException($"Type '{MA_TYPE_NAME}' not found. Is MasterAudio installed?");
                return _maTypeCached;
            }
        }

        /// <summary>
        /// Search all loaded assemblies for a type by full name.
        /// </summary>
        static Type? FindType(string fullTypeName)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? type = asm.GetType(fullTypeName);
                if (type != null) return type;
            }
            return null;
        }

        /// <summary>
        /// Get a property or field value via reflection. If target is null, reads static members.
        /// </summary>
        static object? Get(Type type, object? target, string name)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            PropertyInfo? prop = type.GetProperty(name, flags);
            if (prop != null) return prop.GetValue(target);

            FieldInfo? field = type.GetField(name, flags);
            if (field != null) return field.GetValue(target);

            return null;
        }

        /// <summary>
        /// Get a static property or field from MasterAudio type.
        /// </summary>
        static object? GetStatic(string name)
        {
            return Get(MasterAudioType, null, name);
        }

        /// <summary>
        /// Set a property or field value via reflection. Returns true if successful.
        /// </summary>
        static bool Set(Type type, object? target, string name, object value)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
            PropertyInfo? prop = type.GetProperty(name, flags);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, Convert.ChangeType(value, prop.PropertyType));
                return true;
            }

            FieldInfo? field = type.GetField(name, flags);
            if (field != null)
            {
                field.SetValue(target, Convert.ChangeType(value, field.FieldType));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Call an instance method on a target object via reflection.
        /// </summary>
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

        /// <summary>
        /// Call a static method on a type via reflection.
        /// Attempts exact type match first, then falls back to compatible overload search.
        /// </summary>
        static object? CallStatic(Type type, string methodName, params object?[] args)
        {
            // Build arg types for exact match (skip if any arg is null)
            bool hasNulls = false;
            Type[] argTypes = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == null) { hasNulls = true; break; }
                argTypes[i] = args[i]!.GetType();
            }

            MethodInfo? method = hasNulls
                ? null
                : type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, argTypes, null);
            if (method == null)
            {
                // Fall back: find all static methods with the name and matching parameter count
                MethodInfo[] candidates = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (MethodInfo m in candidates)
                {
                    if (m.Name != methodName) continue;
                    ParameterInfo[] parms = m.GetParameters();
                    if (parms.Length < args.Length) continue;

                    // Check required params match count (optional params fill the rest)
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

            // Build the full argument array, filling in defaults for optional params
            ParameterInfo[] parameters = method.GetParameters();
            object?[] fullArgs = new object?[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i < args.Length)
                {
                    if (args[i] == null)
                        fullArgs[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue : null;
                    else if (args[i] is IConvertible && parameters[i].ParameterType != args[i]!.GetType()
                             && !parameters[i].ParameterType.IsAssignableFrom(args[i]!.GetType()))
                        fullArgs[i] = Convert.ChangeType(args[i], parameters[i].ParameterType);
                    else
                        fullArgs[i] = args[i];
                }
                else if (parameters[i].HasDefaultValue)
                    fullArgs[i] = parameters[i].DefaultValue;
                else
                    fullArgs[i] = parameters[i].ParameterType.IsValueType
                        ? Activator.CreateInstance(parameters[i].ParameterType)
                        : null;
            }

            return method.Invoke(null, fullArgs);
        }

        /// <summary>
        /// Call a static MasterAudio method by name.
        /// </summary>
        static object? CallMA(string methodName, params object?[] args)
        {
            return CallStatic(MasterAudioType, methodName, args);
        }

        /// <summary>
        /// Verify MasterAudio singleton exists in scene.
        /// </summary>
        static void EnsureInstance()
        {
            object? instance = GetStatic("SafeInstance");
            if (instance == null)
                throw new InvalidOperationException("MasterAudio instance not found in scene.");
        }
    }
}
