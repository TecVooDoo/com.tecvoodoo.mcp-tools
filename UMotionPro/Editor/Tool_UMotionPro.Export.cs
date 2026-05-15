#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UMotionEditor.API;
using UnityEngine;

namespace MCPTools.UMotionPro.Editor
{
    public partial class Tool_UMotionPro
    {
        [McpPluginTool("umotion-export", Title = "UMotion Pro / Export Clip(s) to AnimationClip Assets")]
        [Description(@"Export the current clip OR all clips of the loaded UMotion project to AnimationClip assets.
mode:
  - 'current':  exports only the clip currently selected in the Clip Editor (ClipEditor.ExportCurrentClip).
  - 'all':      exports every clip in the project (ClipEditor.ExportAllClips).
  - 'variants': iterates a list of layer-mute presets, exporting once per preset. Requires variants[].
                Each variant entry has form 'variantName:layerA=mute,layerB=unmute' where each layer is
                marked 'mute' or 'unmute'. The tool snapshots current layer mute states, applies the
                variant, sets the temporary clip name to '<originalSelectedClip>_<variantName>',
                exports, then restores both the layer mutes and the original clip name.

All three modes block until export is finished (per UMotion API). The output location is configured by
the UMotion project settings (see ClipEditor manual / project settings) -- this tool does not override
the destination path.

Requires the Clip Editor window open and a project loaded.")]
        public string Export(
            [Description("Export mode: 'current', 'all', or 'variants'.")]
            string mode,
            [Description("For mode='variants': list of layer-mute presets. Each entry: 'variantName:layerA=mute,layerB=unmute'. mute marks layer mute=true, unmute marks layer mute=false. Layers not mentioned are left at their current setting.")]
            string[]? variants = null,
            [Description("Optional: for mode='variants', a clip name to select before iterating variants. Defaults to the currently selected clip.")]
            string? clipName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireProjectLoaded();

                switch (mode)
                {
                    case "current":
                        ClipEditor.ExportCurrentClip();
                        return $"OK: ExportCurrentClip() of '{ClipEditor.GetSelectedClipName()}' completed.";

                    case "all":
                        ClipEditor.ExportAllClips();
                        return $"OK: ExportAllClips() completed ({(ClipEditor.GetAllClipNames()?.Length ?? 0)} clip(s)).";

                    case "variants":
                    {
                        if (variants == null || variants.Length == 0)
                            throw new Exception("'variants' mode requires variants[] (e.g. ['NoBow:Bow=mute', 'WithBow:Bow=unmute']).");

                        if (!string.IsNullOrEmpty(clipName))
                            ClipEditor.SelectClip(clipName!);

                        string originalClipName = ClipEditor.GetSelectedClipName() ?? throw new Exception("No clip selected for variants export.");

                        // Snapshot current layer mute/weight so we can restore.
                        string[] layerNames = ClipEditor.GetClipLayerNames() ?? new string[0];
                        var snapshot = new List<(string name, bool mute, float weight)>(layerNames.Length);
                        foreach (string ln in layerNames)
                        {
                            bool m; float w;
                            ClipEditor.GetClipLayerBlendProperties(ln, out m, out w);
                            snapshot.Add((ln, m, w));
                        }

                        var log = new StringBuilder();
                        log.AppendLine($"Variants export of '{originalClipName}':");

                        try
                        {
                            foreach (string variant in variants)
                            {
                                if (string.IsNullOrEmpty(variant)) continue;
                                int colon = variant.IndexOf(':');
                                if (colon < 1)
                                    throw new Exception($"Bad variant '{variant}'. Format: 'variantName:layerA=mute,layerB=unmute'.");

                                string variantName = variant.Substring(0, colon).Trim();
                                string body = variant.Substring(colon + 1).Trim();
                                if (string.IsNullOrEmpty(variantName))
                                    throw new Exception($"Variant '{variant}' has empty name.");

                                // Apply layer mute changes
                                foreach (string pair in body.Split(','))
                                {
                                    string trimmed = pair.Trim();
                                    if (string.IsNullOrEmpty(trimmed)) continue;
                                    int eq = trimmed.IndexOf('=');
                                    if (eq < 1)
                                        throw new Exception($"Bad layer spec '{pair}' in variant '{variant}'. Expected 'layerName=mute' or 'layerName=unmute'.");
                                    string ln = trimmed.Substring(0, eq).Trim();
                                    string state = trimmed.Substring(eq + 1).Trim();
                                    bool muteIt;
                                    if (state.Equals("mute", StringComparison.OrdinalIgnoreCase)) muteIt = true;
                                    else if (state.Equals("unmute", StringComparison.OrdinalIgnoreCase)) muteIt = false;
                                    else throw new Exception($"Bad layer state '{state}' in variant '{variant}'. Use 'mute' or 'unmute'.");

                                    bool currentMute; float currentWeight;
                                    ClipEditor.GetClipLayerBlendProperties(ln, out currentMute, out currentWeight);
                                    ClipEditor.SetClipLayerBlendProperties(ln, muteIt, currentWeight);
                                }

                                string exportName = $"{originalClipName}_{variantName}";
                                ClipEditor.SetClipName(originalClipName, exportName);
                                try
                                {
                                    ClipEditor.ExportCurrentClip();
                                    log.AppendLine($"  + '{exportName}' exported (variant '{variantName}').");
                                }
                                finally
                                {
                                    // Restore the original clip name before processing next variant
                                    ClipEditor.SetClipName(exportName, originalClipName);
                                }
                            }
                        }
                        finally
                        {
                            // Restore original layer mute/weight
                            foreach (var entry in snapshot)
                                ClipEditor.SetClipLayerBlendProperties(entry.name, entry.mute, entry.weight);
                        }

                        return log.ToString();
                    }

                    default:
                        throw new Exception($"Unknown mode '{mode}'. Valid: current, all, variants.");
                }
            });
        }
    }
}
