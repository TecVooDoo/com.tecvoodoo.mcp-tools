#if HAS_ULIPSYNC
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.uLipSync.Editor
{
    public partial class Tool_uLipSync
    {
        [McpPluginTool("lipsync-bake", Title = "uLipSync / Bake AudioClips to BakedData")]
        [Description(@"Bakes AudioClip(s) into uLipSync BakedData assets using a calibrated Profile.
Can bake a single clip by name, or batch-bake all AudioClips in a directory.
The output BakedData assets can be used with uLipSyncBakedDataPlayer or Timeline tracks.
This is the MCP equivalent of Window > uLipSync > Baked Data Generator.")]
        public string BakeLipSyncData(
            [Description("Name of the Profile asset to use for baking. Must be a calibrated Profile.")]
            string profileName,
            [Description("Name of a single AudioClip asset to bake. Mutually exclusive with inputDirectory.")]
            string? audioClipName = null,
            [Description("Asset directory path containing AudioClips to batch-bake (e.g. 'Assets/_M3/Audio/Voice/Episode1'). Mutually exclusive with audioClipName.")]
            string? inputDirectory = null,
            [Description("Asset directory path for output BakedData assets (e.g. 'Assets/_M3/Audio/BakedData'). Created if it doesn't exist. Default: same directory as input clip(s).")]
            string? outputDirectory = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (string.IsNullOrEmpty(audioClipName) && string.IsNullOrEmpty(inputDirectory))
                    throw new Exception("Provide either audioClipName or inputDirectory.");
                if (!string.IsNullOrEmpty(audioClipName) && !string.IsNullOrEmpty(inputDirectory))
                    throw new Exception("Provide audioClipName OR inputDirectory, not both.");

                // Find profile
                UnityEngine.Object? profileAsset = null;
                string[] profileGuids = AssetDatabase.FindAssets($"{profileName} t:uLipSync.Profile");
                foreach (string guid in profileGuids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    UnityEngine.Object? candidate = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                    if (candidate != null && candidate.name == profileName)
                    {
                        profileAsset = candidate;
                        break;
                    }
                }
                if (profileAsset == null)
                    throw new Exception($"Profile '{profileName}' not found.");

                // Collect AudioClips
                List<AudioClip> clips = new List<AudioClip>();
                if (!string.IsNullOrEmpty(audioClipName))
                {
                    string[] clipGuids = AssetDatabase.FindAssets($"{audioClipName} t:AudioClip");
                    foreach (string guid in clipGuids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        AudioClip? clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                        if (clip != null && clip.name == audioClipName)
                        {
                            clips.Add(clip);
                            break;
                        }
                    }
                    if (clips.Count == 0)
                        throw new Exception($"AudioClip '{audioClipName}' not found.");
                }
                else if (!string.IsNullOrEmpty(inputDirectory))
                {
                    string[] clipGuids = AssetDatabase.FindAssets("t:AudioClip", new string[] { inputDirectory });
                    foreach (string guid in clipGuids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        AudioClip? clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                        if (clip != null)
                            clips.Add(clip);
                    }
                    if (clips.Count == 0)
                        throw new Exception($"No AudioClips found in '{inputDirectory}'.");
                }

                // Resolve output directory
                string outDir = outputDirectory ?? "";
                if (string.IsNullOrEmpty(outDir))
                {
                    if (!string.IsNullOrEmpty(inputDirectory))
                        outDir = inputDirectory;
                    else if (clips.Count > 0)
                    {
                        string clipPath = AssetDatabase.GetAssetPath(clips[0]);
                        outDir = Path.GetDirectoryName(clipPath)?.Replace('\\', '/') ?? "Assets";
                    }
                }

                // Create output directory if needed
                if (!AssetDatabase.IsValidFolder(outDir))
                {
                    string[] parts = outDir.Split('/');
                    string current = parts[0]; // "Assets"
                    for (int i = 1; i < parts.Length; i++)
                    {
                        string next = current + "/" + parts[i];
                        if (!AssetDatabase.IsValidFolder(next))
                            AssetDatabase.CreateFolder(current, parts[i]);
                        current = next;
                    }
                }

                // Get BakedData type and BakedDataEditor type
                Type? bakedDataType = BakedDataType;
                if (bakedDataType == null)
                    throw new Exception("BakedData type not found.");

                Type? bakedDataEditorType = null;
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    bakedDataEditorType = asm.GetType("uLipSync.BakedDataEditor");
                    if (bakedDataEditorType != null) break;
                }
                if (bakedDataEditorType == null)
                    throw new Exception("BakedDataEditor type not found.");

                MethodInfo? bakeMethod = bakedDataEditorType.GetMethod("Bake", BindingFlags.Public | BindingFlags.Instance);
                if (bakeMethod == null)
                    throw new Exception("BakedDataEditor.Bake() method not found.");

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Baking {clips.Count} clip(s) with profile '{profileName}'...");
                int successCount = 0;

                foreach (AudioClip clip in clips)
                {
                    try
                    {
                        ScriptableObject data = ScriptableObject.CreateInstance(bakedDataType) as ScriptableObject
                            ?? throw new Exception("Failed to create BakedData instance.");

                        // Set profile and audioClip
                        FieldInfo? profileField = bakedDataType.GetField("profile", BindingFlags.Public | BindingFlags.Instance);
                        FieldInfo? audioClipField = bakedDataType.GetField("audioClip", BindingFlags.Public | BindingFlags.Instance);
                        profileField?.SetValue(data, profileAsset);
                        audioClipField?.SetValue(data, clip);
                        data.name = clip.name;

                        // Create editor and bake
                        UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(data, bakedDataEditorType);
                        bakeMethod.Invoke(editor, null);
                        UnityEngine.Object.DestroyImmediate(editor);

                        // Save asset
                        string assetPath = outDir + "/" + clip.name + ".asset";
                        AssetDatabase.CreateAsset(data, assetPath);

                        object? duration = Get(data, "duration");
                        sb.AppendLine($"  OK: {clip.name} -> {assetPath} ({duration}s)");
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"  FAIL: {clip.name} -- {ex.Message}");
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                sb.AppendLine($"\nBaked {successCount}/{clips.Count} clips to '{outDir}'.");
                return sb.ToString();
            });
        }
    }
}
#endif
