#if HAS_GENA
#nullable enable
using System;
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
Reports PER POINT whether anything was actually placed (objects counted under the 'GeNa Spawns' root) and the terrain slope there: GeNa's spawn criteria silently REJECT points (measured: a street lamp placed at 3 deg, nothing at 17 or 30 deg, no message).
Each call ADDS props -- not idempotent. 26 of 52 GeNa sample materials render MAGENTA on URP until the Render Pipeline Converter runs.")]
        public string Spawn(
            [Description("Project path of a GeNa spawner prefab.")] string spawner,
            [Description("Spawn positions: 'x,z' (snapped to terrain) or 'x,y,z'.")] string[] points
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
                    int before = SpawnedObjectCount();
                    Invoke(instance, "Spawn", new[] { typeof(Vector3) }, position);
                    int placed = SpawnedObjectCount() - before;
                    placedTotal += Math.Max(0, placed);
                    string slope = SlopeAt(position);
                    sb.AppendLine(placed > 0
                        ? $"  [{i}] PLACED {placed} object(s) at ({position.x:F1}, {position.y:F1}, {position.z:F1}){slope}"
                        : $"  [{i}] NOTHING placed at ({position.x:F1}, {position.y:F1}, {position.z:F1}){slope} -- rejected by the spawner's criteria (often slope)");
                }
                // SpawnedSomething() is sticky (true after any earlier success), so the per-point counts are the real answer.
                sb.AppendLine($"Placed {placedTotal} object(s) over {points.Length} point(s).");
                return sb.ToString();
            });
        }

        /// <summary>Objects under GeNa's 'GeNa Spawns' output root (inactive included); 0 when it does not exist yet.</summary>
        static int SpawnedObjectCount()
        {
            int count = 0;
            for (int s = 0; s < UnityEngine.SceneManagement.SceneManager.sceneCount; s++)
            {
                UnityEngine.SceneManagement.Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(s);
                if (!scene.isLoaded)
                    continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    if (root.name == "GeNa Spawns")
                        count += root.GetComponentsInChildren<Transform>(true).Length - 1;
                }
            }
            return count;
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
