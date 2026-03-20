#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Unity.Entities;

namespace MCPTools.UnityEntities.Editor
{
    public partial class Tool_UnityEntities
    {
        [McpPluginTool("ecs-create-destroy", Title = "ECS / Create or Destroy Entity")]
        [Description(@"Creates or destroys an ECS entity.
For 'create': provide componentTypeNames (comma-separated fully qualified type names).
  Creates an entity with those unmanaged IComponentData components attached.
For 'destroy': provide entityIndex and entityVersion to destroy a specific entity.
Requires Play mode.")]
        public string CreateOrDestroyEntity(
            [Description("Action to perform: 'create' or 'destroy'.")]
            string action,

            [Description("For 'create': comma-separated fully qualified component type names (e.g. 'Unity.Transforms.LocalTransform'). Ignored for 'destroy'.")]
            string? componentTypeNames = null,

            [Description("For 'destroy': entity index. Ignored for 'create'.")]
            int entityIndex = -1,

            [Description("For 'destroy': entity version. Ignored for 'create'.")]
            int entityVersion = -1,

            [Description("Name of the World. Defaults to DefaultGameObjectInjectionWorld.")]
            string? worldName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                World world = ResolveWorld(worldName);
                EntityManager em = world.EntityManager;
                StringBuilder sb = new StringBuilder();

                string normalizedAction = action.Trim().ToLowerInvariant();

                if (normalizedAction == "create")
                {
                    if (string.IsNullOrEmpty(componentTypeNames))
                    {
                        // Create empty entity
                        Entity newEntity = em.CreateEntity();
                        sb.AppendLine($"Created empty Entity [{newEntity.Index}:{newEntity.Version}] in World: {world.Name}");
                        return sb.ToString();
                    }

                    string[] typeNames = componentTypeNames!.Split(',');
                    ComponentType[] types = new ComponentType[typeNames.Length];

                    for (int i = 0; i < typeNames.Length; i++)
                    {
                        string trimmed = typeNames[i].Trim();
                        Type managedType = ResolveComponentType(trimmed);
                        types[i] = new ComponentType(managedType);
                    }

                    Entity entity = em.CreateEntity(types);
                    sb.AppendLine($"Created Entity [{entity.Index}:{entity.Version}] in World: {world.Name}");
                    sb.AppendLine("  Components:");
                    for (int i = 0; i < types.Length; i++)
                    {
                        sb.AppendLine($"    - {typeNames[i].Trim()}");
                    }

                    return sb.ToString();
                }

                if (normalizedAction == "destroy")
                {
                    if (entityIndex < 0)
                        throw new Exception("entityIndex is required for 'destroy' action.");

                    Entity entity = ReconstructEntity(entityIndex, entityVersion);

                    if (!em.Exists(entity))
                        throw new Exception($"Entity [{entityIndex}:{entityVersion}] does not exist.");

                    em.DestroyEntity(entity);
                    sb.AppendLine($"Destroyed Entity [{entityIndex}:{entityVersion}] in World: {world.Name}");
                    return sb.ToString();
                }

                throw new Exception($"Unknown action '{action}'. Use 'create' or 'destroy'.");
            });
        }
    }
}
