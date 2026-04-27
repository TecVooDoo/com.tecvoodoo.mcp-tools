#nullable enable
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.CityGen3D.Editor
{
    [McpPluginToolType]
    public partial class Tool_CityGen3D
    {
        // CityGen3D ships as DLLs (CityGen3D.dll runtime + CityGen3D.EditorExtension.dll editor) under
        // Assets/CityGen3D/Plugins/. Public types confirmed from user-facing scripts in the asset:
        //   CityGen3D.Map (singleton: Map.Instance)
        //   Map.Instance.mapRoads.GetMapRoadAtWorldPosition(x, z, radius) -> MapRoad
        //   Map.Instance.mapBuildings
        //   Map.Instance.mapFeatures
        //   Map.Instance.data.GetOrigin()
        //   CityGen3D.EditorExtension.Generator (editor module orchestrator)

        static readonly Type? MapType = FindType("CityGen3D.Map");
        static readonly Type? GeneratorType = FindType("CityGen3D.EditorExtension.Generator");
        static readonly Type? MapRoadType = FindType("CityGen3D.MapRoad");
        static readonly Type? BlueprintType = FindType("CityGen3D.Blueprint");

        static Type? FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        static void RequireCityGen()
        {
            if (MapType == null)
                throw new Exception("CityGen3D.Map type not found. Is CityGen3D installed?");
        }

        static object? Get(object? target, string name)
        {
            if (target == null) return null;
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (prop != null) return prop.GetValue(target);
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (field != null) return field.GetValue(target);
            return null;
        }

        static object? GetStatic(Type type, string name)
        {
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Static);
            if (prop != null) return prop.GetValue(null);
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            if (field != null) return field.GetValue(null);
            return null;
        }

        static object? Call(object? target, string methodName, params object[] args)
        {
            if (target == null) return null;
            var argTypes = args.Select(a => a?.GetType() ?? typeof(object)).ToArray();
            var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, argTypes, null)
                         ?? target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (method == null) return null;
            return method.Invoke(target, args);
        }

        static object? MapInstance() => MapType != null ? GetStatic(MapType, "Instance") : null;

        /// <summary>
        /// Find the active Generator GameObject (CityGen3D.EditorExtension.Generator) in the scene.
        /// </summary>
        static UnityEngine.Component? FindGenerator()
        {
            if (GeneratorType == null) return null;
            return UnityEngine.Object.FindAnyObjectByType(GeneratorType) as UnityEngine.Component;
        }

        static int CountList(object? target, string name)
        {
            var v = Get(target, name);
            return v is ICollection col ? col.Count : 0;
        }
    }
}
