#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AC;

namespace MCPTools.AdventureCreator.Editor
{
    public partial class Tool_AdventureCreator
    {
        [McpPluginTool("ac-query-managers", Title = "Adventure Creator / Query Managers")]
        [Description(@"Shows the status of all Adventure Creator managers.
Reports which managers are assigned and their basic statistics
(inventory item count, variable count, action type count, etc.).
Does not require play mode.")]
        public string QueryManagers()
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== ADVENTURE CREATOR MANAGERS ===");

                // Settings Manager
                var settings = KickStarter.settingsManager;
                sb.AppendLine($"  Settings Manager: {(settings != null ? "Assigned" : "NOT ASSIGNED")}");

                // Inventory Manager
                var inv = KickStarter.inventoryManager;
                sb.AppendLine($"  Inventory Manager: {(inv != null ? "Assigned" : "NOT ASSIGNED")}");
                if (inv != null)
                {
                    sb.AppendLine($"    Items: {inv.items.Count}");
                    sb.AppendLine($"    Categories: {inv.bins.Count}");
                    sb.AppendLine($"    Recipes: {inv.recipes.Count}");
                    sb.AppendLine($"    Documents: {inv.documents.Count}");
                }

                // Variables Manager
                var vars = KickStarter.variablesManager;
                sb.AppendLine($"  Variables Manager: {(vars != null ? "Assigned" : "NOT ASSIGNED")}");
                if (vars != null)
                    sb.AppendLine($"    Global Variables: {vars.vars.Count}");

                // Actions Manager
                var actions = KickStarter.actionsManager;
                sb.AppendLine($"  Actions Manager: {(actions != null ? "Assigned" : "NOT ASSIGNED")}");
                if (actions != null)
                    sb.AppendLine($"    Action Types: {actions.AllActions.Count}");

                // Scene Manager
                var scene = KickStarter.sceneManager;
                sb.AppendLine($"  Scene Manager: {(scene != null ? "Assigned" : "NOT ASSIGNED")}");

                // Speech Manager
                var speech = KickStarter.speechManager;
                sb.AppendLine($"  Speech Manager: {(speech != null ? "Assigned" : "NOT ASSIGNED")}");

                // Cursor Manager
                var cursor = KickStarter.cursorManager;
                sb.AppendLine($"  Cursor Manager: {(cursor != null ? "Assigned" : "NOT ASSIGNED")}");

                // Menu Manager
                var menu = KickStarter.menuManager;
                sb.AppendLine($"  Menu Manager: {(menu != null ? "Assigned" : "NOT ASSIGNED")}");

                return sb.ToString();
            });
        }
    }
}
