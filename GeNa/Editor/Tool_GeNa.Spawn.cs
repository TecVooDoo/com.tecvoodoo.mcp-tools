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
        [Description(@"Runs a GeNa spawner prefab at each of 'points' ('x,z' snapped to terrain, or 'x,y,z') -- GeNaSpawner.Spawn(Vector3), synchronous (ENTRY-434: a street lamp landed grounded at the exact XZ in ~0.5 s).
'spawner' = project path of a GeNa spawner prefab (e.g. '.../Spawner - Street Lamp.prefab'). Its scene instance is reused if one with the same name exists, otherwise instantiated.
Each call ADDS props -- it is not idempotent; check with gena-query.
26 of 52 GeNa sample materials are Built-in Standard and render MAGENTA on URP until Unity's Render Pipeline Converter runs on the GeNa folder.")]
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
                for (int i = 0; i < points.Length; i++)
                {
                    Vector3 position = ParsePoint(points[i], $"points[{i}]", 0f);
                    Invoke(instance, "Spawn", new[] { typeof(Vector3) }, position);
                    sb.AppendLine($"  [{i}] spawned at ({position.x:F1}, {position.y:F1}, {position.z:F1})");
                }
                sb.AppendLine($"spawnedSomething={Invoke(instance, "SpawnedSomething", Type.EmptyTypes)}");
                return sb.ToString();
            });
        }
    }
}
#endif
