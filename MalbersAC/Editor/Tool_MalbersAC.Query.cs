#if HAS_MALBERS_AC
#nullable enable
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
#if MCP_HAS_AIGD
using AIGD;
#else
using com.IvanMurzak.Unity.MCP.Runtime.Data;
#endif
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using MalbersAnimations.Controller;
using UnityEngine;

namespace MCPTools.MalbersAC.Editor
{
    public partial class Tool_MalbersAC
    {
        [McpPluginTool("ac-query-animal", Title = "Malbers AC / Query Animal")]
        [Description(@"Reads the full Animal Controller setup on a GameObject.
Lists all states (name, ID, active, priority), modes (name, ID, active, abilities),
stances (name, ID, enabled), and speed sets (name, speeds, current index).
Use this to understand an animal's configuration before making changes.")]
        public QueryAnimalResponse QueryAnimal(
            [Description("Reference to the GameObject with MAnimal component.")]
            GameObjectRef targetRef
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var animal = go.GetComponent<MAnimal>();
                if (animal == null)
                    throw new System.Exception($"'{go.name}' has no MAnimal component.");

                var sb = new StringBuilder();

                // States
                sb.AppendLine("=== STATES ===");
                if (animal.states != null)
                {
                    foreach (var state in animal.states)
                    {
                        if (state == null) continue;
                        string idVal = state.ID != null ? state.ID.ID.ToString() : "?";
                        string stateName = state.ID != null ? state.ID.name : state.name;
                        sb.AppendLine($"  [{idVal}] {stateName} | Active: {state.Active} | Priority: {state.Priority} | Input: '{state.Input}' | CanStrafe: {state.CanStrafe}");
                    }
                }

                // Modes
                sb.AppendLine("\n=== MODES ===");
                if (animal.modes != null)
                {
                    foreach (var mode in animal.modes)
                    {
                        if (mode == null) continue;
                        string idVal = mode.ID != null ? mode.ID.ID.ToString() : "?";
                        string modeName = mode.ID != null ? mode.ID.name : "unnamed";
                        int abilityCount = mode.Abilities != null ? mode.Abilities.Count : 0;
                        sb.AppendLine($"  [{idVal}] {modeName} | Active: {mode.Active} | Abilities: {abilityCount} | CoolDown: {mode.CoolDown} | AllowMovement: {mode.AllowMovement}");

                        if (mode.Abilities != null)
                        {
                            foreach (var ability in mode.Abilities)
                            {
                                if (ability == null) continue;
                                sb.AppendLine($"    - [{ability.Index.Value}] {ability.Name} | Active: {ability.Active} | Status: {ability.Status}");
                            }
                        }
                    }
                }

                // Stances
                sb.AppendLine("\n=== STANCES ===");
                if (animal.Stances != null)
                {
                    foreach (var stance in animal.Stances)
                    {
                        if (stance == null) continue;
                        string idVal = stance.ID != null ? stance.ID.ID.ToString() : "?";
                        string stanceName = stance.ID != null ? stance.ID.name : "unnamed";
                        sb.AppendLine($"  [{idVal}] {stanceName} | Enabled: {stance.Enabled} | Persistent: {stance.Persistent} | CanStrafe: {stance.CanStrafe}");
                    }
                }

                // Speed Sets
                sb.AppendLine("\n=== SPEED SETS ===");
                if (animal.speedSets != null)
                {
                    foreach (var speedSet in animal.speedSets)
                    {
                        if (speedSet == null) continue;
                        int speedCount = speedSet.Speeds != null ? speedSet.Speeds.Count : 0;
                        sb.AppendLine($"  {speedSet.name} | Speeds: {speedCount} | TopIndex: {speedSet.TopIndex} | StartIndex: {speedSet.StartVerticalIndex}");

                        if (speedSet.Speeds != null)
                        {
                            foreach (var speed in speedSet.Speeds)
                            {
                                sb.AppendLine($"    - {speed.name} | Position: {speed.position.Value:F2} | Rotation: {speed.rotation.Value:F2} | Animator: {speed.animator.Value:F2}");
                            }
                        }
                    }
                }

                // General info
                string activeStateName = animal.ActiveState != null && animal.ActiveState.ID != null ? animal.ActiveState.ID.name : "none";

                return new QueryAnimalResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    activeState = activeStateName,
                    stateCount = animal.states != null ? animal.states.Count : 0,
                    modeCount = animal.modes != null ? animal.modes.Count : 0,
                    stanceCount = animal.Stances != null ? animal.Stances.Count : 0,
                    speedSetCount = animal.speedSets != null ? animal.speedSets.Count : 0,
                    details = sb.ToString()
                };
            });
        }

        public class QueryAnimalResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Currently active state name")] public string activeState = "";
            [Description("Number of states configured")] public int stateCount;
            [Description("Number of modes configured")] public int modeCount;
            [Description("Number of stances configured")] public int stanceCount;
            [Description("Number of speed sets configured")] public int speedSetCount;
            [Description("Full detailed breakdown of all states, modes, stances, and speed sets")] public string details = "";
        }
    }
}
#endif
