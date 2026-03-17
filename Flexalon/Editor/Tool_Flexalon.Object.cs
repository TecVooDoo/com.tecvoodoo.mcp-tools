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
        [McpPluginTool("flexalon-set-object-size", Title = "Flexalon / Set Object Size")]
        [Description(@"Sets the size of a Flexalon Object component on a GameObject.
FlexalonObject controls how a layout measures the object's bounds.
Add this to a layout container to define total layout size, or to a child to override its measured size.")]
        public SetSizeResponse SetObjectSize(
            [Description("Reference to the target GameObject.")]
            GameObjectRef targetRef,
            [Description("Width (X size). If null, not changed.")]
            float? width = null,
            [Description("Height (Y size). If null, not changed.")]
            float? height = null,
            [Description("Depth (Z size). If null, not changed.")]
            float? depth = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var fObj = go.GetComponent<FlexalonObject>();
                if (fObj == null)
                    fObj = Undo.AddComponent<FlexalonObject>(go);

                if (width.HasValue) fObj.Width = width.Value;
                if (height.HasValue) fObj.Height = height.Value;
                if (depth.HasValue) fObj.Depth = depth.Value;

                EditorUtility.SetDirty(go);

                return new SetSizeResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    width = fObj.Width,
                    height = fObj.Height,
                    depth = fObj.Depth
                };
            });
        }

        [McpPluginTool("flexalon-list-layouts", Title = "Flexalon / List Layouts in Scene")]
        [Description("Lists all Flexalon layout components currently in the scene, with their type and child count.")]
        public ListLayoutsResponse ListLayouts()
        {
            return MainThread.Instance.Run(() =>
            {
                var layouts = Object.FindObjectsByType<LayoutBase>(FindObjectsSortMode.None);
                var entries = new string[layouts.Length];
                for (int i = 0; i < layouts.Length; i++)
                {
                    var l = layouts[i];
                    var typeName = l.GetType().Name.Replace("Flexalon", "");
                    entries[i] = $"[{l.gameObject.GetInstanceID()}] {l.gameObject.name} ({typeName}) - {l.transform.childCount} children";
                }

                return new ListLayoutsResponse
                {
                    count = layouts.Length,
                    layouts = string.Join("\n", entries)
                };
            });
        }

        public class SetSizeResponse
        {
            [Description("Name of the GameObject")]
            public string gameObjectName = "";
            [Description("Instance ID")]
            public int instanceId;
            [Description("Current width")]
            public float width;
            [Description("Current height")]
            public float height;
            [Description("Current depth")]
            public float depth;
        }

        public class ListLayoutsResponse
        {
            [Description("Number of layouts found")]
            public int count;
            [Description("List of layouts with details")]
            public string layouts = "";
        }
    }
}
