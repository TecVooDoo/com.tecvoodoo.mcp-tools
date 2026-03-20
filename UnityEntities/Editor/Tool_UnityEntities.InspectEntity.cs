#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Unity.Collections;
using Unity.Entities;

namespace MCPTools.UnityEntities.Editor
{
    public partial class Tool_UnityEntities
    {
        [McpPluginTool("ecs-inspect-entity", Title = "ECS / Inspect Entity")]
        [Description(@"Inspects a specific ECS entity by index and version.
Lists all components and attempts to read field values from unmanaged IComponentData.
Requires Play mode.")]
        public string InspectEntity(
            [Description("Entity index (from ecs-query-entities output).")]
            int entityIndex,

            [Description("Entity version (from ecs-query-entities output).")]
            int entityVersion,

            [Description("Name of the World. Defaults to DefaultGameObjectInjectionWorld.")]
            string? worldName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                World world = ResolveWorld(worldName);
                EntityManager em = world.EntityManager;
                Entity entity = ReconstructEntity(entityIndex, entityVersion);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"=== ECS Entity [{entity.Index}:{entity.Version}] in World: {world.Name} ===");

                if (!em.Exists(entity))
                {
                    sb.AppendLine("  Entity does NOT exist (may have been destroyed or version mismatch).");
                    return sb.ToString();
                }

                sb.AppendLine("  Entity EXISTS.");

                NativeArray<ComponentType> componentTypes = em.GetComponentTypes(entity, Allocator.Temp);
                sb.AppendLine($"  Component count: {componentTypes.Length}");

                // Cache the generic GetComponentData method for reflection calls
                MethodInfo? getComponentDataMethod = typeof(EntityManager).GetMethod(
                    "GetComponentData",
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] { typeof(Entity) },
                    null
                );

                for (int i = 0; i < componentTypes.Length; i++)
                {
                    ComponentType ct = componentTypes[i];
                    Type managedType = ct.GetManagedType();
                    sb.AppendLine($"\n  [{i}] {managedType.FullName}");

                    // Only try to read values for unmanaged IComponentData (not buffers, shared, managed, tag)
                    bool isUnmanagedComponent = ct.IsComponent
                        && !ct.IsBuffer
                        && !ct.IsSharedComponent
                        && !ct.IsManagedComponent
                        && !ct.IsZeroSized;

                    if (!isUnmanagedComponent)
                    {
                        if (ct.IsZeroSized)
                            sb.AppendLine("      (tag component -- no data)");
                        else if (ct.IsBuffer)
                            sb.AppendLine("      (buffer component -- use typed access)");
                        else if (ct.IsSharedComponent)
                            sb.AppendLine("      (shared component)");
                        else if (ct.IsManagedComponent)
                            sb.AppendLine("      (managed component)");
                        continue;
                    }

                    if (getComponentDataMethod == null)
                    {
                        sb.AppendLine("      (could not find GetComponentData<T> method)");
                        continue;
                    }

                    try
                    {
                        MethodInfo genericMethod = getComponentDataMethod.MakeGenericMethod(managedType);
                        object? componentValue = genericMethod.Invoke(em, new object[] { entity });

                        if (componentValue == null)
                        {
                            sb.AppendLine("      (null value)");
                            continue;
                        }

                        FieldInfo[] fields = managedType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                        if (fields.Length == 0)
                        {
                            sb.AppendLine("      (no public fields)");
                            continue;
                        }

                        foreach (FieldInfo field in fields)
                        {
                            try
                            {
                                object? fieldValue = field.GetValue(componentValue);
                                sb.AppendLine($"      {field.Name}: {fieldValue}");
                            }
                            catch (Exception ex)
                            {
                                sb.AppendLine($"      {field.Name}: (error: {ex.Message})");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"      (read error: {ex.InnerException?.Message ?? ex.Message})");
                    }
                }

                componentTypes.Dispose();
                return sb.ToString();
            });
        }
    }
}
