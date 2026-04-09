#nullable enable
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using AssetInventory;
using UnityEditor;
using UnityEngine;

namespace MCPTools.AssetInventory.Editor
{
    public partial class Tool_AssetInventory
    {
        [McpPluginTool("asset-inventory-import", Title = "Asset Inventory / Import Asset")]
        [Description(@"Imports a single asset from any indexed package into the project, even if the package is not installed.
Asset Inventory extracts the requested file and its dependencies from the cached package.
Use 'asset-inventory-search-prefabs' first to find the exact asset path.
Import is queued and runs asynchronously — check the Unity Console for completion.
Optionally adds the prefab to the scene after import completes.")]
        public ImportResponse ImportAsset(
            [Description("Exact asset path as returned by search, e.g. 'Assets/polyperfect/Low Poly Animated Animals/Prefabs/Animals/Fox.prefab'.")]
            string assetPath,
            [Description("Import with dependencies (materials, textures, meshes). Default true.")]
            bool withDependencies = true,
            [Description("Also add the prefab to the scene after import. Default false.")]
            bool addToScene = false,
            [Description("World position if adding to scene. Default (0,0,0).")]
            Vector3? position = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                // Use AssetSearch to find the asset — search by filename, filter by exact path
                var fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
                var normalizedTarget = assetPath.Replace("\\", "/");

                var options = new AssetSearch.Options
                {
                    SearchPhrase = fileName,
                    MaxResults = 0
                };

                var result = AssetSearch.Execute(options);

                var match = result.Files.FirstOrDefault(f =>
                {
                    var p = f.Path;
                    return p != null && p.Replace("\\", "/") == normalizedTarget;
                });

                // Try looser match via GetPath if exact path fails
                if (match == null)
                {
                    match = result.Files.FirstOrDefault(f =>
                    {
                        var p = f.GetPath(true);
                        return p != null && p.Replace("\\", "/") == normalizedTarget;
                    });
                }

                if (match == null)
                    throw new System.Exception($"Asset not found in database: '{assetPath}'. Use asset-inventory-search-prefabs to verify the exact path.");

                string importFolder = AI.Config.importFolder;
                if (string.IsNullOrEmpty(importFolder))
                    importFolder = "Assets";

                var worldPos = position ?? Vector3.zero;
                bool wantScene = addToScene;
                string packageName = match.SafeName ?? "unknown";

                // Queue async import via delayCall so it runs after MCP response returns
                EditorApplication.delayCall += async () =>
                {
                    try
                    {
                        Debug.Log($"[MCPTools] Importing '{match.FileName}' from '{packageName}'...");
                        string importedPath = await global::AssetInventory.Assets.CopyTo(match, importFolder, withDependencies);

                        if (string.IsNullOrEmpty(importedPath))
                        {
                            Debug.LogWarning($"[MCPTools] Import returned no path for '{match.FileName}'. Package may not be cached.");
                            return;
                        }

                        AssetDatabase.Refresh();
                        Debug.Log($"[MCPTools] Import complete: '{importedPath}'");

                        if (wantScene && importedPath.EndsWith(".prefab"))
                        {
                            EditorApplication.delayCall += () =>
                            {
                                AssetUtils.AddToScene(importedPath, worldPos);
                                Debug.Log($"[MCPTools] Added to scene at {worldPos}");
                            };
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[MCPTools] Import failed: {ex.Message}");
                    }
                };

                return new ImportResponse
                {
                    assetPath = assetPath,
                    packageName = packageName,
                    addToScene = addToScene,
                    message = $"Import queued for '{match.FileName}' from '{packageName}'. Watch the Unity Console for progress."
                };
            });
        }

        public class ImportResponse
        {
            [Description("Requested asset path")] public string assetPath = "";
            [Description("Source package name")] public string packageName = "";
            [Description("Whether it will be added to scene")] public bool addToScene;
            [Description("Status message")] public string message = "";
        }
    }
}
