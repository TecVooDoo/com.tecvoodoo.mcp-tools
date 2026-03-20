#if HAS_ROPE_TOOLKIT
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.RopeToolkit.Editor
{
    public partial class Tool_RopeToolkit
    {
        [McpPluginTool("rope-add-connection", Title = "Rope Toolkit / Add Connection")]
        [Description(@"Adds or configures a RopeConnection component on a rope GameObject.
connectionType options:
  PinRopeToTransform -- pins a rope particle to a fixed transform (no rigidbody needed).
  PinTransformToRope -- pins a transform to follow a rope particle position.
  PullRigidbodyToRope -- applies spring force pulling a rigidbody toward a rope point.
  TwoWayCouplingBetweenRigidbodyAndRope -- full two-way physics between rigidbody and rope.
ropeLocation [0-1]: normalized distance along the rope (0=start, 0.5=middle, 1=end).
autoFindRopeLocation: if true, ignores ropeLocation and finds nearest point automatically.
If a RopeConnection already exists, configures the first one found.")]
        public string AddConnection(
            [Description("Name of the GameObject with the Rope component.")] string gameObjectName,
            [Description("Connection type: PinRopeToTransform, PinTransformToRope, PullRigidbodyToRope, TwoWayCouplingBetweenRigidbodyAndRope.")] string connectionType,
            [Description("Normalized position along rope [0-1]. 0=start, 1=end.")] float ropeLocation = 0f,
            [Description("If true, auto-finds nearest rope point (overrides ropeLocation).")] bool autoFindRopeLocation = false,
            [Description("Stiffness for rigidbody connections [0-1.0].")] float? stiffness = null,
            [Description("Damping for rigidbody connections [0-1.0].")] float? damping = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var rope = GetRope(gameObjectName);
                var go = rope.gameObject;

                var connection = go.GetComponent<global::RopeToolkit.RopeConnection>();
                if (connection == null)
                    connection = go.AddComponent<global::RopeToolkit.RopeConnection>();

                if (!System.Enum.TryParse<global::RopeToolkit.RopeConnectionType>(connectionType, true, out var connType))
                    throw new System.Exception($"Unknown connectionType '{connectionType}'. Valid: PinRopeToTransform, PinTransformToRope, PullRigidbodyToRope, TwoWayCouplingBetweenRigidbodyAndRope");

                connection.type = connType;
                connection.ropeLocation = Mathf.Clamp01(ropeLocation);
                connection.autoFindRopeLocation = autoFindRopeLocation;

                if (stiffness.HasValue) connection.rigidbodySettings.stiffness = Mathf.Clamp01(stiffness.Value);
                if (damping.HasValue) connection.rigidbodySettings.damping = Mathf.Clamp01(damping.Value);

                EditorUtility.SetDirty(go);

                return $"OK: RopeConnection on '{gameObjectName}' configured. type={connection.type} location={connection.ropeLocation:F3} autoFind={connection.autoFindRopeLocation}";
            });
        }
    }
}
#endif
