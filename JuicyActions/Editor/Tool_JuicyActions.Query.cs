#if HAS_JUICY_ACTIONS
#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.JuicyActions.Editor
{
    public partial class Tool_JuicyActions
    {
        [McpPluginTool("juicy-query", Title = "Juicy Actions / Query Triggers")]
        [Description(@"Lists all Juicy Actions trigger components (ActionOnEvent-derived) on a GameObject.
Reports each trigger's type name, enabled state, and configured action executors.
For each ActionExecutor: reports executable items count, time mode, and cooldown.
Use to understand what Juicy Actions are configured on a GameObject.")]
        public string QueryTriggers(
            [Description("Name of the GameObject with Juicy Actions trigger components.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var triggers = GetTriggers(gameObjectName);
                var sb = new StringBuilder();

                sb.AppendLine($"=== Juicy Actions on '{gameObjectName}' ({triggers.Length} trigger(s)) ===");

                for (int i = 0; i < triggers.Length; i++)
                {
                    var trigger = triggers[i];
                    var typeName = trigger.GetType().Name;
                    var enabled = (trigger as Behaviour)?.enabled ?? true;

                    sb.AppendLine($"\n-- [{i}] {typeName} (enabled: {enabled}) --");

                    // Try to find ActionExecutor fields/properties on the trigger
                    if (ActionExecutorType != null)
                    {
                        // Look for serialized ActionExecutor fields on this trigger
                        var fields = trigger.GetType().GetFields(
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);

                        int executorCount = 0;
                        foreach (var field in fields)
                        {
                            if (ActionExecutorType.IsAssignableFrom(field.FieldType))
                            {
                                var executor = field.GetValue(trigger);
                                if (executor != null)
                                {
                                    executorCount++;
                                    ReportExecutor(sb, executor, field.Name);
                                }
                            }
                            // Also check arrays/lists of ActionExecutor
                            else if (field.FieldType.IsArray &&
                                     ActionExecutorType.IsAssignableFrom(field.FieldType.GetElementType()))
                            {
                                var arr = field.GetValue(trigger) as Array;
                                if (arr != null)
                                {
                                    foreach (var executor in arr)
                                    {
                                        if (executor != null)
                                        {
                                            executorCount++;
                                            ReportExecutor(sb, executor, field.Name);
                                        }
                                    }
                                }
                            }
                            else if (typeof(IList).IsAssignableFrom(field.FieldType) &&
                                     field.FieldType.IsGenericType)
                            {
                                var genArgs = field.FieldType.GetGenericArguments();
                                if (genArgs.Length > 0 && ActionExecutorType.IsAssignableFrom(genArgs[0]))
                                {
                                    var list = field.GetValue(trigger) as IList;
                                    if (list != null)
                                    {
                                        foreach (var executor in list)
                                        {
                                            if (executor != null)
                                            {
                                                executorCount++;
                                                ReportExecutor(sb, executor, field.Name);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (executorCount == 0)
                            sb.AppendLine("  (no ActionExecutor fields found)");
                    }
                    else
                    {
                        sb.AppendLine("  (ActionExecutor type not found in assemblies)");
                    }
                }

                return sb.ToString();
            });
        }

        static void ReportExecutor(StringBuilder sb, object executor, string fieldName)
        {
            var execItems = Get(executor, "ExecutableItems") as ICollection;
            var itemCount = execItems?.Count ?? 0;

            var timeMode = Get(executor, "TimeMode");
            var cooldown = Get(executor, "Cooldown");

            sb.AppendLine($"  ActionExecutor ({fieldName}):");
            sb.AppendLine($"    Items:    {itemCount}");
            sb.AppendLine($"    TimeMode: {timeMode ?? "N/A"}");
            sb.AppendLine($"    Cooldown: {cooldown ?? "N/A"}");
        }
    }
}
#endif
