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
        [McpPluginTool("astar-query", Title = "A* Pathfinding / Query")]
        [Description(@"Query A* pathfinding system state and AI agent info.
Without gameObjectName: lists all graphs (type, node count, dimensions for GridGraph, characterRadius for RecastGraph).
With gameObjectName: reports AIPath/AILerp/RichAI settings (maxSpeed, rotationSpeed, endReachedDistance, destination,
reachedDestination, remainingDistance, hasPath) and Seeker settings (traversableTags).")]
        public string QueryAStar(
            [Description("Optional: name of a GameObject with an AI movement agent (AIPath/AILerp/RichAI) to inspect.")] string? gameObjectName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (AstarPath.active == null)
                    throw new System.Exception("No AstarPath instance found in scene. Add an AstarPath component first.");

                StringBuilder sb = new StringBuilder();

                if (gameObjectName == null)
                {
                    // List all graphs
                    sb.AppendLine("=== A* Pathfinding Graphs ===");
                    NavGraph[] graphs = AstarPath.active.data.graphs;
                    if (graphs == null || graphs.Length == 0)
                    {
                        sb.AppendLine("  No graphs configured.");
                        return sb.ToString();
                    }

                    for (int i = 0; i < graphs.Length; i++)
                    {
                        NavGraph graph = graphs[i];
                        if (graph == null) continue;

                        sb.AppendLine($"\n  [{i}] {graph.GetType().Name}: \"{graph.name}\"");
                        sb.AppendLine($"      nodes: {graph.CountNodes()}");

                        if (graph is GridGraph gridGraph)
                        {
                            sb.AppendLine($"      width: {gridGraph.width}  depth: {gridGraph.depth}");
                            sb.AppendLine($"      nodeSize: {gridGraph.nodeSize:F3}");
                            sb.AppendLine($"      center: {FormatVector3(gridGraph.center)}");
                            sb.AppendLine($"      maxSlope: {gridGraph.maxSlope:F1}");
                            sb.AppendLine($"      maxStepHeight: {gridGraph.maxStepHeight:F3}");
                            sb.AppendLine($"      erodeIterations: {gridGraph.erodeIterations}");
                            sb.AppendLine($"      neighbours: {gridGraph.neighbours}");
                            sb.AppendLine($"      cutCorners: {gridGraph.cutCorners}");
                        }

                        if (graph is RecastGraph recastGraph)
                        {
                            sb.AppendLine($"      characterRadius: {recastGraph.characterRadius:F3}");
                            sb.AppendLine($"      walkableHeight: {recastGraph.walkableHeight:F3}");
                            sb.AppendLine($"      walkableClimb: {recastGraph.walkableClimb:F3}");
                            sb.AppendLine($"      maxSlope: {recastGraph.maxSlope:F1}");
                            sb.AppendLine($"      cellSize: {recastGraph.cellSize:F4}");
                            sb.AppendLine($"      contourMaxError: {recastGraph.contourMaxError:F3}");
                            sb.AppendLine($"      minRegionSize: {recastGraph.minRegionSize}");
                            sb.AppendLine($"      useTiles: {recastGraph.useTiles}");
                            sb.AppendLine($"      editorTileSize: {recastGraph.editorTileSize}");
                        }
                    }
                }
                else
                {
                    // Query agent on specific GO
                    GameObject go = FindGO(gameObjectName);
                    sb.AppendLine($"=== A* Agent: {go.name} ===");

                    // Try AIPath, AILerp, RichAI
                    IAstarAI? ai = go.GetComponent<AIPath>();
                    string agentType = "AIPath";
                    if (ai == null)
                    {
                        ai = go.GetComponent<AILerp>();
                        agentType = "AILerp";
                    }
                    if (ai == null)
                    {
                        ai = go.GetComponent<RichAI>();
                        agentType = "RichAI";
                    }

                    if (ai != null)
                    {
                        sb.AppendLine($"\n  Agent Type: {agentType}");
                        sb.AppendLine($"  maxSpeed: {ai.maxSpeed:F2}");
                        sb.AppendLine($"  canMove: {ai.canMove}");
                        sb.AppendLine($"  canSearch: {ai.canSearch}");
                        sb.AppendLine($"  destination: {FormatVector3(ai.destination)}");
                        sb.AppendLine($"  reachedDestination: {ai.reachedDestination}");
                        sb.AppendLine($"  reachedEndOfPath: {ai.reachedEndOfPath}");
                        sb.AppendLine($"  remainingDistance: {ai.remainingDistance:F3}");
                        sb.AppendLine($"  hasPath: {ai.hasPath}");
                        sb.AppendLine($"  pathPending: {ai.pathPending}");
                        sb.AppendLine($"  position: {FormatVector3(ai.position)}");

                        // Report concrete type-specific settings
                        AIPath? aiPath = go.GetComponent<AIPath>();
                        if (aiPath != null)
                        {
                            sb.AppendLine($"  rotationSpeed: {aiPath.rotationSpeed:F2}");
                            sb.AppendLine($"  endReachedDistance: {aiPath.endReachedDistance:F3}");
                            sb.AppendLine($"  slowdownDistance: {aiPath.slowdownDistance:F3}");
                            sb.AppendLine($"  pickNextWaypointDist: {aiPath.pickNextWaypointDist:F3}");
                            sb.AppendLine($"  enableRotation: {aiPath.enableRotation}");
                            sb.AppendLine($"  constrainInsideGraph: {aiPath.constrainInsideGraph}");
                        }

                        RichAI? richAI = go.GetComponent<RichAI>();
                        if (richAI != null)
                        {
                            sb.AppendLine($"  rotationSpeed: {richAI.rotationSpeed:F2}");
                            sb.AppendLine($"  endReachedDistance: {richAI.endReachedDistance:F3}");
                            sb.AppendLine($"  slowdownTime: {richAI.slowdownTime:F3}");
                            sb.AppendLine($"  enableRotation: {richAI.enableRotation}");
                        }
                    }
                    else
                    {
                        sb.AppendLine("  No AIPath, AILerp, or RichAI component found.");
                    }

                    // Seeker
                    Seeker? seeker = go.GetComponent<Seeker>();
                    if (seeker != null)
                    {
                        sb.AppendLine($"\n  -- Seeker --");
                        sb.AppendLine($"  traversableTags: {seeker.traversableTags} (0x{seeker.traversableTags:X8})");
                        int[] penalties = seeker.tagPenalties;
                        if (penalties != null && penalties.Length > 0)
                        {
                            StringBuilder tagSb = new StringBuilder("  tagPenalties: ");
                            for (int t = 0; t < penalties.Length; t++)
                            {
                                if (penalties[t] != 0)
                                    tagSb.Append($"[{t}]={penalties[t]} ");
                            }
                            sb.AppendLine(tagSb.ToString().TrimEnd());
                        }
                    }
                    else
                    {
                        sb.AppendLine("\n  No Seeker component found.");
                    }
                }

                return sb.ToString();
            });
        }
    }
}
