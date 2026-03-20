#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Heathen.UnityPhysics;
using UnityEditor;
using UnityEngine;

namespace MCPTools.HeathenPhysics.Editor
{
    public partial class Tool_HeathenPhysics
    {
        [McpPluginTool("hphys-configure-force-field", Title = "Heathen Physics / Configure Force Field")]
        [Description(@"Adds (if missing) and configures a ForceEffectField component.
ForceEffectField applies forces to nearby ForceEffectReceiver objects within its radius.
strength: force multiplier applied to receivers.
radius: sphere radius within which receivers are affected (ignored when isGlobal=true).
isGlobal: if true, affects all ForceEffectReceiver objects in the scene regardless of distance.
A SphereCollider with isTrigger=true is required for trigger-based detection -- add one manually or set isGlobal=true.")]
        public string ConfigureForceField(
            [Description("Name of the GameObject to add ForceEffectField to.")] string gameObjectName,
            [Description("Force multiplier strength.")] float? strength = null,
            [Description("Effect radius in world units. Only used when isGlobal=false.")] float? radius = null,
            [Description("If true, affects all receivers globally (no radius check).")] bool? isGlobal = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);

                var field = go.GetComponent<ForceEffectField>();
                if (field == null)
                    field = go.AddComponent<ForceEffectField>();

                if (strength.HasValue) field.strength = strength.Value;
                if (radius.HasValue) field.radius = Mathf.Max(0f, radius.Value);
                if (isGlobal.HasValue) field.IsGlobal = isGlobal.Value;

                EditorUtility.SetDirty(field);

                return $"OK: ForceEffectField on '{gameObjectName}' configured. strength={field.strength:F3} radius={field.radius:F3} isGlobal={field.IsGlobal}";
            });
        }

        [McpPluginTool("hphys-configure-force-receiver", Title = "Heathen Physics / Configure Force Receiver")]
        [Description(@"Adds (if missing) and configures a ForceEffectReceiver component.
ForceEffectReceiver makes a rigidbody respond to ForceEffectField forces.
useLinear: whether linear forces are applied.
useAngular: whether angular (torque) forces are applied.
sensitivity: multiplier on all incoming forces (1.0 = normal, 0.5 = half effect).
Requires a PhysicsData component on the same GameObject.")]
        public string ConfigureForceReceiver(
            [Description("Name of the GameObject to add ForceEffectReceiver to.")] string gameObjectName,
            [Description("Accept linear (positional) forces from fields.")] bool? useLinear = null,
            [Description("Accept angular (torque) forces from fields.")] bool? useAngular = null,
            [Description("Sensitivity multiplier for all incoming forces [0-∞]. Default 1.0.")] float? sensitivity = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = FindGO(gameObjectName);

                var receiver = go.GetComponent<ForceEffectReceiver>();
                if (receiver == null)
                    receiver = go.AddComponent<ForceEffectReceiver>();

                if (useLinear.HasValue) receiver.useLinear = useLinear.Value;
                if (useAngular.HasValue) receiver.useAngular = useAngular.Value;
                if (sensitivity.HasValue) receiver.sensitivity = Mathf.Max(0f, sensitivity.Value);

                EditorUtility.SetDirty(receiver);

                return $"OK: ForceEffectReceiver on '{gameObjectName}' configured. useLinear={receiver.useLinear} useAngular={receiver.useAngular} sensitivity={receiver.sensitivity:F3}";
            });
        }
    }
}
