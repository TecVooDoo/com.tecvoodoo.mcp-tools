#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UMotionEditor.API;
using UnityEngine;

namespace MCPTools.UMotionPro.Editor
{
    public partial class Tool_UMotionPro
    {
        [McpPluginTool("umotion-query", Title = "UMotion Pro / Query Editor State")]
        [Description(@"Reports the full state of the UMotion Clip Editor and Pose Editor.
Returns: window-open flags, loaded project path, all clip names + currently selected clip,
clip layer names with mute/weight, frame cursor + last keyframe, pose-editor assigned GameObject,
pivot mode, and optionally the bone hierarchy of the assigned GameObject.

Use this first to discover whether a UMotion session is active and what clips/layers exist
before issuing umotion-project / umotion-import / umotion-export calls.")]
        public string Query(
            [Description("If true, also lists every transform/bone of the Pose Editor's currently-assigned GameObject. Default false (skipped when no GameObject is assigned).")]
            bool listBones = false,
            [Description("If true, also lists the mirror table (left/right transform pairs) of the Pose Editor's currently-assigned GameObject.")]
            bool listMirrorTable = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("UMotion Pro state:");

                // Clip Editor window
                sb.AppendLine($"  ClipEditor.IsWindowOpened: {ClipEditor.IsWindowOpened}");
                if (!ClipEditor.IsWindowOpened)
                {
                    sb.AppendLine("  (ClipEditor window not open — most queries unavailable. Open via umotion-project operation='open-windows'.)");
                }
                else
                {
                    sb.AppendLine($"  ClipEditor.IsProjectLoaded: {ClipEditor.IsProjectLoaded}");
                    if (ClipEditor.IsProjectLoaded)
                    {
                        string projectPath = ClipEditor.GetLoadedProjectPath() ?? "(null)";
                        sb.AppendLine($"  Project: {projectPath}");

                        string[] clipNames = ClipEditor.GetAllClipNames() ?? new string[0];
                        string selected = ClipEditor.GetSelectedClipName() ?? "(none)";
                        sb.AppendLine($"  Clips ({clipNames.Length}): selected='{selected}'");
                        for (int i = 0; i < clipNames.Length; i++)
                            sb.AppendLine($"    [{i}] {clipNames[i]}");

                        string[] layerNames = ClipEditor.GetClipLayerNames() ?? new string[0];
                        sb.AppendLine($"  Layers of selected clip ({layerNames.Length}, base layer excluded):");
                        for (int i = 0; i < layerNames.Length; i++)
                        {
                            bool mute;
                            float weight;
                            ClipEditor.GetClipLayerBlendProperties(layerNames[i], out mute, out weight);
                            sb.AppendLine($"    [{i}] '{layerNames[i]}' mute={mute} weight={weight:F3}");
                        }

                        int cursor = ClipEditor.GetFrameCursorPosition();
                        float playback = ClipEditor.GetPlaybackFramePosition();
                        float lastKey = ClipEditor.GetLastKeyFrame();
                        sb.AppendLine($"  FrameCursor: {cursor}");
                        sb.AppendLine($"  PlaybackFrame: {playback:F2} (negative = not playing)");
                        sb.AppendLine($"  LastKeyFrame: {lastKey:F2}");
                    }
                }

                // Pose Editor
                sb.AppendLine($"  PoseEditor.IsWindowOpened: {PoseEditor.IsWindowOpened}");
                GameObject preview = PoseEditor.AnimatedPreviewGameObject;
                if (preview != null)
                {
                    sb.AppendLine($"  PoseEditor.AnimatedPreviewGameObject: '{preview.name}' (preview clone; original is hidden in scene)");
                    sb.AppendLine($"  PoseEditor.Pivot: {PoseEditor.Pivot}");

                    if (listBones)
                    {
                        var transforms = new List<Transform>();
                        PoseEditor.GetAllTransforms(transforms);
                        sb.AppendLine($"  Bones ({transforms.Count}):");
                        for (int i = 0; i < transforms.Count; i++)
                        {
                            Transform t = transforms[i];
                            bool selected = PoseEditor.GetTransformIsSelected(t);
                            sb.AppendLine($"    [{i}] {(selected ? "[SEL] " : "      ")}{t.name}");
                        }

                        var selectedTransforms = new List<Transform>();
                        PoseEditor.GetSelectedTransforms(selectedTransforms);
                        sb.AppendLine($"  Selected bones: {selectedTransforms.Count}");
                    }

                    if (listMirrorTable)
                    {
                        var left = new List<Transform>();
                        var right = new List<Transform>();
                        PoseEditor.GetMirrorTable(left, right);
                        sb.AppendLine($"  MirrorTable ({left.Count} pairs):");
                        for (int i = 0; i < left.Count; i++)
                        {
                            string ln = left[i] != null ? left[i].name : "(null)";
                            string rn = right[i] != null ? right[i].name : "(null)";
                            sb.AppendLine($"    [{i}] L='{ln}' <-> R='{rn}'");
                        }
                    }
                }
                else
                {
                    sb.AppendLine("  PoseEditor.AnimatedPreviewGameObject: (none assigned)");
                }

                return sb.ToString();
            });
        }
    }
}
