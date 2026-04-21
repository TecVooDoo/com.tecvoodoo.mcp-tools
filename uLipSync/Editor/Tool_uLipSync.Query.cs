#if HAS_ULIPSYNC
#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.uLipSync.Editor
{
    public partial class Tool_uLipSync
    {
        [McpPluginTool("lipsync-query", Title = "uLipSync / Query Components")]
        [Description(@"Reports all uLipSync components on a GameObject hierarchy and their configuration.
Checks for: uLipSync (analyzer), uLipSyncBlendShape, uLipSyncBakedDataPlayer,
uLipSyncTimelineEvent, uLipSyncTexture, uLipSyncAnimator.
Also lists available Profile and BakedData assets in the project.")]
        public string QueryLipSync(
            [Description("Name of the GameObject to inspect. Searches entire hierarchy for uLipSync components. Leave empty to list available Profile/BakedData assets only.")]
            string? gameObjectName = null,
            [Description("If true, also lists all Profile and BakedData assets in the project. Default: false.")]
            bool listAssets = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                StringBuilder sb = new StringBuilder();

                if (!string.IsNullOrEmpty(gameObjectName))
                {
                    GameObject? go = GameObject.Find(gameObjectName);
                    if (go == null)
                        throw new Exception($"GameObject '{gameObjectName}' not found.");

                    sb.AppendLine($"=== uLipSync Components on '{gameObjectName}' (hierarchy) ===");
                    bool found = false;

                    // uLipSync (analyzer)
                    List<UnityEngine.Component> analyzers = FindComponentsInHierarchy(go, ULipSyncType);
                    foreach (UnityEngine.Component comp in analyzers)
                    {
                        found = true;
                        sb.AppendLine($"\n-- uLipSync on '{comp.gameObject.name}' --");
                        object? profile = Get(comp, "profile");
                        sb.AppendLine($"  Profile:          {(profile != null ? ((UnityEngine.Object)profile).name : "(none)")}");
                        sb.AppendLine($"  OutputSoundGain:  {Get(comp, "outputSoundGain")}");

                        // List profile phonemes if available
                        if (profile != null)
                        {
                            object? mfccs = Get(profile, "mfccs");
                            if (mfccs is IList mfccList)
                            {
                                sb.AppendLine($"  Profile Phonemes ({mfccList.Count}):");
                                foreach (object? mfcc in mfccList)
                                {
                                    if (mfcc != null)
                                    {
                                        object? phonemeName = Get(mfcc, "name");
                                        sb.AppendLine($"    - {phonemeName}");
                                    }
                                }
                            }
                        }
                    }

                    // uLipSyncBlendShape
                    List<UnityEngine.Component> blendShapes = FindComponentsInHierarchy(go, BlendShapeType);
                    foreach (UnityEngine.Component comp in blendShapes)
                    {
                        found = true;
                        sb.AppendLine($"\n-- uLipSyncBlendShape on '{comp.gameObject.name}' --");
                        object? smr = Get(comp, "skinnedMeshRenderer");
                        sb.AppendLine($"  SkinnedMeshRenderer: {(smr != null ? ((UnityEngine.Object)smr).name : "(none)")}");
                        sb.AppendLine($"  UpdateMethod:        {Get(comp, "updateMethod")}");
                        sb.AppendLine($"  MinVolume:           {Get(comp, "minVolume")}");
                        sb.AppendLine($"  MaxVolume:           {Get(comp, "maxVolume")}");
                        sb.AppendLine($"  Smoothness:          {Get(comp, "smoothness")}");
                        sb.AppendLine($"  UsePhonemeBlend:     {Get(comp, "usePhonemeBlend")}");
                        sb.AppendLine($"  MaxBlendShapeValue:  {Get(comp, "maxBlendShapeValue")}");

                        object? blendShapeList = Get(comp, "blendShapes");
                        if (blendShapeList is IList bsList)
                        {
                            sb.AppendLine($"  BlendShape Mappings ({bsList.Count}):");
                            foreach (object? bs in bsList)
                            {
                                if (bs != null)
                                {
                                    object? phoneme = Get(bs, "phoneme");
                                    object? index = Get(bs, "index");
                                    object? maxWeight = Get(bs, "maxWeight");
                                    string blendShapeName = "(unknown)";
                                    if (smr != null && index is int idx && idx >= 0)
                                    {
                                        SkinnedMeshRenderer renderer = (SkinnedMeshRenderer)smr;
                                        if (renderer.sharedMesh != null && idx < renderer.sharedMesh.blendShapeCount)
                                            blendShapeName = renderer.sharedMesh.GetBlendShapeName(idx);
                                    }
                                    sb.AppendLine($"    {phoneme} -> [{index}] \"{blendShapeName}\" (maxWeight: {maxWeight})");
                                }
                            }
                        }
                    }

                    // uLipSyncBakedDataPlayer
                    List<UnityEngine.Component> bakedPlayers = FindComponentsInHierarchy(go, BakedDataPlayerType);
                    foreach (UnityEngine.Component comp in bakedPlayers)
                    {
                        found = true;
                        sb.AppendLine($"\n-- uLipSyncBakedDataPlayer on '{comp.gameObject.name}' --");
                        object? bakedData = Get(comp, "bakedData");
                        object? audioSource = Get(comp, "audioSource");
                        sb.AppendLine($"  BakedData:       {(bakedData != null ? ((UnityEngine.Object)bakedData).name : "(none)")}");
                        sb.AppendLine($"  AudioSource:     {(audioSource != null ? "assigned" : "(none)")}");
                        sb.AppendLine($"  PlayOnAwake:     {Get(comp, "playOnAwake")}");
                        sb.AppendLine($"  PlayAudioSource: {Get(comp, "playAudioSource")}");
                        sb.AppendLine($"  Volume:          {Get(comp, "volume")}");
                        sb.AppendLine($"  TimeOffset:      {Get(comp, "timeOffset")}");

                        if (bakedData != null)
                        {
                            object? duration = Get(bakedData, "duration");
                            object? audioClip = Get(bakedData, "audioClip");
                            sb.AppendLine($"  BakedData Duration: {duration}s");
                            sb.AppendLine($"  BakedData Clip:     {(audioClip != null ? ((UnityEngine.Object)audioClip).name : "(none)")}");
                        }
                    }

                    // uLipSyncTimelineEvent
                    List<UnityEngine.Component> timelineEvents = FindComponentsInHierarchy(go, TimelineEventType);
                    foreach (UnityEngine.Component comp in timelineEvents)
                    {
                        found = true;
                        sb.AppendLine($"\n-- uLipSyncTimelineEvent on '{comp.gameObject.name}' --");
                        sb.AppendLine($"  (Receives lip sync data from Timeline track binding)");
                    }

                    // uLipSyncTexture
                    List<UnityEngine.Component> textures = FindComponentsInHierarchy(go, TextureType);
                    foreach (UnityEngine.Component comp in textures)
                    {
                        found = true;
                        sb.AppendLine($"\n-- uLipSyncTexture on '{comp.gameObject.name}' --");
                        object? targetRenderer = Get(comp, "targetRenderer");
                        sb.AppendLine($"  TargetRenderer: {(targetRenderer != null ? ((UnityEngine.Object)targetRenderer).name : "(none)")}");
                        sb.AppendLine($"  UpdateMethod:   {Get(comp, "updateMethod")}");
                        sb.AppendLine($"  MinVolume:      {Get(comp, "minVolume")}");
                        sb.AppendLine($"  MinDuration:    {Get(comp, "minDuration")}");

                        object? textureList = Get(comp, "textures");
                        if (textureList is IList texList)
                        {
                            sb.AppendLine($"  Texture Mappings ({texList.Count}):");
                            foreach (object? tex in texList)
                            {
                                if (tex != null)
                                {
                                    object? phoneme = Get(tex, "phoneme");
                                    object? texture = Get(tex, "texture");
                                    sb.AppendLine($"    {phoneme} -> {(texture != null ? ((UnityEngine.Object)texture).name : "(default)")}");
                                }
                            }
                        }
                    }

                    // uLipSyncAnimator
                    List<UnityEngine.Component> animators = FindComponentsInHierarchy(go, AnimatorType);
                    foreach (UnityEngine.Component comp in animators)
                    {
                        found = true;
                        sb.AppendLine($"\n-- uLipSyncAnimator on '{comp.gameObject.name}' --");
                    }

                    if (!found)
                        sb.AppendLine("\nNo uLipSync components found in hierarchy.");
                }

                if (listAssets || string.IsNullOrEmpty(gameObjectName))
                {
                    sb.AppendLine("\n=== Available Profile Assets ===");
                    string[] profileGuids = UnityEditor.AssetDatabase.FindAssets("t:uLipSync.Profile");
                    sb.AppendLine($"Found: {profileGuids.Length}");
                    foreach (string guid in profileGuids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        UnityEngine.Object? asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                        if (asset != null && ProfileType != null)
                        {
                            object? mfccs = Get(asset, "mfccs");
                            int phonemeCount = 0;
                            if (mfccs is IList mfccList)
                                phonemeCount = mfccList.Count;
                            sb.AppendLine($"  {asset.name} ({phonemeCount} phonemes) -- {path}");
                        }
                    }

                    sb.AppendLine("\n=== Available BakedData Assets ===");
                    string[] bakedGuids = UnityEditor.AssetDatabase.FindAssets("t:uLipSync.BakedData");
                    sb.AppendLine($"Found: {bakedGuids.Length}");
                    foreach (string guid in bakedGuids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        UnityEngine.Object? asset = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                        if (asset != null)
                        {
                            object? duration = Get(asset, "duration");
                            object? audioClip = Get(asset, "audioClip");
                            string clipName = audioClip != null ? ((UnityEngine.Object)audioClip).name : "(none)";
                            sb.AppendLine($"  {asset.name} (clip: {clipName}, duration: {duration}s) -- {path}");
                        }
                    }
                }

                return sb.ToString();
            });
        }
    }
}
#endif
