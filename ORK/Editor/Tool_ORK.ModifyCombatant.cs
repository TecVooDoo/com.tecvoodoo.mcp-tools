#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace MCPTools.ORK.Editor
{
    public partial class Tool_ORK
    {
        [McpPluginTool("ork-modify-combatant", Title = "ORK / Modify Combatant")]
        [Description(@"Modifies a runtime Combatant.

action options:
  set-status     -- Set a StatusValue's base value (e.g. HP=50). Pass statusName + value.
  add-status     -- Add to a StatusValue's base value (e.g. HP+=10). Pass statusName + value.
  set-level      -- Set Level. Pass value.
  heal           -- Set HP/MP-style values to their max (uses status.SetBaseValue with display max).
  revive         -- Calls Status.Revive() if combatant is dead.
  fire-changed   -- Forces UI refresh via FireChanged(true).

Requires Play mode.")]
        public string ModifyCombatant(
            [Description("'set-status' | 'add-status' | 'set-level' | 'heal' | 'revive' | 'fire-changed'")]
            string action,
            [Description("Name of combatant in the active group / scene.")]
            string combatantName,
            [Description("StatusValue name for set-status / add-status / heal (e.g. 'HP', 'MP').")]
            string? statusName = null,
            [Description("Numeric value for set-status / add-status / set-level.")]
            int? value = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireORKInitialized();
                action = (action ?? "").Trim().ToLowerInvariant();

                var combatant = FindCombatantByName(combatantName)
                                ?? throw new System.Exception($"Combatant '{combatantName}' not found.");
                var status = Get(combatant, "Status")
                             ?? throw new System.Exception($"Combatant '{combatantName}' has no Status.");

                switch (action)
                {
                    case "set-status":
                    {
                        if (string.IsNullOrEmpty(statusName) || !value.HasValue)
                            throw new System.Exception("set-status requires statusName + value.");
                        var setting = FindSettingByName(StatusValuesDB(), statusName);
                        if (setting == null) throw new System.Exception($"StatusValue '{statusName}' not found in DB.");
                        var sv = Call(status, "Get", setting) ?? throw new System.Exception($"Combatant has no '{statusName}'.");
                        Call(sv, "SetBaseValueAccess", value.Value);
                        Call(combatant, "FireChanged", true);
                        return $"OK: Set '{combatantName}'.{statusName} base = {value.Value}.";
                    }

                    case "add-status":
                    {
                        if (string.IsNullOrEmpty(statusName) || !value.HasValue)
                            throw new System.Exception("add-status requires statusName + value.");
                        var setting = FindSettingByName(StatusValuesDB(), statusName);
                        if (setting == null) throw new System.Exception($"StatusValue '{statusName}' not found in DB.");
                        var sv = Call(status, "Get", setting) ?? throw new System.Exception($"Combatant has no '{statusName}'.");
                        Call(sv, "AddBaseValueAccess", value.Value);
                        Call(combatant, "FireChanged", true);
                        return $"OK: '{combatantName}'.{statusName} += {value.Value}.";
                    }

                    case "set-level":
                    {
                        if (!value.HasValue) throw new System.Exception("set-level requires value.");
                        // CombatantStatus.Level has a public setter via ORK; some versions only allow add via Class.
                        // Try property first.
                        var levelProp = status.GetType().GetProperty("Level");
                        if (levelProp != null && levelProp.CanWrite)
                        {
                            levelProp.SetValue(status, value.Value);
                        }
                        else
                        {
                            // Fallback: Combatant.Level setter
                            var cl = combatant.GetType().GetProperty("Level");
                            if (cl != null && cl.CanWrite) cl.SetValue(combatant, value.Value);
                            else throw new System.Exception("No writable Level property found on Combatant or CombatantStatus.");
                        }
                        Call(combatant, "FireChanged", true);
                        return $"OK: '{combatantName}'.Level = {value.Value}.";
                    }

                    case "heal":
                    {
                        if (string.IsNullOrEmpty(statusName))
                            throw new System.Exception("heal requires statusName (e.g. 'HP').");
                        var setting = FindSettingByName(StatusValuesDB(), statusName);
                        if (setting == null) throw new System.Exception($"StatusValue '{statusName}' not found in DB.");
                        var sv = Call(status, "Get", setting) ?? throw new System.Exception($"Combatant has no '{statusName}'.");
                        int max = Call(sv, "GetDisplayMaxValue") is int mx ? mx : 0;
                        Call(sv, "SetBaseValueAccess", max);
                        Call(combatant, "FireChanged", true);
                        return $"OK: '{combatantName}'.{statusName} healed to {max}.";
                    }

                    case "revive":
                    {
                        // Try CombatantStatus.Revive() (no-arg)
                        var ok = Call(status, "Revive");
                        Call(combatant, "FireChanged", true);
                        return ok != null ? $"OK: Revive() called on '{combatantName}'." : $"No Revive() method found on CombatantStatus -- combatant may need to be revived via game logic.";
                    }

                    case "fire-changed":
                    {
                        Call(combatant, "FireChanged", true);
                        return $"OK: FireChanged(true) on '{combatantName}'.";
                    }

                    default:
                        throw new System.Exception($"Unknown action '{action}'. Use one of: set-status, add-status, set-level, heal, revive, fire-changed.");
                }
            });
        }
    }
}
