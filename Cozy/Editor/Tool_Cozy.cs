#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using com.IvanMurzak.McpPlugin;
using DistantLands.Cozy;
using DistantLands.Cozy.Data;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Cozy.Editor
{
    [McpPluginToolType]
    public partial class Tool_Cozy
    {
        static CozyWeather GetWeather(string? gameObjectName)
        {
            if (!string.IsNullOrEmpty(gameObjectName))
            {
                var go = GameObject.Find(gameObjectName);
                if (go == null)
                    throw new Exception($"GameObject '{gameObjectName}' not found.");
                var cw = go.GetComponent<CozyWeather>();
                if (cw == null)
                    throw new Exception($"'{gameObjectName}' has no CozyWeather component.");
                return cw;
            }

            var instance = CozyWeather.instance;
            if (instance == null)
                throw new Exception("No CozyWeather instance in scene. Pass gameObjectName or add a Cozy weather sphere first.");
            return instance;
        }

        static CozyBiome GetBiome(string gameObjectName)
        {
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new Exception($"GameObject '{gameObjectName}' not found.");
            var biome = go.GetComponent<CozyBiome>();
            if (biome == null)
                throw new Exception($"'{gameObjectName}' has no CozyBiome component.");
            return biome;
        }

        static WeatherProfile FindWeatherProfile(string nameOrPath)
        {
            // Path lookup wins over name lookup if it points to an asset
            if (nameOrPath.Contains('/') || nameOrPath.EndsWith(".asset"))
            {
                var byPath = AssetDatabase.LoadAssetAtPath<WeatherProfile>(nameOrPath);
                if (byPath != null) return byPath;
            }

            var guids = AssetDatabase.FindAssets($"t:WeatherProfile {System.IO.Path.GetFileNameWithoutExtension(nameOrPath)}");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<WeatherProfile>(path);
                if (asset == null) continue;
                if (string.Equals(asset.name, System.IO.Path.GetFileNameWithoutExtension(nameOrPath), StringComparison.OrdinalIgnoreCase))
                    return asset;
            }

            // Fall back: first match
            if (guids.Length > 0)
            {
                var asset = AssetDatabase.LoadAssetAtPath<WeatherProfile>(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (asset != null) return asset;
            }

            throw new Exception($"WeatherProfile '{nameOrPath}' not found in project.");
        }

        static List<string> ListWeatherProfileNames()
        {
            var names = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:WeatherProfile"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var name = System.IO.Path.GetFileNameWithoutExtension(path);
                names.Add(name);
            }
            names.Sort(StringComparer.OrdinalIgnoreCase);
            return names;
        }

        static Type? FindModuleType(string moduleTypeName)
        {
            // Accept short ("CozyClimateModule"), short-no-prefix ("ClimateModule"/"Climate"), or full name
            string[] candidates =
            {
                moduleTypeName,
                $"DistantLands.Cozy.{moduleTypeName}",
                $"DistantLands.Cozy.Cozy{moduleTypeName}",
                $"DistantLands.Cozy.Cozy{moduleTypeName}Module",
            };

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var name in candidates)
                {
                    var t = asm.GetType(name);
                    if (t != null && typeof(CozyModule).IsAssignableFrom(t))
                        return t;
                }
            }

            // Last resort: scan all CozyModule subclasses for short-name match (case-insensitive)
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch { continue; }

                foreach (var t in types)
                {
                    if (!typeof(CozyModule).IsAssignableFrom(t)) continue;
                    if (t.IsAbstract || t.IsGenericTypeDefinition) continue;
                    if (string.Equals(t.Name, moduleTypeName, StringComparison.OrdinalIgnoreCase)) return t;
                }
            }

            return null;
        }

        static List<Type> ListModuleTypes()
        {
            var list = new List<Type>();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch { continue; }

                foreach (var t in types)
                {
                    if (!typeof(CozyModule).IsAssignableFrom(t)) continue;
                    if (t.IsAbstract || t.IsGenericTypeDefinition) continue;
                    list.Add(t);
                }
            }
            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return list;
        }
    }
}
