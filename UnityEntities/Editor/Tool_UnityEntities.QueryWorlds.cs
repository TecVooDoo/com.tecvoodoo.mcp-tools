#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Unity.Entities;

namespace MCPTools.UnityEntities.Editor
{
    public partial class Tool_UnityEntities
    {
        [McpPluginTool("ecs-query-worlds", Title = "ECS / Query Worlds")]
        [Description(@"Lists all active Unity ECS Worlds.
Shows each world's name, entity count, system count, IsCreated status,
and whether it is the DefaultGameObjectInjectionWorld.
Requires Play mode to be active.")]
        public string QueryWorlds()
        {
            return MainThread.Instance.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== ECS Worlds ===");

                World? defaultWorld = World.DefaultGameObjectInjectionWorld;
                int worldIndex = 0;

                foreach (World world in World.All)
                {
                    bool isDefault = defaultWorld != null && world == defaultWorld;
                    string defaultTag = isDefault ? " [DEFAULT]" : "";

                    sb.AppendLine($"\n--- World {worldIndex}: {world.Name}{defaultTag} ---");
                    sb.AppendLine($"  IsCreated:    {world.IsCreated}");
                    sb.AppendLine($"  Flags:        {world.Flags}");

                    if (world.IsCreated)
                    {
                        EntityManager em = world.EntityManager;
                        int entityCount = em.Debug.EntityCount;
                        int systemCount = world.Systems.Count;

                        sb.AppendLine($"  EntityCount:  {entityCount}");
                        sb.AppendLine($"  SystemCount:  {systemCount}");
                    }

                    worldIndex++;
                }

                if (worldIndex == 0)
                    sb.AppendLine("  No worlds found. Is Play mode active?");

                return sb.ToString();
            });
        }
    }
}
