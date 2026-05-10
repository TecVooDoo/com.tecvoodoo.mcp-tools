#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
#if MCP_HAS_AIGD
using AIGD;
#else
using com.IvanMurzak.Unity.MCP.Runtime.Data;
#endif
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using UnityEditor;
using UnityEngine;

namespace MCPTools.PWB.Editor
{
    public partial class Tool_PWB
    {
        [McpPluginTool("pwb-place-line", Title = "PWB / Place Brush in Line")]
        [Description(@"Places multiple instances of a PWB brush prefab in a line.
Specify start and end positions and the number of instances.
Objects are evenly spaced along the line. Great for fences, walls, paths, etc.")]
        public PlaceLineResponse PlaceLine(
            [Description("Start position of the line.")]
            Vector3 startPosition,
            [Description("End position of the line.")]
            Vector3 endPosition,
            [Description("Number of instances to place. Default 5.")]
            int count = 5,
            [Description("Index of the palette. Default 0.")]
            int paletteIndex = 0,
            [Description("Index of the brush within the palette. Default 0.")]
            int brushIndex = 0,
            [Description("Alternative: search for a brush by name (overrides palette/brush index).")]
            string? brushName = null,
            [Description("If true, objects face along the line direction. Default false.")]
            bool alignToLine = false,
            [Description("Scale. Default (1,1,1).")]
            Vector3? scale = null,
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
                                break;
                            }
                        }
                        if (brush != null) break;
                    }
                    if (brush == null)
                        throw new Exception($"No brush found matching '{brushName}'.");
                }
                else
                {
                    if (paletteIndex < 0 || paletteIndex >= palettes.Count)
                        throw new Exception($"Palette index {paletteIndex} out of range.");
                    var palette = palettes[paletteIndex]!;
                    var brushes = Get(palette, "brushes") as Array;
                    if (brushes == null)
                        throw new Exception("Could not read brushes from palette.");
                    if (brushIndex < 0 || brushIndex >= brushes.Length)
                        throw new Exception($"Brush index {brushIndex} out of range.");
                    brush = brushes.GetValue(brushIndex)!;
                }

