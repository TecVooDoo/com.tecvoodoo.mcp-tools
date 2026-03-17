#if HAS_PWB
#nullable enable
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using PluginMaster;
using UnityEditor;
using UnityEngine;

namespace MCPTools.PWB.Editor
{
    public partial class Tool_PWB
    {
        [McpPluginTool("pwb-place-brush", Title = "PWB / Place Brush Prefab")]
        [Description(@"Places a prefab from a PWB palette brush at a specified position in the scene.
Use 'pwb-list-palettes' first to see available palettes and brushes.
Specify the palette and brush by index, or search by brush name.")]
        public PlaceBrushResponse PlaceBrush(
            [Description("World position to place the prefab.")]
            Vector3 position,
            [Description("Index of the palette (from pwb-list-palettes). Default 0.")]
            int paletteIndex = 0,
            [Description("Index of the brush within the palette. Default 0.")]
            int brushIndex = 0,
            [Description("Alternative: search for a brush by name (case-insensitive partial match). Overrides paletteIndex/brushIndex if provided.")]
            string? brushName = null,
            [Description("Rotation in euler angles. Default (0,0,0).")]
            Vector3? rotation = null,
            [Description("Scale. Default (1,1,1).")]
            Vector3? scale = null,
            [Description("Item index within the brush (for multi-prefab brushes). Default 0.")]
            int itemIndex = 0,
            [Description("Parent GameObject reference. Null for scene root.")]
            GameObjectRef? parentGameObjectRef = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var palettes = PaletteManager.allPalettes;
                MultibrushSettings? brush = null;
                string paletteName = "";

                if (!string.IsNullOrEmpty(brushName))
                {
                    // Search by name across all palettes
                    var searchLower = brushName.ToLower();
                    foreach (var pal in palettes)
                    {
                        foreach (var b in pal.brushes)
                        {
                            if (b.name.ToLower().Contains(searchLower))
                            {
                                brush = b;
                                paletteName = pal.name;
                                break;
                            }
                        }
                        if (brush != null) break;
                    }
                    if (brush == null)
                        throw new System.Exception($"No brush found matching name '{brushName}' in any palette.");
                }
                else
                {
                    if (paletteIndex < 0 || paletteIndex >= palettes.Count)
                        throw new System.Exception($"Palette index {paletteIndex} out of range. Have {palettes.Count} palettes.");

                    var palette = palettes[paletteIndex];
                    paletteName = palette.name;
                    var brushes = palette.brushes;

                    if (brushIndex < 0 || brushIndex >= brushes.Length)
                        throw new System.Exception($"Brush index {brushIndex} out of range. Palette '{palette.name}' has {brushes.Length} brushes.");

                    brush = brushes[brushIndex];
                }

                var items = brush.items.Where(i => i.prefab != null).ToArray();
                if (items.Length == 0)
                    throw new System.Exception($"Brush '{brush.name}' has no valid prefab items.");

                if (itemIndex < 0 || itemIndex >= items.Length)
                    itemIndex = 0;

                var prefab = items[itemIndex].prefab;

                // Instantiate
                var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                Undo.RegisterCreatedObjectUndo(go, "PWB Place Brush");

                // Set parent
                if (parentGameObjectRef?.IsValid(out _) == true)
                {
                    var parentGo = parentGameObjectRef.FindGameObject(out var error);
                    if (error != null) throw new System.Exception(error);
                    if (parentGo != null)
                        go.transform.SetParent(parentGo.transform, false);
                }

                go.transform.position = position;
                go.transform.eulerAngles = rotation ?? Vector3.zero;
                go.transform.localScale = scale ?? Vector3.one;

                EditorUtility.SetDirty(go);

                return new PlaceBrushResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    prefabName = prefab.name,
                    brushName = brush.name,
                    paletteName = paletteName,
                    position = FormatVector3(go.transform.position)
                };
            });
        }

        public class PlaceBrushResponse
        {
            [Description("Name of the placed GameObject")]
            public string gameObjectName = "";
            [Description("Instance ID")]
            public int instanceId;
            [Description("Prefab name used")]
            public string prefabName = "";
            [Description("Brush name it came from")]
            public string brushName = "";
            [Description("Palette name")]
            public string paletteName = "";
            [Description("World position")]
            public string position = "";
        }
    }
}
#endif
