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
        [McpPluginTool("ac-list-variables", Title = "Adventure Creator / List Variables")]
        [Description(@"Lists all global variables defined in the Adventure Creator Variables Manager.
Shows variable ID, label, type, and initial value.
Does not require play mode.")]
        public string ListVariables()
        {
            return MainThread.Instance.Run(() =>
            {
                var vars = KickStarter.variablesManager;
                if (vars == null)
                    return "ERROR: No Variables Manager assigned in Adventure Creator.";

                var sb = new StringBuilder();
                sb.AppendLine($"=== AC GLOBAL VARIABLES ({vars.vars.Count}) ===");

                int limit = System.Math.Min(vars.vars.Count, 100);
                for (int i = 0; i < limit; i++)
                {
                    var v = vars.vars[i];
                    sb.AppendLine($"  [{v.id}] \"{v.label}\" | Type: {v.type}");
                }

                if (vars.vars.Count > 100)
                    sb.AppendLine($"  ... and {vars.vars.Count - 100} more variables.");

                return sb.ToString();
            });
        }
    }
}
