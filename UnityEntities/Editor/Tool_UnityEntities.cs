#nullable enable
using System;
using com.IvanMurzak.McpPlugin;
using Unity.Entities;

namespace MCPTools.UnityEntities.Editor
{
    [McpPluginToolType]
    public partial class Tool_UnityEntities
    {
        static World ResolveWorld(string? worldName)
        {
            if (string.IsNullOrEmpty(worldName))
            {
                World defaultWorld = World.DefaultGameObjectInjectionWorld;
                if (defaultWorld == null || !defaultWorld.IsCreated)
                    throw new Exception("DefaultGameObjectInjectionWorld is null or not created. Is Play mode active?");
                return defaultWorld;
            }

            foreach (World w in World.All)
            {
                if (w.IsCreated && w.Name == worldName)
                    return w;
            }

            throw new Exception($"World '{worldName}' not found. Use ecs-query-worlds to list available worlds.");
        }

        static Entity ReconstructEntity(int entityIndex, int entityVersion)
        {
            Entity entity = new Entity
            {
                Index = entityIndex,
                Version = entityVersion
            };
            return entity;
        }

        static Type ResolveComponentType(string fullyQualifiedTypeName)
        {
            Type? type = Type.GetType(fullyQualifiedTypeName);
            if (type == null)
            {
                // Try searching all loaded assemblies
                foreach (System.Reflection.Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = asm.GetType(fullyQualifiedTypeName);
                    if (type != null) break;
                }
            }
            if (type == null)
                throw new Exception($"Type '{fullyQualifiedTypeName}' not found in any loaded assembly.");
            return type;
        }
    }
}
