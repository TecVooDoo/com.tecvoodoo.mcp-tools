#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Pathfinding;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_AStarPathfinding
    {
        [McpPluginTool("astar-scan", Title = "A* Pathfinding / Scan Graphs")]
        [Description(@"Trigger a graph scan or runtime graph update.
action='scan': Full rescan of all graphs or a specific graph by index.
action='update': Bounds-based graph update at a specific position. Can modify walkability,
penalty, and tags within the update bounds. Useful for dynamic obstacle changes.")]
        public string ScanGraphs(
            [Description("Action to perform: 'scan' (full rescan) or 'update' (bounds-based update).")] string action,
            [Description("Specific graph index to scan (null = all graphs). Only used with 'scan' action.")] int? graphIndex = null,
            [Description("Center X of update bounds (for 'update' action).")] float? boundsX = null,
            [Description("Center Y of update bounds (for 'update' action).")] float? boundsY = null,
            [Description("Center Z of update bounds (for 'update' action).")] float? boundsZ = null,
            [Description("Size of the cubic update bounds (default 10).")] float? boundsSize = null,
            [Description("Penalty delta to apply in the update area.")] int? penaltyDelta = null,
            [Description("Set walkability of nodes in update area.")] bool? setWalkability = null,
            [Description("Tag value (0-31) to assign to nodes in update area.")] int? setTag = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (AstarPath.active == null)
                    throw new System.Exception("No AstarPath instance found in scene.");

                StringBuilder sb = new StringBuilder();

                if (action == "scan")
                {
                    if (graphIndex.HasValue)
                    {
                        NavGraph[] graphs = AstarPath.active.data.graphs;
                        if (graphs == null || graphIndex.Value < 0 || graphIndex.Value >= graphs.Length)
                            throw new System.Exception($"Graph index {graphIndex.Value} out of range.");

                        AstarPath.active.Scan(graphs[graphIndex.Value]);
                        sb.Append($"OK: Scanned graph [{graphIndex.Value}] ({graphs[graphIndex.Value].GetType().Name}).");
                        sb.Append($" Nodes: {graphs[graphIndex.Value].CountNodes()}");
                    }
                    else
                    {
                        AstarPath.active.Scan();
                        sb.Append("OK: Full scan completed.");
                        NavGraph[] graphs = AstarPath.active.data.graphs;
                        if (graphs != null)
                        {
                            for (int i = 0; i < graphs.Length; i++)
                            {
                                if (graphs[i] != null)
                                    sb.Append($" [{i}] {graphs[i].GetType().Name}: {graphs[i].CountNodes()} nodes.");
                            }
                        }
                    }
                }
                else if (action == "update")
                {
                    float size = boundsSize ?? 10f;
                    Vector3 center = new Vector3(boundsX ?? 0f, boundsY ?? 0f, boundsZ ?? 0f);
                    Bounds bounds = new Bounds(center, Vector3.one * size);

                    GraphUpdateObject guo = new GraphUpdateObject(bounds);

                    if (setWalkability.HasValue)
                    {
                        guo.modifyWalkability = true;
                        guo.setWalkability = setWalkability.Value;
                    }

                    if (penaltyDelta.HasValue)
                    {
                        guo.addPenalty = penaltyDelta.Value;
                    }

                    if (setTag.HasValue)
                    {
                        if (setTag.Value < 0 || setTag.Value > 31)
                            throw new System.Exception($"Tag must be 0-31, got {setTag.Value}.");
                        guo.modifyTag = true;
                        guo.setTag = new Pathfinding.PathfindingTag((uint)setTag.Value);
                    }

                    AstarPath.active.UpdateGraphs(guo);
                    sb.Append($"OK: Graph update queued at {FormatVector3(center)} size={size:F1}.");
                    if (setWalkability.HasValue) sb.Append($" walkability={setWalkability.Value}");
                    if (penaltyDelta.HasValue) sb.Append($" penaltyDelta={penaltyDelta.Value}");
                    if (setTag.HasValue) sb.Append($" tag={setTag.Value}");
                }
                else
                {
                    throw new System.Exception($"Invalid action '{action}'. Use 'scan' or 'update'.");
                }

                return sb.ToString();
            });
        }
    }
}
