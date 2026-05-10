#if HAS_MALBERS_AC
#nullable enable
using System.ComponentModel;
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
using UnityEditor;
using UnityEngine;

namespace MCPTools.MalbersAC.Editor
{
    public partial class Tool_MalbersAC
    {
        [McpPluginTool("ac-configure-state", Title = "Malbers AC / Configure State")]
        [Description(@"Configures a state on an MAnimal component by state name or ID.
Only provided parameters are changed; others are left as-is.
Use 'ac-query-animal' first to see available states and their current values.")]
        public ConfigureStateResponse ConfigureState(
            [Description("Reference to the GameObject with MAnimal component.")]
            GameObjectRef targetRef,
            [Description("State name to find (e.g. 'Locomotion', 'Jump', 'Fall', 'Swim', 'Fly'). Case-insensitive partial match.")]
            string stateName,
            [Description("Enable or disable this state. Null to keep current.")]
            bool? active = null,
            [Description("State priority (higher = more important). Null to keep current.")]
            int? priority = null,
            [Description("Input name to activate state. Null to keep current.")]
            string? input = null,
            [Description("Allow strafing in this state. Null to keep current.")]
            bool? canStrafe = null,
            [Description("Can transition to itself. Null to keep current.")]
            bool? canTransitionToItself = null
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

                State? found = null;
                string searchLower = stateName.ToLower();

                if (animal.states != null)
                {
                    foreach (var state in animal.states)
                    {
                        if (state == null) continue;
                        string name = state.ID != null ? state.ID.name : state.name;
                        if (name.ToLower().Contains(searchLower))
                        {
                            found = state;
                            break;
                        }
                    }
                }

                if (found == null)
                    throw new System.Exception($"State '{stateName}' not found on '{go.name}'.");

                if (active.HasValue) found.Active = active.Value;
                if (priority.HasValue) found.Priority = priority.Value;
                if (input != null) found.Input = input;
                if (canStrafe.HasValue) found.CanStrafe = canStrafe.Value;
                if (canTransitionToItself.HasValue) found.CanTransitionToItself = canTransitionToItself.Value;

                EditorUtility.SetDirty(go);

                string foundName = found.ID != null ? found.ID.name : found.name;

                return new ConfigureStateResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    stateName = foundName,
                    active = found.Active,
                    priority = found.Priority,
                    input = found.Input,
                    canStrafe = found.CanStrafe
                };
            });
        }

        public class ConfigureStateResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("State name that was configured")] public string stateName = "";
            [Description("State active status")] public bool active;
            [Description("State priority")] public int priority;
            [Description("Input name")] public string input = "";
            [Description("Can strafe in this state")] public bool canStrafe;
        }
    }
}
#endif
