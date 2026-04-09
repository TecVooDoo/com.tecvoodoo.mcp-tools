#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace MCPTools.PWB.Editor
{
    public partial class Tool_PWB
    {
        [McpPluginTool("pwb-list-palettes", Title = "PWB / List Palettes")]
        [Description(@"Lists all Prefab World Builder palettes and their brushes.
Each palette contains brushes, and each brush references one or more prefabs.
Use the brush name or palette index with other PWB tools.")]
        public ListPalettesResponse ListPalettes(
            [Description("If true, also list individual brushes in each palette. Default true.")]
            bool includeBrushes = true
        )
        {
            return MainThread.Instance.Run(() =>
            {
                // PaletteManager.allPalettes (static property)
                var palettes = GetStatic(PaletteManagerType, "allPalettes") as IList;
                if (palettes == null)
                    throw new Exception("Could not retrieve PaletteManager.allPalettes.");

                var lines = new System.Collections.Generic.List<string>();

                for (int p = 0; p < palettes.Count; p++)
                {
                    var palette = palettes[p]!;
                    string palName  = Get(palette, "name")?.ToString() ?? "?";
                    int brushCount  = (int)(Get(palette, "brushCount") ?? 0);
                    string hexId    = Get(palette, "hexId")?.ToString() ?? "?";
                    lines.Add($"Palette [{p}]: \"{palName}\" ({brushCount} brushes, id={hexId})");

                    if (includeBrushes)
                    {
                        var brushes = Get(palette, "brushes") as Array;
                        if (brushes != null)
                        {
                            for (int b = 0; b < brushes.Length; b++)
                            {
                                var brush = brushes.GetValue(b)!;
                                string bName = Get(brush, "name")?.ToString() ?? "?";
                                int itemCount = (int)(Get(brush, "itemCount") ?? 0);

                                // Collect prefab names from items
                                var items = Get(brush, "items") as IList;
                                var prefabNames = new System.Collections.Generic.List<string>();
                                if (items != null)
                                {
                                    foreach (var item in items)
                                    {
                                        if (item == null) continue;
                                        var prefab = Get(item, "prefab") as UnityEngine.GameObject;
                                        if (prefab != null)
                                            prefabNames.Add(prefab.name);
                                    }
                                }
                                lines.Add($"  Brush [{b}]: \"{bName}\" ({itemCount} items: {string.Join(", ", prefabNames)})");
                            }
                        }
                    }
                }

                return new ListPalettesResponse
                {
                    paletteCount = palettes.Count,
                    details = string.Join("\n", lines)
                };
            });
        }

        public class ListPalettesResponse
        {
            [Description("Number of palettes found")]
            public int paletteCount;
            [Description("Detailed palette and brush listing")]
            public string details = "";
        }
    }
}
