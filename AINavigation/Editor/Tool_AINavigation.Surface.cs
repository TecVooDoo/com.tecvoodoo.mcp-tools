#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace MCPTools.AINavigation.Editor
{
    public partial class Tool_AINavigation
    {
        [McpPluginTool("nav-configure-surface", Title = "AI Navigation / Configure Surface")]
        [Description(@"Configures a NavMeshSurface component. All parameters optional — only provided values change.
collectObjects: All, Volume, Children, MarkedWithModifier.
useGeometry: RenderMeshes or PhysicsColliders.
After configuring, call nav-bake to rebuild the NavMesh.")]
        public string ConfigureSurface(
            [Description("Name of the GameObject with NavMeshSurface.")] string gameObjectName,
            [Description("Agent type ID.")] int? agentTypeID = null,
            [Description("Object collection: All, Volume, Children, MarkedWithModifier.")] string? collectObjects = null,
            [Description("Geometry source: RenderMeshes or PhysicsColliders.")] string? useGeometry = null,
            [Description("Layer mask value for filtering.")] int? layerMask = null,
            [Description("Default area type for unmapped objects.")] int? defaultArea = null,
            [Description("Ignore GameObjects with NavMeshAgent.")] bool? ignoreNavMeshAgent = null,
            [Description("Ignore GameObjects with NavMeshObstacle.")] bool? ignoreNavMeshObstacle = null,
            [Description("Override tile size.")] bool? overrideTileSize = null,
            [Description("Tile size in voxels [16-1024].")] int? tileSize = null,
            [Description("Override voxel size.")] bool? overrideVoxelSize = null,
            [Description("Voxel size in world units.")] float? voxelSize = null,
            [Description("Minimum region area threshold.")] float? minRegionArea = null,
            [Description("Generate height mesh.")] bool? buildHeightMesh = null,
            [Description("Volume size X.")] float? sizeX = null,
            [Description("Volume size Y.")] float? sizeY = null,
            [Description("Volume size Z.")] float? sizeZ = null,
            [Description("Volume center X.")] float? centerX = null,
            [Description("Volume center Y.")] float? centerY = null,
            [Description("Volume center Z.")] float? centerZ = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var surface = GetSurface(gameObjectName);

                if (agentTypeID.HasValue) surface.agentTypeID = agentTypeID.Value;
                if (collectObjects != null)
                    surface.collectObjects = System.Enum.Parse<CollectObjects>(collectObjects, true);
                if (useGeometry != null)
                    surface.useGeometry = System.Enum.Parse<NavMeshCollectGeometry>(useGeometry, true);
                if (layerMask.HasValue) surface.layerMask = layerMask.Value;
                if (defaultArea.HasValue) surface.defaultArea = defaultArea.Value;
                if (ignoreNavMeshAgent.HasValue) surface.ignoreNavMeshAgent = ignoreNavMeshAgent.Value;
                if (ignoreNavMeshObstacle.HasValue) surface.ignoreNavMeshObstacle = ignoreNavMeshObstacle.Value;
                if (overrideTileSize.HasValue) surface.overrideTileSize = overrideTileSize.Value;
                if (tileSize.HasValue) surface.tileSize = Mathf.Clamp(tileSize.Value, 16, 1024);
                if (overrideVoxelSize.HasValue) surface.overrideVoxelSize = overrideVoxelSize.Value;
                if (voxelSize.HasValue) surface.voxelSize = Mathf.Max(0.001f, voxelSize.Value);
                if (minRegionArea.HasValue) surface.minRegionArea = Mathf.Max(0f, minRegionArea.Value);
                if (buildHeightMesh.HasValue) surface.buildHeightMesh = buildHeightMesh.Value;

                if (sizeX.HasValue || sizeY.HasValue || sizeZ.HasValue)
                {
                    var s = surface.size;
                    if (sizeX.HasValue) s.x = sizeX.Value;
                    if (sizeY.HasValue) s.y = sizeY.Value;
                    if (sizeZ.HasValue) s.z = sizeZ.Value;
                    surface.size = s;
                }

                if (centerX.HasValue || centerY.HasValue || centerZ.HasValue)
                {
                    var c = surface.center;
                    if (centerX.HasValue) c.x = centerX.Value;
                    if (centerY.HasValue) c.y = centerY.Value;
                    if (centerZ.HasValue) c.z = centerZ.Value;
                    surface.center = c;
                }

                EditorUtility.SetDirty(surface);
                return $"OK: NavMeshSurface on '{gameObjectName}' configured. collect={surface.collectObjects} geometry={surface.useGeometry} size={FormatVector3(surface.size)}";
            });
        }

        [McpPluginTool("nav-bake", Title = "AI Navigation / Bake NavMesh")]
        [Description(@"Triggers a NavMesh bake on a NavMeshSurface. Synchronous — blocks until complete.
If no gameObjectName is provided, bakes ALL active surfaces in the scene.")]
        public string Bake(
            [Description("Name of the GameObject with NavMeshSurface. Omit to bake all.")] string? gameObjectName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (gameObjectName != null)
                {
                    var surface = GetSurface(gameObjectName);
                    surface.BuildNavMesh();
                    EditorUtility.SetDirty(surface);
                    return $"OK: NavMesh baked on '{gameObjectName}'. hasBakedData={surface.navMeshData != null}";
                }
                else
                {
                    var surfaces = NavMeshSurface.activeSurfaces;
                    if (surfaces.Count == 0) return "No active NavMeshSurfaces found.";
                    foreach (var s in surfaces)
                    {
                        s.BuildNavMesh();
                        EditorUtility.SetDirty(s);
                    }
                    return $"OK: NavMesh baked on {surfaces.Count} surface(s).";
                }
            });
        }
    }
}
