#if HAS_BOINGKIT
#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.BoingKit.Editor
{
    public partial class Tool_BoingKit
    {
        [McpPluginTool("boing-configure", Title = "Boing Kit / Configure Component")]
        [Description(@"Configures a Boing Kit component on a GameObject. Specify the componentType and only the
parameters you want to change -- all others remain untouched.
componentType must be one of: Effector, Behavior, Bones, or ReactorField.
For Effector: radius, fullEffectRadiusRatio, maxImpulseSpeed, continuousMotion, moveDistance, linearImpulse, rotationAngle, angularImpulse.
For Behavior: enablePositionEffect, enableRotationEffect, enableScaleEffect.
For ReactorField: cellSize, cellsX, cellsY, cellsZ, falloffMode, falloffRatio, enablePositionEffect, enableRotationEffect, enablePropagation.")]
        public string ConfigureBoingComponent(
            [Description("Name of the GameObject with the Boing Kit component.")]
            string gameObjectName,
            [Description("Component type to configure: Effector, Behavior, Bones, or ReactorField.")]
            string componentType,
            [Description("(Effector) Effect radius.")] float? radius = null,
            [Description("(Effector) Ratio of full effect within radius [0-1].")] float? fullEffectRadiusRatio = null,
            [Description("(Effector) Maximum impulse speed.")] float? maxImpulseSpeed = null,
            [Description("(Effector) Enable continuous motion effect.")] bool? continuousMotion = null,
            [Description("(Effector) Move distance for impulse.")] float? moveDistance = null,
            [Description("(Effector) Linear impulse strength.")] float? linearImpulse = null,
            [Description("(Effector) Rotation angle for angular effect.")] float? rotationAngle = null,
            [Description("(Effector) Angular impulse strength.")] float? angularImpulse = null,
            [Description("(Behavior/ReactorField) Enable position effect.")] bool? enablePositionEffect = null,
            [Description("(Behavior/ReactorField) Enable rotation effect.")] bool? enableRotationEffect = null,
            [Description("(Behavior) Enable scale effect.")] bool? enableScaleEffect = null,
            [Description("(ReactorField) Size of each reactor cell.")] float? cellSize = null,
            [Description("(ReactorField) Number of cells along the X axis.")] int? cellsX = null,
            [Description("(ReactorField) Number of cells along the Y axis.")] int? cellsY = null,
            [Description("(ReactorField) Number of cells along the Z axis.")] int? cellsZ = null,
            [Description("(ReactorField) Falloff mode (e.g. None, Linear, Quadratic).")] string? falloffMode = null,
            [Description("(ReactorField) Falloff ratio [0-1].")] float? falloffRatio = null,
            [Description("(ReactorField) Enable propagation of effects across cells.")] bool? enablePropagation = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = GameObject.Find(gameObjectName);
                if (go == null)
                    throw new Exception($"GameObject '{gameObjectName}' not found.");

                var type = ResolveComponentType(componentType);
                var fullName = ResolveFullTypeName(componentType);
                var comp = GetRequiredBoingComponent(go, type, fullName);

                var changes = new StringBuilder();
                int changeCount = 0;

                void Apply(string fieldName, object? value)
                {
                    if (value == null) return;
                    if (Set(comp, fieldName, value))
                    {
                        changes.AppendLine($"  {fieldName} = {value}");
                        changeCount++;
                    }
                    else
                    {
                        changes.AppendLine($"  {fieldName}: field/property not found or not writable.");
                    }
                }

                void ApplyEnum(string fieldName, string? value, Type enumType)
                {
                    if (value == null) return;
                    var enumVal = Enum.Parse(enumType, value, ignoreCase: true);
                    if (Set(comp, fieldName, enumVal))
                    {
                        changes.AppendLine($"  {fieldName} = {enumVal}");
                        changeCount++;
                    }
                    else
                    {
                        changes.AppendLine($"  {fieldName}: field/property not found or not writable.");
                    }
                }

                switch (componentType)
                {
                    case "Effector":
                        Apply("Radius", radius);
                        Apply("FullEffectRadiusRatio", fullEffectRadiusRatio);
                        Apply("MaxImpulseSpeed", maxImpulseSpeed);
                        Apply("ContinuousMotion", continuousMotion);
                        Apply("MoveDistance", moveDistance);
                        Apply("LinearImpulse", linearImpulse);
                        Apply("RotationAngle", rotationAngle);
                        Apply("AngularImpulse", angularImpulse);
                        break;

                    case "Behavior":
                        Apply("EnablePositionEffect", enablePositionEffect);
                        Apply("EnableRotationEffect", enableRotationEffect);
                        Apply("EnableScaleEffect", enableScaleEffect);
                        break;

                    case "Bones":
                        // Bones has limited direct configuration; most setup is via BoneChains array.
                        // Expose what's available at top level.
                        break;

                    case "ReactorField":
                        Apply("CellSize", cellSize);
                        Apply("CellsX", cellsX);
                        Apply("CellsY", cellsY);
                        Apply("CellsZ", cellsZ);
                        Apply("FalloffRatio", falloffRatio);
                        Apply("EnablePositionEffect", enablePositionEffect);
                        Apply("EnableRotationEffect", enableRotationEffect);
                        Apply("EnablePropagation", enablePropagation);

                        // FalloffMode is an enum -- resolve via reflection
                        if (falloffMode != null)
                        {
                            var falloffField = comp.GetType().GetField("FalloffMode", BindingFlags.Public | BindingFlags.Instance);
                            if (falloffField != null)
                            {
                                ApplyEnum("FalloffMode", falloffMode, falloffField.FieldType);
                            }
                            else
                            {
                                var falloffProp = comp.GetType().GetProperty("FalloffMode", BindingFlags.Public | BindingFlags.Instance);
                                if (falloffProp != null)
                                    ApplyEnum("FalloffMode", falloffMode, falloffProp.PropertyType);
                                else
                                    changes.AppendLine("  FalloffMode: field/property not found.");
                            }
                        }
                        break;
                }

                if (changeCount > 0)
                    EditorUtility.SetDirty(comp);

                if (changeCount == 0)
                    return $"No changes applied to {fullName} on '{gameObjectName}'. Check parameter names.";

                return $"OK: {fullName} on '{gameObjectName}' updated ({changeCount} change(s)):\n{changes}";
            });
        }
    }
}
#endif
