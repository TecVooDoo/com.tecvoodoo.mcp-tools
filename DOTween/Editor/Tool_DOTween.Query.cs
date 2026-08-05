#if HAS_DOTWEEN
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using DG.Tweening;
// System.ComponentModel (imported for [Description]) also defines Component.
using Component = UnityEngine.Component;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_DOTween
    {
        [AiTool("dotween-query", Title = "DOTween / Query Animations")]
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
                Component[] anims = GetAnims(go);

                if (anims.Length == 0)
                    return $"No DOTweenAnimation components found on '{gameObjectName}'.";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"=== DOTweenAnimations on '{gameObjectName}' ({anims.Length}) ===");

                for (int i = 0; i < anims.Length; i++)
                {
                    Component anim = anims[i];
                    string id = GetField<string>(anim, "id");
                    string animTypeName = GetAnimationTypeName(anim);

                    sb.AppendLine($"\n-- [{i}] --");
                    sb.AppendLine($"  AnimationType: {animTypeName}");
                    sb.AppendLine($"  Id:            {(string.IsNullOrEmpty(id) ? "(none)" : id)}");
                    sb.AppendLine($"  Duration:      {GetField<float>(anim, "duration"):F2}s");
                    sb.AppendLine($"  Delay:         {GetField<float>(anim, "delay"):F2}s");
                    sb.AppendLine($"  Ease:          {GetField<Ease>(anim, "easeType")}");
                    sb.AppendLine($"  Loops:         {GetField<int>(anim, "loops")} ({GetField<LoopType>(anim, "loopType")})");
                    sb.AppendLine($"  AutoPlay:      {GetField<bool>(anim, "autoPlay")}");
                    sb.AppendLine($"  IsRelative:    {GetField<bool>(anim, "isRelative")}");
                    sb.AppendLine($"  IsFrom:        {GetField<bool>(anim, "isFrom")}");
                    sb.AppendLine($"  IsActive:      {GetField<bool>(anim, "isActive")}");
                    sb.AppendLine($"  AutoKill:      {GetField<bool>(anim, "autoKill")}");

                    // Report end values based on animation type. Compared by NAME because the
                    // nested AnimationType enum is not referenceable at compile time.
                    switch (animTypeName)
                    {
                        case "Move":
                        case "LocalMove":
                        case "Rotate":
                        case "LocalRotate":
                        case "Scale":
                        case "PunchPosition":
                        case "PunchRotation":
                        case "PunchScale":
                        case "ShakePosition":
                        case "ShakeRotation":
                        case "ShakeScale":
                        case "UIWidthHeight":
                        {
                            Vector3 endV3 = GetField<Vector3>(anim, "endValueV3");
                            sb.AppendLine($"  EndValueV3:    ({endV3.x:F2}, {endV3.y:F2}, {endV3.z:F2})");
                            break;
                        }
                        case "Color":
                        {
                            Color endColor = GetField<Color>(anim, "endValueColor");
                            sb.AppendLine($"  EndValueColor: ({endColor.r:F2}, {endColor.g:F2}, {endColor.b:F2}, {endColor.a:F2})");
                            break;
                        }
                        case "Fade":
                        case "FillAmount":
                        case "CameraFieldOfView":
                        {
                            sb.AppendLine($"  EndValueFloat: {GetField<float>(anim, "endValueFloat"):F2}");
                            break;
                        }
                    }

                    // 'tween' is declared on the ABSAnimationComponent base; its Tween type
                    // lives in DOTween.dll, which this assembly does reference.
                    Tween? tween = GetField(anim, "tween") as Tween;
                    bool isPlaying = tween != null && tween.active;
                    sb.AppendLine($"  TweenActive:   {isPlaying}");
                }

                return sb.ToString();
            });
        }
    }
}
#endif
