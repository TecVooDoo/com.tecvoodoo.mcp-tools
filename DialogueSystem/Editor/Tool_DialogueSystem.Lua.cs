#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

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
                if (!HasDialogueManager())
                    return "ERROR: No DialogueManager instance found in the scene.";

                // Lua.Run(code) returns a Lua.Result
                object? result = CallStatic(LuaType, "Run", code);
                if (result == null)
                    return "ERROR: Lua.Run returned null.";

                string asString = Get(result, "asString")?.ToString() ?? "(null)";
                string asBool = Get(result, "asBool")?.ToString() ?? "(null)";
                string asFloat = Get(result, "asFloat")?.ToString() ?? "(null)";
                string isTable = Get(result, "isTable")?.ToString() ?? "(null)";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Lua code: {code}");
                sb.AppendLine($"Result (string): {asString}");
                sb.AppendLine($"Result (bool): {asBool}");
                sb.AppendLine($"Result (float): {asFloat}");
                sb.AppendLine($"IsTable: {isTable}");

                return sb.ToString();
            });
        }
    }
}
