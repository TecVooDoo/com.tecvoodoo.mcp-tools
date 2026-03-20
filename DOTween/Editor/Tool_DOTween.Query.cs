#if HAS_DOTWEEN
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using DG.Tweening;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_DOTween
    {
        [McpPluginTool("dotween-query", Title = "DOTween / Query Animations")]
        [Description(@"Lists all DOTweenAnimation components on a GameObject.
Reports animation type, id, duration, delay, ease, loops, end values, and play state.")]
        public string Query(
            [Description("Name of the GameObject to inspect.")]
            string gameObjectName
        )
        {
            if (string.IsNullOrEmpty(gameObjectName))
                throw new ArgumentException("gameObjectName cannot be null or empty.", nameof(gameObjectName));

            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                DOTweenAnimation[] anims = go.GetComponents<DOTweenAnimation>();

                if (anims.Length == 0)
                    return $"No DOTweenAnimation components found on '{gameObjectName}'.";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"=== DOTweenAnimations on '{gameObjectName}' ({anims.Length}) ===");

                for (int i = 0; i < anims.Length; i++)
                {
                    DOTweenAnimation anim = anims[i];
                    sb.AppendLine($"\n-- [{i}] --");
                    sb.AppendLine($"  AnimationType: {anim.animationType}");
                    sb.AppendLine($"  Id:            {(string.IsNullOrEmpty(anim.id) ? "(none)" : anim.id)}");
                    sb.AppendLine($"  Duration:      {anim.duration:F2}s");
                    sb.AppendLine($"  Delay:         {anim.delay:F2}s");
                    sb.AppendLine($"  Ease:          {anim.easeType}");
                    sb.AppendLine($"  Loops:         {anim.loops} ({anim.loopType})");
                    sb.AppendLine($"  AutoPlay:      {anim.autoPlay}");
                    sb.AppendLine($"  IsRelative:    {anim.isRelative}");
                    sb.AppendLine($"  IsFrom:        {anim.isFrom}");
                    sb.AppendLine($"  IsActive:      {anim.isActive}");
                    sb.AppendLine($"  AutoKill:      {anim.autoKill}");

                    // Report end values based on animation type
                    DOTweenAnimation.AnimationType animType = anim.animationType;
                    if (animType == DOTweenAnimation.AnimationType.Move
                        || animType == DOTweenAnimation.AnimationType.LocalMove
                        || animType == DOTweenAnimation.AnimationType.Rotate
                        || animType == DOTweenAnimation.AnimationType.LocalRotate
                        || animType == DOTweenAnimation.AnimationType.Scale
                        || animType == DOTweenAnimation.AnimationType.PunchPosition
                        || animType == DOTweenAnimation.AnimationType.PunchRotation
                        || animType == DOTweenAnimation.AnimationType.PunchScale
                        || animType == DOTweenAnimation.AnimationType.ShakePosition
                        || animType == DOTweenAnimation.AnimationType.ShakeRotation
                        || animType == DOTweenAnimation.AnimationType.ShakeScale
                        || animType == DOTweenAnimation.AnimationType.UIWidthHeight)
                    {
                        sb.AppendLine($"  EndValueV3:    ({anim.endValueV3.x:F2}, {anim.endValueV3.y:F2}, {anim.endValueV3.z:F2})");
                    }
                    else if (animType == DOTweenAnimation.AnimationType.Color)
                    {
                        sb.AppendLine($"  EndValueColor: ({anim.endValueColor.r:F2}, {anim.endValueColor.g:F2}, {anim.endValueColor.b:F2}, {anim.endValueColor.a:F2})");
                    }
                    else if (animType == DOTweenAnimation.AnimationType.Fade
                        || animType == DOTweenAnimation.AnimationType.FillAmount
                        || animType == DOTweenAnimation.AnimationType.CameraFieldOfView)
                    {
                        sb.AppendLine($"  EndValueFloat: {anim.endValueFloat:F2}");
                    }

                    // Check tween play state
                    bool isPlaying = anim.tween != null;
                    if (isPlaying)
                    {
                        isPlaying = anim.tween.active;
                    }
                    sb.AppendLine($"  TweenActive:   {isPlaying}");
                }

                return sb.ToString();
            });
        }
    }
}
#endif
