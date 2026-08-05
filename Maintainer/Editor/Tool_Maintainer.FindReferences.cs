#if HAS_MAINTAINER
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using CodeStage.Maintainer.References;

namespace MCPTools.Maintainer.Editor
{
    public partial class Tool_Maintainer
    {
        [AiTool("maintainer-find-references", Title = "Maintainer / Find Asset References")]
        [Description(@"Answers 'what references this asset?' using Code Stage Maintainer's References Finder —
the single most useful check before safely deleting or refactoring an asset. Pass an asset project path
(e.g. 'Assets/Art/Player.prefab') or a plain asset name to resolve by search. Returns the target asset
followed by the reference-tree entries Maintainer produced, indented by tree depth. When no referencing
entries come back, the asset appears unused in the project (a safe-to-delete candidate — still verify).
Backed by Maintainer's Assets Map cache; the first call after large project changes may take longer while
the map rebuilds.")]
        public string FindReferences(
            [Description("Asset project path ('Assets/...') or a plain asset name to resolve by search.")]
            string assetPathOrName
        )
        {
            return MainThread.Instance.Run(() => RunSuppressed(() =>
            {
                string assetPath = ResolveAssetPath(assetPathOrName);
                ProjectReferenceItem[] items = ReferencesFinder.FindAssetReferences(assetPath, false);

                var sb = new StringBuilder();
                sb.AppendLine($"=== References for '{assetPath}' ===");

                if (items == null || items.Length == 0)
                {
                    sb.AppendLine("  (no data returned — the search was canceled or the asset is not in the map)");
                    return sb.ToString();
                }

                // The result is a flattened tree: Depth -1 is a synthetic root, Depth 0 is a target
                // asset, and Depth >= 1 are the assets/objects that reference it. AssetPath / Name / Depth
                // are the public surface of ProjectReferenceItem / TreeItem, so no reflection is needed.
                //
                // NOTE: Maintainer MERGES each search into an accumulated result set (it re-includes the
                // previous "last searched" targets), so `items` can carry Depth-0 targets from earlier
                // calls. We therefore render only the subtree of the asset we were actually asked about:
                // once a Depth-0 target matches `assetPath` we emit its referencing entries until the next
                // Depth-0 boundary.
                int referencing = 0;
                bool inRequested = false;
                bool sawRequested = false;
                foreach (var item in items)
                {
                    if (item == null || item.Depth < 0) continue; // skip nulls + synthetic root

                    string label = string.IsNullOrEmpty(item.AssetPath) ? item.Name : item.AssetPath;

                    if (item.Depth == 0)
                    {
                        inRequested = string.Equals(item.AssetPath, assetPath, StringComparison.OrdinalIgnoreCase);
                        if (inRequested)
                        {
                            sawRequested = true;
                            sb.AppendLine($"  TARGET: {label}");
                        }
                        continue;
                    }

                    if (!inRequested) continue; // referencing entry under some other (stale) target

                    referencing++;
                    var indent = new string(' ', 4 + (item.Depth - 1) * 2);
                    sb.AppendLine($"{indent}- {label}");
                }

                if (!sawRequested)
                {
                    sb.AppendLine($"  TARGET: {assetPath}");
                    sb.AppendLine("  (asset not found in the reference map — it may be excluded by the References filters, or the search was canceled)");
                    return sb.ToString();
                }

                sb.AppendLine($"  --> {referencing} referencing item(s) found.");
                if (referencing == 0)
                    sb.AppendLine("  Nothing references this asset — it appears unused (verify before deleting).");
                return sb.ToString();
            }));
        }
    }
}
#endif
