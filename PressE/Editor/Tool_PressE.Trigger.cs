#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.PressE.Editor
{
    public partial class Tool_PressE
    {
        [McpPluginTool("pe-trigger", Title = "PressE PRO 2 / Trigger Interaction")]
        [Description(@"Programmatically triggers an Interactable's Interact() method.
Most useful in play mode to fire UnityEvents or simulate input.
Returns an error if the editor is not in play mode for runtime-only interactions.")]
        public string Trigger(
            [Description("GameObject name with Interactable.")] string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var c = GetComponentByTypeName(go, INTERACTABLE_TYPE);
                if (c == null) throw new Exception($"'{gameObjectName}' has no Interactable component.");

                if (!EditorApplication.isPlaying)
                {
                    // Try anyway — UnityEvent mode may work without play mode
                    try { Call(c, "Interact"); }
                    catch (Exception ex)
                    {
                        return $"WARN: Interact() failed in edit mode: {ex.InnerException?.Message ?? ex.Message}. Try in play mode.";
                    }
                    return $"OK: Interact() called on '{gameObjectName}' in edit mode (UnityEvent mode only).";
                }

                Call(c, "Interact");
                return $"OK: Interact() triggered on '{gameObjectName}' (play mode).";
            });
        }
    }
}
