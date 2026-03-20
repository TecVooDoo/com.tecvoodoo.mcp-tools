#if HAS_DIALOGUE_SYSTEM
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using PixelCrushers.DialogueSystem;

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
                if (!DialogueManager.hasInstance)
                    return "ERROR: No DialogueManager instance found in the scene.";

                string actionLower = (action ?? "get").ToLowerInvariant();

                switch (actionLower)
                {
                    case "get":
                    {
                        Lua.Result result = Lua.Run("return Variable['" + variableName + "']");
                        return $"Variable['{variableName}'] = {result.asString}";
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

                        string luaCode = "Variable['" + variableName + "'] = " + luaValue;
                        Lua.Run(luaCode);

                        // Read back to confirm
                        Lua.Result confirmResult = Lua.Run("return Variable['" + variableName + "']");
                        return $"Set Variable['{variableName}'] = {luaValue}. Current value: {confirmResult.asString}";
                    }

                    default:
                        return $"ERROR: Unknown action '{action}'. Valid actions: get, set.";
                }
            });
        }
    }
}
#endif
