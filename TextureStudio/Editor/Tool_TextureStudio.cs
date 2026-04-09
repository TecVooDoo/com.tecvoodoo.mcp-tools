#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.TextureStudio.Editor
{
    [McpPluginToolType]
    public partial class Tool_TextureStudio
    {
        const string MAP_TYPE = "TextureStudio.CompositeMap";
        const string LAYER_TYPE = "TextureStudio.Layer";
        const string MANAGER_TYPE = "TextureStudio.MapManager";

        static Type? FindType(string fullTypeName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullTypeName);
                if (t != null) return t;
            }
            return null;
        }

        static UnityEngine.Object GetMap(string assetName)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"{assetName} t:ScriptableObject");
            var mapType = FindType(MAP_TYPE);
            if (mapType == null) throw new Exception("CompositeMap type not found.");

            foreach (string guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, mapType);
                if (asset != null) return asset;
            }
            throw new Exception($"CompositeMap '{assetName}' not found. Search for ScriptableObjects of type CompositeMap.");
        }

        static object? Get(object target, string name)
        {
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(target);
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(target);
            return null;
        }

        static bool Set(object target, string name, object value)
        {
            var type = target.GetType();
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, Convert.ChangeType(value, field.FieldType)); return true; }
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, Convert.ChangeType(value, prop.PropertyType)); return true; }
            return false;
        }

        static object? Call(object target, string methodName, params object[] args)
        {
            var type = target.GetType();
            Type[] argTypes = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
                argTypes[i] = args[i].GetType();

            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, argTypes, null);
            if (method == null)
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
                throw new Exception($"Method '{methodName}' not found on {type.Name}.");
            return method.Invoke(target, args.Length > 0 ? args : null);
        }

        static IList? GetAllLayers(object map)
        {
            return Call(map, "GetAllLayers") as IList;
        }
    }
}
