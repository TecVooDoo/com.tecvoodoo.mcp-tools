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
using Debug = UnityEngine.Debug;

namespace MCPTools.GeNa.Editor
{
    public partial class Tool_GeNa
    {
        const string RoadResultKeyPrefix = "TMCP.GeNa.RoadResult.";
        const double SharedScanBudgetSeconds = 8.0;

        [AiTool("gena-road", Title = "GeNa / Road")]
        [Description(@"Builds and BAKES a GeNa road along 'points' ('x,z' snapped to terrain, or 'x,y,z'), via GeNa's 'Add Road Spline' menu item (carve, clear trees/details/colliders, texture, road mesh).
DESTRUCTIVE, no undo: carves the heightmap and deletes trees IN the TerrainData asset.
REFUSES a TerrainData another scene references (Save-As shares it; the road would carve every copy) unless allowSharedTerrainData=true. The scene scan is budgeted: in a big project the first call may answer 'scan incomplete' -- call again to continue.
IDEMPOTENT: if 'name' was already built, returns that build's result instead of carving again (the MCP client re-sends slow calls).
Bakes in GeNa's required order -- spline Bake() then the road extension's Bake(true); a plain scripted Bake leaves carved ground with NO road.
Leaves GeNa scaffolding (a 'GeNa' root, 'Baked_RoadMeshes'). Props on the terrain are not moved by the carve.")]
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
                    if (existing.gameObject.name != roadName)
                        continue;
                    // A re-sent call lands here after the first attempt built the road: hand back that result.
                    string previous = SessionState.GetString(RoadResultKeyPrefix + roadName, "");
                    if (previous.Length > 0)
                        return "(Already built by an earlier attempt of this call -- not carved again.)\n" + previous;
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
                        List<string> others = OtherScenesUsing(terrain.terrainData, SharedScanBudgetSeconds, out int scanned, out int total);
                        if (others.Count > 0)
                            throw new Exception($"REFUSED: terrain '{terrain.name}' uses TerrainData '{AssetDatabase.GetAssetPath(terrain.terrainData)}', which {others.Count} other scene(s) also reference ({string.Join(", ", others)}). " +
                                                "The bake would carve all of them. Duplicate the TerrainData for this scene first, or pass allowSharedTerrainData=true.");
                        if (scanned < total)
                            return $"Shared-TerrainData scan incomplete: {scanned} of {total} scenes checked within the {SharedScanBudgetSeconds:F0} s budget, none sharing so far. NOTHING was built. " +
                                   "Call gena-road again with the same arguments to continue the scan (progress is kept), or pass allowSharedTerrainData=true if you know the TerrainData is this scene's own.";
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
                object? previousNode = null;
                foreach (Vector3 position in positions)
                {
                    object? node = Invoke(spline, "CreateNewNode", createSignature, position, null, uint.MaxValue);
                    if (previousNode == null)
                        Invoke(spline, "AddNode", new[] { nodeType }, node);
                    else
                        Invoke(spline, "AddCurve", new[] { nodeType, nodeType, typeof(bool) }, previousNode, node, true);
                    previousNode = node;
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
                string summary = sb.ToString();

                // Kept and logged so a reply lost to an MCP re-send is still recoverable (SetDesign 2026-10-10).
                SessionState.SetString(RoadResultKeyPrefix + roadName, summary);
                Debug.Log("[TMCP gena-road] " + summary);
                return summary;
            });
        }
    }
}
#endif
