#nullable enable
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AC;

namespace MCPTools.AdventureCreator.Editor
{
    public partial class Tool_AdventureCreator
    {
        [McpPluginTool("ac-list-actions", Title = "Adventure Creator / List Actions")]
        [Description(@"Lists all available Action types registered in the Adventure Creator Actions Manager.
Shows action title, filename, and category. Actions are the building blocks of ActionLists.
An optional filter can narrow results by name. Does not require play mode.")]
        public string ListActions(
            [Description("Optional filter to search actions by title. Leave empty to list all.")]
            string? filter = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var actions = KickStarter.actionsManager;
                if (actions == null)
                    return "ERROR: No Actions Manager assigned in Adventure Creator.";

                var sb = new StringBuilder();

                var filtered = actions.AllActions.AsEnumerable();
                if (!string.IsNullOrEmpty(filter))
                {
                    string filterLower = filter!.ToLowerInvariant();
                    filtered = filtered.Where(a =>
                        (a.title != null && a.title.ToLowerInvariant().Contains(filterLower)) ||
                        (a.fileName != null && a.fileName.ToLowerInvariant().Contains(filterLower)));
                }

                var list = filtered.OrderBy(a => a.title).ToList();

                sb.AppendLine($"=== AC ACTIONS ({list.Count}{(filter != null ? $" matching '{filter}'" : "")}) ===");

                foreach (var action in list)
                    sb.AppendLine($"  {action.title} ({action.fileName})");

                return sb.ToString();
            });
        }
    }
}
