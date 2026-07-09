#if HAS_MAINTAINER
#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using CodeStage.Maintainer.Cleaner;

namespace MCPTools.Maintainer.Editor
{
    public partial class Tool_Maintainer
    {
        [McpPluginTool("maintainer-scan-unused", Title = "Maintainer / Scan Unused Assets")]
        [Description(@"Runs Code Stage Maintainer's Project Cleaner in REPORT-ONLY mode and lists the
unreferenced / garbage assets and empty folders it flags as safe-to-remove candidates. This tool NEVER
deletes anything — it only reports; deletion stays a deliberate, human-reviewed step in the Maintainer
window. Uses the current Project Cleaner settings / filters; backed by the Assets Map, so the first run
may take longer while the cache rebuilds.")]
        public string ScanUnused()
        {
            return MainThread.Instance.Run(() => RunSuppressed(() =>
            {
                // We call StartSearch + format the records ourselves rather than
                // ProjectCleaner.SearchAndReport(): that convenience method stringifies every record via
                // AssetRecord.ConstructHeader, which dereferences a null 'assetType' for unreferenced
                // assets whose type can't be resolved -> NullReferenceException (Maintainer 2.3.3).
                // AssetRecord.AssetPath / RecordBase.GetCompactLine() are the public, NRE-free accessors.
                CleanerRecord[] records = ProjectCleaner.StartSearch(false);

                var sb = new StringBuilder();
                sb.AppendLine("=== Project Cleaner: unused-asset / empty-folder candidates (report only) ===");

                if (records == null || records.Length == 0)
                {
                    sb.AppendLine("  No garbage found (or the search was canceled).");
                    return sb.ToString();
                }

                var countsByType = new Dictionary<RecordType, int>();
                foreach (var record in records)
                {
                    if (record == null) continue;

                    countsByType.TryGetValue(record.Type, out var count);
                    countsByType[record.Type] = count + 1;

                    string label;
                    if (record is AssetRecord asset && !string.IsNullOrEmpty(asset.AssetPath))
                    {
                        label = asset.AssetPath;
                    }
                    else
                    {
                        try { label = record.GetCompactLine(); }
                        catch { label = "(unprintable record)"; }
                    }

                    sb.AppendLine($"  [{record.Type}] {label}");
                }

                sb.AppendLine("  ---");
                sb.AppendLine($"  {records.Length} candidate item(s):");
                foreach (var kv in countsByType)
                    sb.AppendLine($"    {kv.Key}: {kv.Value}");
                sb.AppendLine("  Nothing was deleted — review candidates in the Maintainer window before removing.");
                return sb.ToString();
            }));
        }
    }
}
#endif
