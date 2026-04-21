#nullable enable
using System.Collections;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.PressE.Editor
{
    public partial class Tool_PressE
    {
        [McpPluginTool("pe-query", Title = "PressE PRO 2 / Query")]
        [Description(@"Reports PressE PRO 2 components on a GameObject.
Lists Interactables (range, prompt, conditions, mode), Keys (name, behaviors).")]
        public string Query(
            [Description("GameObject name with PressE components.")] string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var sb = new StringBuilder();

                sb.AppendLine($"PressE components on '{gameObjectName}':");

                var interactables = GetComponentsByTypeName(go, INTERACTABLE_TYPE);
                if (interactables.Length > 0)
                {
                    sb.AppendLine($"  Interactables ({interactables.Length}):");
                    foreach (var c in interactables)
                    {
                        sb.AppendLine($"    interactMode: {Get(c, "interactMode")}");
                        sb.AppendLine($"    HasSensor: {Get(c, "HasSensor")}  SensorRadius: {Get(c, "SensorRadius")}");
                        sb.AppendLine($"    UseConditions: {Get(c, "UseConditions")}  CanInteract: {Get(c, "CanInteract")}");
                        sb.AppendLine($"    OverrideInteractionKey: {Get(c, "OverrideInteractionKey")}");
                        sb.AppendLine($"    maxInteractions: {Get(c, "maxInteractions")}");
                        sb.AppendLine($"    GrabId: '{Get(c, "GrabId")}'");

                        var conditions = Get(c, "Conditions");
                        if (conditions != null)
                        {
                            var items = Get(conditions, "items") as IList;
                            if (items != null)
                                sb.AppendLine($"    Conditions: {items.Count} item(s)");
                        }
                    }
                }

                var keys = GetComponentsByTypeName(go, KEY_TYPE);
                if (keys.Length > 0)
                {
                    sb.AppendLine($"  Keys ({keys.Length}):");
                    foreach (var k in keys)
                    {
                        sb.AppendLine($"    KeyName: '{Get(k, "KeyName")}'  CanInteract: {Get(k, "CanInteract")}");
                        sb.AppendLine($"    AddKeyWhenInteract: {Get(k, "AddKeyWhenInteract")}  RemoveKeyWhenUsed: {Get(k, "RemoveKeyWhenUsed")}");
                        sb.AppendLine($"    DisableObjectWhenInteract: {Get(k, "DisableObjectWhenInteract")}");
                    }
                }

                if (interactables.Length == 0 && keys.Length == 0)
                    sb.AppendLine("  (no PressE components found)");

                // Also list InteractionManager state if present
                var mgrType = FindType(MANAGER_TYPE);
                if (mgrType != null)
                {
                    var instProp = mgrType.GetProperty("singleton", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    var mgrInst = instProp?.GetValue(null);
                    if (mgrInst != null)
                    {
                        sb.AppendLine($"\nInteractionManager (singleton):");
                        sb.AppendLine($"  InteractionDistance: {Get(mgrInst, "InteractionDistance")}");
                        var keysList = Get(mgrInst, "ObtainedKeys") as IList;
                        if (keysList != null)
                            sb.AppendLine($"  ObtainedKeys: {keysList.Count}");
                        var heldObj = Get(mgrInst, "heldingObject");
                        if (heldObj != null)
                            sb.AppendLine($"  HeldingObject: {(heldObj as Object)?.name}");
                    }
                }

                return sb.ToString();
            });
        }
    }
}
