#nullable enable
using System.Collections;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;

namespace MCPTools.ORK.Editor
{
    public partial class Tool_ORK
    {
        [McpPluginTool("ork-battle", Title = "ORK / Battle State")]
        [Description(@"Reads or modifies the current battle.

action options:
  query  -- snapshot of the current battle (active state, turn, participating combatants)
  end    -- end the current battle (calls Battle.End or EndBattle if available)
  flee   -- attempt to flee the current battle

Requires Play mode + an active battle for end/flee.")]
        public string Battle(
            [Description("'query' | 'end' | 'flee'")]
            string action = "query"
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireORK();
                action = (action ?? "query").Trim().ToLowerInvariant();
                var battle = BattleHandler();

                if (action == "query")
                {
                    var sb = new StringBuilder();
                    if (battle == null)
                    {
                        sb.AppendLine("Battle: (handler not available -- not in play mode?)");
                        return sb.ToString();
                    }

                    bool active = Get(battle, "BattleActive") is bool ba && ba;
                    bool turn = Get(battle, "InTurn") is bool it && it;
                    sb.AppendLine($"Battle active: {active}");
                    sb.AppendLine($"In turn:       {turn}");

                    var participants = Call(battle, "GetParticipants");
                    if (participants is IList pList)
                    {
                        sb.AppendLine($"Participants ({pList.Count}):");
                        foreach (var c in pList)
                        {
                            if (c == null) continue;
                            string name = Call(c, "GetName") as string ?? c.ToString();
                            sb.AppendLine($"  - {name}");
                        }
                    }
                    else
                    {
                        // Fallback: ORK.Game.Combatants.GetInBattle()
                        var inBattle = Call(RuntimeCombatants(), "GetInBattle");
                        if (inBattle is IList ibl)
                        {
                            sb.AppendLine($"InBattle combatants ({ibl.Count}):");
                            foreach (var c in ibl)
                            {
                                if (c == null) continue;
                                string name = Call(c, "GetName") as string ?? c.ToString();
                                sb.AppendLine($"  - {name}");
                            }
                        }
                    }
                    return sb.ToString();
                }

                if (battle == null) throw new System.Exception("Battle handler not available -- not in play mode?");

                switch (action)
                {
                    case "end":
                    {
                        // Try common method names: End, EndBattle, ForceEnd, Stop
                        foreach (var m in new[] { "End", "EndBattle", "ForceEnd", "Stop" })
                        {
                            var r = Call(battle, m);
                            if (r != null) return $"OK: Battle.{m}() called.";
                        }
                        // Try with bool argument
                        foreach (var m in new[] { "End", "EndBattle" })
                        {
                            var r = Call(battle, m, false);
                            if (r != null) return $"OK: Battle.{m}(false) called.";
                        }
                        throw new System.Exception("Could not find a method to end the battle (tried End, EndBattle, ForceEnd, Stop).");
                    }

                    case "flee":
                    {
                        foreach (var m in new[] { "Flee", "Escape", "TryFlee" })
                        {
                            var r = Call(battle, m);
                            if (r != null) return $"OK: Battle.{m}() called.";
                        }
                        throw new System.Exception("Could not find a method to flee the battle.");
                    }

                    default:
                        throw new System.Exception($"Unknown action '{action}'. Use one of: query, end, flee.");
                }
            });
        }
    }
}
