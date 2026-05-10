#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Editor
{
    /// <summary>
    /// Auto-syncs precompiledReferences across every TMCP tool-group asmdef to match
    /// whatever McpPlugin / McpPlugin.Common / ReflectorNet DLL filenames are currently
    /// present under Assets/Plugins/NuGet/. MCP releases ship the same logical DLLs
    /// under different filenames across versions (some versioned, some not), so a static
    /// list goes stale on every bump. This sync makes the asmdefs a function of the
    /// project's current state instead of a hard-coded version.
    ///
    /// Triggers:
    ///   1. [InitializeOnLoad] -- runs on every domain reload, including the one right
    ///      after a package upgrade.
    ///   2. Tools > TecVooDoo > Sync MCP DLL References -- manual button.
    ///
    /// Idempotent: if the asmdef refs already match the discovered DLL filenames, no
    /// write happens. No infinite-recompile loop.
    /// </summary>
    [InitializeOnLoad]
    static class MCPToolsAsmdefSync
    {
        const string NugetRoot = "Assets/Plugins/NuGet";
        const string AsmdefPattern = "MCPTools.*.asmdef";

        static MCPToolsAsmdefSync()
        {
            EditorApplication.delayCall += () => Sync(verbose: false);
        }

        [MenuItem("Tools/TecVooDoo/Sync MCP DLL References")]
        public static void SyncMenu() => Sync(verbose: true);

        static void Sync(bool verbose)
        {
            List<string> targetRefs = ScanCurrentDllFilenames();
            if (targetRefs.Count == 0)
            {
                if (verbose) Debug.LogWarning($"[TMCP] No McpPlugin / ReflectorNet DLLs found under {NugetRoot}; asmdef sync skipped.");
                return;
            }

            string? packageRoot = GetTmcpPackageRoot();
            if (packageRoot == null)
            {
                if (verbose) Debug.LogWarning("[TMCP] Couldn't locate TMCP package root; asmdef sync skipped.");
                return;
            }

            int updated = 0;
            foreach (string asmdefPath in Directory.EnumerateFiles(packageRoot, AsmdefPattern, SearchOption.AllDirectories))
            {
                if (UpdateAsmdef(asmdefPath, targetRefs))
                    updated++;
            }

            if (updated > 0)
            {
                AssetDatabase.Refresh();
                Debug.Log($"[TMCP] Synced {updated} asmdef(s) to current MCP DLL filenames: [{string.Join(", ", targetRefs)}].");
            }
            else if (verbose)
            {
                Debug.Log($"[TMCP] All asmdefs already match current MCP DLL filenames: [{string.Join(", ", targetRefs)}].");
            }
        }

        // Scans Assets/Plugins/NuGet/ recursively for McpPlugin / McpPlugin.Common / ReflectorNet DLLs
        // and returns the filename list ordered as: McpPlugin*, McpPlugin.Common*, ReflectorNet*.
        static List<string> ScanCurrentDllFilenames()
        {
            var result = new List<string>();
            if (!Directory.Exists(NugetRoot)) return result;

            var mcp = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            var mcpCommon = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            var reflector = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string path in Directory.EnumerateFiles(NugetRoot, "*.dll", SearchOption.AllDirectories))
            {
                string name = Path.GetFileName(path);
                // McpPlugin.Common must be checked BEFORE McpPlugin (prefix overlap).
                if (name.StartsWith("McpPlugin.Common", StringComparison.OrdinalIgnoreCase))
                    mcpCommon.Add(name);
                else if (name.StartsWith("McpPlugin", StringComparison.OrdinalIgnoreCase))
                    mcp.Add(name);
                else if (name.StartsWith("ReflectorNet", StringComparison.OrdinalIgnoreCase))
                    reflector.Add(name);
            }

            result.AddRange(mcp);
            result.AddRange(mcpCommon);
            result.AddRange(reflector);
            return result;
        }

        // Walks up from MCPTools.Editor.asmdef to find the package root folder.
        static string? GetTmcpPackageRoot()
        {
            string[] guids = AssetDatabase.FindAssets("MCPTools.Editor t:asmdef");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(path) == "MCPTools.Editor.asmdef")
                {
                    // .../<package>/Editor/MCPTools.Editor.asmdef -> <package>
                    string editorDir = Path.GetDirectoryName(path) ?? "";
                    string packageDir = Path.GetDirectoryName(editorDir) ?? "";
                    return string.IsNullOrEmpty(packageDir) ? null : packageDir;
                }
            }
            return null;
        }

        // Returns true if the asmdef was modified.
        static bool UpdateAsmdef(string asmdefPath, List<string> targetManagedRefs)
        {
            string content = File.ReadAllText(asmdefPath);
            Match match = Regex.Match(content, @"""precompiledReferences""\s*:\s*\[(?<body>[^\]]*)\]", RegexOptions.Singleline);
            if (!match.Success) return false;

            // Extract current entries from the array body.
            var current = Regex.Matches(match.Groups["body"].Value, @"""([^""]+)""")
                .Cast<Match>()
                .Select(m => m.Groups[1].Value)
                .ToList();

            // Only manage asmdefs that already reference at least one of our families.
            bool managesAny = current.Any(IsManagedFamilyName);
            if (!managesAny) return false;

            // Preserve any non-managed entries in their original order.
            var preserved = current.Where(name => !IsManagedFamilyName(name)).ToList();
            var desired = targetManagedRefs.Concat(preserved).ToList();

            if (desired.SequenceEqual(current, StringComparer.Ordinal)) return false;

            string refLines = string.Join(",\n", desired.Select(r => $"        \"{r}\""));
            string newBlock = $"\"precompiledReferences\": [\n{refLines}\n    ]";

            string newContent = Regex.Replace(content,
                @"""precompiledReferences""\s*:\s*\[[^\]]*\]",
                newBlock,
                RegexOptions.Singleline);

            File.WriteAllText(asmdefPath, newContent);
            return true;
        }

        static bool IsManagedFamilyName(string dllName)
        {
            return dllName.StartsWith("McpPlugin", StringComparison.OrdinalIgnoreCase)
                || dllName.StartsWith("ReflectorNet", StringComparison.OrdinalIgnoreCase);
        }
    }
}
