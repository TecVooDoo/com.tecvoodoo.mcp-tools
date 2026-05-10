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
        [McpPluginTool("flexalon-create-grid-layout", Title = "Flexalon / Create Grid Layout")]
        [Description(@"Creates a new GameObject with a Flexalon Grid Layout component.
Children added to this object will be automatically arranged in a grid pattern.
Use 'flexalon-add-child' to populate the grid with objects.")]
        public GridLayoutResponse CreateGridLayout(
            [Description("Name for the grid layout GameObject.")]
            string name = "FlexalonGrid",
            [Description("Number of columns in the grid. Default 3.")]
            int columns = 3,
            [Description("Number of rows in the grid. Default 3.")]
            int rows = 3,
            [Description("Number of layers (depth). Default 1.")]
            int layers = 1,
            [Description("Fixed cell width. If 0, cells fill available space. Default 1.")]
            float columnSize = 1f,
            [Description("Fixed cell height. If 0, cells fill available space. Default 1.")]
            float rowSize = 1f,
            [Description("Spacing between columns. Default 0.1.")]
            float columnSpacing = 0.1f,
            [Description("Spacing between rows. Default 0.1.")]
            float rowSpacing = 0.1f,
            [Description("World position for the grid. Default (0,0,0).")]
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
                Undo.RegisterCreatedObjectUndo(go, "Create Flexalon Grid");

                if (parentGo != null)
                    go.transform.SetParent(parentGo.transform, false);

                go.transform.position = position ?? Vector3.zero;

                var grid = go.AddComponent<FlexalonGridLayout>();
                grid.Columns = (uint)Mathf.Max(1, columns);
                grid.Rows = (uint)Mathf.Max(1, rows);
                grid.Layers = (uint)Mathf.Max(1, layers);

                if (columnSize > 0) grid.ColumnSize = columnSize;
                if (rowSize > 0) grid.RowSize = rowSize;
                grid.ColumnSpacing = columnSpacing;
                grid.RowSpacing = rowSpacing;

                EditorUtility.SetDirty(go);

                return new GridLayoutResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    columns = (int)grid.Columns,
                    rows = (int)grid.Rows,
                    layers = (int)grid.Layers,
                    position = FormatVector3(go.transform.position)
                };
            });
        }

        public class GridLayoutResponse
        {
            [Description("Name of the created GameObject")]
            public string gameObjectName = "";
            [Description("Instance ID of the created GameObject")]
            public int instanceId;
            [Description("Number of columns")]
            public int columns;
            [Description("Number of rows")]
            public int rows;
            [Description("Number of layers")]
            public int layers;
            [Description("World position")]
            public string position = "";
        }
    }
}
