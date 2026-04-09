#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace MCPTools.AINavigation.Editor
{
    public partial class Tool_AINavigation
    {
        [McpPluginTool("nav-query", Title = "AI Navigation / Query")]
        [Description(@"Lists all NavMeshSurfaces, NavMeshLinks, NavMeshModifiers, and NavMeshModifierVolumes in the scene.
For each surface: agent type, collect mode, geometry source, layer mask, tile/voxel settings, bake status.
For each link: start/end points, width, cost, bidirectional, area.
Also reports NavMeshAgent count if any exist.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();

                // Surfaces
                var surfaces = NavMeshSurface.activeSurfaces;
                sb.AppendLine($"NavMeshSurfaces ({surfaces.Count}):");
                foreach (var s in surfaces)
                {
                    sb.AppendLine($"  [{s.gameObject.name}] agentTypeID={s.agentTypeID} collect={s.collectObjects} geometry={s.useGeometry}");
                    sb.AppendLine($"    size={FormatVector3(s.size)} center={FormatVector3(s.center)} layerMask={s.layerMask.value}");
                    sb.AppendLine($"    defaultArea={s.defaultArea} minRegionArea={s.minRegionArea:F2} buildHeightMesh={s.buildHeightMesh}");
                    if (s.overrideTileSize) sb.AppendLine($"    tileSize={s.tileSize} (override)");
                    if (s.overrideVoxelSize) sb.AppendLine($"    voxelSize={s.voxelSize:F4} (override)");
                    sb.AppendLine($"    hasBakedData={s.navMeshData != null}");
                }

                // Links
#if UNITY_2023_1_OR_NEWER
                var links = Object.FindObjectsByType<NavMeshLink>(FindObjectsSortMode.None);
#else
                var links = Object.FindObjectsOfType<NavMeshLink>();
#endif
                sb.AppendLine($"\nNavMeshLinks ({links.Length}):");
                foreach (var l in links)
                {
                    sb.AppendLine($"  [{l.gameObject.name}] start={FormatVector3(l.startPoint)} end={FormatVector3(l.endPoint)}");
                    sb.AppendLine($"    width={l.width:F2} cost={l.costModifier:F2} bidirectional={l.bidirectional} area={l.area} activated={l.activated}");
                    if (l.startTransform != null) sb.AppendLine($"    startTransform={l.startTransform.name}");
                    if (l.endTransform != null) sb.AppendLine($"    endTransform={l.endTransform.name}");
                }

                // Modifiers
#if UNITY_2023_1_OR_NEWER
                var modifiers = Object.FindObjectsByType<NavMeshModifier>(FindObjectsSortMode.None);
#else
                var modifiers = Object.FindObjectsOfType<NavMeshModifier>();
#endif
                if (modifiers.Length > 0)
                {
                    sb.AppendLine($"\nNavMeshModifiers ({modifiers.Length}):");
                    foreach (var m in modifiers)
                    {
                        sb.AppendLine($"  [{m.gameObject.name}] area={m.area} overrideArea={m.overrideArea} ignoreFromBuild={m.ignoreFromBuild} applyToChildren={m.applyToChildren}");
                    }
                }

                // Modifier Volumes
#if UNITY_2023_1_OR_NEWER
                var volumes = Object.FindObjectsByType<NavMeshModifierVolume>(FindObjectsSortMode.None);
#else
                var volumes = Object.FindObjectsOfType<NavMeshModifierVolume>();
#endif
                if (volumes.Length > 0)
                {
                    sb.AppendLine($"\nNavMeshModifierVolumes ({volumes.Length}):");
                    foreach (var v in volumes)
                    {
                        sb.AppendLine($"  [{v.gameObject.name}] size={FormatVector3(v.size)} center={FormatVector3(v.center)} area={v.area}");
                    }
                }

                // Agents
#if UNITY_2023_1_OR_NEWER
                var agents = Object.FindObjectsByType<NavMeshAgent>(FindObjectsSortMode.None);
#else
                var agents = Object.FindObjectsOfType<NavMeshAgent>();
#endif
                if (agents.Length > 0)
                {
                    sb.AppendLine($"\nNavMeshAgents ({agents.Length}):");
                    foreach (var a in agents)
                    {
                        sb.AppendLine($"  [{a.gameObject.name}] speed={a.speed:F2} angularSpeed={a.angularSpeed:F1} radius={a.radius:F2} height={a.height:F2} stoppingDistance={a.stoppingDistance:F2}");
                    }
                }

                return sb.ToString();
            });
        }
    }
}
