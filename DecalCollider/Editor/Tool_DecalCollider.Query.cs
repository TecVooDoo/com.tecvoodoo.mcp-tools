#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.DecalCollider.Editor
{
    public partial class Tool_DecalCollider
    {
        [McpPluginTool("decal-query", Title = "Decal Collider / Query")]
        [Description(@"Reads full configuration and status of a DecalCollider component.
Reports: decal mode, projection direction/space, size, subdivisions, surface offset,
alpha threshold, LOD settings, hit objects, rebuild stats (triangles, rays, time, memory),
and visual/collider mesh bounds.")]
        public string Query(
            [Description("Name of the GameObject with the DecalCollider component.")] string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var dc = GetDecal(gameObjectName);
                var sb = new StringBuilder();

                sb.AppendLine($"DecalCollider on '{gameObjectName}':");
                sb.AppendLine($"  Mode: {Get(dc, "decalMode")}");
                sb.AppendLine($"  Projection: direction={Get(dc, "projectionDirection")} space={Get(dc, "projectionSpace")}");
                sb.AppendLine($"  Size: {Get(dc, "size")}");
                sb.AppendLine($"  MaxDistance: {Get(dc, "maxDistance")}");
                sb.AppendLine($"  Center: {Get(dc, "center")}");
                sb.AppendLine($"  SurfaceOffset: {Get(dc, "surfaceOffset")}");
                sb.AppendLine($"  Subdivisions: mesh={Get(dc, "meshSubdivisions")} collider={Get(dc, "colliderSubdivisions")}");
                sb.AppendLine($"  AlphaThreshold: {Get(dc, "alphaThreshold")}");
                sb.AppendLine($"  WrapMask: {Get(dc, "wrapMask")}");
                sb.AppendLine($"  AlwaysRebuild: {Get(dc, "alwaysRebuild")}  IgnoreSelf: {Get(dc, "ignoreSelf")}  CullIfInvisible: {Get(dc, "cullIfInvisible")}");
                sb.AppendLine($"  DynamicLOD: {Get(dc, "useDynamicLOD")} distance={Get(dc, "lodDistance")} interval={Get(dc, "lodCheckInterval")}");
                sb.AppendLine($"  LiveUpdate: interval={Get(dc, "liveUpdateInterval")} collider={Get(dc, "updateColliderOnLive")}");

                var mode = Get(dc, "decalMode")?.ToString();
                if (mode == "MeshProjection")
                {
                    sb.AppendLine($"  MeshScale: {Get(dc, "meshScale")}");
                    sb.AppendLine($"  MeshOffset: {Get(dc, "meshOffset")}");
                    var inputMesh = Get(dc, "inputMesh") as Mesh;
                    sb.AppendLine($"  InputMesh: {(inputMesh != null ? inputMesh.name : "none")}");
                    sb.AppendLine($"  ColliderScale: {Get(dc, "colliderScale")}");
                }
                if (mode == "GridProjection")
                    sb.AppendLine($"  RaycastGridExtent: {Get(dc, "raycastGridExtent")}");

                // Rebuild stats
                var stats = Get(dc, "LastRebuildStats");
                if (stats != null)
                {
                    sb.AppendLine($"  RebuildStats: trisVisual={Get(stats, "TrianglesVisual")} trisCollider={Get(stats, "TrianglesCollider")} rays={Get(stats, "RaysCast")} hits={Get(stats, "RaysHit")} time={Get(stats, "BuildTimeMS")}ms memory={Get(stats, "MemoryKb")}KB");
                }
                sb.AppendLine($"  RebuildCount: {Get(dc, "RebuildCounter")}  LastTime: {Get(dc, "LastRebuildTimeMS")}ms");

                // Hit objects
                var hits = CallMethod(dc, "GetHitObjects") as IList<GameObject>;
                if (hits != null && hits.Count > 0)
                {
                    sb.Append($"  HitObjects ({hits.Count}): ");
                    for (int i = 0; i < Mathf.Min(hits.Count, 10); i++)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append(hits[i] != null ? hits[i].name : "null");
                    }
                    if (hits.Count > 10) sb.Append($" ... +{hits.Count - 10} more");
                    sb.AppendLine();
                }

                // Mesh info
                var visualMesh = Get(dc, "VisualMesh") as Mesh;
                if (visualMesh != null) sb.AppendLine($"  VisualMesh: bounds={Get(dc, "VisualBoundsWS")}");
                var colliderMesh = Get(dc, "ColliderMesh") as Mesh;
                if (colliderMesh != null) sb.AppendLine($"  ColliderMesh: bounds={Get(dc, "ColliderBoundsWS")}");

                return sb.ToString();
            });
        }
    }
}
