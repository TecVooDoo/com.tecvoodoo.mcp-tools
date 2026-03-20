#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Heathen.UnityPhysics;
using UnityEngine;

namespace MCPTools.HeathenBallistics.Editor
{
    public partial class Tool_HeathenBallistics
    {
        [McpPluginTool("ballistic-query", Title = "Heathen Ballistics / Query Components")]
        [Description(@"Lists all Heathen Ballistics components on a GameObject.
Reports BallisticAim (speed, limits), TrickShot (speed, bounces, radius),
BallisticPathLineRender (resolution, maxLength, bounces), and BallisticTargeting.
Use before configuring to understand the current ballistic setup.")]
        public string QueryBallistics(
            [Description("Name of the GameObject to inspect.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var sb = new StringBuilder();
                sb.AppendLine($"=== Heathen Ballistics: {go.name} ===");

                var aim = go.GetComponent<BallisticAim>();
                if (aim != null)
                {
                    sb.AppendLine($"\n-- BallisticAim --");
                    sb.AppendLine($"  initialSpeed:  {aim.initialSpeed:F3}");
                    sb.AppendLine($"  acceleration:  {FormatVector3(aim.constantAcceleration)}");
                    sb.AppendLine($"  yLimit:        ({aim.yLimit.x:F1} to {aim.yLimit.y:F1})");
                    sb.AppendLine($"  xLimit:        ({aim.xLimit.x:F1} to {aim.xLimit.y:F1})");
                    sb.AppendLine($"  yPivot:        {(aim.yPivot != null ? aim.yPivot.name : "none")}");
                    sb.AppendLine($"  xPivot:        {(aim.xPivot != null ? aim.xPivot.name : "none")}");
                }

                var targeting = go.GetComponent<BallisticTargeting>();
                if (targeting != null)
                {
                    sb.AppendLine($"\n-- BallisticTargeting --");
                    sb.AppendLine($"  target:        {(targeting.targetTransform != null ? targeting.targetTransform.name : "none")}");
                    sb.AppendLine($"  HasSolution:   {targeting.HasSolution}");
                }

                var trickshot = go.GetComponent<TrickShot>();
                if (trickshot != null)
                {
                    sb.AppendLine($"\n-- TrickShot --");
                    sb.AppendLine($"  speed:         {trickshot.speed:F3}");
                    sb.AppendLine($"  radius:        {trickshot.radius:F3}");
                    sb.AppendLine($"  bounces:       {trickshot.bounces}");
                    sb.AppendLine($"  bounceDamping: {trickshot.bounceDamping:F3}");
                    sb.AppendLine($"  distance:      {trickshot.distance:F3}");
                    sb.AppendLine($"  resolution:    {trickshot.resolution:F4}");
                    sb.AppendLine($"  totalLength:   {trickshot.distanceIsTotalLength}");
                    sb.AppendLine($"  template:      {(trickshot.template != null ? trickshot.template.name : "none")}");
                }

                var viz = go.GetComponent<BallisticPathLineRender>();
                if (viz != null)
                {
                    sb.AppendLine($"\n-- BallisticPathLineRender --");
                    sb.AppendLine($"  resolution:    {viz.resolution:F4}");
                    sb.AppendLine($"  maxLength:     {viz.maxLength:F3}");
                    sb.AppendLine($"  maxBounces:    {viz.maxBounces}");
                    sb.AppendLine($"  bounceDamping: {viz.bounceDamping:F3}");
                    sb.AppendLine($"  gravityMode:   {viz.gravityMode}");
                    sb.AppendLine($"  continuousRun: {viz.continuousRun}");
                }

                if (aim == null && targeting == null && trickshot == null && viz == null)
                    sb.AppendLine("\nNo Heathen Ballistics components found on this GameObject.");

                return sb.ToString();
            });
        }
    }
}
