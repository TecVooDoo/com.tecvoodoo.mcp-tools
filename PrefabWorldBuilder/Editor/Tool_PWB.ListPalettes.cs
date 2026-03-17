#if HAS_PWB
#nullable enable
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using PluginMaster;

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
                var palettes = PaletteManager.allPalettes;
                var lines = new System.Collections.Generic.List<string>();

                for (int p = 0; p < palettes.Count; p++)
                {
                    var palette = palettes[p];
                    lines.Add($"Palette [{p}]: \"{palette.name}\" ({palette.brushCount} brushes, id={palette.hexId})");

                    if (includeBrushes)
                    {
                        var brushes = palette.brushes;
                        for (int b = 0; b < brushes.Length; b++)
                        {
                            var brush = brushes[b];
                            var prefabNames = string.Join(", ",
                                brush.items
                                    .Where(i => i.prefab != null)
                                    .Select(i => i.prefab.name));
                            lines.Add($"  Brush [{b}]: \"{brush.name}\" ({brush.itemCount} items: {prefabNames})");
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
#endif
