#if HAS_GENA
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;
using Component = UnityEngine.Component;

namespace MCPTools.GeNa.Editor
{
    public partial class Tool_GeNa
    {
        [AiTool("gena-road", Title = "GeNa / Road")]
        [Description(@"Builds and BAKES a GeNa road along 'points' ('x,z' snapped to terrain, or 'x,y,z'), in order, via GeNa's own 'Add Road Spline' menu item (carve, clear colliders/details/trees, texture, road mesh, endpoint fade).
DESTRUCTIVE and NOT undoable: the bake carves the heightmap, deletes terrain trees and paints texture IN the TerrainData asset.
REFUSES when the terrain's TerrainData is also referenced by another scene (ENTRY-434: Save-As shares it, so the road would carve every copy) unless allowSharedTerrainData=true. Copy the TerrainData first instead.
REFUSES when a GeNa spline named 'name' already exists (a re-sent call would carve twice).
Bakes in GeNa's required order: spline Bake() (terrain), then the road extension's Bake(true) (mesh) -- a plain Bake() from a script leaves carved ground with NO road.
Returns trees cleared and road meshes / triangles built. Props already on the terrain are NOT moved by the carve -- re-snap them.")]
        public string Road(
            [Description("Name for the road spline GameObject.")] string name,
            [Description("Road nodes in order: 'x,z' (snapped to terrain) or 'x,y,z'. At least 2.")] string[] points,
            [Description("Height added to snapped 'x,z' nodes, metres. Default 0.")] float yOffset = 0f,
            [Description("Bake even though another scene shares the TerrainData. Default false.")] bool allowSharedTerrainData = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (Application.isPlaying)
                    throw new Exception("Build GeNa roads in edit mode.");
                if (string.IsNullOrWhiteSpace(name))
                    throw new Exception("'name' is required.");
                if (points == null || points.Length < 2)
                    throw new Exception("'points' needs at least 2 entries.");

                string roadName = name.Trim();
                foreach (Component existing in FindAll(SplineTypeName))
                {
                    if (existing.gameObject.name == roadName)
                        throw new Exception($"A GeNa spline named '{roadName}' already exists (active={existing.gameObject.activeInHierarchy}) -- NOT built again: a second bake would carve the terrain twice. Use a new name, or delete it (its terrain edits stay).");
                }

                Vector3[] positions = new Vector3[points.Length];
                for (int i = 0; i < points.Length; i++)
                    positions[i] = ParsePoint(points[i], $"points[{i}]", yOffset);

                HashSet<Terrain> touched = new HashSet<Terrain>();
                foreach (Vector3 position in positions)
                {
                    Terrain? terrain = TerrainAt(position);
                    if (terrain != null)
                        touched.Add(terrain);
                }
                if (touched.Count == 0)
                    throw new Exception("No point lies over a terrain.");
                if (!allowSharedTerrainData)
                {
                    foreach (Terrain terrain in touched)
                    {
                        List<string> others = OtherScenesUsing(terrain.terrainData);
                        if (others.Count > 0)
                            throw new Exception($"REFUSED: terrain '{terrain.name}' uses TerrainData '{AssetDatabase.GetAssetPath(terrain.terrainData)}', which {others.Count} other scene(s) also reference ({string.Join(", ", others)}). " +
                                                "The bake would carve all of them. Duplicate the TerrainData for this scene first, or pass allowSharedTerrainData=true.");
                    }
                }

                int treesBefore = 0;
                foreach (Terrain terrain in touched)
                    treesBefore += terrain.terrainData.treeInstanceCount;
                HashSet<MeshFilter> meshesBefore = new HashSet<MeshFilter>(UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include));

                if (!EditorApplication.ExecuteMenuItem("GameObject/GeNa/Add Road Spline"))
                    throw new Exception("GeNa menu item 'GameObject/GeNa/Add Road Spline' did not run.");
                GameObject? splineObject = Selection.activeGameObject;
                Component? spline = splineObject != null ? splineObject.GetComponent(FindType(SplineTypeName)) : null;
                if (spline == null)
                    throw new Exception("GeNa's 'Add Road Spline' did not leave a spline selected -- menu behaviour changed?");
                splineObject!.name = roadName;

                // Node pattern from GeNa's own RiverFlow.cs: CreateNewNode + AddNode for the first, then AddCurve(prev, next).
                Type nodeType = FindType("GeNa.Core.GeNaNode");
                Type[] createSignature = { typeof(Vector3), typeof(Vector3?), typeof(uint) };
                object? previous = null;
                foreach (Vector3 position in positions)
                {
                    object? node = Invoke(spline, "CreateNewNode", createSignature, position, null, uint.MaxValue);
                    if (previous == null)
                        Invoke(spline, "AddNode", new[] { nodeType }, node);
                    else
                        Invoke(spline, "AddCurve", new[] { nodeType, nodeType, typeof(bool) }, previous, node, true);
                    previous = node;
                }

                Stopwatch timer = Stopwatch.StartNew();
                Invoke(spline, "Bake", Type.EmptyTypes);
                // ENTRY-434: the plain Bake only MOVES road meshes that already exist; Bake(true) runs PreExecute + Execute first.
                object? road = GetExtension(spline, FindType(RoadExtensionTypeName));
                if (road == null)
                    throw new Exception("Baked the terrain half, but the spline has no road extension -- no road mesh was built.");
                Invoke(road, "Bake", new[] { typeof(bool) }, true);
                timer.Stop();

                int treesAfter = 0;
                foreach (Terrain terrain in touched)
                    treesAfter += terrain.terrainData.treeInstanceCount;
                int newMeshes = 0, triangles = 0;
                foreach (MeshFilter filter in UnityEngine.Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include))
                {
                    if (meshesBefore.Contains(filter) || filter.sharedMesh == null)
                        continue;
                    newMeshes++;
                    triangles += filter.sharedMesh.triangles.Length / 3;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Baked road '{roadName}': {positions.Length} nodes, {timer.ElapsedMilliseconds} ms.");
                sb.AppendLine($"Terrain trees {treesBefore} -> {treesAfter} ({treesBefore - treesAfter} cleared). New meshes: {newMeshes} ({triangles} triangles).");
                if (newMeshes == 0)
                    sb.AppendLine("** No road mesh appeared -- the terrain WAS carved. Check the spline's 'Road' extension. **");
                sb.AppendLine("The spline is now inactive (baked). Props already on the terrain were not moved by the carve.");
                return sb.ToString();
            });
        }
    }
}
#endif
