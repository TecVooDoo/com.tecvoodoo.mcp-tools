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
        [AiTool("ecs-set-component-enabled", Title = "ECS / Set Component Enabled")]
        [Description(@"Enables or disables an IEnableableComponent on an ECS entity (EntityManager.SetComponentEnabled).
Not a structural change -- the component stays on the entity; only its enabled bit flips.
Use this instead of ecs-modify-entity, which writes field values only.
QUERY TRAP: an entity whose component is DISABLED does not match a query that requires that component,
so ecs-query-entities filtered by that type will not list it. Pass ignoreComponentEnabledState=true there
(EntityQueryOptions.IgnoreComponentEnabledState). Do NOT use EntityQueryOptions.IncludeDisabledEntities --
that governs the whole-entity 'Disabled' tag, not per-component enabled state.
Requires Play mode.")]
        public string SetComponentEnabled(
            [Description("Entity index (from ecs-query-entities output).")]
            int entityIndex,

            [Description("Entity version (from ecs-query-entities output).")]
            int entityVersion,

            [Description("Fully qualified component type name. Must implement IEnableableComponent.")]
            string componentTypeName,

            [Description("True to enable the component, false to disable it.")]
            bool enabled,

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

                Type managedType = ResolveComponentType(componentTypeName);
                ComponentType componentType = ComponentType.ReadWrite(managedType);

                if (!componentType.IsEnableable)
                    throw new Exception($"Type '{componentTypeName}' does not implement IEnableableComponent. Use ecs-modify-entity for field values, or ecs-create-destroy for structural changes.");

                if (!em.HasComponent(entity, componentType))
                    throw new Exception($"Entity [{entityIndex}:{entityVersion}] has no '{componentTypeName}' component. Use ecs-inspect-entity to list its components.");

                bool before = em.IsComponentEnabled(entity, componentType);
                em.SetComponentEnabled(entity, componentType, enabled);
                bool after = em.IsComponentEnabled(entity, componentType);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Entity [{entityIndex}:{entityVersion}] in World: {world.Name}");
                sb.AppendLine($"  Component: {componentTypeName}");
                sb.AppendLine($"  Enabled:   {before} -> {after}");
                if (before == after)
                    sb.AppendLine("  (no change -- component was already in the requested state)");
                return sb.ToString();
            });
        }
    }
}
