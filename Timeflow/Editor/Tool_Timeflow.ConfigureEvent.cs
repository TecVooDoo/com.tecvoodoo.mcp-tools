#if HAS_TIMEFLOW
#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.Timeflow.Editor
{
    public partial class Tool_Timeflow
    {
        [McpPluginTool("timeflow-configure-event", Title = "Timeflow / Configure Event")]
        [Description(@"Configures a TimeflowEvent behavior on a GameObject. The GO must have a TimeflowEvent component.
TimeflowEvents trigger at a specific time during Timeflow playback.
triggerTime: time in seconds when the event fires.
targetName: name of the GameObject to send the message to.
function: method name to call via SendMessage on the target.
parameter: string parameter passed to the function.
triggerLimit: max number of times to trigger (0 = unlimited).
lockTime: prevent the trigger time from being changed in the editor.
logEnabled: log trigger events to console.")]
        public string ConfigureEvent(
            [Description("Name of the GameObject with a TimeflowEvent component.")] string gameObjectName,
            [Description("Time in seconds when the event fires.")] float? triggerTime = null,
            [Description("Name of the target GameObject for SendMessage.")] string? targetName = null,
            [Description("Method name to call on the target.")] string? function = null,
            [Description("String parameter to pass to the function.")] string? parameter = null,
            [Description("Max trigger count (0 = unlimited).")] int? triggerLimit = null,
            [Description("Lock trigger time in editor.")] bool? lockTime = null,
            [Description("Log trigger events to console.")] bool? logEnabled = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                UnityEngine.Component evt = GetComponentOfType(gameObjectName, TimeflowEventType, "TimeflowEvent");
                StringBuilder changes = new StringBuilder();
                int changeCount = 0;

                if (triggerTime.HasValue)
                {
                    // TriggerTime is a property with get/set
                    PropertyInfo? trigTimeProp = evt.GetType().GetProperty("TriggerTime", BindingFlags.Public | BindingFlags.Instance);
                    if (trigTimeProp != null && trigTimeProp.CanWrite)
                    {
                        trigTimeProp.SetValue(evt, triggerTime.Value);
                        changes.AppendLine($"  TriggerTime = {triggerTime.Value}s");
                        changeCount++;
                    }
                }

                if (targetName != null)
                {
                    GameObject? target = GameObject.Find(targetName);
                    if (target == null)
                        throw new Exception($"Target GameObject '{targetName}' not found.");
                    FieldInfo? objField = evt.GetType().GetField("Obj", BindingFlags.Public | BindingFlags.Instance);
                    if (objField != null)
                    {
                        objField.SetValue(evt, target);
                        changes.AppendLine($"  Obj = {targetName}");
                        changeCount++;
                    }
                }

                if (function != null)
                {
                    Set(evt, "Function", function);
                    changes.AppendLine($"  Function = {function}");
                    changeCount++;
                }

                if (parameter != null)
                {
                    Set(evt, "Parameter", parameter);
                    changes.AppendLine($"  Parameter = {parameter}");
                    changeCount++;
                }

                if (triggerLimit.HasValue)
                {
                    Set(evt, "TriggerLimit", triggerLimit.Value);
                    changes.AppendLine($"  TriggerLimit = {triggerLimit.Value}");
                    changeCount++;
                }

                if (lockTime.HasValue)
                {
                    PropertyInfo? lockProp = evt.GetType().GetProperty("LockTime", BindingFlags.Public | BindingFlags.Instance);
                    if (lockProp != null && lockProp.CanWrite)
                    {
                        lockProp.SetValue(evt, lockTime.Value);
                        changes.AppendLine($"  LockTime = {lockTime.Value}");
                        changeCount++;
                    }
                }

                if (logEnabled.HasValue)
                {
                    Set(evt, "LogEnabled", logEnabled.Value);
                    changes.AppendLine($"  LogEnabled = {logEnabled.Value}");
                    changeCount++;
                }

                if (changeCount > 0)
                    UnityEditor.EditorUtility.SetDirty(evt);

                if (changeCount == 0)
                    return $"No changes applied to TimeflowEvent on '{gameObjectName}'.";

                return $"OK: TimeflowEvent on '{gameObjectName}' updated ({changeCount} change(s)):\n{changes}";
            });
        }
    }
}
#endif
