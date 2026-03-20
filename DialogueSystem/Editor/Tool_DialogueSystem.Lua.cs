#if HAS_DIALOGUE_SYSTEM
#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using PixelCrushers.DialogueSystem;

namespace MCPTools.DialogueSystem.Editor
{
    public partial class Tool_DialogueSystem
    {
        [McpPluginTool("ds-lua", Title = "Dialogue System / Lua Execute")]
        [Description(@"Executes arbitrary Lua code in the Dialogue System's Lua environment.
Returns the result as a string, along with type information (bool, float, string, table).
Use single quotes for strings inside Lua code to avoid escaping issues.
Examples: 'return Variable[""PlayerName""]', 'return GetRelationship(1, 2, ""romantic"")'.")]
        public string ExecuteLua(
            [Description("Lua code to execute in the Dialogue System Lua environment.")]
            string code
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!DialogueManager.hasInstance)
                    return "ERROR: No DialogueManager instance found in the scene.";

                Lua.Result result = Lua.Run(code);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Lua code: {code}");
                sb.AppendLine($"Result (string): {result.asString}");
                sb.AppendLine($"Result (bool): {result.asBool}");
                sb.AppendLine($"Result (float): {result.asFloat}");
                sb.AppendLine($"IsTable: {result.isTable}");

                return sb.ToString();
            });
        }
    }
}
#endif
