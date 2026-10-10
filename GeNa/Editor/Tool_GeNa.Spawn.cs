#if HAS_GENA
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        [AiTool("gena-spawn", Title = "GeNa / Spawn")]
        [Description(@"Runs a GeNa spawner prefab at each of 'points' ('x,z' snapped to terrain, or 'x,y,z') -- GeNaSpawner.Spawn(Vector3), synchronous.
'spawner' = project path of a GeNa spawner prefab (e.g. '.../Spawner - Street Lamp.prefab'); its scene instance is reused if one with the same name exists.
Reports PER POINT: PLACED / NOTHING (GeNa silently REJECTS steep points -- a street lamp placed up to 11.2 deg, rejected from 16.8) and the slope.
FIND A SPOT: searchRadius > 0 moves each point whose slope exceeds maxSlope (default 11) to the flattest spot within that radius first. Road verges are steep after a carve; e.g. searchRadius=12 beside a road.
Each call ADDS props -- not idempotent; on an occupied point GeNa STACKS the new prop onto the old one's collider (flagged per point). 26 of 52 GeNa sample materials render MAGENTA on URP until the Render Pipeline Converter runs.")]
        public string Spawn(
            [Description("Project path of a GeNa spawner prefab.")] string spawner,
            [Description("Spawn positions: 'x,z' (snapped to terrain) or 'x,y,z'.")] string[] points,
            [Description("Search this far (metres) for a flatter spot when a point is steeper than maxSlope. Default 0 = spawn exactly at the point.")] float searchRadius = 0f,
            [Description("Slope limit (degrees) for the search. Default 11 (highest slope a street lamp was measured to accept).")] float maxSlope = 11f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (Application.isPlaying)
                    throw new Exception("Run GeNa spawners in edit mode.");
                if (points == null || points.Length == 0)
                    throw new Exception("'points' needs at least 1 entry.");

                GameObject? prefab = AssetDatabase.LoadAssetAtPath<GameObject>(spawner);
                if (prefab == null)
                    throw new Exception($"No prefab at '{spawner}'.");
                Type spawnerType = FindType(SpawnerTypeName);
                if (prefab.GetComponent(spawnerType) == null)
                    throw new Exception($"'{spawner}' has no GeNaSpawner component.");

                Component? instance = null;
                bool created = false;
                foreach (Component existing in FindAll(SpawnerTypeName))
                {
                    if (existing.gameObject.name == prefab.name)
                    {
                        instance = existing;
                        break;
                    }
                }
                if (instance == null)
                {
                    GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    Undo.RegisterCreatedObjectUndo(go, "Instantiate " + prefab.name);
                    instance = go.GetComponent(spawnerType);
                    created = true;
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Spawner '{instance!.gameObject.name}'{(created ? " (instantiated)" : " (reused)")}:");
                int placedTotal = 0;
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 position = ParsePoint(points[i], $"points[{i}]", 0f);
                    string moved = "";
                    if (searchRadius > 0f)
                    {
                        Vector3 requested = position;
                        position = FlattestNear(position, searchRadius, maxSlope);
                        float distance = Vector2.Distance(new Vector2(requested.x, requested.z), new Vector2(position.x, position.z));
                        if (distance > 0.01f)
                            moved = $" (moved {distance:F1} m from ({requested.x:F1}, {requested.z:F1}) to find a flatter spot)";
                    }
                    HashSet<Transform> before = SpawnedTransforms();
                    Invoke(instance, "Spawn", new[] { typeof(Vector3) }, position);
                    HashSet<Transform> after = SpawnedTransforms();
                    after.ExceptWith(before);
                    int placed = after.Count;
                    placedTotal += placed;
                    string slope = SlopeAt(position);
                    sb.AppendLine(placed > 0
                        ? $"  [{i}] PLACED {placed} object(s) at ({position.x:F1}, {position.y:F1}, {position.z:F1}){slope}{moved}{LandingCheck(after)}"
                        : $"  [{i}] NOTHING placed at ({position.x:F1}, {position.y:F1}, {position.z:F1}){slope}{moved} -- rejected by the spawner's criteria (often slope{(searchRadius > 0f ? "" : "; try searchRadius")})");
                }
                // SpawnedSomething() is sticky (true after any earlier success), so the per-point counts are the real answer.
                sb.AppendLine($"Placed {placedTotal} object(s) over {points.Length} point(s).");
                return sb.ToString();
            });
        }

        const string SpawnsContainer = "GeNa Spawns";

        /// <summary>Every object under every 'GeNa Spawns' container at ANY depth (inactive included), containers excluded.</summary>
        /// <remarks>
        /// Not just scene roots: on a terrain GeNa parents 'GeNa Spawns' UNDER the terrain it spawned on
        /// (measured SetDesign 2026-10-10: 'Gaia Terrains/Terrain_0_0.../GeNa Spawns/...'), so a root-only search always read 0.
        /// A set (not a count) so nested containers -- GeNa nests one under a prop it stacks onto -- are never double-counted.
        /// </remarks>
        static HashSet<Transform> SpawnedTransforms()
        {
            HashSet<Transform> result = new HashSet<Transform>();
            foreach (Transform t in UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (t.name != SpawnsContainer)
                    continue;
                foreach (Transform child in t.GetComponentsInChildren<Transform>(true))
                {
                    if (child != t)
                        result.Add(child);
                }
            }
            return result;
        }

        /// <summary>
        /// Flags a placement that did not land on the terrain. GeNa raycasts onto COLLIDERS, so spawning on an occupied
        /// point stacks the new prop on top of the old one (measured: a second street lamp 8.0 m up, on the first's BoxCollider).
        /// </summary>
        static string LandingCheck(HashSet<Transform> placed)
        {
            Transform? lowestProp = null;
            foreach (Transform t in placed)
            {
                if (t.name == SpawnsContainer || t.name.StartsWith("GeNa Spawner", StringComparison.Ordinal))
                    continue;
                Transform? parent = t.parent;
                if (parent != null && placed.Contains(parent) && parent.name != SpawnsContainer && !parent.name.StartsWith("GeNa Spawner", StringComparison.Ordinal))
                    continue; // only the placed roots
                if (lowestProp == null || t.position.y < lowestProp.position.y)
                    lowestProp = t;
            }
            if (lowestProp == null)
                return "";
            Terrain? terrain = TerrainAt(lowestProp.position);
            if (terrain == null)
                return $", landed at y {lowestProp.position.y:F2}";
            float ground = terrain.SampleHeight(lowestProp.position) + terrain.GetPosition().y;
            float above = lowestProp.position.y - ground;
            return Mathf.Abs(above) <= 0.5f
                ? $", grounded (y {lowestProp.position.y:F2})"
                : $" -- ** landed {above:F2} m {(above > 0f ? "ABOVE" : "BELOW")} the terrain (on another object's collider?) **";
        }

        /// <summary>
        /// The point itself if it is within maxSlope; otherwise the least-steep terrain point on a 2 m grid within
        /// radius, preferring the nearest one that passes. Snapped to the terrain surface.
        /// </summary>
        /// <remarks>
        /// SetDesign trial 2026-10-10: after a road carve both verges were 16-25 deg (30-68 at the summit), nothing
        /// 5-6 m out was under 10 deg, and the agent had to hand-write slope scans to place three lamps.
        /// </remarks>
        static Vector3 FlattestNear(Vector3 origin, float radius, float maxSlope)
        {
            Terrain? home = TerrainAt(origin);
            if (home == null || Steepness(home, origin) <= maxSlope)
                return origin;

            const float step = 2f;
            Vector3 best = origin;
            float bestSlope = Steepness(home, origin);
            float bestPassingDistance = float.MaxValue;
            bool foundPassing = false;
            for (float dz = -radius; dz <= radius; dz += step)
            {
                for (float dx = -radius; dx <= radius; dx += step)
                {
                    float distance = Mathf.Sqrt(dx * dx + dz * dz);
                    if (distance > radius)
                        continue;
                    Vector3 candidate = new Vector3(origin.x + dx, 0f, origin.z + dz);
                    Terrain? terrain = TerrainAt(candidate);
                    if (terrain == null)
                        continue;
                    float slope = Steepness(terrain, candidate);
                    if (slope <= maxSlope)
                    {
                        if (!foundPassing || distance < bestPassingDistance)
                        {
                            foundPassing = true;
                            bestPassingDistance = distance;
                            best = candidate;
                        }
                    }
                    else if (!foundPassing && slope < bestSlope)
                    {
                        bestSlope = slope;
                        best = candidate;
                    }
                }
            }
            Terrain? bestTerrain = TerrainAt(best);
            if (bestTerrain != null)
                best.y = bestTerrain.SampleHeight(best) + bestTerrain.GetPosition().y;
            return best;
        }

        static float Steepness(Terrain terrain, Vector3 position)
        {
            Vector3 local = position - terrain.GetPosition();
            Vector3 size = terrain.terrainData.size;
            return terrain.terrainData.GetSteepness(local.x / size.x, local.z / size.z);
        }

        static string SlopeAt(Vector3 position)
        {
            Terrain? terrain = TerrainAt(position);
            if (terrain == null)
                return ", no terrain";
            Vector3 local = position - terrain.GetPosition();
            Vector3 size = terrain.terrainData.size;
            return $", slope {terrain.terrainData.GetSteepness(local.x / size.x, local.z / size.z):F1} deg";
        }
    }
}
#endif
