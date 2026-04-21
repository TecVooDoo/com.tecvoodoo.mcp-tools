#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.UltimateTerrain.Editor
{
    public partial class Tool_UltimateTerrain
    {
        [McpPluginTool("ut-configure", Title = "Ultimate Terrain / Configure")]
        [Description(@"Sets configuration on an UltimateTerrain instance. All parameters optional.")]
        public string Configure(
            [Description("Name of the GameObject with UltimateTerrain.")] string gameObjectName,
            [Description("Position X (terrain local).")] float? positionX = null,
            [Description("Position Y (terrain local).")] float? positionY = null,
            [Description("Scale X.")] float? scaleX = null,
            [Description("Scale Y.")] float? scaleY = null,
            [Description("Animation duration in seconds.")] float? duration = null,
            [Description("Enable animation tweening.")] bool? enableAnimation = null,
            [Description("Sync delay across executions.")] bool? delaySync = null,
            [Description("Enable multi-terrain operations.")] bool? multiTerrainActive = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var ut = GetUT(gameObjectName);

                if (positionX.HasValue || positionY.HasValue)
                {
                    var p = ut.position;
                    if (positionX.HasValue) p.x = positionX.Value;
                    if (positionY.HasValue) p.y = positionY.Value;
                    ut.position = p;
                }
                if (scaleX.HasValue || scaleY.HasValue)
                {
                    var s = ut.scale;
                    if (scaleX.HasValue) s.x = scaleX.Value;
                    if (scaleY.HasValue) s.y = scaleY.Value;
                    ut.scale = s;
                }
                if (duration.HasValue) ut.duration = Mathf.Max(0f, duration.Value);
                if (enableAnimation.HasValue) ut.enableAnimation = enableAnimation.Value;
                if (delaySync.HasValue) ut.delaySync = delaySync.Value;
                if (multiTerrainActive.HasValue) ut.multiTerrainActive = multiTerrainActive.Value;

                EditorUtility.SetDirty(ut);
                return $"OK: UltimateTerrain on '{gameObjectName}' configured. position={FormatVec2(ut.position)} scale={FormatVec2(ut.scale)} duration={ut.duration:F2}s anim={ut.enableAnimation}";
            });
        }
    }
}
