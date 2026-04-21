#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.TextureStudio.Editor
{
    public partial class Tool_TextureStudio
    {
        [McpPluginTool("texstudio-render", Title = "Texture Studio / Render & Apply")]
        [Description(@"Renders the CompositeMap to texture and optionally applies it to linked materials or bakes to a PNG file.
Call this after texstudio-set-param to see changes reflected on materials.
width/height: override render dimensions (default: map's current size).
bakeToPath: saves a Texture2D as PNG at the given asset path (e.g. Assets/Textures/output.png).
applyToMaterials: pushes the rendered texture to all linked materials (default true).")]
        public string Render(
            [Description("Name of the CompositeMap asset.")] string assetName,
            [Description("Override render width in pixels.")] int? width = null,
            [Description("Override render height in pixels.")] int? height = null,
            [Description("Apply rendered texture to linked materials.")] bool applyToMaterials = true,
            [Description("Optional: Save baked PNG to this asset path (e.g. Assets/Textures/out.png).")] string? bakeToPath = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var map = GetMap(assetName);
                var sb = new StringBuilder();

                int w = width ?? (int)(Get(map, "width") ?? 512);
                int h = height ?? (int)(Get(map, "height") ?? 512);

                Call(map, "CreateMapTexture", w, h, false, false);
                Call(map, "UpdateTexture");
                sb.AppendLine($"OK: CompositeMap '{assetName}' rendered at {w}x{h}.");

                if (applyToMaterials)
                {
                    Call(map, "ApplyMap");
                    var mtms = Get(map, "mapToMaterials") as System.Collections.IList;
                    sb.AppendLine($"  Applied to {mtms?.Count ?? 0} material link(s).");
                }

                if (bakeToPath != null)
                {
                    var tex2d = Call(map, "CreateTexture", w, h) as Texture2D;
                    if (tex2d != null)
                    {
                        byte[] png = tex2d.EncodeToPNG();
                        string fullPath = bakeToPath;
                        if (!fullPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                            fullPath += ".png";

                        string dir = System.IO.Path.GetDirectoryName(fullPath) ?? "";
                        if (!string.IsNullOrEmpty(dir))
                        {
                            string fullDir = System.IO.Path.Combine(Application.dataPath, "..", dir);
                            System.IO.Directory.CreateDirectory(fullDir);
                        }

                        string diskPath = System.IO.Path.Combine(Application.dataPath, "..", fullPath);
                        System.IO.File.WriteAllBytes(diskPath, png);
                        AssetDatabase.Refresh();
                        sb.AppendLine($"  Baked PNG: {fullPath} ({png.Length / 1024}KB)");

                        Object.DestroyImmediate(tex2d);
                    }
                }

                EditorUtility.SetDirty(map);
                return sb.ToString();
            });
        }
    }
}
