#nullable enable
using System.ComponentModel;
using System.IO;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using KINEMATION.RetargetPro.Runtime;
using KINEMATION.Shared.KAnimationCore.Runtime.Rig;
using UnityEditor;
using UnityEngine;

namespace MCPTools.RetargetPro.Editor
{
    public partial class Tool_RetargetPro
    {
        [McpPluginTool("retarget-create-profile", Title = "Retarget Pro / Create Profile")]
        [Description(@"Creates a RetargetProfile ScriptableObject asset for animation retargeting.
A profile defines the source and target characters, their rigs, and reference poses.
Both characters must have KRigComponent attached and KRig assets created.
After creating a profile, add retarget features via the Inspector, then use 'retarget-batch-bake' to process animations.")]
        public CreateProfileResponse CreateProfile(
            [Description("Asset path for the new profile (e.g. 'Assets/_Sandbox/_AQS/Data/Retarget/PolyToAC.asset').")]
            string assetPath,
            [Description("Asset path to the source character prefab (retarget FROM). Must have KRigComponent.")]
            string sourceCharacterPath,
            [Description("Asset path to the target character prefab (retarget TO). Must have KRigComponent.")]
            string targetCharacterPath,
            [Description("Asset path to the source KRig asset. Null to skip (set manually in Inspector).")]
            string? sourceRigPath = null,
            [Description("Asset path to the target KRig asset. Null to skip (set manually in Inspector).")]
            string? targetRigPath = null,
            [Description("Asset path to the source reference pose AnimationClip (A-pose or T-pose). Null to skip.")]
            string? sourcePosePath = null,
            [Description("Asset path to the target reference pose AnimationClip (A-pose or T-pose). Null to skip.")]
            string? targetPosePath = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string directory = Path.GetDirectoryName(assetPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                GameObject sourceChar = AssetDatabase.LoadAssetAtPath<GameObject>(sourceCharacterPath);
                if (sourceChar == null)
                    throw new System.Exception($"Source character not found at '{sourceCharacterPath}'.");

                GameObject targetChar = AssetDatabase.LoadAssetAtPath<GameObject>(targetCharacterPath);
                if (targetChar == null)
                    throw new System.Exception($"Target character not found at '{targetCharacterPath}'.");

                RetargetProfile profile = ScriptableObject.CreateInstance<RetargetProfile>();
                profile.sourceCharacter = sourceChar;
                profile.targetCharacter = targetChar;

                if (!string.IsNullOrEmpty(sourceRigPath))
                {
                    KRig sourceRig = AssetDatabase.LoadAssetAtPath<KRig>(sourceRigPath);
                    if (sourceRig != null) profile.sourceRig = sourceRig;
                }

                if (!string.IsNullOrEmpty(targetRigPath))
                {
                    KRig targetRig = AssetDatabase.LoadAssetAtPath<KRig>(targetRigPath);
                    if (targetRig != null) profile.targetRig = targetRig;
                }

                if (!string.IsNullOrEmpty(sourcePosePath))
                {
                    AnimationClip sourcePose = AssetDatabase.LoadAssetAtPath<AnimationClip>(sourcePosePath);
                    if (sourcePose != null) profile.sourcePose = sourcePose;
                }

                if (!string.IsNullOrEmpty(targetPosePath))
                {
                    AnimationClip targetPose = AssetDatabase.LoadAssetAtPath<AnimationClip>(targetPosePath);
                    if (targetPose != null) profile.targetPose = targetPose;
                }

                AssetDatabase.CreateAsset(profile, assetPath);
                AssetDatabase.SaveAssets();

                return new CreateProfileResponse
                {
                    assetPath = assetPath,
                    sourceCharacter = sourceChar.name,
                    targetCharacter = targetChar.name,
                    hasSourceRig = profile.sourceRig != null,
                    hasTargetRig = profile.targetRig != null,
                    hasSourcePose = profile.sourcePose != null,
                    hasTargetPose = profile.targetPose != null
                };
            });
        }

        [McpPluginTool("retarget-query-profiles", Title = "Retarget Pro / Query Profiles")]
        [Description(@"Lists all RetargetProfile ScriptableObject assets in the project.
Shows source/target characters, rig assignments, and feature count for each profile.")]
        public QueryProfilesResponse QueryProfiles(
            [Description("Folder to search (e.g. 'Assets/_Sandbox/_AQS/Data/Retarget'). Null to search entire project.")]
            string? searchFolder = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string[] guids;
                if (!string.IsNullOrEmpty(searchFolder))
                    guids = AssetDatabase.FindAssets("t:RetargetProfile", new[] { searchFolder });
                else
                    guids = AssetDatabase.FindAssets("t:RetargetProfile");

                StringBuilder sb = new StringBuilder();
                int count = 0;

                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    RetargetProfile profile = AssetDatabase.LoadAssetAtPath<RetargetProfile>(path);
                    if (profile == null) continue;

                    count++;
                    string sourceName = profile.sourceCharacter != null ? profile.sourceCharacter.name : "NONE";
                    string targetName = profile.targetCharacter != null ? profile.targetCharacter.name : "NONE";
                    string sourceRigName = profile.sourceRig != null ? profile.sourceRig.name : "NONE";
                    string targetRigName = profile.targetRig != null ? profile.targetRig.name : "NONE";
                    int featureCount = profile.retargetFeatures != null ? profile.retargetFeatures.Count : 0;

                    sb.AppendLine($"  {profile.name}");
                    sb.AppendLine($"    Source: {sourceName} (Rig: {sourceRigName}) | Target: {targetName} (Rig: {targetRigName})");
                    sb.AppendLine($"    Features: {featureCount} | SourcePose: {(profile.sourcePose != null ? profile.sourcePose.name : "NONE")} | TargetPose: {(profile.targetPose != null ? profile.targetPose.name : "NONE")}");
                    sb.AppendLine($"    Path: {path}");
                }

                return new QueryProfilesResponse
                {
                    profileCount = count,
                    details = sb.ToString()
                };
            });
        }

        public class CreateProfileResponse
        {
            [Description("Asset path where profile was saved")] public string assetPath = "";
            [Description("Source character name")] public string sourceCharacter = "";
            [Description("Target character name")] public string targetCharacter = "";
            [Description("Source rig assigned")] public bool hasSourceRig;
            [Description("Target rig assigned")] public bool hasTargetRig;
            [Description("Source pose assigned")] public bool hasSourcePose;
            [Description("Target pose assigned")] public bool hasTargetPose;
        }

        public class QueryProfilesResponse
        {
            [Description("Number of profiles found")] public int profileCount;
            [Description("Detailed profile listing")] public string details = "";
        }
    }
}