                // Get valid items (those with non-null prefab)
                var items = Get(brush, "items") as IList;
                var validItems = new System.Collections.Generic.List<GameObject>();
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        if (item == null) continue;
                        var prefab = Get(item, "prefab") as GameObject;
                        if (prefab != null)
                            validItems.Add(prefab);
                    }
                }
                if (validItems.Count == 0)
                    throw new Exception($"Brush '{Get(brush, "name")}' has no valid prefabs.");

                GameObject? parentGo = null;
                if (parentGameObjectRef?.IsValid(out _) == true)
                {
                    parentGo = parentGameObjectRef.FindGameObject(out var error);
                    if (error != null) throw new Exception(error);
                }

                string brushNameStr = Get(brush, "name")?.ToString() ?? "brush";
                var container = new GameObject($"PWBLine_{brushNameStr}");
                Undo.RegisterCreatedObjectUndo(container, "PWB Place Line");
                if (parentGo != null)
                    container.transform.SetParent(parentGo.transform, false);

                container.transform.position = (startPosition + endPosition) / 2f;

                var direction = endPosition - startPosition;
                var lineRotation = alignToLine && direction.sqrMagnitude > 0.001f
                    ? Quaternion.LookRotation(direction.normalized)
                    : Quaternion.identity;

                int placed = 0;
                for (int i = 0; i < count; i++)
                {
                    float t = count > 1 ? i / (float)(count - 1) : 0.5f;
                    var pos = Vector3.Lerp(startPosition, endPosition, t);

                    var prefab = validItems[i % validItems.Count];
                    var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    Undo.RegisterCreatedObjectUndo(go, "PWB Place Line Item");
                    go.transform.SetParent(container.transform);
                    go.transform.position = pos;
                    go.transform.rotation = lineRotation;
                    go.transform.localScale = scale ?? Vector3.one;
                    placed++;
                }

                EditorUtility.SetDirty(container);

                return new PlaceLineResponse
                {
                    containerName = container.name,
                    containerInstanceId = container.GetInstanceID(),
                    brushName = brushNameStr,
                    placedCount = placed,
                    startPosition = FormatVector3(startPosition),
                    endPosition = FormatVector3(endPosition)
                };
            });
        }

        [McpPluginTool("pwb-add-to-palette", Title = "PWB / Add Prefab to Palette")]
        [Description(@"Adds a prefab asset to a PWB palette as a new brush.
The prefab is specified by its asset path. Use this to populate palettes programmatically.")]
        public AddToPaletteResponse AddToPalette(
            [Description("Asset path to the prefab (e.g. 'Assets/Synty/.../SM_Something.prefab').")]
            string prefabPath,
            [Description("Index of the target palette. Default 0.")]
            int paletteIndex = 0
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                    throw new Exception($"Prefab not found at: {prefabPath}");

                var palettes = GetStatic(PaletteManagerType, "allPalettes") as IList;
                if (palettes == null)
                    throw new Exception("Could not retrieve PaletteManager.allPalettes.");

                if (palettes.Count == 0)
                {
                    // Create a new palette: new PaletteData("Palette1", DateTime.Now.ToBinary())
                    var paletteDataType = FindType(PALETTE_DATA_TYPE);
                    if (paletteDataType == null)
                        throw new Exception($"Type '{PALETTE_DATA_TYPE}' not found.");
                    var newPalette = Activator.CreateInstance(paletteDataType, "Palette1", DateTime.Now.ToBinary());
                    // PaletteManager.AddPalette(newPalette, save: true)
                    CallStatic(PaletteManagerType, "AddPalette", newPalette!, true);
                    palettes = GetStatic(PaletteManagerType, "allPalettes") as IList;
                    if (palettes == null)
                        throw new Exception("Could not retrieve palettes after creating one.");
                }
                if (paletteIndex < 0 || paletteIndex >= palettes.Count)
                    throw new Exception($"Palette index {paletteIndex} out of range. Have {palettes.Count} palettes.");

                var palette = palettes[paletteIndex]!;
                string palName = Get(palette, "name")?.ToString() ?? "";

                // Check if already in palette
                var brushes = Get(palette, "brushes") as Array;
                if (brushes != null)
                {
                    foreach (var b in brushes)
                    {
                        if (b == null) continue;
                        bool contains = (bool)(Call(b, "ContainsPrefabPath", prefabPath) ?? false);
                        if (contains)
                            throw new Exception($"Prefab '{prefab.name}' already exists in palette '{palName}'.");
                    }
                }

                // MultibrushSettings.Create(prefab, palette)
                var multibrushType = FindType(MULTIBRUSH_TYPE);
                if (multibrushType == null)
                    throw new Exception($"Type '{MULTIBRUSH_TYPE}' not found.");
                var newBrush = CallStatic(multibrushType, "Create", prefab, palette);
                if (newBrush == null)
                    throw new Exception("MultibrushSettings.Create returned null.");

                // palette.AddBrush(brush)
                Call(palette, "AddBrush", newBrush);

                string newBrushName = Get(newBrush, "name")?.ToString() ?? "";
                int totalBrushes = (int)(Get(palette, "brushCount") ?? 0);

                return new AddToPaletteResponse
                {
                    prefabName = prefab.name,
                    paletteName = palName,
                    brushName = newBrushName,
                    totalBrushes = totalBrushes
                };
            });
        }

        public class PlaceLineResponse
        {
            [Description("Name of the container GameObject")]
            public string containerName = "";
            [Description("Instance ID of the container")]
            public int containerInstanceId;
            [Description("Brush name used")]
            public string brushName = "";
            [Description("Number of instances placed")]
            public int placedCount;
            [Description("Start position")]
            public string startPosition = "";
            [Description("End position")]
            public string endPosition = "";
        }

        public class AddToPaletteResponse
        {
            [Description("Prefab name added")]
            public string prefabName = "";
            [Description("Palette it was added to")]
            public string paletteName = "";
            [Description("Brush name created")]
            public string brushName = "";
            [Description("Total brushes in palette")]
            public int totalBrushes;
        }
    }
}
