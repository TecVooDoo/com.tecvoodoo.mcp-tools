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
        [McpPluginTool("astar-configure-grid", Title = "A* Pathfinding / Configure Grid Graph")]
        [Description(@"Configure a GridGraph in the A* Pathfinding system.
Sets grid dimensions, node size, center position, slope/step settings, erosion, and neighbour mode.
Does NOT trigger a scan -- call astar-scan afterwards to apply changes.")]
        public string ConfigureGridGraph(
            [Description("Graph index in AstarPath.data.graphs (default 0).")] int? graphIndex = null,
            [Description("Grid width in nodes.")] int? width = null,
            [Description("Grid depth in nodes.")] int? depth = null,
            [Description("World units per node.")] float? nodeSize = null,
            [Description("Grid center X position.")] float? centerX = null,
            [Description("Grid center Y position.")] float? centerY = null,
            [Description("Grid center Z position.")] float? centerZ = null,
            [Description("Max walkable slope in degrees.")] float? maxSlope = null,
            [Description("Max step/climb height in world units.")] float? maxStepHeight = null,
            [Description("Obstacle erosion iterations (margin around unwalkable).")] int? erodeIterations = null,
            [Description("Neighbour connection mode: Four, Eight, or Six.")] string? neighbours = null,
            [Description("Allow diagonal movement to cut corners.")] bool? cutCorners = null
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

                GridGraph grid = graphs[idx] as GridGraph;
                if (grid == null)
                    throw new System.Exception($"Graph [{idx}] is {graphs[idx].GetType().Name}, not a GridGraph.");

                if (width.HasValue) grid.width = width.Value;
                if (depth.HasValue) grid.depth = depth.Value;
                if (nodeSize.HasValue) grid.nodeSize = nodeSize.Value;

                if (centerX.HasValue || centerY.HasValue || centerZ.HasValue)
                {
                    Vector3 center = grid.center;
                    if (centerX.HasValue) center.x = centerX.Value;
                    if (centerY.HasValue) center.y = centerY.Value;
                    if (centerZ.HasValue) center.z = centerZ.Value;
                    grid.center = center;
                }

                if (maxSlope.HasValue) grid.maxSlope = maxSlope.Value;
                if (maxStepHeight.HasValue) grid.maxStepHeight = maxStepHeight.Value;
                if (erodeIterations.HasValue) grid.erodeIterations = erodeIterations.Value;

                if (neighbours != null)
                {
                    if (!System.Enum.TryParse<NumNeighbours>(neighbours, true, out NumNeighbours mode))
                        throw new System.Exception($"Invalid neighbours '{neighbours}'. Valid: Four, Eight, Six.");
                    grid.neighbours = mode;
                }

                if (cutCorners.HasValue) grid.cutCorners = cutCorners.Value;

                EditorUtility.SetDirty(AstarPath.active);

                StringBuilder sb = new StringBuilder();
                sb.Append($"OK: GridGraph [{idx}] configured.");
                sb.Append($" width={grid.width} depth={grid.depth} nodeSize={grid.nodeSize:F3}");
                sb.Append($" center={FormatVector3(grid.center)}");
                sb.Append($" maxSlope={grid.maxSlope:F1} maxStepHeight={grid.maxStepHeight:F3}");
                sb.Append($" erodeIterations={grid.erodeIterations} neighbours={grid.neighbours} cutCorners={grid.cutCorners}");
                sb.Append(" | Call astar-scan to apply.");
                return sb.ToString();
            });
        }
    }
}
