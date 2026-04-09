#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace MCPTools.DialogueSystem.Editor
{
    public partial class Tool_DialogueSystem
    {
        [McpPluginTool("ds-variable", Title = "Dialogue System / Variable")]
        [Description(@"Get or set Dialogue System Lua variables.
For 'get': reads the current value of a variable via Lua.
For 'set': sets a variable value, auto-detecting type (bool, number, string).
Variable names should match exactly as defined in the dialogue database.")]
        public string ManageVariable(
            [Description("Name of the Dialogue System variable (e.g. 'PlayerName', 'QuestProgress').")]
            string variableName,
            [Description("Action: 'get' or 'set'. Default 'get'.")]
            string? action = "get",
            [Description("Value to set (for 'set' action). Auto-detects type: 'true'/'false' = bool, numeric = number, otherwise = string.")]
            string? value = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!HasDialogueManager())
                    return "ERROR: No DialogueManager instance found in the scene.";

                string actionLower = (action ?? "get").ToLowerInvariant();

                switch (actionLower)
                {
                    case "get":
                    {
                        string luaCode = "return Variable['" + variableName + "']";
                        object? result = CallStatic(LuaType, "Run", luaCode);
                        string asString = result != null ? (Get(result, "asString")?.ToString() ?? "(null)") : "(null)";
                        return $"Variable['{variableName}'] = {asString}";
                    }

                    case "set":
                    {
                        if (value == null)
                            return "ERROR: 'value' parameter is required for 'set' action.";

                        string luaValue;
                        string valueLower = value.ToLowerInvariant();

                        if (valueLower == "true" || valueLower == "false")
                        {
                            luaValue = valueLower;
                        }
                        else if (float.TryParse(value, System.Globalization.NumberStyles.Float,
                                     System.Globalization.CultureInfo.InvariantCulture, out float _))
                        {
                            luaValue = value;
                        }
                        else
                        {
                            luaValue = "\"" + value.Replace("\"", "\\\"") + "\"";
                        }

                        string setCode = "Variable['" + variableName + "'] = " + luaValue;
                        CallStatic(LuaType, "Run", setCode);

                        // Read back to confirm
                        string confirmCode = "return Variable['" + variableName + "']";
                        object? confirmResult = CallStatic(LuaType, "Run", confirmCode);
                        string asString = confirmResult != null ? (Get(confirmResult, "asString")?.ToString() ?? "(null)") : "(null)";
                        return $"Set Variable['{variableName}'] = {luaValue}. Current value: {asString}";
                    }

                    default:
                        return $"ERROR: Unknown action '{action}'. Valid actions: get, set.";
                }
            });
        }
    }
}
