#if HAS_GENA
#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MCPTools.GeNa.Editor
{
    /// <summary>
    /// GeNa Pro (Procedural Worlds) tool group, driven by reflection: the spline/spawner core is a
    /// precompiled DLL and the road extension is asmdef-less source in Assembly-CSharp (Sandbox ENTRY-434).
    /// </summary>
    /// <remarks>
    /// Measured rules this group encodes (ENTRY-434): GeNaSpline.Bake() applies the DESTRUCTIVE terrain
    /// half (carve / clear trees / texture) but builds NO road mesh from a script, because meshes are built
    /// on an editor re-process that an MCP call never triggers -- GeNaRoadExtension.Bake(true) must follow;
    /// road splines write straight into the TerrainData asset, which duplicated (Save As) scenes share, so
    /// the road tool refuses a TerrainData another scene references; baked splines go inactive, so every
    /// search includes inactive objects.
    /// </remarks>
    [AiToolType]
    public partial class Tool_GeNa
    {
        const string SplineTypeName = "GeNa.Core.GeNaSpline";
        const string SpawnerTypeName = "GeNa.Core.GeNaSpawner";
        const string RoadExtensionTypeName = "GeNa.Core.GeNaRoadExtension";
        const BindingFlags Instance = BindingFlags.Public | BindingFlags.Instance;

        static Type FindType(string fullName)
        {
#if UNITY_6000_4_OR_NEWER
            IReadOnlyList<Assembly> assemblies = UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies();
#else
            IReadOnlyList<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
#endif
            for (int i = 0; i < assemblies.Count; i++)
            {
                Type? t = assemblies[i].GetType(fullName);
                if (t != null)
                    return t;
            }
            throw new Exception($"Type '{fullName}' not found -- is GeNa Pro installed?");
        }

        static List<Component> FindAll(string typeName)
        {
            List<Component> result = new List<Component>();
            foreach (UnityEngine.Object found in UnityEngine.Object.FindObjectsByType(FindType(typeName), FindObjectsInactive.Include))
            {
                if (found is Component component)
                    result.Add(component);
            }
            return result;
        }

        static object? GetProperty(object target, string name)
        {
            PropertyInfo? property = target.GetType().GetProperty(name, Instance);
            if (property == null)
                throw new Exception($"GeNa property '{name}' not found on {target.GetType().Name} -- API changed?");
            return property.GetValue(target);
        }

        static object? Invoke(object target, string name, Type[] signature, params object?[] args)
        {
            MethodInfo? method = target.GetType().GetMethod(name, Instance, null, signature, null);
            if (method == null)
                throw new Exception($"GeNa method '{name}' not found on {target.GetType().Name} -- API changed?");
            return method.Invoke(target, args);
        }

        /// <summary>GeNaSpline.GetExtension&lt;T&gt;() for a runtime type.</summary>
        static object? GetExtension(Component spline, Type extensionType)
        {
            foreach (MethodInfo method in spline.GetType().GetMethods(Instance))
            {
                if (method.Name == "GetExtension" && method.IsGenericMethodDefinition && method.GetParameters().Length == 0)
                    return method.MakeGenericMethod(extensionType).Invoke(spline, null);
            }
            throw new Exception("GeNa method 'GetExtension<T>' not found -- API changed?");
        }

        static int Count(object? list)
        {
            return list is ICollection collection ? collection.Count : 0;
        }

        /// <summary>Parses 'x,y,z', or 'x,z' snapped to the terrain surface (+ yOffset).</summary>
        static Vector3 ParsePoint(string text, string role, float yOffset)
        {
            string[] parts = text.Split(',');
            float[] values = new float[parts.Length];
            for (int i = 0; i < parts.Length; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out values[i]))
                    throw new Exception($"{role} '{text}' is not 'x,z' or 'x,y,z'.");
            }
            if (parts.Length == 3)
                return new Vector3(values[0], values[1], values[2]);
            if (parts.Length != 2)
                throw new Exception($"{role} '{text}' is not 'x,z' or 'x,y,z'.");

            Vector3 flat = new Vector3(values[0], 0f, values[1]);
            Terrain? terrain = TerrainAt(flat);
            if (terrain == null)
                throw new Exception($"{role} '{text}': no terrain under it to snap to. Give 'x,y,z'.");
            return new Vector3(flat.x, terrain.SampleHeight(flat) + terrain.GetPosition().y + yOffset, flat.z);
        }

        static Terrain? TerrainAt(Vector3 position)
        {
            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                if (terrain == null || terrain.terrainData == null)
                    continue;
                Vector3 origin = terrain.GetPosition();
                Vector3 size = terrain.terrainData.size;
                if (position.x >= origin.x && position.z >= origin.z && position.x <= origin.x + size.x && position.z <= origin.z + size.z)
                    return terrain;
            }
            return null;
        }

        /// <summary>Other scene assets that reference this TerrainData (ENTRY-434: Save-As shares it, so a road would carve them all).</summary>
        static List<string> OtherScenesUsing(TerrainData data)
        {
            List<string> result = new List<string>();
            string dataPath = AssetDatabase.GetAssetPath(data);
            if (string.IsNullOrEmpty(dataPath))
                return result;
            string activeScene = SceneManager.GetActiveScene().path;
            foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
            {
                string scenePath = AssetDatabase.GUIDToAssetPath(guid);
                if (scenePath == activeScene)
                    continue;
                foreach (string dependency in AssetDatabase.GetDependencies(scenePath, false))
                {
                    if (dependency == dataPath)
                    {
                        result.Add(scenePath);
                        break;
                    }
                }
            }
            return result;
        }
    }
}
#endif
