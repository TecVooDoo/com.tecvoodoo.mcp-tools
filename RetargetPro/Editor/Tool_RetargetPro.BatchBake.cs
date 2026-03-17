#nullable enable
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using KINEMATION.RetargetPro.Editor;
using KINEMATION.RetargetPro.Runtime;
using UnityEditor;
using UnityEngine;

namespace MCPTools.RetargetPro.Editor
{
    public partial class Tool_RetargetPro
    {
        [McpPluginTool("retarget-batch-bake", Title = "Retarget Pro / Batch Bake Animations")]
        [Description(@"Batch-retargets animation clips using a RetargetProfile asset.
Takes a folder of source AnimationClips and bakes retargeted versions to an output folder.
The RetargetProfile must already be configured with source/target characters and rigs.
Use this for retargeting animations between different character skeletons (e.g. polyperfect to Malbers AC).")]
        public BatchBakeResponse BatchBake(
            [Description("Asset path to the RetargetProfile SO (e.g. 'Assets/_Sandbox/_AQS/Data/Retarget/PolyToAC.asset').")]
            string profilePath,
            [Description("Folder containing source AnimationClip assets to retarget (e.g. 'Assets/polyperfect/Low Poly Animated Animals/Animations/Wolf').")]
            string sourceFolder,
            [Description("Output folder for retargeted clips (e.g. 'Assets/_Sandbox/_AQS/Animations/Retargeted'). Created if it doesn't exist.")]
            string outputFolder,
            [Description("Frame rate for baked animations (24-240). Default 30.")]
            float frameRate = 30f,
            [Description("Copy clip settings (loop time, bake into pose, events). Default true.")]
            bool copyClipSettings = true,
            [Description("Bake root motion curves. Default true.")]
            bool useRootMotion = true,
            [Description("Keyframe all bones (true) or optimize curves (false). Default true.")]
            bool keyframeAll = true,
            [Description("Max clips to process (0 = all). Useful for testing with a few clips first. Default 0.")]
            int maxClips = 0
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RetargetProfile profile = AssetDatabase.LoadAssetAtPath<RetargetProfile>(profilePath);
                if (profile == null)
                    throw new System.Exception($"RetargetProfile not found at '{profilePath}'.");

                if (profile.sourceCharacter == null)
                    throw new System.Exception("RetargetProfile has no source character assigned.");
                if (profile.targetCharacter == null)
                    throw new System.Exception("RetargetProfile has no target character assigned.");

                string[] clipGuids = AssetDatabase.FindAssets("t:AnimationClip", new[] { sourceFolder });
                if (clipGuids.Length == 0)
                    throw new System.Exception($"No AnimationClips found in '{sourceFolder}'.");

                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                RetargetAnimBaker baker = new RetargetAnimBaker();
                baker.retargetProfile = profile;

                // Set private fields via reflection
                SetPrivateField(baker, "_frameRate", frameRate);
                SetPrivateField(baker, "_copyClipSettings", copyClipSettings);
                SetPrivateField(baker, "_useRootMotion", useRootMotion);
                SetPrivateField(baker, "_keyframeAll", keyframeAll);
                SetPrivateStaticField(typeof(RetargetAnimBaker), "_savePath", outputFolder);

                int processed = 0;
                int failed = 0;
                StringBuilder sb = new StringBuilder();

                int clipLimit = maxClips > 0 ? System.Math.Min(maxClips, clipGuids.Length) : clipGuids.Length;

                for (int i = 0; i < clipLimit; i++)
                {
                    string clipPath = AssetDatabase.GUIDToAssetPath(clipGuids[i]);
                    AnimationClip sourceClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                    if (sourceClip == null) continue;

                    try
                    {
                        baker.InitializeBaker();
                        AnimationClip result = baker.BakeAnimation(sourceClip);
                        baker.UnInitializeBaker();

                        if (result != null)
                        {
                            processed++;
                            string resultPath = AssetDatabase.GetAssetPath(result);
                            sb.AppendLine($"  OK: {sourceClip.name} -> {resultPath}");
                        }
                        else
                        {
                            failed++;
                            sb.AppendLine($"  FAIL: {sourceClip.name} (BakeAnimation returned null)");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        failed++;
                        sb.AppendLine($"  FAIL: {sourceClip.name} ({ex.Message})");
                        try { baker.UnInitializeBaker(); } catch { }
                    }
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                return new BatchBakeResponse
                {
                    profilePath = profilePath,
                    sourceFolder = sourceFolder,
                    outputFolder = outputFolder,
                    totalClipsFound = clipGuids.Length,
                    processed = processed,
                    failed = failed,
                    details = sb.ToString()
                };
            });
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            FieldInfo field = obj.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) field.SetValue(obj, value);
        }

        private static void SetPrivateStaticField(System.Type type, string fieldName, object value)
        {
            FieldInfo field = type.GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null) field.SetValue(null, value);
        }

        public class BatchBakeResponse
        {
            [Description("Profile asset path used")] public string profilePath = "";
            [Description("Source animation folder")] public string sourceFolder = "";
            [Description("Output folder for retargeted clips")] public string outputFolder = "";
            [Description("Total clips found in source folder")] public int totalClipsFound;
            [Description("Successfully processed clips")] public int processed;
            [Description("Failed clips")] public int failed;
            [Description("Per-clip results")] public string details = "";
        }
    }
}
