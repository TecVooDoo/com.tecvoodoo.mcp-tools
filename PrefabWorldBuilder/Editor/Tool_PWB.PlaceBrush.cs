#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
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
                var palettes = GetStatic(PaletteManagerType, "allPalettes") as IList;
                if (palettes == null)
                    throw new Exception("Could not retrieve PaletteManager.allPalettes.");

                object? brush = null;
                string paletteName = "";

                if (!string.IsNullOrEmpty(brushName))
                {
                    var searchLower = brushName!.ToLower();
                    foreach (var pal in palettes)
                    {
                        if (pal == null) continue;
                        var brushes = Get(pal, "brushes") as Array;
                        if (brushes == null) continue;
                        foreach (var b in brushes)
                        {
                            if (b == null) continue;
                            string bName = Get(b, "name")?.ToString() ?? "";
                            if (bName.ToLower().Contains(searchLower))
                            {
                                brush = b;
                                paletteName = Get(pal, "name")?.ToString() ?? "";
                                break;
                            }
                        }
                        if (brush != null) break;
                    }
                    if (brush == null)
                        throw new Exception($"No brush found matching name '{brushName}' in any palette.");
                }
                else
                {
                    if (paletteIndex < 0 || paletteIndex >= palettes.Count)
                        throw new Exception($"Palette index {paletteIndex} out of range. Have {palettes.Count} palettes.");

                    var palette = palettes[paletteIndex]!;
                    paletteName = Get(palette, "name")?.ToString() ?? "";
                    var brushes = Get(palette, "brushes") as Array;
                    if (brushes == null)
                        throw new Exception($"Could not read brushes from palette '{paletteName}'.");

                    if (brushIndex < 0 || brushIndex >= brushes.Length)
                        throw new Exception($"Brush index {brushIndex} out of range. Palette '{paletteName}' has {brushes.Length} brushes.");

                    brush = brushes.GetValue(brushIndex)!;
                }

                // Get valid items (those with non-null prefab)
                var items = Get(brush, "items") as IList;
                var validItems = new System.Collections.Generic.List<object>();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item == null) continue;
                        var prefab = Get(item, "prefab") as GameObject;
                        if (prefab != null)
                            validItems.Add(item);
                    }
                }
                if (validItems.Count == 0)
                    throw new Exception($"Brush '{Get(brush, "name")}' has no valid prefab items.");

                if (itemIndex < 0 || itemIndex >= validItems.Count)
                    itemIndex = 0;

                var selectedPrefab = (GameObject)Get(validItems[itemIndex], "prefab")!;

                var go = (GameObject)PrefabUtility.InstantiatePrefab(selectedPrefab);
                Undo.RegisterCreatedObjectUndo(go, "PWB Place Brush");

                if (parentGameObjectRef?.IsValid(out _) == true)
                {
                    var parentGo = parentGameObjectRef.FindGameObject(out var error);
                    if (error != null) throw new Exception(error);
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
                    prefabName = selectedPrefab.name,
                    brushName = Get(brush, "name")?.ToString() ?? "",
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
