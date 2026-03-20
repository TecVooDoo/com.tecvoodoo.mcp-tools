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
        [McpPluginTool("astar-configure-agent", Title = "A* Pathfinding / Configure Agent")]
        [Description(@"Configure an AI movement agent (AIPath, AILerp, or RichAI) on a GameObject.
Sets speed, rotation, destination threshold, slowdown, waypoint look-ahead, and other movement settings.
Optionally sets the agent's destination to another GameObject's position via destinationName.")]
        public string ConfigureAgent(
            [Description("Name of the GameObject with an AI movement component.")] string gameObjectName,
            [Description("Maximum movement speed.")] float? maxSpeed = null,
            [Description("Rotation speed in degrees/sec.")] float? rotationSpeed = null,
            [Description("Distance threshold to consider destination reached.")] float? endReachedDistance = null,
            [Description("Distance at which agent starts slowing down.")] float? slowdownDistance = null,
            [Description("Path waypoint look-ahead distance.")] float? pickNextWaypointDist = null,
            [Description("Enable rotation towards movement direction.")] bool? enableRotation = null,
            [Description("Constrain agent position inside walkable graph.")] bool? constrainInsideGraph = null,
            [Description("Name of a GameObject whose position becomes the agent's destination.")] string? destinationName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                StringBuilder sb = new StringBuilder();

                // Find the concrete agent type
                AIPath? aiPath = go.GetComponent<AIPath>();
                RichAI? richAI = go.GetComponent<RichAI>();
                AILerp? aiLerp = go.GetComponent<AILerp>();

                if (aiPath != null)
                {
                    if (maxSpeed.HasValue) aiPath.maxSpeed = maxSpeed.Value;
                    if (rotationSpeed.HasValue) aiPath.rotationSpeed = rotationSpeed.Value;
                    if (endReachedDistance.HasValue) aiPath.endReachedDistance = endReachedDistance.Value;
                    if (slowdownDistance.HasValue) aiPath.slowdownDistance = slowdownDistance.Value;
                    if (pickNextWaypointDist.HasValue) aiPath.pickNextWaypointDist = pickNextWaypointDist.Value;
                    if (enableRotation.HasValue) aiPath.enableRotation = enableRotation.Value;
                    if (constrainInsideGraph.HasValue) aiPath.constrainInsideGraph = constrainInsideGraph.Value;

                    if (destinationName != null)
                    {
                        GameObject destGO = FindGO(destinationName);
                        aiPath.destination = destGO.transform.position;
                    }

                    EditorUtility.SetDirty(aiPath);
                    sb.Append($"OK: AIPath on '{go.name}' configured.");
                    sb.Append($" maxSpeed={aiPath.maxSpeed:F2} rotationSpeed={aiPath.rotationSpeed:F2}");
                    sb.Append($" endReachedDistance={aiPath.endReachedDistance:F3} slowdownDistance={aiPath.slowdownDistance:F3}");
                    sb.Append($" pickNextWaypointDist={aiPath.pickNextWaypointDist:F3}");
                    sb.Append($" enableRotation={aiPath.enableRotation} constrainInsideGraph={aiPath.constrainInsideGraph}");
                }
                else if (richAI != null)
                {
                    if (maxSpeed.HasValue) richAI.maxSpeed = maxSpeed.Value;
                    if (rotationSpeed.HasValue) richAI.rotationSpeed = rotationSpeed.Value;
                    if (endReachedDistance.HasValue) richAI.endReachedDistance = endReachedDistance.Value;
                    if (enableRotation.HasValue) richAI.enableRotation = enableRotation.Value;

                    if (destinationName != null)
                    {
                        GameObject destGO = FindGO(destinationName);
                        richAI.destination = destGO.transform.position;
                    }

                    EditorUtility.SetDirty(richAI);
                    sb.Append($"OK: RichAI on '{go.name}' configured.");
                    sb.Append($" maxSpeed={richAI.maxSpeed:F2} rotationSpeed={richAI.rotationSpeed:F2}");
                    sb.Append($" endReachedDistance={richAI.endReachedDistance:F3}");
                    sb.Append($" enableRotation={richAI.enableRotation}");
                }
                else if (aiLerp != null)
                {
                    if (maxSpeed.HasValue) aiLerp.speed = maxSpeed.Value;
                    if (enableRotation.HasValue) aiLerp.enableRotation = enableRotation.Value;

                    if (destinationName != null)
                    {
                        GameObject destGO = FindGO(destinationName);
                        aiLerp.destination = destGO.transform.position;
                    }

                    EditorUtility.SetDirty(aiLerp);
                    sb.Append($"OK: AILerp on '{go.name}' configured.");
                    sb.Append($" speed={aiLerp.speed:F2} enableRotation={aiLerp.enableRotation}");
                }
                else
                {
                    throw new System.Exception($"'{go.name}' has no AIPath, RichAI, or AILerp component.");
                }

                if (destinationName != null)
                    sb.Append($" destination set to '{destinationName}'");

                return sb.ToString();
            });
        }
    }
}
