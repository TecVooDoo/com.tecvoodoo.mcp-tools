#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
#if MCP_HAS_AIGD
using AIGD;
#else
using com.IvanMurzak.Unity.MCP.Runtime.Data;
#endif
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using Flexalon;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Flexalon.Editor
{
    public partial class Tool_Flexalon
    {
        [McpPluginTool("flexalon-create-circle-layout", Title = "Flexalon / Create Circle Layout")]
        [Description(@"Creates a new GameObject with a Flexalon Circle Layout component.
Children added to this object will be arranged in a circle or spiral.
Use 'flexalon-add-child' to populate the circle with objects.")]
        public CircleLayoutResponse CreateCircleLayout(
            [Description("Name for the circle layout GameObject.")]
            string name = "FlexalonCircle",
            [Description("Radius of the circle. Default 3.")]
            float radius = 3f,
            [Description("Plane for the circle: XZ (horizontal, default), XY (vertical), or ZY.")]
            string plane = "XZ",
            [Description("Spacing type: 'Evenly' distributes children equally, 'Fixed' uses spacingDegrees. Default 'Evenly'.")]
            string spacingType = "Evenly",
            [Description("Degrees between children when spacingType is 'Fixed'. Default 30.")]
            float spacingDegrees = 30f,
            [Description("Starting angle offset in degrees. Default 0.")]
            float startAtDegrees = 0f,
            [Description("Child rotation: 'None', 'Out', 'In', 'Forward', 'Backward'. Default 'None'.")]
            string rotate = "None",
            [Description("Enable spiral mode (children at increasing heights). Default false.")]
            bool spiral = false,
            [Description("Vertical spacing between objects in spiral. Default 0.5.")]
            float spiralSpacing = 0.5f,
            [Description("World position. Default (0,0,0).")]
            Vector3? position = null,
            [Description("Parent GameObject reference. Null for scene root.")]
            GameObjectRef? parentGameObjectRef = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject? parentGo = null;
                if (parentGameObjectRef?.IsValid(out _) == true)
                {
                    parentGo = parentGameObjectRef.FindGameObject(out var error);
                    if (error != null) throw new System.Exception(error);
                }

                var go = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(go, "Create Flexalon Circle");

                if (parentGo != null)
                    go.transform.SetParent(parentGo.transform, false);

                go.transform.position = position ?? Vector3.zero;

                var circle = go.AddComponent<FlexalonCircleLayout>();
                circle.Radius = radius;
                circle.StartAtDegrees = startAtDegrees;
                circle.Spiral = spiral;
                circle.SpiralSpacing = spiralSpacing;
                circle.SpacingDegrees = spacingDegrees;

                // Parse plane
                switch (plane.ToUpper())
                {
                    case "XY": circle.Plane = global::Flexalon.Plane.XY; break;
                    case "ZY": circle.Plane = global::Flexalon.Plane.ZY; break;
                    default:   circle.Plane = global::Flexalon.Plane.XZ; break;
                }

                // Parse spacing type
                circle.SpacingType = spacingType.ToLower() == "fixed"
                    ? FlexalonCircleLayout.SpacingOptions.Fixed
                    : FlexalonCircleLayout.SpacingOptions.Evenly;

                // Parse rotation
                switch (rotate.ToLower())
                {
                    case "out":      circle.Rotate = FlexalonCircleLayout.RotateOptions.Out; break;
                    case "in":       circle.Rotate = FlexalonCircleLayout.RotateOptions.In; break;
                    case "forward":  circle.Rotate = FlexalonCircleLayout.RotateOptions.Forward; break;
                    case "backward": circle.Rotate = FlexalonCircleLayout.RotateOptions.Backward; break;
                    default:         circle.Rotate = FlexalonCircleLayout.RotateOptions.None; break;
                }

                EditorUtility.SetDirty(go);

                return new CircleLayoutResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    radius = circle.Radius,
                    plane = circle.Plane.ToString(),
                    position = FormatVector3(go.transform.position)
                };
            });
        }

        public class CircleLayoutResponse
        {
            [Description("Name of the created GameObject")]
            public string gameObjectName = "";
            [Description("Instance ID of the created GameObject")]
            public int instanceId;
            [Description("Radius of the circle")]
            public float radius;
            [Description("Plane the circle is on")]
            public string plane = "";
            [Description("World position")]
            public string position = "";
        }
    }
}
