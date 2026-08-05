#if HAS_MAINTAINER
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using CodeStage.Maintainer.Issues;

namespace MCPTools.Maintainer.Editor
{
    public partial class Tool_Maintainer
    {
        [AiTool("maintainer-scan-issues", Title = "Maintainer / Scan Project Issues")]
        [Description(@"Runs Code Stage Maintainer's Issues Finder over the project and returns the text
report (the same content the Maintainer window exports). Detects: missing scripts / prefabs /
references, missing or duplicate components, invalid or duplicate layers and sorting layers, shader
errors, huge transform positions, inconsistent terrain data, deleted build scenes, and more.
Read-only — it reports issues, it does not fix them. Search scope (assets / scenes / project settings)
follows the Maintainer window's current settings.")]
        public string ScanIssues()
        {
            return MainThread.Instance.Run(() => RunSuppressed(() =>
            {
                var report = IssuesFinder.SearchAndReport();
                return string.IsNullOrEmpty(report)
                    ? "Issues Finder returned no report (nothing selected to search, or search was canceled)."
                    : report;
            }));
        }
    }
}
#endif
