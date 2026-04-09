#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

namespace MCPTools.AINavigation.Editor
{
    public partial class Tool_AINavigation
    {
        [McpPluginTool("nav-configure-link", Title = "AI Navigation / Configure Link")]
        [Description(@"Configures a NavMeshLink component. All parameters optional.
Sets up navigable connections between NavMesh surfaces (e.g. jump points, ladders, teleporters).
startTransformName/endTransformName: names of GameObjects to track (overrides manual start/end points).")]
        public string ConfigureLink(
            [Description("Name of the GameObject with NavMeshLink.")] string gameObjectName,
            [Description("Agent type ID.")] int? agentTypeID = null,
            [Description("Start point X (local space).")] float? startX = null,
            [Description("Start point Y (local space).")] float? startY = null,
            [Description("Start point Z (local space).")] float? startZ = null,
            [Description("End point X (local space).")] float? endX = null,
            [Description("End point Y (local space).")] float? endY = null,
            [Description("End point Z (local space).")] float? endZ = null,
            [Description("Name of GameObject to track as start point.")] string? startTransformName = null,
            [Description("Name of GameObject to track as end point.")] string? endTransformName = null,
            [Description("Link width.")] float? width = null,
            [Description("Cost modifier (negative = use area cost).")] float? costModifier = null,
            [Description("Allow traversal in both directions.")] bool? bidirectional = null,
            [Description("Auto-update when transforms move.")] bool? autoUpdate = null,
            [Description("Area type of the link.")] int? area = null,
            [Description("Whether agents can traverse this link.")] bool? activated = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var link = GetLink(gameObjectName);

                if (agentTypeID.HasValue) link.agentTypeID = agentTypeID.Value;
                if (width.HasValue) link.width = Mathf.Max(0f, width.Value);
                if (costModifier.HasValue) link.costModifier = costModifier.Value;
                if (bidirectional.HasValue) link.bidirectional = bidirectional.Value;
                if (autoUpdate.HasValue) link.autoUpdate = autoUpdate.Value;
                if (area.HasValue) link.area = area.Value;
                if (activated.HasValue) link.activated = activated.Value;

                if (startX.HasValue || startY.HasValue || startZ.HasValue)
                {
                    var sp = link.startPoint;
                    if (startX.HasValue) sp.x = startX.Value;
                    if (startY.HasValue) sp.y = startY.Value;
                    if (startZ.HasValue) sp.z = startZ.Value;
                    link.startPoint = sp;
                }

                if (endX.HasValue || endY.HasValue || endZ.HasValue)
                {
                    var ep = link.endPoint;
                    if (endX.HasValue) ep.x = endX.Value;
                    if (endY.HasValue) ep.y = endY.Value;
                    if (endZ.HasValue) ep.z = endZ.Value;
                    link.endPoint = ep;
                }

                if (startTransformName != null)
                    link.startTransform = FindGO(startTransformName).transform;
                if (endTransformName != null)
                    link.endTransform = FindGO(endTransformName).transform;

                link.UpdateLink();
                EditorUtility.SetDirty(link);

                return $"OK: NavMeshLink on '{gameObjectName}' configured. start={FormatVector3(link.startPoint)} end={FormatVector3(link.endPoint)} width={link.width:F2} bidirectional={link.bidirectional}";
            });
        }
    }
}
