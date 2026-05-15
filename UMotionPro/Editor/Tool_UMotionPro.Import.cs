#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UMotionEditor.API;
using UnityEditor;
using UnityEngine;

namespace MCPTools.UMotionPro.Editor
{
    public partial class Tool_UMotionPro
    {
        [McpPluginTool("umotion-import", Title = "UMotion Pro / Import AnimationClips")]
        [Description(@"Imports AnimationClips into the loaded UMotion project via ClipEditor.ImportClips. Sources: clipPaths (.anim asset paths), fbxPaths (FBX paths — extracts every AnimationClip sub-asset), clipNames (short names resolved via AssetDatabase.FindAssets). Any combo accepted; at least one must yield a clip. Flag params mirror UMotion's clip-import dialog and override ImportClipSettings.Default when non-null: convertToProgressive, disableAnimCompression, fkToIkConversion, fkToIkDeleteFkKeys, humanoidHandIKEnable, humanoidLosslessKeyframeReduction. Requires ClipEditor window open + project loaded. Blocks until import finishes.")]
        public string Import(
            [Description("Asset paths to AnimationClip (.anim) assets to import. May be empty.")]
            string[]? clipPaths = null,
            [Description("Asset paths to FBX files. The tool extracts every AnimationClip sub-asset from each FBX. May be empty.")]
            string[]? fbxPaths = null,
            [Description("Short clip names to look up via AssetDatabase.FindAssets. May be empty.")]
            string[]? clipNames = null,
            [Description("ImportClipSettings.ConvertToProgressive (default false).")]
            bool? convertToProgressive = null,
            [Description("ImportClipSettings.DisableAnimCompression (default false).")]
            bool? disableAnimCompression = null,
            [Description("ImportClipSettings.FkToIkConversion (default false). Requires the project's IK rigs to be set up.")]
            bool? fkToIkConversion = null,
            [Description("ImportClipSettings.FkToIkDeleteFkKeys (default false).")]
            bool? fkToIkDeleteFkKeys = null,
            [Description("ImportClipSettings.HumanoidHandIKEnable (default false).")]
            bool? humanoidHandIKEnable = null,
            [Description("ImportClipSettings.HumanoidLosslessKeyframeReduction (default false).")]
            bool? humanoidLosslessKeyframeReduction = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireProjectLoaded();

                var clips = new List<AnimationClip>();
                var sb = new System.Text.StringBuilder();

                if (clipPaths != null)
                {
                    foreach (string p in clipPaths)
                    {
                        if (string.IsNullOrEmpty(p)) continue;
                        var c = AssetDatabase.LoadAssetAtPath<AnimationClip>(p);
                        if (c == null)
                            throw new Exception($"AnimationClip not found at '{p}'.");
                        clips.Add(c);
                        sb.AppendLine($"  + clip @ {p} -> '{c.name}'");
                    }
                }

                if (fbxPaths != null)
                {
                    foreach (string p in fbxPaths)
                    {
                        if (string.IsNullOrEmpty(p)) continue;
                        UnityEngine.Object[] subs = AssetDatabase.LoadAllAssetsAtPath(p);
                        if (subs == null || subs.Length == 0)
                            throw new Exception($"No assets loaded at '{p}'.");
                        int fbxClipCount = 0;
                        foreach (var s in subs)
                        {
                            if (s is AnimationClip clip && !clip.name.StartsWith("__preview__"))
                            {
                                clips.Add(clip);
                                fbxClipCount++;
                                sb.AppendLine($"  + fbx-clip {p}#'{clip.name}'");
                            }
                        }
                        if (fbxClipCount == 0)
                            sb.AppendLine($"  (no AnimationClip sub-assets in '{p}')");
                    }
                }

                if (clipNames != null)
                {
                    foreach (string name in clipNames)
                    {
                        if (string.IsNullOrEmpty(name)) continue;
                        string[] guids = AssetDatabase.FindAssets($"{name} t:AnimationClip");
                        if (guids.Length == 0)
                            throw new Exception($"No AnimationClip matching '{name}' found in AssetDatabase.");
                        foreach (string g in guids)
                        {
                            string p = AssetDatabase.GUIDToAssetPath(g);
                            var c = AssetDatabase.LoadAssetAtPath<AnimationClip>(p);
                            if (c != null && c.name == name)
                            {
                                clips.Add(c);
                                sb.AppendLine($"  + name='{name}' -> {p}");
                            }
                        }
                    }
                }

                if (clips.Count == 0)
                    throw new Exception("No clips resolved from clipPaths / fbxPaths / clipNames.");

                ClipEditor.ImportClipSettings settings = ClipEditor.ImportClipSettings.Default;
                if (convertToProgressive.HasValue) settings.ConvertToProgressive = convertToProgressive.Value;
                if (disableAnimCompression.HasValue) settings.DisableAnimCompression = disableAnimCompression.Value;
                if (fkToIkConversion.HasValue) settings.FkToIkConversion = fkToIkConversion.Value;
                if (fkToIkDeleteFkKeys.HasValue) settings.FkToIkDeleteFkKeys = fkToIkDeleteFkKeys.Value;
                if (humanoidHandIKEnable.HasValue) settings.HumanoidHandIKEnable = humanoidHandIKEnable.Value;
                if (humanoidLosslessKeyframeReduction.HasValue) settings.HumanoidLosslessKeyframeReduction = humanoidLosslessKeyframeReduction.Value;

                ClipEditor.ImportClips(clips, settings);

                return $"OK: ImportClips({clips.Count} clip(s)) completed.\nClips:\n{sb}";
            });
        }
    }
}
