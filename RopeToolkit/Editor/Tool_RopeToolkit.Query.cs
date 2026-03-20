#if HAS_ROPE_TOOLKIT
#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using UnityEngine;

namespace MCPTools.RopeToolkit.Editor
{
    public partial class Tool_RopeToolkit
    {
        [McpPluginTool("rope-query", Title = "Rope Toolkit / Query Rope")]
        [Description(@"Reads the full configuration of a Rope Toolkit Rope component.
Returns simulation settings (stiffness, energyLoss, gravity, substeps), collision settings
(enabled, friction, stride), appearance (radius, radialVertices), and measurements
(particleCount, realCurveLength). Use before configuring to understand current state.")]
        public string QueryRope(
            [Description("Name of the GameObject with the Rope component.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var rope = GetRope(gameObjectName);
                var m = rope.measurements;
                var sb = new StringBuilder();

                sb.AppendLine($"=== Rope: {rope.name} ===");
                sb.AppendLine($"\n-- Simulation --");
                sb.AppendLine($"  resolution:         {rope.simulation.resolution:F3}");
                sb.AppendLine($"  massPerMeter:       {rope.simulation.massPerMeter:F3}");
                sb.AppendLine($"  stiffness:          {rope.simulation.stiffness:F3}");
                sb.AppendLine($"  energyLoss:         {rope.simulation.energyLoss:F3}");
                sb.AppendLine($"  lengthMultiplier:   {rope.simulation.lengthMultiplier:F3}");
                sb.AppendLine($"  gravityMultiplier:  {rope.simulation.gravityMultiplier:F3}");
                sb.AppendLine($"  useCustomGravity:   {rope.simulation.useCustomGravity}");
                if (rope.simulation.useCustomGravity)
                    sb.AppendLine($"  customGravity:      {FormatVector3(rope.simulation.customGravity)}");
                sb.AppendLine($"  substeps:           {rope.simulation.substeps}");
                sb.AppendLine($"  solverIterations:   {rope.simulation.solverIterations}");
                sb.AppendLine($"  enabled:            {rope.simulation.enabled}");

                sb.AppendLine($"\n-- Collision --");
                sb.AppendLine($"  enabled:            {rope.collisions.enabled}");
                sb.AppendLine($"  influenceRigidbodies: {rope.collisions.influenceRigidbodies}");
                sb.AppendLine($"  friction:           {rope.collisions.friction:F3}");
                sb.AppendLine($"  stride:             {rope.collisions.stride}");
                sb.AppendLine($"  collisionMargin:    {rope.collisions.collisionMargin:F3}");

                sb.AppendLine($"\n-- Appearance --");
                sb.AppendLine($"  radius:             {rope.radius:F4}");
                sb.AppendLine($"  radialVertices:     {rope.radialVertices}");
                sb.AppendLine($"  isLoop:             {rope.isLoop}");
                sb.AppendLine($"  material:           {(rope.material != null ? rope.material.name : "none")}");
                sb.AppendLine($"  shadowMode:         {rope.shadowMode}");

                sb.AppendLine($"\n-- Measurements --");
                sb.AppendLine($"  spawnCurveLength:   {m.spawnCurveLength:F3}");
                sb.AppendLine($"  realCurveLength:    {m.realCurveLength:F3}");
                sb.AppendLine($"  segmentCount:       {m.segmentCount}");
                sb.AppendLine($"  particleCount:      {m.particleCount}");
                sb.AppendLine($"  particleSpacing:    {m.particleSpacing:F4}");
                sb.AppendLine($"  spawnPoints:        {rope.spawnPoints.Count}");

                var connections = rope.GetComponents<global::RopeToolkit.RopeConnection>();
                sb.AppendLine($"\n-- Connections ({connections.Length}) --");
                foreach (var c in connections)
                    sb.AppendLine($"  type={c.type} location={c.ropeLocation:F3} autoFind={c.autoFindRopeLocation}");

                return sb.ToString();
            });
        }
    }
}
#endif
