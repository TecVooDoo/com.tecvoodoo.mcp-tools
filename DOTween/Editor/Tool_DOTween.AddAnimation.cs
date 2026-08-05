#if HAS_DOTWEEN
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using UnityEditor;
using DG.Tweening;
// System.ComponentModel (imported for [Description]) also defines Component.
using Component = UnityEngine.Component;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_DOTween
    {
        [AiTool("dotween-add-animation", Title = "DOTween / Add Animation")]
        [Description(@"Adds and configures a DOTweenAnimation component on a GameObject.
Supports Move, Scale, Rotate, Fade, Color, PunchPosition, ShakePosition, and all other animation types.")]
        public string AddAnimation(
            [Description("Name of the GameObject to add the animation to.")]
            string gameObjectName,
            [Description("Animation type: Move, LocalMove, Rotate, LocalRotate, Scale, Color, Fade, Text, PunchPosition, PunchRotation, PunchScale, ShakePosition, ShakeRotation, ShakeScale, CameraAspect, CameraBackgroundColor, CameraFieldOfView, CameraOrthoSize, CameraPixelRect, CameraRect, UIWidthHeight, FillAmount. The installed DOTween Pro version is authoritative; an invalid value reports the exact valid set.")]
            string animationType,
            [Description("Duration in seconds. Default 1.0.")]
            float? duration = null,
            [Description("Delay in seconds before start. Default 0.")]
            float? delay = null,
            [Description("Ease type name: Linear, InSine, OutSine, InOutSine, InQuad, OutQuad, InOutQuad, InCubic, OutCubic, InOutCubic, InBounce, OutBounce, InOutBounce, InElastic, OutElastic, InOutElastic, InBack, OutBack, InOutBack. Default OutQuad.")]
            string? ease = null,
            [Description("Number of loops. -1 for infinite. Default 1.")]
            int? loops = null,
            [Description("Loop type: Restart, Yoyo, Incremental. Default Restart.")]
            string? loopType = null,
            [Description("End value X component for Vector3-based animations.")]
            float? endX = null,
            [Description("End value Y component for Vector3-based animations.")]
            float? endY = null,
            [Description("End value Z component for Vector3-based animations.")]
            float? endZ = null,
            [Description("End value float for Fade, FillAmount, CameraFieldOfView, etc.")]
            float? endFloat = null,
            [Description("Tween identifier string for targeting specific tweens.")]
            string? id = null,
            [Description("Whether the tween plays automatically on start. Default true.")]
            bool? autoPlay = null,
            [Description("Whether end values are relative to current values. Default false.")]
            bool? isRelative = null,
            [Description("Whether the tween animates FROM the end value to the current value. Default false.")]
            bool? isFrom = null
        )
        {
            if (string.IsNullOrEmpty(gameObjectName))
                throw new ArgumentException("gameObjectName cannot be null or empty.", nameof(gameObjectName));
            if (string.IsNullOrEmpty(animationType))
                throw new ArgumentException("animationType cannot be null or empty.", nameof(animationType));

            // Boxed value of DOTweenAnimation.AnimationType -- the nested enum is not
            // referenceable at compile time (see Tool_DOTween.cs).
            object parsedAnimType = ParseAnimationType(animationType);

            Ease parsedEase = Ease.OutQuad;
            if (!string.IsNullOrEmpty(ease))
            {
                if (!Enum.TryParse<Ease>(ease, true, out parsedEase))
                    throw new ArgumentException($"Invalid ease '{ease}'.", nameof(ease));
            }

            LoopType parsedLoopType = DG.Tweening.LoopType.Restart;
            if (!string.IsNullOrEmpty(loopType))
            {
                if (!Enum.TryParse<LoopType>(loopType, true, out parsedLoopType))
                    throw new ArgumentException($"Invalid loopType '{loopType}'. Valid values: Restart, Yoyo, Incremental.", nameof(loopType));
            }

            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                Component anim = AddAnim(go);

                SetField(anim, "animationType", parsedAnimType);
                SetField(anim, "duration", duration ?? 1f);
                SetField(anim, "delay", delay ?? 0f);
                SetField(anim, "easeType", parsedEase);
                SetField(anim, "loops", loops ?? 1);
                SetField(anim, "loopType", parsedLoopType);
                SetField(anim, "autoPlay", autoPlay ?? true);
                SetField(anim, "isRelative", isRelative ?? false);
                SetField(anim, "isFrom", isFrom ?? false);
                SetField(anim, "autoKill", true);
                SetField(anim, "isActive", true);

                if (!string.IsNullOrEmpty(id))
                    SetField(anim, "id", id!);

                if (endX.HasValue || endY.HasValue || endZ.HasValue)
                    SetField(anim, "endValueV3", new Vector3(endX ?? 0f, endY ?? 0f, endZ ?? 0f));

                if (endFloat.HasValue)
                    SetField(anim, "endValueFloat", endFloat.Value);

                CallCreateTween(anim, false, false);

                EditorUtility.SetDirty(go);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"OK: Added DOTweenAnimation to '{gameObjectName}'");
                sb.AppendLine($"  Type:       {GetAnimationTypeName(anim)}");
                sb.AppendLine($"  Duration:   {GetField<float>(anim, "duration"):F2}s");
                sb.AppendLine($"  Delay:      {GetField<float>(anim, "delay"):F2}s");
                sb.AppendLine($"  Ease:       {parsedEase}");
                sb.AppendLine($"  Loops:      {GetField<int>(anim, "loops")} ({parsedLoopType})");
                sb.AppendLine($"  AutoPlay:   {GetField<bool>(anim, "autoPlay")}");
                sb.AppendLine($"  IsRelative: {GetField<bool>(anim, "isRelative")}");
                sb.AppendLine($"  IsFrom:     {GetField<bool>(anim, "isFrom")}");
                if (!string.IsNullOrEmpty(id))
                    sb.AppendLine($"  Id:         {id}");
                if (endX.HasValue || endY.HasValue || endZ.HasValue)
                {
                    Vector3 endV3 = GetField<Vector3>(anim, "endValueV3");
                    sb.AppendLine($"  EndValueV3: ({endV3.x:F2}, {endV3.y:F2}, {endV3.z:F2})");
                }
                if (endFloat.HasValue)
                    sb.AppendLine($"  EndFloat:   {GetField<float>(anim, "endValueFloat"):F2}");

                return sb.ToString();
            });
        }
    }
}
#endif
