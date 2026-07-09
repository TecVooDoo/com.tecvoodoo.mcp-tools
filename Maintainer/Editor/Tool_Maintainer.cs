#if HAS_MAINTAINER
#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEditor;

namespace MCPTools.Maintainer.Editor
{
    [McpPluginToolType]
    public partial class Tool_Maintainer
    {
        // Maintainer's global dialog-suppression flag (CodeStage.Maintainer.Maintainer.SuppressDialogs)
        // is internal, so we can't touch it from this assembly at compile time. Reach it best-effort
        // via reflection so agent-driven scans don't stall on a modal prompt. If the internal API ever
        // moves, the reflection quietly no-ops and the tool still runs (a dialog may appear).
        static PropertyInfo? SuppressDialogsProperty()
        {
            try
            {
                var maintainerType = typeof(CodeStage.Maintainer.Issues.IssuesFinder).Assembly
                    .GetType("CodeStage.Maintainer.Maintainer");
                return maintainerType?.GetProperty("SuppressDialogs",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            }
            catch
            {
                return null;
            }
        }

        // Runs <paramref name="body"/> with Maintainer dialogs suppressed, restoring the prior flag
        // value afterwards so we don't leave the editor in a surprising state for later manual use.
        static string RunSuppressed(Func<string> body)
        {
            var prop = SuppressDialogsProperty();
            object? previous = null;
            bool applied = false;
            try
            {
                if (prop != null)
                {
                    previous = prop.GetValue(null);
                    prop.SetValue(null, true);
                    applied = true;
                }
                return body();
            }
            finally
            {
                if (applied && prop != null)
                {
                    try { prop.SetValue(null, previous); }
                    catch { /* best-effort restore */ }
                }
            }
        }

        // Resolves a user-supplied asset reference (a project path or a plain asset name) to a concrete
        // "Assets/..."-rooted path. Exact paths win; otherwise falls back to a name search. Throws when
        // nothing matches so the caller surfaces a clear error instead of scanning the wrong thing.
        static string ResolveAssetPath(string assetPathOrName)
        {
            if (string.IsNullOrWhiteSpace(assetPathOrName))
                throw new Exception("assetPathOrName is required.");

            if (AssetDatabase.IsValidFolder(assetPathOrName)
                || AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPathOrName) != null)
                return assetPathOrName;

            var query = System.IO.Path.GetFileNameWithoutExtension(assetPathOrName);
            var guids = AssetDatabase.FindAssets(query);
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.Equals(System.IO.Path.GetFileNameWithoutExtension(path), query,
                        StringComparison.OrdinalIgnoreCase))
                    return path;
            }

            if (guids.Length > 0)
                return AssetDatabase.GUIDToAssetPath(guids[0]);

            throw new Exception($"No asset found for '{assetPathOrName}' (tried exact path then name search).");
        }
    }
}
#endif
