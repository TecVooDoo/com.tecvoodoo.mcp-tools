#if HAS_MALBERS_AC
#nullable enable
using System.ComponentModel;
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
using MalbersAnimations;
using UnityEditor;
using UnityEngine;

namespace MCPTools.MalbersAC.Editor
{
    public partial class Tool_MalbersAC
    {
        [McpPluginTool("ac-query-stats", Title = "Malbers AC / Query Stats")]
        [Description(@"Reads all stats on a Stats component attached to a GameObject.
Lists each stat's name, ID, value, max, min, regen rate, and degen rate.
Use this before configuring stats to see current values.")]
        public QueryStatsResponse QueryStats(
            [Description("Reference to the GameObject with Stats component.")]
            GameObjectRef targetRef
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var stats = go.GetComponent<Stats>();
                if (stats == null)
                    throw new System.Exception($"'{go.name}' has no Stats component.");

                var sb = new StringBuilder();
                int count = 0;

                if (stats.stats != null)
                {
                    foreach (var stat in stats.stats)
                    {
                        if (stat == null) continue;
                        count++;
                        string statName = stat.ID != null ? stat.ID.name : "unnamed";
                        string idVal = stat.ID != null ? stat.ID.ID.ToString() : "?";
                        sb.AppendLine($"  [{idVal}] {statName} | Active: {stat.Active} | Value: {stat.Value:F1}/{stat.MaxValue:F1} (min: {stat.MinValue:F1}) | Regen: {stat.RegenRate.Value:F2} | Degen: {stat.DegenRate.Value:F2} | Immune: {stat.ImmuneTime.Value:F1}s");
                    }
                }

                return new QueryStatsResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    statCount = count,
                    details = sb.ToString()
                };
            });
        }

        [McpPluginTool("ac-configure-stat", Title = "Malbers AC / Configure Stat")]
        [Description(@"Configures a specific stat on a Stats component by stat name.
Only provided parameters are changed; others are left as-is.
Common stats: Health, Stamina, Energy, Mana.
Use 'ac-query-stats' first to see available stats.")]
        public ConfigureStatResponse ConfigureStat(
            [Description("Reference to the GameObject with Stats component.")]
            GameObjectRef targetRef,
            [Description("Stat name to find (e.g. 'Health', 'Stamina', 'Energy'). Case-insensitive partial match.")]
            string statName,
            [Description("Enable or disable this stat. Null to keep current.")]
            bool? active = null,
            [Description("Current value. Null to keep current.")]
            float? value = null,
            [Description("Maximum value. Null to keep current.")]
            float? maxValue = null,
            [Description("Minimum value. Null to keep current.")]
            float? minValue = null,
            [Description("Regeneration rate (units per second). Null to keep current.")]
            float? regenRate = null,
            [Description("Degeneration rate (units per second). Null to keep current.")]
            float? degenRate = null,
            [Description("Wait time before regeneration starts (seconds). Null to keep current.")]
            float? regenWaitTime = null,
            [Description("Immunity time after receiving damage (seconds). Null to keep current.")]
            float? immuneTime = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var stats = go.GetComponent<Stats>();
                if (stats == null)
                    throw new System.Exception($"'{go.name}' has no Stats component.");

                Stat? found = null;
                string searchLower = statName.ToLower();

                if (stats.stats != null)
                {
                    foreach (var stat in stats.stats)
                    {
                        if (stat == null) continue;
                        string name = stat.ID != null ? stat.ID.name : "";
                        if (name.ToLower().Contains(searchLower))
                        {
                            found = stat;
                            break;
                        }
                    }
                }

                if (found == null)
                    throw new System.Exception($"Stat '{statName}' not found on '{go.name}'.");

                if (active.HasValue) found.Active = active.Value;
                if (value.HasValue) found.Value = value.Value;
                if (maxValue.HasValue) found.MaxValue = maxValue.Value;
                if (minValue.HasValue) found.MinValue = minValue.Value;
                if (regenRate.HasValue) found.RegenRate.Value = regenRate.Value;
                if (degenRate.HasValue) found.DegenRate.Value = degenRate.Value;
                if (regenWaitTime.HasValue) found.RegenWaitTime.Value = regenWaitTime.Value;
                if (immuneTime.HasValue) found.ImmuneTime.Value = immuneTime.Value;

                EditorUtility.SetDirty(go);

                string foundName = found.ID != null ? found.ID.name : "unnamed";

                return new ConfigureStatResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    statName = foundName,
                    active = found.Active,
                    value = found.Value,
                    maxValue = found.MaxValue,
                    minValue = found.MinValue,
                    regenRate = found.RegenRate.Value
                };
            });
        }

        public class QueryStatsResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Number of stats configured")] public int statCount;
            [Description("Detailed stat breakdown")] public string details = "";
        }

        public class ConfigureStatResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Stat name that was configured")] public string statName = "";
            [Description("Stat active status")] public bool active;
            [Description("Current value")] public float value;
            [Description("Maximum value")] public float maxValue;
            [Description("Minimum value")] public float minValue;
            [Description("Regeneration rate")] public float regenRate;
        }
    }
}
#endif
