#nullable enable
using System;
using Animancer;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.Animancer.Editor
{
    [McpPluginToolType]
    public partial class Tool_Animancer
    {
        static GameObject FindGO(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) throw new Exception($"GameObject '{name}' not found.");
            return go;
        }

        static AnimancerComponent GetAnimancer(string gameObjectName)
        {
            var go = FindGO(gameObjectName);
            var ac = go.GetComponent<AnimancerComponent>();
            if (ac == null) throw new Exception($"'{gameObjectName}' has no AnimancerComponent.");
            return ac;
        }

        static AnimationClip FindClip(string clipName)
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets($"{clipName} t:AnimationClip");
            if (guids.Length == 0)
                throw new Exception($"AnimationClip '{clipName}' not found in AssetDatabase.");
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            var clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
                throw new Exception($"Failed to load AnimationClip at '{path}'.");
            return clip;
        }
    }
}
