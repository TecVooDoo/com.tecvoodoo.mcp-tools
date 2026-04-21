#nullable enable
using System;
using com.IvanMurzak.McpPlugin;
using MK.EdgeDetection;
using MK.EdgeDetection.PostProcessing.Generic;
using MK.EdgeDetection.UniversalVolumeComponents;
using MK.EdgeDetection.UniversalRendererFeatures;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MCPTools.MKEdge.Editor
{
    [McpPluginToolType]
    public partial class Tool_MKEdge
    {
        // Returns either the URP VolumeComponent or RendererFeature variant
        // Throws if neither is found.
        static (MKEdgeDetection? volComp, MKEdgeDetectionRendererFeature? feature) FindMKEdge(string targetName)
        {
            // Try GameObject.Find for Volume host
            var go = GameObject.Find(targetName);
            if (go != null)
            {
                var volume = go.GetComponent<Volume>();
                if (volume != null && volume.profile != null && volume.profile.TryGet<MKEdgeDetection>(out var vc))
                    return (vc, null);
            }

            // Try VolumeProfile asset by name
            string[] profileGuids = UnityEditor.AssetDatabase.FindAssets($"{targetName} t:VolumeProfile");
            foreach (var guid in profileGuids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var profile = UnityEditor.AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
                if (profile != null && profile.TryGet<MKEdgeDetection>(out var vc))
                    return (vc, null);
            }

            // Try Universal Renderer Data assets for the RendererFeature
            string[] rendererGuids = UnityEditor.AssetDatabase.FindAssets("t:UniversalRendererData");
            foreach (var guid in rendererGuids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var data = UnityEditor.AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
                if (data == null) continue;
                if (!path.Contains(targetName) && data.name != targetName) continue;
                foreach (var f in data.rendererFeatures)
                {
                    if (f is MKEdgeDetectionRendererFeature mkf) return (null, mkf);
                }
            }

            // Try any UniversalRendererData containing a feature with matching name
            foreach (var guid in rendererGuids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var data = UnityEditor.AssetDatabase.LoadAssetAtPath<UniversalRendererData>(path);
                if (data == null) continue;
                foreach (var f in data.rendererFeatures)
                {
                    if (f is MKEdgeDetectionRendererFeature mkf && (mkf.name == targetName || data.name == targetName))
                        return (null, mkf);
                }
            }

            throw new Exception($"MK Edge Detection target '{targetName}' not found. Provide a Volume GameObject name, a VolumeProfile asset name, or a UniversalRendererData name with an MKEdgeDetection feature.");
        }
    }
}
