#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.FinalIK.Editor
{
    [McpPluginToolType]
    public partial class Tool_FinalIK
    {
        static readonly Type? IKType              = FindType("RootMotion.FinalIK.IK");
        static readonly Type? LookAtIKType        = FindType("RootMotion.FinalIK.LookAtIK");
        static readonly Type? AimIKType           = FindType("RootMotion.FinalIK.AimIK");
        static readonly Type? FullBodyBipedIKType = FindType("RootMotion.FinalIK.FullBodyBipedIK");
        static readonly Type? BipedReferencesType = FindType("RootMotion.BipedReferences");
        static readonly Type? IKSolverFBBType     = FindType("RootMotion.FinalIK.IKSolverFullBodyBiped");

        static string FormatVector3(Vector3 v)
            => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

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

        /// <summary>Calls a static method on a type.</summary>
        static object? CallStatic(Type type, string methodName, params object?[] args)
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static);
            foreach (var m in methods)
            {
                if (m.Name != methodName) continue;
                var parms = m.GetParameters();
                if (parms.Length == args.Length)
                    return m.Invoke(null, args);
            }
            // Try with matching by ref params
            foreach (var m in methods)
            {
                if (m.Name != methodName) continue;
                return m.Invoke(null, args);
            }
            throw new Exception($"Static method '{methodName}' not found on {type.Name}.");
        }
    }
}
