#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using Flexalon;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Flexalon.Editor
{
    public partial class Tool_Flexalon
    {
        [McpPluginTool("flexalon-create-flexible-layout", Title = "Flexalon / Create Flexible Layout")]
        [Description(@"Creates a new GameObject with a Flexalon Flexible Layout component.
This is a linear layout (like CSS flexbox). Children are placed one after another along a direction.
Supports wrapping. Use 'flexalon-add-child' to populate it with objects.")]
        public FlexibleLayoutResponse CreateFlexibleLayout(
            [Description("Name for the layout GameObject.")]
            string name = "FlexalonFlex",
            [Description("Direction to lay out children: 'PositiveX' (right), 'NegativeX' (left), 'PositiveY' (up), 'NegativeY' (down), 'PositiveZ' (forward), 'NegativeZ' (back). Default 'PositiveX'.")]
            string direction = "PositiveX",
            [Description("Gap between children. Default 0.1.")]
            float gap = 0.1f,
            [Description("Enable wrapping to next line when out of space. Default false.")]
            bool wrap = false,
            [Description("Direction to wrap: 'NegativeY' (default), 'PositiveY', 'PositiveZ', etc.")]
            string wrapDirection = "NegativeY",
            [Description("Gap between wrap lines. Default 0.1.")]
            float wrapGap = 0.1f,
            [Description("Horizontal alignment: 'Start', 'Center', 'End'. Default 'Center'.")]
            string horizontalAlign = "Center",
            [Description("Vertical alignment: 'Start', 'Center', 'End'. Default 'Center'.")]
            string verticalAlign = "Center",
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
                Undo.RegisterCreatedObjectUndo(go, "Create Flexalon Flexible");

                if (parentGo != null)
                    go.transform.SetParent(parentGo.transform, false);

                go.transform.position = position ?? Vector3.zero;

                var flex = go.AddComponent<FlexalonFlexibleLayout>();
                flex.Direction = ParseDirection(direction);
                flex.Gap = gap;
                flex.Wrap = wrap;
                flex.WrapDirection = ParseDirection(wrapDirection);
                flex.WrapGap = wrapGap;
                flex.HorizontalAlign = ParseAlign(horizontalAlign);
                flex.VerticalAlign = ParseAlign(verticalAlign);

                EditorUtility.SetDirty(go);

                return new FlexibleLayoutResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    direction = flex.Direction.ToString(),
                    gap = flex.Gap,
                    wrap = flex.Wrap,
                    position = FormatVector3(go.transform.position)
                };
            });
        }

        static Direction ParseDirection(string dir)
        {
            switch (dir.ToLower().Replace(" ", ""))
            {
                case "positivex": case "right":   return Direction.PositiveX;
                case "negativex": case "left":    return Direction.NegativeX;
                case "positivey": case "up":      return Direction.PositiveY;
                case "negativey": case "down":    return Direction.NegativeY;
                case "positivez": case "forward": return Direction.PositiveZ;
                case "negativez": case "back":    return Direction.NegativeZ;
                default: return Direction.PositiveX;
            }
        }

        static Align ParseAlign(string align)
        {
            switch (align.ToLower())
            {
                case "start": return Align.Start;
                case "end":   return Align.End;
                default:      return Align.Center;
            }
        }

        public class FlexibleLayoutResponse
        {
            [Description("Name of the created GameObject")]
            public string gameObjectName = "";
            [Description("Instance ID of the created GameObject")]
            public int instanceId;
            [Description("Direction children are laid out")]
            public string direction = "";
            [Description("Gap between children")]
            public float gap;
            [Description("Whether wrapping is enabled")]
            public bool wrap;
            [Description("World position")]
            public string position = "";
        }
    }
}
