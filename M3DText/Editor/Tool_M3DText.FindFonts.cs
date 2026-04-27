#nullable enable
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;

namespace MCPTools.M3DText.Editor
{
    public partial class Tool_M3DText
    {
        [McpPluginTool("m3dt-find-fonts", Title = "Modular 3D Text / Find Fonts")]
        [Description(@"Lists Modular 3D Text Font assets (TinyGiantStudio.Text.Font ScriptableObjects) in the project.

Use to discover available fonts before calling m3dt-configure with the font name.
Each font listed with its asset path; counts character coverage if available.

If filter is provided, narrows the search to fonts whose name contains the filter (case-insensitive).")]
        public string FindFonts(
            [Description("Optional filename filter (substring, case-insensitive).")]
            string? filter = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (FontType == null)
                    return "TinyGiantStudio.Text.Font type not found. Modular 3D Text may not be installed.";

                var sb = new StringBuilder();
                var fonts = ListAllFonts();

                if (!string.IsNullOrEmpty(filter))
                {
                    var f = filter.ToLowerInvariant();
                    fonts = fonts.Where(x => x.name.ToLowerInvariant().Contains(f)).ToList();
                }

                sb.AppendLine($"Modular 3D Text fonts in project ({fonts.Count}):");
                foreach (var font in fonts)
                {
                    var path = AssetDatabase.GetAssetPath(font);
                    var chars = Get(font, "characters") as System.Collections.ICollection;
                    int charCount = chars?.Count ?? 0;
                    sb.AppendLine($"  - {font.name}  (chars={charCount})  [{path}]");
                }
                return sb.ToString();
            });
        }
    }
}
