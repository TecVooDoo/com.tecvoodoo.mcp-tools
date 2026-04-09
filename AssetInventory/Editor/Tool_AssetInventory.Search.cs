#nullable enable
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using AssetInventory;
using UnityEditor;

namespace MCPTools.AssetInventory.Editor
{
    public partial class Tool_AssetInventory
    {
        [McpPluginTool("asset-inventory-search", Title = "Asset Inventory / Search Assets")]
        [Description(@"Searches the Asset Inventory database for assets by name, type, or tag.
Returns matching files with their asset paths, package names, and metadata.
Use this to find prefabs, textures, models, or any indexed asset across all installed packages.
The search uses the Asset Inventory 4 indexed database for fast results.")]
        public SearchResponse SearchAssets(
            [Description("Search phrase (name, partial match). e.g. 'chair', 'SM_Bld', 'tree'.")]
            string searchPhrase,
            [Description("Filter by file type: 'Prefabs', 'Models', 'Images', 'Materials', 'Audio', 'Scripts', etc. Null for all types.")]
            string? fileType = null,
            [Description("Filter results by package name (partial match, case-insensitive). e.g. 'polyperfect', 'synty'. Null for all packages.")]
            string? packageFilter = null,
            [Description("Maximum number of results to return. Default 20.")]
            int maxResults = 20
        )
        {
            return MainThread.Instance.Run(() =>
            {
                bool hasPackageFilter = !string.IsNullOrEmpty(packageFilter);
                var options = new AssetSearch.Options
                {
                    SearchPhrase = searchPhrase,
                    MaxResults = hasPackageFilter ? 0 : maxResults,
                    RawSearchType = fileType
                };

                var result = AssetSearch.Execute(options);

                var files = result.Files.AsEnumerable();
                if (hasPackageFilter)
                    files = files.Where(f => (f.SafeName ?? "").ToLower().Contains(packageFilter!.ToLower()));

                var lines = files.Take(maxResults).Select(f =>
                {
                    var pkg = f.SafeName ?? "unknown";
                    var path = f.GetPath(true) ?? f.FileName ?? "?";
                    var type = f.Type ?? "?";
                    return $"[{type}] {path} (pkg: {pkg})";
                }).ToArray();

                return new SearchResponse
                {
                    resultCount = result.ResultCount,
                    returnedCount = lines.Length,
                    results = string.Join("\n", lines)
                };
            });
        }

        [McpPluginTool("asset-inventory-search-prefabs", Title = "Asset Inventory / Search Prefabs")]
        [Description(@"Searches specifically for prefab assets by name. Returns asset paths ready for instantiation.
This is a convenience wrapper that filters to Prefabs type only.")]
        public SearchResponse SearchPrefabs(
            [Description("Search phrase for prefab names. e.g. 'chair', 'wall', 'tree'.")]
            string searchPhrase,
            [Description("Filter results by package name (partial match, case-insensitive). e.g. 'polyperfect', 'synty'. Null for all packages.")]
            string? packageFilter = null,
            [Description("Maximum results. Default 20.")]
            int maxResults = 20
        )
        {
            return MainThread.Instance.Run(() =>
            {
                bool hasPackageFilter = !string.IsNullOrEmpty(packageFilter);
                var options = new AssetSearch.Options
                {
                    SearchPhrase = searchPhrase,
                    MaxResults = hasPackageFilter ? 0 : maxResults,
                    RawSearchType = "Prefabs"
                };

                var result = AssetSearch.Execute(options);

                var files = result.Files.AsEnumerable();
                if (hasPackageFilter)
                    files = files.Where(f => (f.SafeName ?? "").ToLower().Contains(packageFilter!.ToLower()));

                var lines = files.Take(maxResults).Select(f =>
                {
                    var path = f.GetPath(true) ?? f.FileName ?? "?";
                    var pkg = f.SafeName ?? "unknown";
                    return $"{path} (pkg: {pkg})";
                }).ToArray();

                return new SearchResponse
                {
                    resultCount = result.ResultCount,
                    returnedCount = lines.Length,
                    results = string.Join("\n", lines)
                };
            });
        }

        [McpPluginTool("asset-inventory-list-packages", Title = "Asset Inventory / List Packages")]
        [Description(@"Lists all indexed packages in the Asset Inventory database.
Shows package names, asset counts, and status. Useful for understanding what content is available.")]
        public ListPackagesResponse ListPackages(
            [Description("Filter packages by name (partial match). Null for all.")]
            string? nameFilter = null,
            [Description("Maximum packages to list. Default 50.")]
            int maxResults = 50
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var allAssets = Assets.Load().ToList();
                var packages = allAssets
                    .Where(a => a.IsIndexed && a.SafeName != Asset.NONE)
                    .Where(a => nameFilter == null || (a.SafeName ?? "").ToLower().Contains(nameFilter.ToLower()))
                    .Take(maxResults)
                    .Select(a => $"[{a.Id}] {a.SafeName} (v{a.GetVersion()}, files: {a.FileCount})")
                    .ToArray();

                return new ListPackagesResponse
                {
                    packageCount = packages.Length,
                    details = string.Join("\n", packages)
                };
            });
        }

        public class SearchResponse
        {
            [Description("Total matching results in database")] public int resultCount;
            [Description("Number of results returned")] public int returnedCount;
            [Description("Search results with paths and package names")] public string results = "";
        }

        public class ListPackagesResponse
        {
            [Description("Number of packages listed")] public int packageCount;
            [Description("Package details")] public string details = "";
        }
    }
}
