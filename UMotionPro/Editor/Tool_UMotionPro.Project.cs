#nullable enable
using System;
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
        [McpPluginTool("umotion-project", Title = "UMotion Pro / Project & Clip Control")]
        [Description(@"Control surface for UMotion Clip Editor + Pose Editor. operation = open-windows | load | close | select-clip | rename-clip | delete-clip | set-frame | set-layer-blend | assign-pose-go | clear-pose-go. Most ops need ClipEditor window open AND a project loaded (chain open-windows + load first; call umotion-query to verify IsProjectLoaded before further ops). 'load' needs projectPath; '*-clip' need clipName (+ newClipName for rename); 'set-frame' needs frame; 'set-layer-blend' needs layerName + at least one of layerMute/layerWeight; 'assign-pose-go' needs poseGameObjectName. clearMode='revert' (default) reverts pose, 'keep' bakes it.")]
        public string Project(
            [Description("Operation to perform. One of: open-windows, load, close, select-clip, rename-clip, delete-clip, set-frame, set-layer-blend, assign-pose-go, clear-pose-go.")]
            string operation,
            [Description("Asset path of the UMotion project (.asset). Required for 'load'.")]
            string? projectPath = null,
            [Description("Clip name. Required for select-clip / rename-clip / delete-clip.")]
            string? clipName = null,
            [Description("New clip name. Required for rename-clip.")]
            string? newClipName = null,
            [Description("Frame index (>=0). Required for set-frame.")]
            int? frame = null,
            [Description("Layer name (base layer is excluded). Required for set-layer-blend.")]
            string? layerName = null,
            [Description("Mute flag for the layer. Used by set-layer-blend.")]
            bool? layerMute = null,
            [Description("Blend weight [0..1] for the layer. Used by set-layer-blend.")]
            float? layerWeight = null,
            [Description("Name of a GameObject in the current active scene. Required for assign-pose-go.")]
            string? poseGameObjectName = null,
            [Description("Clear mode for clear-pose-go: 'revert' (default, restores original pose) or 'keep' (bakes current pose into the original).")]
            string? clearMode = null,
            [Description("Undo-stack label for the action. Null = action is not added to undo stack. Default 'MCP UMotion: <operation>'.")]
            string? undoLabel = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string label = string.IsNullOrEmpty(undoLabel) ? $"MCP UMotion: {operation}" : undoLabel!;

                switch (operation)
                {
                    case "open-windows":
                        ClipEditor.OpenWindow();
                        PoseEditor.OpenWindow();
                        return "OK: ClipEditor + PoseEditor OpenWindow() requested. Allow one or more editor frames before issuing further calls; verify with umotion-query.";

                    case "load":
                    {
                        if (string.IsNullOrEmpty(projectPath))
                            throw new Exception("'load' requires projectPath.");
                        if (!ClipEditor.IsWindowOpened) ClipEditor.OpenWindow();
                        ClipEditor.LoadProject(projectPath!);
                        return $"OK: LoadProject('{projectPath}') requested. After a domain settle, umotion-query should show IsProjectLoaded=true.";
                    }

                    case "close":
                        RequireClipEditorWindow();
                        ClipEditor.CloseProject();
                        return "OK: CloseProject() called.";

                    case "select-clip":
                    {
                        if (string.IsNullOrEmpty(clipName))
                            throw new Exception("'select-clip' requires clipName.");
                        RequireProjectLoaded();
                        ClipEditor.SelectClip(clipName!);
                        return $"OK: SelectClip('{clipName}').";
                    }

                    case "rename-clip":
                    {
                        if (string.IsNullOrEmpty(clipName) || string.IsNullOrEmpty(newClipName))
                            throw new Exception("'rename-clip' requires clipName (old) and newClipName.");
                        RequireProjectLoaded();
                        ClipEditor.SetClipName(clipName!, newClipName!);
                        return $"OK: SetClipName('{clipName}' -> '{newClipName}').";
                    }

                    case "delete-clip":
                    {
                        if (string.IsNullOrEmpty(clipName))
                            throw new Exception("'delete-clip' requires clipName.");
                        RequireProjectLoaded();
                        ClipEditor.DeleteClip(clipName!);
                        return $"OK: DeleteClip('{clipName}').";
                    }

                    case "set-frame":
                    {
                        if (!frame.HasValue)
                            throw new Exception("'set-frame' requires frame (int).");
                        RequireProjectLoaded();
                        ClipEditor.SetFrameCursorPosition(frame.Value, label);
                        int actual = ClipEditor.GetFrameCursorPosition();
                        return $"OK: SetFrameCursorPosition({frame.Value}) requested; actual={actual} (call may be no-op in config mode).";
                    }

                    case "set-layer-blend":
                    {
                        if (string.IsNullOrEmpty(layerName))
                            throw new Exception("'set-layer-blend' requires layerName.");
                        if (!layerMute.HasValue && !layerWeight.HasValue)
                            throw new Exception("'set-layer-blend' requires at least one of layerMute / layerWeight.");
                        RequireProjectLoaded();

                        bool currentMute;
                        float currentWeight;
                        ClipEditor.GetClipLayerBlendProperties(layerName!, out currentMute, out currentWeight);
                        bool newMute = layerMute ?? currentMute;
                        float newWeight = layerWeight.HasValue ? Mathf.Clamp01(layerWeight.Value) : currentWeight;
                        ClipEditor.SetClipLayerBlendProperties(layerName!, newMute, newWeight);
                        return $"OK: SetClipLayerBlendProperties('{layerName}' mute={newMute} weight={newWeight:F3}).";
                    }

                    case "assign-pose-go":
                    {
                        if (string.IsNullOrEmpty(poseGameObjectName))
                            throw new Exception("'assign-pose-go' requires poseGameObjectName.");
                        RequireProjectLoaded();
                        if (!PoseEditor.IsWindowOpened) PoseEditor.OpenWindow();
                        GameObject? go = GameObject.Find(poseGameObjectName);
                        if (go == null)
                            throw new Exception($"GameObject '{poseGameObjectName}' not found in the active scene.");
                        PoseEditor.SetAnimatedGameObject(go);
                        return $"OK: SetAnimatedGameObject('{poseGameObjectName}'). Note: UMotion clones this GO and edits the clone (see PoseEditor.AnimatedPreviewGameObject).";
                    }

                    case "clear-pose-go":
                    {
                        PoseEditor.ClearMode mode = PoseEditor.ClearMode.RevertChanges;
                        if (!string.IsNullOrEmpty(clearMode))
                        {
                            if (clearMode!.Equals("keep", StringComparison.OrdinalIgnoreCase))
                                mode = PoseEditor.ClearMode.KeepChanges;
                            else if (!clearMode!.Equals("revert", StringComparison.OrdinalIgnoreCase))
                                throw new Exception($"clearMode must be 'revert' or 'keep' (got '{clearMode}').");
                        }
                        PoseEditor.ClearAnimatedGameObject(mode);
                        return $"OK: ClearAnimatedGameObject(mode={mode}).";
                    }

                    default:
                        throw new Exception($"Unknown operation '{operation}'. Valid: open-windows, load, close, select-clip, rename-clip, delete-clip, set-frame, set-layer-blend, assign-pose-go, clear-pose-go.");
                }
            });
        }
    }
}
