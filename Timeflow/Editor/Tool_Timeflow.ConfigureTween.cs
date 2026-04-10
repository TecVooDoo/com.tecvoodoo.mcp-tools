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
        [McpPluginTool("timeflow-configure-tween", Title = "Timeflow / Configure Tween")]
        [Description(@"Configures a Tween behavior on a GameObject. The GO must have a Tween component.
All parameters optional except gameObjectName -- only provided values change.
interpolation: None, Linear, EaseIn, EaseOut, EaseInOut, EaseInExpo, EaseOutExpo, EaseInOutExpo,
  EaseInCircle, EaseOutCircle, EaseInOutCircle, AnimationCurve, Switch.
repeatMode: Forever, Every, None.
minValue/maxValue: animation range (float). For vector tweens use minX/minY/minZ/maxX/maxY/maxZ.
amount: effect strength multiplier [0-1+].
pingPong: alternate direction each cycle.
Use timeflow-query first to see what behaviors exist on the GameObject.")]
        public string ConfigureTween(
            [Description("Name of the GameObject with a Tween component.")] string gameObjectName,
            [Description("Easing mode: None, Linear, EaseIn, EaseOut, EaseInOut, EaseInExpo, EaseOutExpo, EaseInOutExpo, EaseInCircle, EaseOutCircle, EaseInOutCircle, AnimationCurve, Switch.")] string? interpolation = null,
            [Description("Repeat mode: Forever, Every, None.")] string? repeatMode = null,
            [Description("Min float value for tween range.")] float? minValue = null,
            [Description("Max float value for tween range.")] float? maxValue = null,
            [Description("Min X component for vector tweens.")] float? minX = null,
            [Description("Min Y component for vector tweens.")] float? minY = null,
            [Description("Min Z component for vector tweens.")] float? minZ = null,
            [Description("Max X component for vector tweens.")] float? maxX = null,
            [Description("Max Y component for vector tweens.")] float? maxY = null,
            [Description("Max Z component for vector tweens.")] float? maxZ = null,
            [Description("Effect strength multiplier.")] float? amount = null,
            [Description("Alternate direction each cycle.")] bool? pingPong = null,
            [Description("Invert the interpolation curve.")] bool? invertInterpolation = null,
            [Description("Allow external triggering.")] bool? allowTrigger = null,
            [Description("Toggle mode for triggers.")] bool? triggerIsToggle = null,
            [Description("Time offset in seconds.")] float? timeOffset = null,
            [Description("Time scale multiplier.")] float? timeScale = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                UnityEngine.Component tween = GetComponentOfType(gameObjectName, TweenType, "Tween");
                StringBuilder changes = new StringBuilder();
                int changeCount = 0;

                void Apply(string fieldName, object? value)
                {
                    if (value == null) return;
                    if (Set(tween, fieldName, value))
                    {
                        changes.AppendLine($"  {fieldName} = {value}");
                        changeCount++;
                    }
                    else
                    {
                        changes.AppendLine($"  {fieldName}: not found or not writable.");
                    }
                }

                void ApplyEnum(string fieldName, string? value)
                {
                    if (value == null) return;
                    if (SetEnum(tween, fieldName, value))
                    {
                        changes.AppendLine($"  {fieldName} = {value}");
                        changeCount++;
                    }
                    else
                    {
                        changes.AppendLine($"  {fieldName}: not found or not writable.");
                    }
                }

                ApplyEnum("Interpolation", interpolation);
                ApplyEnum("RepeatMode", repeatMode);
                Apply("Amount", amount);
                Apply("PingPong", pingPong);
                Apply("InvertInterpolation", invertInterpolation);
                Apply("AllowTrigger", allowTrigger);
                Apply("TriggerIsToggle", triggerIsToggle);
                Apply("_TimeOffset", timeOffset);
                Apply("_TimeScale", timeScale);

                // Handle min/max values -- float and vector
                if (minValue.HasValue) Apply("MinValue", minValue);
                if (maxValue.HasValue) Apply("MaxValue", maxValue);

                // Vector min/max
                if (minX.HasValue || minY.HasValue || minZ.HasValue)
                {
                    object? currentMin = Get(tween, "MinVector");
                    if (currentMin is Vector4 minVec)
                    {
                        Vector4 newMin = new Vector4(
                            minX ?? minVec.x,
                            minY ?? minVec.y,
                            minZ ?? minVec.z,
                            minVec.w
                        );
                        FieldInfo? minField = tween.GetType().GetField("MinVector", BindingFlags.Public | BindingFlags.Instance);
                        if (minField != null)
                        {
                            minField.SetValue(tween, newMin);
                            changes.AppendLine($"  MinVector = ({newMin.x}, {newMin.y}, {newMin.z})");
                            changeCount++;
                        }
                    }
                }

                if (maxX.HasValue || maxY.HasValue || maxZ.HasValue)
                {
                    object? currentMax = Get(tween, "MaxVector");
                    if (currentMax is Vector4 maxVec)
                    {
                        Vector4 newMax = new Vector4(
                            maxX ?? maxVec.x,
                            maxY ?? maxVec.y,
                            maxZ ?? maxVec.z,
                            maxVec.w
                        );
                        FieldInfo? maxField = tween.GetType().GetField("MaxVector", BindingFlags.Public | BindingFlags.Instance);
                        if (maxField != null)
                        {
                            maxField.SetValue(tween, newMax);
                            changes.AppendLine($"  MaxVector = ({newMax.x}, {newMax.y}, {newMax.z})");
                            changeCount++;
                        }
                    }
                }

                if (changeCount > 0)
                    UnityEditor.EditorUtility.SetDirty(tween);

                if (changeCount == 0)
                    return $"No changes applied to Tween on '{gameObjectName}'.";

                return $"OK: Tween on '{gameObjectName}' updated ({changeCount} change(s)):\n{changes}";
            });
        }
    }
}
#endif
