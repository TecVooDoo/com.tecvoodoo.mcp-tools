#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_AStarPathfinding
    {
        [McpPluginTool("astar-configure-recast", Title = "A* Pathfinding / Configure Recast Graph")]
        [Description(@"Configure a RecastGraph in the A* Pathfinding system.
Sets character radius, walkable height/climb, slope, voxel resolution, contour error,
region size, tiling options. Does NOT trigger a scan -- call astar-scan afterwards.")]
        public string ConfigureRecastGraph(
            [Description("Graph index in AstarPath.data.graphs (default 0).")] int? graphIndex = null,
            [Description("Agent radius for navmesh erosion.")] float? characterRadius = null,
            [Description("Minimum walkable height (character height).")] float? walkableHeight = null,
            [Description("Maximum step/climb height.")] float? walkableClimb = null,
            [Description("Maximum walkable slope in degrees.")] float? maxSlope = null,
            [Description("Voxel cell size (smaller = more precise, slower).")] float? cellSize = null,
            [Description("Max edge simplification error.")] float? contourMaxError = null,
            [Description("Minimum region size to keep.")] int? minRegionSize = null,
            [Description("Enable tiling for large worlds.")] bool? useTiles = null,
            [Description("Tile size in voxels (when tiling enabled).")] int? editorTileSize = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (AstarPath.active == null)
                    throw new System.Exception("No AstarPath instance found in scene.");

                int idx = graphIndex ?? 0;
                NavGraph[] graphs = AstarPath.active.data.graphs;
                if (graphs == null || idx < 0 || idx >= graphs.Length)
                    throw new System.Exception($"Graph index {idx} out of range. {(graphs != null ? graphs.Length : 0)} graphs exist.");

                RecastGraph recast = graphs[idx] as RecastGraph;
                if (recast == null)
                    throw new System.Exception($"Graph [{idx}] is {graphs[idx].GetType().Name}, not a RecastGraph.");

                if (characterRadius.HasValue) recast.characterRadius = characterRadius.Value;
                if (walkableHeight.HasValue) recast.walkableHeight = walkableHeight.Value;
                if (walkableClimb.HasValue) recast.walkableClimb = walkableClimb.Value;
                if (maxSlope.HasValue) recast.maxSlope = maxSlope.Value;
                if (cellSize.HasValue) recast.cellSize = cellSize.Value;
                if (contourMaxError.HasValue) recast.contourMaxError = contourMaxError.Value;
                if (minRegionSize.HasValue) recast.minRegionSize = minRegionSize.Value;
                if (useTiles.HasValue) recast.useTiles = useTiles.Value;
                if (editorTileSize.HasValue) recast.editorTileSize = editorTileSize.Value;

                EditorUtility.SetDirty(AstarPath.active);

                StringBuilder sb = new StringBuilder();
                sb.Append($"OK: RecastGraph [{idx}] configured.");
                sb.Append($" characterRadius={recast.characterRadius:F3} walkableHeight={recast.walkableHeight:F3}");
                sb.Append($" walkableClimb={recast.walkableClimb:F3} maxSlope={recast.maxSlope:F1}");
                sb.Append($" cellSize={recast.cellSize:F4} contourMaxError={recast.contourMaxError:F3}");
                sb.Append($" minRegionSize={recast.minRegionSize} useTiles={recast.useTiles} editorTileSize={recast.editorTileSize}");
                sb.Append(" | Call astar-scan to apply.");
                return sb.ToString();
            });
        }
    }
}
