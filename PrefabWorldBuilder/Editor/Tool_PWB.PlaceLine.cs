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
                var palettes = PaletteManager.allPalettes;
                MultibrushSettings? brush = null;

                if (!string.IsNullOrEmpty(brushName))
                {
                    var searchLower = brushName.ToLower();
                    foreach (var pal in palettes)
                    {
                        foreach (var b in pal.brushes)
                        {
                            if (b.name.ToLower().Contains(searchLower))
                            {
                                brush = b;
                                break;
                            }
                        }
                        if (brush != null) break;
                    }
                    if (brush == null)
                        throw new System.Exception($"No brush found matching '{brushName}'.");
                }
                else
                {
                    if (paletteIndex < 0 || paletteIndex >= palettes.Count)
                        throw new System.Exception($"Palette index {paletteIndex} out of range.");
                    var palette = palettes[paletteIndex];
                    var brushes = palette.brushes;
                    if (brushIndex < 0 || brushIndex >= brushes.Length)
                        throw new System.Exception($"Brush index {brushIndex} out of range.");
                    brush = brushes[brushIndex];
                }

                var items = brush.items.Where(i => i.prefab != null).ToArray();
                if (items.Length == 0)
                    throw new System.Exception($"Brush '{brush.name}' has no valid prefabs.");

                GameObject? parentGo = null;
                if (parentGameObjectRef?.IsValid(out _) == true)
                {
                    parentGo = parentGameObjectRef.FindGameObject(out var error);
                    if (error != null) throw new System.Exception(error);
                }

                // Create a container
                var container = new GameObject($"PWBLine_{brush.name}");
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

                    // Cycle through items for multi-prefab brushes
                    var prefab = items[i % items.Length].prefab;
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
                    brushName = brush.name,
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
                    throw new System.Exception($"Prefab not found at: {prefabPath}");

                var palettes = PaletteManager.allPalettes;
                if (palettes.Count == 0)
                {
                    var newPalette = new PaletteData("Palette1", System.DateTime.Now.ToBinary());
                    PaletteManager.AddPalette(newPalette, save: true);
                    palettes = PaletteManager.allPalettes;
                }
                if (paletteIndex < 0 || paletteIndex >= palettes.Count)
                    throw new System.Exception($"Palette index {paletteIndex} out of range. Have {palettes.Count} palettes.");

                var palette = palettes[paletteIndex];

                // Check if already in palette
                if (palette.brushes.Any(b => b.ContainsPrefabPath(prefabPath)))
                    throw new System.Exception($"Prefab '{prefab.name}' already exists in palette '{palette.name}'.");

                var brush = MultibrushSettings.Create(prefab, palette);
                palette.AddBrush(brush);

                return new AddToPaletteResponse
                {
                    prefabName = prefab.name,
                    paletteName = palette.name,
                    brushName = brush.name,
                    totalBrushes = palette.brushCount
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
#endif
