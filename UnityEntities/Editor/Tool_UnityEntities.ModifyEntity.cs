#nullable enable
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Unity.Entities;

namespace MCPTools.UnityEntities.Editor
{
    public partial class Tool_UnityEntities
    {
        [McpPluginTool("ecs-modify-entity", Title = "ECS / Modify Entity")]
        [Description(@"Modifies a field on an unmanaged IComponentData component of an ECS entity.
Uses reflection to get the component, modify the field, and set it back.
Only works for unmanaged IComponentData with public fields.
Requires Play mode.")]
        public string ModifyEntity(
            [Description("Entity index.")]
            int entityIndex,

            [Description("Entity version.")]
            int entityVersion,

            [Description("Fully qualified component type name (e.g. 'Unity.Transforms.LocalTransform').")]
            string componentTypeName,

            [Description("Name of the public field to modify.")]
            string fieldName,

            [Description("New value as a string. Supports int, float, bool, string. For Vector3/float3 use 'x,y,z' format.")]
            string value,

            [Description("Name of the World. Defaults to DefaultGameObjectInjectionWorld.")]
            string? worldName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                World world = ResolveWorld(worldName);
                EntityManager em = world.EntityManager;
                Entity entity = ReconstructEntity(entityIndex, entityVersion);

                if (!em.Exists(entity))
                    throw new Exception($"Entity [{entityIndex}:{entityVersion}] does not exist.");

                Type componentType = ResolveComponentType(componentTypeName);

                // Get GetComponentData<T> and SetComponentData<T>
                MethodInfo? getMethod = typeof(EntityManager).GetMethod(
                    "GetComponentData",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(Entity) },
                    null
                );

                MethodInfo? setMethod = typeof(EntityManager).GetMethod(
                    "SetComponentData",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(Entity), componentType },
                    null
                );

                if (getMethod == null)
                    throw new Exception("Could not find EntityManager.GetComponentData<T>(Entity) method.");

                // For SetComponentData, the non-generic lookup won't work because the second param
                // is generic T. We need to find it differently.
                MethodInfo[] allMethods = typeof(EntityManager).GetMethods(BindingFlags.Public | BindingFlags.Instance);
                MethodInfo? setGenericMethod = null;
                foreach (MethodInfo m in allMethods)
                {
                    if (m.Name == "SetComponentData" && m.IsGenericMethodDefinition)
                    {
                        ParameterInfo[] parameters = m.GetParameters();
                        if (parameters.Length == 2
                            && parameters[0].ParameterType == typeof(Entity))
                        {
                            setGenericMethod = m;
                            break;
                        }
                    }
                }

                if (setGenericMethod == null)
                    throw new Exception("Could not find EntityManager.SetComponentData<T>(Entity, T) method.");

                MethodInfo getGeneric = getMethod.MakeGenericMethod(componentType);
                MethodInfo setGenericBound = setGenericMethod.MakeGenericMethod(componentType);

                // Get current component value
                object? componentValue = getGeneric.Invoke(em, new object[] { entity });
                if (componentValue == null)
                    throw new Exception($"Component '{componentTypeName}' returned null on entity [{entityIndex}:{entityVersion}].");

                // Find the field
                FieldInfo? field = componentType.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                if (field == null)
                    throw new Exception($"Public field '{fieldName}' not found on type '{componentTypeName}'. Use ecs-inspect-entity to see available fields.");

                // Parse the value
                object parsedValue = ParseFieldValue(field.FieldType, value);

                // Set the field on the boxed struct
                field.SetValue(componentValue, parsedValue);

                // Write back to the entity
                setGenericBound.Invoke(em, new object[] { entity, componentValue });

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Modified Entity [{entityIndex}:{entityVersion}]");
                sb.AppendLine($"  Component: {componentTypeName}");
                sb.AppendLine($"  Field:     {fieldName}");
                sb.AppendLine($"  NewValue:  {parsedValue}");
                return sb.ToString();
            });
        }

        static object ParseFieldValue(Type fieldType, string value)
        {
            if (fieldType == typeof(int))
                return int.Parse(value, CultureInfo.InvariantCulture);
            if (fieldType == typeof(float))
                return float.Parse(value, CultureInfo.InvariantCulture);
            if (fieldType == typeof(double))
                return double.Parse(value, CultureInfo.InvariantCulture);
            if (fieldType == typeof(bool))
                return bool.Parse(value);
            if (fieldType == typeof(string))
                return value;
            if (fieldType == typeof(long))
                return long.Parse(value, CultureInfo.InvariantCulture);
            if (fieldType == typeof(byte))
                return byte.Parse(value, CultureInfo.InvariantCulture);
            if (fieldType == typeof(short))
                return short.Parse(value, CultureInfo.InvariantCulture);
            if (fieldType == typeof(uint))
                return uint.Parse(value, CultureInfo.InvariantCulture);

            // Handle Unity.Mathematics float3 (x,y,z format)
            if (fieldType.FullName == "Unity.Mathematics.float3")
            {
                string[] parts = value.Split(',');
                if (parts.Length != 3)
                    throw new Exception($"float3 requires 'x,y,z' format, got '{value}'.");
                float x = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
                float y = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                float z = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
                return Activator.CreateInstance(fieldType, x, y, z)
                    ?? throw new Exception("Failed to create float3 instance.");
            }

            // Handle Unity.Mathematics quaternion (x,y,z,w format)
            if (fieldType.FullName == "Unity.Mathematics.quaternion")
            {
                string[] parts = value.Split(',');
                if (parts.Length != 4)
                    throw new Exception($"quaternion requires 'x,y,z,w' format, got '{value}'.");
                float x = float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture);
                float y = float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture);
                float z = float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture);
                float w = float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture);
                return Activator.CreateInstance(fieldType, x, y, z, w)
                    ?? throw new Exception("Failed to create quaternion instance.");
            }

            // Enum support
            if (fieldType.IsEnum)
                return Enum.Parse(fieldType, value, ignoreCase: true);

            throw new Exception($"Unsupported field type '{fieldType.FullName}'. Supported: int, float, double, bool, string, long, byte, short, uint, float3, quaternion, enums.");
        }
    }
}
