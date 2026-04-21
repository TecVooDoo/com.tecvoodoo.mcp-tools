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
        [McpPluginTool("pe-configure-interactable", Title = "PressE PRO 2 / Configure Interactable")]
        [Description(@"Sets common Interactable properties on a GameObject.
interactMode: enum value (UnityEvent, Grab, Drag, Hold, Inspection — exact names depend on installed version).
hasSensor: enable proximity sensor.
sensorRadius: detection radius (when HasSensor true).
useConditions: enable condition gating before interaction allowed.
overrideKey: override the global E key with a per-interactable binding.
canInteract: master enable for the interactable.
grabId: identifier used by GrabDeposit to match objects.
maxInteractions: limit on total interactions (-1 = unlimited).")]
        public string ConfigureInteractable(
            [Description("GameObject name with Interactable.")] string gameObjectName,
            [Description("Interaction mode enum name.")] string? interactMode = null,
            [Description("Enable proximity sensor.")] bool? hasSensor = null,
            [Description("Sensor radius.")] float? sensorRadius = null,
            [Description("Enable conditional gating.")] bool? useConditions = null,
            [Description("Override the global key with a per-interactable binding.")] bool? overrideKey = null,
            [Description("Master enable for the interactable.")] bool? canInteract = null,
            [Description("GrabId string for deposit matching.")] string? grabId = null,
            [Description("Max interactions allowed.")] int? maxInteractions = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var c = GetComponentByTypeName(go, INTERACTABLE_TYPE);
                if (c == null) throw new Exception($"'{gameObjectName}' has no Interactable component.");

                int changes = 0;
                if (interactMode != null) { Set(c, "interactMode", interactMode); changes++; }
                if (hasSensor.HasValue) { Set(c, "HasSensor", hasSensor.Value); changes++; }
                if (sensorRadius.HasValue) { Set(c, "SensorRadius", sensorRadius.Value); changes++; }
                if (useConditions.HasValue) { Set(c, "UseConditions", useConditions.Value); changes++; }
                if (overrideKey.HasValue) { Set(c, "OverrideInteractionKey", overrideKey.Value); changes++; }
                if (canInteract.HasValue)
                {
                    try { Call(c, "SetCanInteract", canInteract.Value); }
                    catch { Set(c, "CanInteract", canInteract.Value); }
                    changes++;
                }
                if (grabId != null) { Set(c, "GrabId", grabId); changes++; }
                if (maxInteractions.HasValue) { Set(c, "maxInteractions", maxInteractions.Value); changes++; }

                EditorUtility.SetDirty(c);
                return $"OK: Interactable on '{gameObjectName}' updated. {changes} change(s) applied.";
            });
        }

        [McpPluginTool("pe-configure-key", Title = "PressE PRO 2 / Configure Key")]
        [Description(@"Sets Key component properties — used by interactables that gate on collected keys.")]
        public string ConfigureKey(
            [Description("GameObject name with Key.")] string gameObjectName,
            [Description("Key name string (used by conditions).")] string? keyName = null,
            [Description("Add key to InteractionManager.ObtainedKeys when picked up.")] bool? addKeyWhenInteract = null,
            [Description("Remove key from manager when used.")] bool? removeKeyWhenUsed = null,
            [Description("Disable the GameObject after interaction.")] bool? disableObjectWhenInteract = null,
            [Description("Master enable.")] bool? canInteract = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);
                var k = GetComponentByTypeName(go, KEY_TYPE);
                if (k == null) throw new Exception($"'{gameObjectName}' has no Key component.");

                int changes = 0;
                if (keyName != null) { Set(k, "KeyName", keyName); changes++; }
                if (addKeyWhenInteract.HasValue) { Set(k, "AddKeyWhenInteract", addKeyWhenInteract.Value); changes++; }
                if (removeKeyWhenUsed.HasValue) { Set(k, "RemoveKeyWhenUsed", removeKeyWhenUsed.Value); changes++; }
                if (disableObjectWhenInteract.HasValue) { Set(k, "DisableObjectWhenInteract", disableObjectWhenInteract.Value); changes++; }
                if (canInteract.HasValue) { Set(k, "CanInteract", canInteract.Value); changes++; }

                EditorUtility.SetDirty(k);
                return $"OK: Key on '{gameObjectName}' updated. {changes} change(s) applied.";
            });
        }
    }
}
