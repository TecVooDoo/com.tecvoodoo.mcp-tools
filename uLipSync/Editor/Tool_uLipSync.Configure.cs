#if HAS_ULIPSYNC
#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
        [McpPluginTool("lipsync-configure", Title = "uLipSync / Configure Components")]
        [Description(@"Configures uLipSync components on a GameObject.
Can set the Profile on the uLipSync analyzer, configure uLipSyncBlendShape mappings
(phoneme-to-blendshape-index pairs), and adjust volume/smoothness parameters.
For blendshape mappings, provide phonemes and blendShapeNames as comma-separated lists
(e.g. phonemes='A,I,U,E,O' blendShapeNames='viseme_aa,viseme_ih,viseme_ou,viseme_E,viseme_oh').")]
        public string ConfigureLipSync(
            [Description("Name of the GameObject with uLipSync components.")]
            string gameObjectName,
            [Description("Name of the Profile asset to assign to the uLipSync analyzer. Must match an existing Profile asset name.")]
            string? profileName = null,
            [Description("(BlendShape) Comma-separated phoneme names (e.g. 'A,I,U,E,O,N,-'). Must match phonemes in the Profile.")]
            string? phonemes = null,
            [Description("(BlendShape) Comma-separated blendshape names on the SkinnedMeshRenderer (e.g. 'viseme_aa,viseme_ih,viseme_ou,viseme_E,viseme_oh,viseme_nn,viseme_sil'). Must match count of phonemes.")]
            string? blendShapeNames = null,
            [Description("(BlendShape) Name of the SkinnedMeshRenderer GameObject to target. If not set, uses the existing assignment.")]
            string? skinnedMeshRendererName = null,
            [Description("(BlendShape) Minimum volume threshold (log10). Default: -2.5")]
            float? minVolume = null,
            [Description("(BlendShape) Maximum volume threshold (log10). Default: -1.5")]
            float? maxVolume = null,
            [Description("(BlendShape) Mouth response smoothness [0-0.3]. Default: 0.05")]
            float? smoothness = null,
            [Description("(BlendShape) Use phoneme blend mode (weighted blend vs winner-take-all). Default: false")]
            bool? usePhonemeBlend = null,
            [Description("(BakedDataPlayer) Volume [0-1]. Default: 1")]
            float? playerVolume = null,
            [Description("(BakedDataPlayer) Time offset in seconds [-0.3 to 0.3]. Positive = mouth opens earlier. Default: 0.1")]
            float? timeOffset = null,
            [Description("(Analyzer) Output sound gain [0-1]. Set to 0 to mute playback while analyzing. Default: 1")]
            float? outputSoundGain = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject? go = GameObject.Find(gameObjectName);
                if (go == null)
                    throw new Exception($"GameObject '{gameObjectName}' not found.");

                StringBuilder sb = new StringBuilder();
                int changeCount = 0;

                // Configure uLipSync analyzer
                UnityEngine.Component? analyzer = GetComponent(go, ULipSyncType, "uLipSync");
                if (analyzer != null)
                {
                    if (profileName != null)
                    {
                        string[] guids = AssetDatabase.FindAssets($"{profileName} t:uLipSync.Profile");
                        UnityEngine.Object? profileAsset = null;
                        foreach (string guid in guids)
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
                            throw new Exception($"Profile asset '{profileName}' not found. Use lipsync-query with listAssets=true to see available profiles.");

                        FieldInfo? profileField = analyzer.GetType().GetField("profile", BindingFlags.Public | BindingFlags.Instance);
                        if (profileField != null)
                        {
                            profileField.SetValue(analyzer, profileAsset);
                            sb.AppendLine($"  uLipSync.profile = {profileName}");
                            changeCount++;
                        }
                    }

                    if (outputSoundGain.HasValue)
                    {
                        if (Set(analyzer, "outputSoundGain", outputSoundGain.Value))
                        {
                            sb.AppendLine($"  uLipSync.outputSoundGain = {outputSoundGain.Value}");
                            changeCount++;
                        }
                    }

                    EditorUtility.SetDirty(analyzer);
                }

                // Configure uLipSyncBlendShape
                UnityEngine.Component? blendShape = GetComponent(go, BlendShapeType, "uLipSyncBlendShape");
                if (blendShape != null)
                {
                    // Set SkinnedMeshRenderer if specified
                    if (skinnedMeshRendererName != null)
                    {
                        Transform? smrTransform = FindChildRecursive(go.transform, skinnedMeshRendererName);
                        if (smrTransform == null)
                            throw new Exception($"Child '{skinnedMeshRendererName}' not found under '{gameObjectName}'.");
                        SkinnedMeshRenderer? smr = smrTransform.GetComponent<SkinnedMeshRenderer>();
                        if (smr == null)
                            throw new Exception($"'{skinnedMeshRendererName}' has no SkinnedMeshRenderer.");

                        FieldInfo? smrField = blendShape.GetType().GetField("skinnedMeshRenderer", BindingFlags.Public | BindingFlags.Instance);
                        if (smrField != null)
                        {
                            smrField.SetValue(blendShape, smr);
                            sb.AppendLine($"  BlendShape.skinnedMeshRenderer = {smrTransform.name}");
                            changeCount++;
                        }
                    }

                    // Set blendshape mappings
                    if (phonemes != null && blendShapeNames != null)
                    {
                        string[] phonemeArr = phonemes.Split(',');
                        string[] bsNameArr = blendShapeNames.Split(',');
                        if (phonemeArr.Length != bsNameArr.Length)
                            throw new Exception($"Phoneme count ({phonemeArr.Length}) must match blendShapeNames count ({bsNameArr.Length}).");

                        // Get the SkinnedMeshRenderer
                        FieldInfo? smrField = blendShape.GetType().GetField("skinnedMeshRenderer", BindingFlags.Public | BindingFlags.Instance);
                        SkinnedMeshRenderer? renderer = smrField?.GetValue(blendShape) as SkinnedMeshRenderer;
                        if (renderer == null || renderer.sharedMesh == null)
                            throw new Exception("SkinnedMeshRenderer must be assigned before setting blendshape mappings.");

                        // Clear existing and rebuild
                        FieldInfo? bsListField = blendShape.GetType().GetField("blendShapes", BindingFlags.Public | BindingFlags.Instance);
                        if (bsListField == null)
                            throw new Exception("Cannot access blendShapes field.");

                        IList? bsList = bsListField.GetValue(blendShape) as IList;
                        if (bsList == null)
                            throw new Exception("blendShapes field is null.");

                        bsList.Clear();

                        // Get the BlendShapeInfo type
                        Type? bsInfoType = blendShape.GetType().GetNestedType("BlendShapeInfo");
                        if (bsInfoType == null)
                            throw new Exception("BlendShapeInfo type not found.");

                        for (int i = 0; i < phonemeArr.Length; i++)
                        {
                            string phoneme = phonemeArr[i].Trim();
                            string bsName = bsNameArr[i].Trim();

                            int bsIndex = -1;
                            Mesh mesh = renderer.sharedMesh;
                            for (int j = 0; j < mesh.blendShapeCount; j++)
                            {
                                if (mesh.GetBlendShapeName(j) == bsName)
                                {
                                    bsIndex = j;
                                    break;
                                }
                            }

                            object? bsInfo = Activator.CreateInstance(bsInfoType);
                            if (bsInfo != null)
                            {
                                FieldInfo? phonemeField = bsInfoType.GetField("phoneme");
                                FieldInfo? indexField = bsInfoType.GetField("index");
                                FieldInfo? maxWeightField = bsInfoType.GetField("maxWeight");
                                phonemeField?.SetValue(bsInfo, phoneme);
                                indexField?.SetValue(bsInfo, bsIndex);
                                maxWeightField?.SetValue(bsInfo, 1f);
                                bsList.Add(bsInfo);
                                sb.AppendLine($"  BlendShape mapping: {phoneme} -> [{bsIndex}] \"{bsName}\"{(bsIndex < 0 ? " (NOT FOUND)" : "")}");
                                changeCount++;
                            }
                        }
                    }

                    // Set scalar parameters
                    if (minVolume.HasValue && Set(blendShape, "minVolume", minVolume.Value))
                    {
                        sb.AppendLine($"  BlendShape.minVolume = {minVolume.Value}");
                        changeCount++;
                    }
                    if (maxVolume.HasValue && Set(blendShape, "maxVolume", maxVolume.Value))
                    {
                        sb.AppendLine($"  BlendShape.maxVolume = {maxVolume.Value}");
                        changeCount++;
                    }
                    if (smoothness.HasValue && Set(blendShape, "smoothness", smoothness.Value))
                    {
                        sb.AppendLine($"  BlendShape.smoothness = {smoothness.Value}");
                        changeCount++;
                    }
                    if (usePhonemeBlend.HasValue && Set(blendShape, "usePhonemeBlend", usePhonemeBlend.Value))
                    {
                        sb.AppendLine($"  BlendShape.usePhonemeBlend = {usePhonemeBlend.Value}");
                        changeCount++;
                    }

                    EditorUtility.SetDirty(blendShape);
                }

                // Configure uLipSyncBakedDataPlayer
                UnityEngine.Component? bakedPlayer = GetComponent(go, BakedDataPlayerType, "uLipSyncBakedDataPlayer");
                if (bakedPlayer != null)
                {
                    if (playerVolume.HasValue && Set(bakedPlayer, "volume", playerVolume.Value))
                    {
                        sb.AppendLine($"  BakedDataPlayer.volume = {playerVolume.Value}");
                        changeCount++;
                    }
                    if (timeOffset.HasValue && Set(bakedPlayer, "timeOffset", timeOffset.Value))
                    {
                        sb.AppendLine($"  BakedDataPlayer.timeOffset = {timeOffset.Value}");
                        changeCount++;
                    }

                    EditorUtility.SetDirty(bakedPlayer);
                }

                if (changeCount == 0)
                    return $"No changes applied on '{gameObjectName}'. Check that the target components exist and parameter names are correct.";

                return $"OK: uLipSync on '{gameObjectName}' updated ({changeCount} change(s)):\n{sb}";
            });
        }

        static Transform? FindChildRecursive(Transform parent, string name)
        {
            if (parent.name == name) return parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform? found = FindChildRecursive(parent.GetChild(i), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
#endif
