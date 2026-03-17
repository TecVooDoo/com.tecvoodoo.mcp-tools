#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using MalbersAnimations.Controller;
using UnityEditor;
using UnityEngine;

namespace MCPTools.MalbersAC.Editor
{
    public partial class Tool_MalbersAC
    {
        [McpPluginTool("ac-configure-mode", Title = "Malbers AC / Configure Mode")]
        [Description(@"Configures a mode on an MAnimal component by mode name or ID.
Only provided parameters are changed; others are left as-is.
Common modes: Attack1, Attack2, Action, Damage, etc.
Use 'ac-query-animal' first to see available modes.")]
        public ConfigureModeResponse ConfigureMode(
            [Description("Reference to the GameObject with MAnimal component.")]
            GameObjectRef targetRef,
            [Description("Mode name to find (e.g. 'Attack1', 'Action', 'Damage'). Case-insensitive partial match.")]
            string modeName,
            [Description("Enable or disable this mode. Null to keep current.")]
            bool? active = null,
            [Description("Cooldown time in seconds. Null to keep current.")]
            float? coolDown = null,
            [Description("Allow movement during mode. Null to keep current.")]
            bool? allowMovement = null,
            [Description("Allow rotation during mode. Null to keep current.")]
            bool? allowRotation = null,
            [Description("Ignore lower-priority modes. Null to keep current.")]
            bool? ignoreLowerModes = null,
            [Description("Input name to activate mode. Null to keep current.")]
            string? input = null
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

                Mode? found = null;
                string searchLower = modeName.ToLower();

                if (animal.modes != null)
                {
                    foreach (var mode in animal.modes)
                    {
                        if (mode == null) continue;
                        string name = mode.ID != null ? mode.ID.name : "";
                        if (name.ToLower().Contains(searchLower))
                        {
                            found = mode;
                            break;
                        }
                    }
                }

                if (found == null)
                    throw new System.Exception($"Mode '{modeName}' not found on '{go.name}'.");

                if (active.HasValue) found.Active = active.Value;
                if (coolDown.HasValue) found.CoolDown = coolDown.Value;
                if (allowMovement.HasValue) found.AllowMovement = allowMovement.Value;
                if (allowRotation.HasValue) found.AllowRotation = allowRotation.Value;
                if (ignoreLowerModes.HasValue) found.IgnoreLowerModes = ignoreLowerModes.Value;
                if (input != null) found.Input = input;

                EditorUtility.SetDirty(go);

                string foundName = found.ID != null ? found.ID.name : "unnamed";
                int abilityCount = found.Abilities != null ? found.Abilities.Count : 0;

                return new ConfigureModeResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    modeName = foundName,
                    active = found.Active,
                    coolDown = found.CoolDown,
                    allowMovement = found.AllowMovement,
                    allowRotation = found.AllowRotation,
                    abilityCount = abilityCount
                };
            });
        }

        public class ConfigureModeResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Mode name that was configured")] public string modeName = "";
            [Description("Mode active status")] public bool active;
            [Description("Cooldown time in seconds")] public float coolDown;
            [Description("Movement allowed during mode")] public bool allowMovement;
            [Description("Rotation allowed during mode")] public bool allowRotation;
            [Description("Number of abilities in this mode")] public int abilityCount;
        }
    }
}
