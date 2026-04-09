#if HAS_BOINGKIT
#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.BoingKit.Editor
{
    [McpPluginToolType]
    public partial class Tool_BoingKit
    {
        static readonly Type? BoingBehaviorType     = FindType("BoingKit.BoingBehavior");
        static readonly Type? BoingBonesType         = FindType("BoingKit.BoingBones");
        static readonly Type? BoingEffectorType      = FindType("BoingKit.BoingEffector");
        static readonly Type? BoingReactorFieldType  = FindType("BoingKit.BoingReactorField");

        static Type? FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        static Component? GetBoingComponent(GameObject go, Type? type, string typeName)
        {
            if (type == null)
                throw new Exception($"{typeName} type not found in loaded assemblies.");
            return go.GetComponent(type);
        }

        static Component GetRequiredBoingComponent(GameObject go, Type? type, string typeName)
        {
            var comp = GetBoingComponent(go, type, typeName);
            if (comp == null)
                throw new Exception($"'{go.name}' has no {typeName} component.");
            return comp;
        }

        static Type? ResolveComponentType(string componentType)
        {
            return componentType switch
            {
                "Effector"     => BoingEffectorType,
                "Behavior"     => BoingBehaviorType,
                "Bones"        => BoingBonesType,
                "ReactorField" => BoingReactorFieldType,
                _ => throw new Exception($"Unknown componentType '{componentType}'. Use: Effector, Behavior, Bones, or ReactorField.")
            };
        }

        static string ResolveFullTypeName(string componentType)
        {
            return componentType switch
            {
                "Effector"     => "BoingKit.BoingEffector",
                "Behavior"     => "BoingKit.BoingBehavior",
                "Bones"        => "BoingKit.BoingBones",
                "ReactorField" => "BoingKit.BoingReactorField",
                _ => componentType
            };
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
    }
}
#endif
