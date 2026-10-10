#if HAS_GENA
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using Component = UnityEngine.Component;

namespace MCPTools.GeNa.Editor
{
    public partial class Tool_GeNa
    {
        [AiTool("gena-query", Title = "GeNa / Query", ReadOnlyHint = true)]
        [Description(@"Lists GeNa Pro splines and spawners in the open scenes (read-only).
Splines: name, active (a baked spline is INACTIVE), node and curve counts, extension count, and whether a road extension has baked road meshes.
Spawners: name and whether they have spawned anything. Includes inactive objects.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                StringBuilder sb = new StringBuilder();
                Type roadType = FindType(RoadExtensionTypeName);

                List<Component> splines = FindAll(SplineTypeName);
                sb.AppendLine($"GeNa splines: {splines.Count}");
                foreach (Component spline in splines)
                {
                    object? road = GetExtension(spline, roadType);
                    string roadState = road == null ? "no road extension"
                        : (bool)Invoke(road, "HasBakedRoads", Type.EmptyTypes)! ? "road BAKED" : "road not baked";
                    sb.AppendLine($"  '{spline.gameObject.name}' active={spline.gameObject.activeInHierarchy}, nodes={Count(GetProperty(spline, "Nodes"))}, " +
                                  $"curves={Count(GetProperty(spline, "Curves"))}, extensions={Count(GetProperty(spline, "Extensions"))}, {roadState}");
                }

                List<Component> spawners = FindAll(SpawnerTypeName);
                sb.AppendLine($"GeNa spawners: {spawners.Count}");
                foreach (Component spawner in spawners)
                    sb.AppendLine($"  '{spawner.gameObject.name}' spawnedSomething={Invoke(spawner, "SpawnedSomething", Type.EmptyTypes)}");
                return sb.ToString();
            });
        }
    }
}
#endif
