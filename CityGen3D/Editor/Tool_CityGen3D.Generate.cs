#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;

namespace MCPTools.CityGen3D.Editor
{
    public partial class Tool_CityGen3D
    {
        [McpPluginTool("cg-generate", Title = "CityGen3D / Generate")]
        [Description(@"Triggers a CityGen3D map generation.

action options:
  generate -- run the full Generator pipeline (Generate / GenerateMap / GenerateAll method on the Generator)
  clear    -- clear the existing map (Clear / ClearMap method on the Generator)
  cancel   -- stop / cancel an in-progress generation if available

Generation is editor-only and may take several seconds depending on map size and source data.
This tool runs synchronously on the main thread; consider committing scene changes after.")]
        public string Generate(
            [Description("'generate' | 'clear' | 'cancel'")]
            string action = "generate"
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireCityGen();
                action = (action ?? "generate").Trim().ToLowerInvariant();

                var gen = FindGenerator() ?? throw new System.Exception("No CityGen3D Generator in active scene.");

                string[] candidates = action switch
                {
                    "generate" => new[] { "Generate", "GenerateMap", "GenerateAll", "Build", "Run" },
                    "clear"    => new[] { "Clear", "ClearMap", "Reset", "ResetMap" },
                    "cancel"   => new[] { "Cancel", "CancelGenerate", "Stop", "Abort" },
                    _ => throw new System.Exception($"Unknown action '{action}'. Use 'generate' | 'clear' | 'cancel'.")
                };

                foreach (var name in candidates)
                {
                    var method = gen.GetType().GetMethod(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance, null, System.Type.EmptyTypes, null);
                    if (method == null) continue;
                    method.Invoke(gen, null);
                    EditorUtility.SetDirty(gen);
                    return $"OK: Generator.{name}() invoked.";
                }

                throw new System.Exception($"Could not find any of: {string.Join(", ", candidates)} on Generator (type {gen.GetType().FullName}). Use cg-generator-configure with action='list' to inspect available members.");
            });
        }
    }
}
