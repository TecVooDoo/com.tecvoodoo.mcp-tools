#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
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
                var sim = GetStruct(rope, "simulation")
                          ?? throw new Exception("Could not read rope.simulation.");
                var col = GetStruct(rope, "collisions")
                          ?? throw new Exception("Could not read rope.collisions.");
                var m   = Get(rope, "measurements");

                var sb = new StringBuilder();

                var ropeName = Get(rope, "name");
                sb.AppendLine($"=== Rope: {ropeName} ===");

                sb.AppendLine($"\n-- Simulation --");
                sb.AppendLine($"  resolution:         {GetStructField(sim, "resolution"):F3}");
                sb.AppendLine($"  massPerMeter:       {GetStructField(sim, "massPerMeter"):F3}");
                sb.AppendLine($"  stiffness:          {GetStructField(sim, "stiffness"):F3}");
                sb.AppendLine($"  energyLoss:         {GetStructField(sim, "energyLoss"):F3}");
                sb.AppendLine($"  lengthMultiplier:   {GetStructField(sim, "lengthMultiplier"):F3}");
                sb.AppendLine($"  gravityMultiplier:  {GetStructField(sim, "gravityMultiplier"):F3}");
                var useCustomGravity = GetStructField(sim, "useCustomGravity");
                sb.AppendLine($"  useCustomGravity:   {useCustomGravity}");
                if (useCustomGravity is true)
                {
                    var cg = GetStructField(sim, "customGravity");
                    if (cg is Vector3 cgVec)
                        sb.AppendLine($"  customGravity:      {FormatVector3(cgVec)}");
                    else
                        sb.AppendLine($"  customGravity:      {cg}");
                }
                sb.AppendLine($"  substeps:           {GetStructField(sim, "substeps")}");
                sb.AppendLine($"  solverIterations:   {GetStructField(sim, "solverIterations")}");
                sb.AppendLine($"  enabled:            {GetStructField(sim, "enabled")}");

                sb.AppendLine($"\n-- Collision --");
                sb.AppendLine($"  enabled:            {GetStructField(col, "enabled")}");
                sb.AppendLine($"  influenceRigidbodies: {GetStructField(col, "influenceRigidbodies")}");
                sb.AppendLine($"  friction:           {GetStructField(col, "friction"):F3}");
                sb.AppendLine($"  stride:             {GetStructField(col, "stride")}");
                sb.AppendLine($"  collisionMargin:    {GetStructField(col, "collisionMargin"):F3}");

                sb.AppendLine($"\n-- Appearance --");
                sb.AppendLine($"  radius:             {Get(rope, "radius"):F4}");
                sb.AppendLine($"  radialVertices:     {Get(rope, "radialVertices")}");
                sb.AppendLine($"  isLoop:             {Get(rope, "isLoop")}");
                var material = Get(rope, "material");
                string matName = "none";
                if (material is UnityEngine.Object matObj && matObj != null) matName = matObj.name;
                sb.AppendLine($"  material:           {matName}");
                sb.AppendLine($"  shadowMode:         {Get(rope, "shadowMode")}");

                sb.AppendLine($"\n-- Measurements --");
                if (m != null)
                {
                    sb.AppendLine($"  spawnCurveLength:   {GetStructField(m, "spawnCurveLength"):F3}");
                    sb.AppendLine($"  realCurveLength:    {GetStructField(m, "realCurveLength"):F3}");
                    sb.AppendLine($"  segmentCount:       {GetStructField(m, "segmentCount")}");
                    sb.AppendLine($"  particleCount:      {GetStructField(m, "particleCount")}");
                    sb.AppendLine($"  particleSpacing:    {GetStructField(m, "particleSpacing"):F4}");
                }

                var spawnPoints = Get(rope, "spawnPoints");
                if (spawnPoints is ICollection spCol)
                    sb.AppendLine($"  spawnPoints:        {spCol.Count}");

                // RopeConnection components
                if (RopeConnectionType != null)
                {
                    var ropeGo = (rope as Component)!.gameObject;
                    var connections = ropeGo.GetComponents(RopeConnectionType);
                    sb.AppendLine($"\n-- Connections ({connections.Length}) --");
                    foreach (var c in connections)
                    {
                        var cType = Get(c, "type");
                        var cLoc  = Get(c, "ropeLocation");
                        var cAuto = Get(c, "autoFindRopeLocation");
                        sb.AppendLine($"  type={cType} location={cLoc:F3} autoFind={cAuto}");
                    }
                }

                return sb.ToString();
            });
        }
    }
}
