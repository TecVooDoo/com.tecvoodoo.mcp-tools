#nullable enable
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using DistantLands.Cozy;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Cozy.Editor
{
    public partial class Tool_Cozy
    {
        [McpPluginTool("cozy-set-biome", Title = "Cozy / Configure Biome")]
        [Description(@"Lists, configures, or activates a CozyBiome.

action options:
  list    -- list all CozyBiome instances in the scene with their mode and current weight
  set     -- modify the named biome's mode/transition/weight settings
  isolate -- set this biome's maxWeight=1, all other CozyBiomes' maxWeight=0 (great for testing a single biome)
  reset   -- restore all biomes to maxWeight=1

For 'set':
  mode: 'Global' (always active at maxWeight) or 'Local' (uses trigger collider).
  transitionMode: 'Distance' (fade by distance from collider) or 'Time' (fade in/out over transitionTime).
  maxWeight: 0..1, biome's contribution at full strength.
  transitionDistance: meters of fade-in around trigger (Distance mode).
  transitionTime: seconds to fade in/out (Time mode).
  triggerObject: name of a child GameObject whose Collider becomes the biome trigger (Local mode).")]
        public string SetBiome(
            [Description("'list' | 'set' | 'isolate' | 'reset'.")]
            string action,
            [Description("Biome GameObject name. Required for 'set' and 'isolate'. Ignored for 'list' and 'reset'.")]
            string? gameObjectName = null,
            [Description("Mode: 'Global' or 'Local'.")] string? mode = null,
            [Description("TransitionMode: 'Distance' or 'Time'.")] string? transitionMode = null,
            [Description("Max weight 0..1.")] float? maxWeight = null,
            [Description("Transition distance (meters, Distance mode).")] float? transitionDistance = null,
            [Description("Transition time (seconds, Time mode).")] float? transitionTime = null,
            [Description("Name of child GO with the trigger Collider (Local mode).")] string? triggerObject = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                action = (action ?? "").Trim().ToLowerInvariant();

                if (action == "list")
                {
                    var biomes = Object.FindObjectsByType<CozyBiome>(FindObjectsSortMode.None);
                    var sb = new StringBuilder();
                    sb.AppendLine($"CozyBiome instances in scene ({biomes.Length}):");
                    foreach (var b in biomes.OrderBy(x => x.gameObject.name))
                    {
                        sb.AppendLine($"  - '{b.gameObject.name}' mode={b.mode} transitionMode={b.transitionMode} maxWeight={b.maxWeight:F2} weight={b.weight:F3} target={b.targetWeight:F3} trigger={(b.trigger != null ? b.trigger.name : "(none)")}");
                    }
                    return sb.ToString();
                }

                if (action == "reset")
                {
                    var biomes = Object.FindObjectsByType<CozyBiome>(FindObjectsSortMode.None);
                    foreach (var b in biomes)
                    {
                        b.maxWeight = 1f;
                        EditorUtility.SetDirty(b);
                    }
                    return $"OK: Reset {biomes.Length} biome(s) to maxWeight=1.";
                }

                if (string.IsNullOrEmpty(gameObjectName))
                    throw new System.Exception("'gameObjectName' is required for this action.");

                CozyBiome biome = GetBiome(gameObjectName);

                if (action == "isolate")
                {
                    var all = Object.FindObjectsByType<CozyBiome>(FindObjectsSortMode.None);
                    int zeroed = 0;
                    foreach (var b in all)
                    {
                        if (b == biome)
                            b.maxWeight = 1f;
                        else
                        {
                            b.maxWeight = 0f;
                            zeroed++;
                        }
                        EditorUtility.SetDirty(b);
                    }
                    return $"OK: Isolated '{gameObjectName}' (maxWeight=1). Zeroed {zeroed} other biome(s).";
                }

                if (action == "set")
                {
                    var changes = new StringBuilder();
                    int changeCount = 0;

                    if (mode != null)
                    {
                        if (System.Enum.TryParse<CozyBiome.BiomeMode>(mode, ignoreCase: true, out var m))
                        {
                            biome.mode = m;
                            changes.AppendLine($"  mode = {m}");
                            changeCount++;
                        }
                        else throw new System.Exception($"Invalid mode '{mode}'. Use 'Global' or 'Local'.");
                    }

                    if (transitionMode != null)
                    {
                        if (System.Enum.TryParse<CozyBiome.TransitionMode>(transitionMode, ignoreCase: true, out var tm))
                        {
                            biome.transitionMode = tm;
                            changes.AppendLine($"  transitionMode = {tm}");
                            changeCount++;
                        }
                        else throw new System.Exception($"Invalid transitionMode '{transitionMode}'. Use 'Distance' or 'Time'.");
                    }

                    if (maxWeight.HasValue)
                    {
                        biome.maxWeight = Mathf.Clamp01(maxWeight.Value);
                        changes.AppendLine($"  maxWeight = {biome.maxWeight:F3}");
                        changeCount++;
                    }

                    if (transitionDistance.HasValue)
                    {
                        biome.transitionDistance = transitionDistance.Value;
                        changes.AppendLine($"  transitionDistance = {transitionDistance.Value}");
                        changeCount++;
                    }

                    if (transitionTime.HasValue)
                    {
                        biome.transitionTime = transitionTime.Value;
                        changes.AppendLine($"  transitionTime = {transitionTime.Value}");
                        changeCount++;
                    }

                    if (triggerObject != null)
                    {
                        var triggerGO = GameObject.Find(triggerObject);
                        if (triggerGO == null)
                            throw new System.Exception($"Trigger GameObject '{triggerObject}' not found.");
                        var col = triggerGO.GetComponent<Collider>();
                        if (col == null)
                            throw new System.Exception($"Trigger GameObject '{triggerObject}' has no Collider.");
                        biome.trigger = col;
                        changes.AppendLine($"  trigger = {triggerObject} ({col.GetType().Name})");
                        changeCount++;
                    }

                    if (changeCount == 0)
                        return $"No changes applied to biome '{biome.gameObject.name}'.";

                    EditorUtility.SetDirty(biome);
                    return $"OK: Biome '{biome.gameObject.name}' updated ({changeCount} change(s)):\n{changes}";
                }

                throw new System.Exception($"Unknown action '{action}'. Use one of: list, set, isolate, reset.");
            });
        }
    }
}
