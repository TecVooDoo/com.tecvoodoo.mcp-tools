#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.UCC.Editor
{
    [McpPluginToolType]
    public partial class Tool_UCC
    {
        static GameObject FindGO(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
                throw new Exception($"GameObject '{name}' not found.");
            return go;
        }

        /// <summary>
        /// Find a component on the GameObject by fully-qualified type name using reflection.
        /// Returns null if the type or component is not found.
        /// </summary>
        static Component? FindComponentByTypeName(GameObject go, string fullTypeName)
        {
            Type? type = FindTypeInAllAssemblies(fullTypeName);
            if (type == null) return null;
            return go.GetComponent(type);
        }

        /// <summary>
        /// Find a component on the GameObject or any child by fully-qualified type name.
        /// Returns null if the type or component is not found.
        /// </summary>
        static Component? FindComponentInChildrenByTypeName(GameObject go, string fullTypeName)
        {
            Type? type = FindTypeInAllAssemblies(fullTypeName);
            if (type == null) return null;
            return go.GetComponentInChildren(type, true);
        }

        /// <summary>
        /// Search all loaded assemblies for a type by full name.
        /// </summary>
        static Type? FindTypeInAllAssemblies(string fullTypeName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asm in assemblies)
            {
                Type? type = asm.GetType(fullTypeName);
                if (type != null) return type;
            }
            return null;
        }

        /// <summary>
        /// Get a property value via reflection. Returns null if not found.
        /// </summary>
        static object? GetPropValue(object target, string propertyName)
        {
            Type type = target.GetType();
            PropertyInfo? prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(target);

            FieldInfo? field = type.GetField(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(target);

            return null;
        }

        /// <summary>
        /// Set a property or field value via reflection. Returns true if successful.
        /// </summary>
        static bool SetPropValue(object target, string name, object value)
        {
            Type type = target.GetType();
            PropertyInfo? prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(target, Convert.ChangeType(value, prop.PropertyType));
                return true;
            }

            FieldInfo? field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(target, Convert.ChangeType(value, field.FieldType));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Call a method on a target object via reflection. Returns the result or null.
        /// </summary>
        static object? CallMethod(object target, string methodName, params object[] args)
        {
            Type type = target.GetType();
            Type[] argTypes = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                argTypes[i] = args[i].GetType();
            }

            MethodInfo? method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, argTypes, null);
            if (method == null)
            {
                // Try without type matching (simpler overload resolution)
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            }
            if (method == null)
                throw new Exception($"Method '{methodName}' not found on {type.Name}.");

            return method.Invoke(target, args);
        }

        static string FormatVector3(Vector3 v) => $"({v.x:F2}, {v.y:F2}, {v.z:F2})";

        // Common UCC type names
        const string UCL_TYPE = "Opsive.UltimateCharacterController.Character.UltimateCharacterLocomotion";
        const string ATTR_MANAGER_TYPE = "Opsive.UltimateCharacterController.Traits.AttributeManager";
        const string INVENTORY_TYPE = "Opsive.UltimateCharacterController.Inventory.InventoryBase";
    }
}
