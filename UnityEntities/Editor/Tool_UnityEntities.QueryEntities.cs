#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Unity.Collections;
using Unity.Entities;

namespace MCPTools.UnityEntities.Editor
{
    public partial class Tool_UnityEntities
    {
        [McpPluginTool("ecs-query-entities", Title = "ECS / Query Entities")]
        [Description(@"Queries entities in an ECS World, optionally filtered by component types.
Returns entity index+version and component type names for each match.
Output is capped at 50 entities. Requires Play mode.")]
        public string QueryEntities(
            [Description("Comma-separated fully qualified component type names to filter by (e.g. 'Unity.Transforms.LocalTransform,Unity.Rendering.RenderMesh'). Leave empty to list all entities.")]
            string? componentTypeNames = null,

            [Description("Name of the World to query. Defaults to DefaultGameObjectInjectionWorld.")]
            string? worldName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                World world = ResolveWorld(worldName);
                EntityManager em = world.EntityManager;
                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"=== ECS Entities in World: {world.Name} ===");

                EntityQuery query;

                if (!string.IsNullOrEmpty(componentTypeNames))
                {
                    string[] typeNames = componentTypeNames!.Split(',');
                    ComponentType[] componentTypes = new ComponentType[typeNames.Length];

                    for (int i = 0; i < typeNames.Length; i++)
                    {
                        string trimmed = typeNames[i].Trim();
                        Type managedType = ResolveComponentType(trimmed);
                        componentTypes[i] = ComponentType.ReadOnly(managedType);
                    }

                    query = em.CreateEntityQuery(componentTypes);
                }
                else
                {
                    query = em.CreateEntityQuery(new EntityQueryDesc());
                }

                int totalCount = query.CalculateEntityCount();
                sb.AppendLine($"Total matching entities: {totalCount}");

                if (totalCount == 0)
                {
                    query.Dispose();
                    return sb.ToString();
                }

                NativeArray<Entity> entities = query.ToEntityArray(Allocator.Temp);
                int displayCount = Math.Min(entities.Length, 50);

                for (int i = 0; i < displayCount; i++)
                {
                    Entity entity = entities[i];
                    sb.AppendLine($"\n  Entity [{entity.Index}:{entity.Version}]");

                    NativeArray<ComponentType> types = em.GetComponentTypes(entity, Allocator.Temp);
                    for (int t = 0; t < types.Length; t++)
                    {
                        ComponentType ct = types[t];
                        Type managedType = ct.GetManagedType();
                        sb.AppendLine($"    - {managedType.FullName}");
                    }
                    types.Dispose();
                }

                if (totalCount > 50)
                    sb.AppendLine($"\n  ... and {totalCount - 50} more entities (capped at 50).");

                entities.Dispose();
                query.Dispose();

                return sb.ToString();
            });
        }
    }
}
