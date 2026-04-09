#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
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

                if (RopeConnectionType == null)
                    throw new Exception("RopeToolkit.RopeConnection type not found.");
                if (RopeConnectionTypeEnum == null)
                    throw new Exception("RopeToolkit.RopeConnectionType enum not found.");

                var connection = go.GetComponent(RopeConnectionType);
                if (connection == null)
                    connection = go.AddComponent(RopeConnectionType);

                if (!Enum.IsDefined(RopeConnectionTypeEnum, connectionType))
                {
                    // Try case-insensitive parse
                    object? parsed = null;
                    foreach (var name in Enum.GetNames(RopeConnectionTypeEnum))
                    {
                        if (string.Equals(name, connectionType, StringComparison.OrdinalIgnoreCase))
                        {
                            parsed = Enum.Parse(RopeConnectionTypeEnum, name);
                            break;
                        }
                    }
                    if (parsed == null)
                        throw new Exception($"Unknown connectionType '{connectionType}'. Valid: PinRopeToTransform, PinTransformToRope, PullRigidbodyToRope, TwoWayCouplingBetweenRigidbodyAndRope");

                    var typeField = connection.GetType().GetField("type", BindingFlags.Public | BindingFlags.Instance);
                    if (typeField != null) typeField.SetValue(connection, parsed);
                }
                else
                {
                    var parsed = Enum.Parse(RopeConnectionTypeEnum, connectionType);
                    var typeField = connection.GetType().GetField("type", BindingFlags.Public | BindingFlags.Instance);
                    if (typeField != null) typeField.SetValue(connection, parsed);
                }

                Set(connection, "ropeLocation", Mathf.Clamp01(ropeLocation));
                Set(connection, "autoFindRopeLocation", autoFindRopeLocation);

                if (stiffness.HasValue || damping.HasValue)
                {
                    var rbSettings = Get(connection, "rigidbodySettings");
                    if (rbSettings != null)
                    {
                        if (stiffness.HasValue) SetStructField(rbSettings, "stiffness", Mathf.Clamp01(stiffness.Value));
                        if (damping.HasValue)   SetStructField(rbSettings, "damping", Mathf.Clamp01(damping.Value));
                        // rigidbodySettings might be a struct -- write back
                        var rbField = connection.GetType().GetField("rigidbodySettings", BindingFlags.Public | BindingFlags.Instance);
                        if (rbField != null) rbField.SetValue(connection, rbSettings);
                        else
                        {
                            var rbProp = connection.GetType().GetProperty("rigidbodySettings", BindingFlags.Public | BindingFlags.Instance);
                            if (rbProp != null && rbProp.CanWrite) rbProp.SetValue(connection, rbSettings);
                        }
                    }
                }

                EditorUtility.SetDirty(go);

                var connTypeVal = Get(connection, "type");
                var locVal      = Get(connection, "ropeLocation");
                var autoVal     = Get(connection, "autoFindRopeLocation");

                return $"OK: RopeConnection on '{gameObjectName}' configured. type={connTypeVal} location={locVal:F3} autoFind={autoVal}";
            });
        }
    }
}
