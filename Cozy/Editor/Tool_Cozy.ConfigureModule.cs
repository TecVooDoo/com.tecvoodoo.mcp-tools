#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
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
        [McpPluginTool("cozy-configure-module", Title = "Cozy / Configure Module")]
        [Description(@"Adds, removes, enables, disables, or sets fields on a CozyModule (or any subclass) on a CozyWeather sphere.

moduleType accepts: 'Climate' | 'Wind' | 'Time' | 'Atmosphere' | 'Ambience' | 'Weather' | 'Reflections' | 'Satellite' |
'Interactions' | 'Event' | 'SaveLoad' | 'Debug' | 'Microsplat' | 'PureNature' | 'TVE' | 'Buto' | 'Transit' | 'SystemTime'
(or any full type name like 'DistantLands.Cozy.CozyClimateModule', or a CozyModule subclass short name).

action options:
  list     -- list all CozyModule subclasses available + which are attached
  query    -- read serialized fields from the named module
  add      -- AddComponent + InitializeModule on the weather sphere's module holder
  remove   -- DeintitializeModule (calls CheckIfModuleCanBeRemoved first)
  reset    -- DeintitializeModule + re-add
  enable   -- set .enabled = true
  disable  -- set .enabled = false
  set      -- assign field=value pairs (see fieldAssignments)

fieldAssignments format: comma-separated 'field=value' pairs, e.g.
  'snowMeltSpeed=0.2,dryingSpeed=0.6,useWindzone=false'.
Booleans, ints, floats, strings, and enum values (case-insensitive) are supported.")]
        public string ConfigureModule(
            [Description("'list' | 'query' | 'add' | 'remove' | 'reset' | 'enable' | 'disable' | 'set'.")]
            string action,
            [Description("Optional GameObject name with CozyWeather. Omit to use the active scene instance. Ignored when action='list'.")]
            string? gameObjectName = null,
            [Description("Module type short name or full name. Required for all actions except 'list'.")]
            string? moduleType = null,
            [Description("For action='set': comma-separated 'field=value' pairs.")]
            string? fieldAssignments = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                action = (action ?? "").Trim().ToLowerInvariant();

                if (action == "list")
                {
                    var sb = new StringBuilder();
                    var all = ListModuleTypes();
                    sb.AppendLine($"Available CozyModule subclasses ({all.Count}):");
                    CozyWeather? cw = null;
                    try { cw = GetWeather(gameObjectName); } catch { /* OK -- list works without a sphere */ }

                    foreach (var t in all)
                    {
                        bool attached = cw != null && cw.GetModule(t) != null;
                        sb.AppendLine($"  {(attached ? "[x]" : "[ ]")} {t.FullName}");
                    }
                    return sb.ToString();
                }

                if (string.IsNullOrEmpty(moduleType))
                    throw new Exception("'moduleType' is required for this action.");

                CozyWeather weather = GetWeather(gameObjectName);
                Type? type = FindModuleType(moduleType);
                if (type == null)
                    throw new Exception($"CozyModule subclass '{moduleType}' not found. Use action='list' to see available types.");

                CozyModule? existing = weather.GetModule(type);

                switch (action)
                {
                    case "query":
                    {
                        if (existing == null)
                            return $"Module '{type.Name}' is NOT attached to '{weather.gameObject.name}'.";

                        var sb = new StringBuilder();
                        sb.AppendLine($"=== {type.FullName} on '{weather.gameObject.name}/{existing.gameObject.name}' ===");
                        sb.AppendLine($"  enabled: {existing.enabled}");
                        var so = new SerializedObject(existing);
                        var it = so.GetIterator();
                        bool entered = it.NextVisible(true);
                        while (entered)
                        {
                            string val = SerializedPropToString(it);
                            sb.AppendLine($"  {it.name}: {val}");
                            entered = it.NextVisible(false);
                        }
                        return sb.ToString();
                    }

                    case "add":
                    {
                        if (existing != null)
                            return $"Module '{type.Name}' is already attached to '{weather.gameObject.name}'.";
                        weather.InitializeModule(type);
                        EditorUtility.SetDirty(weather);
                        if (weather.moduleHolder != null) EditorUtility.SetDirty(weather.moduleHolder);
                        return $"OK: Added {type.Name} to '{weather.gameObject.name}'.";
                    }

                    case "remove":
                    {
                        if (existing == null)
                            return $"Module '{type.Name}' is not attached -- nothing to remove.";
                        weather.DeintitializeModule(existing);
                        EditorUtility.SetDirty(weather);
                        return $"OK: Removed {type.Name} from '{weather.gameObject.name}'.";
                    }

                    case "reset":
                    {
                        if (existing == null)
                            throw new Exception($"Module '{type.Name}' is not attached -- nothing to reset.");
                        weather.ResetModule(existing);
                        return $"OK: Reset {type.Name} on '{weather.gameObject.name}' (coroutine started).";
                    }

                    case "enable":
                    {
                        if (existing == null)
                            throw new Exception($"Module '{type.Name}' is not attached.");
                        existing.enabled = true;
                        EditorUtility.SetDirty(existing);
                        return $"OK: Enabled {type.Name}.";
                    }

                    case "disable":
                    {
                        if (existing == null)
                            throw new Exception($"Module '{type.Name}' is not attached.");
                        existing.enabled = false;
                        EditorUtility.SetDirty(existing);
                        return $"OK: Disabled {type.Name}.";
                    }

                    case "set":
                    {
                        if (existing == null)
                            throw new Exception($"Module '{type.Name}' is not attached. Use action='add' first.");
                        if (string.IsNullOrEmpty(fieldAssignments))
                            throw new Exception("'fieldAssignments' is required for action='set'.");

                        var sb = new StringBuilder();
                        int changeCount = 0;
                        foreach (var pair in fieldAssignments.Split(','))
                        {
                            var trimmed = pair.Trim();
                            if (trimmed.Length == 0) continue;
                            int eq = trimmed.IndexOf('=');
                            if (eq <= 0)
                            {
                                sb.AppendLine($"  [skip] '{trimmed}' is not 'field=value'");
                                continue;
                            }

                            string fieldName = trimmed.Substring(0, eq).Trim();
                            string rawValue = trimmed.Substring(eq + 1).Trim();

                            if (TrySetMember(existing, fieldName, rawValue, out string applied))
                            {
                                sb.AppendLine($"  {fieldName} = {applied}");
                                changeCount++;
                            }
                            else
                            {
                                sb.AppendLine($"  [skip] field/property '{fieldName}' not found or unsupported type.");
                            }
                        }

                        if (changeCount > 0)
                            EditorUtility.SetDirty(existing);

                        return changeCount == 0
                            ? $"No changes applied to {type.Name}.\n{sb}"
                            : $"OK: {type.Name} updated ({changeCount} change(s)):\n{sb}";
                    }

                    default:
                        throw new Exception($"Unknown action '{action}'. Use one of: list, query, add, remove, reset, enable, disable, set.");
                }
            });
        }

        static bool TrySetMember(object target, string name, string raw, out string applied)
        {
            applied = "";
            var type = target.GetType();
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null)
            {
                if (TryConvert(raw, field.FieldType, out object? converted))
                {
                    field.SetValue(target, converted);
                    applied = converted?.ToString() ?? "null";
                    return true;
                }
                return false;
            }

            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                if (TryConvert(raw, prop.PropertyType, out object? converted))
                {
                    prop.SetValue(target, converted);
                    applied = converted?.ToString() ?? "null";
                    return true;
                }
            }
            return false;
        }

        static bool TryConvert(string raw, Type targetType, out object? result)
        {
            result = null;

            if (targetType == typeof(string)) { result = raw; return true; }
            if (targetType == typeof(bool))
            {
                if (bool.TryParse(raw, out var b)) { result = b; return true; }
                return false;
            }
            if (targetType.IsEnum)
            {
                try { result = Enum.Parse(targetType, raw, ignoreCase: true); return true; }
                catch { return false; }
            }
            if (targetType == typeof(int)) { if (int.TryParse(raw, out var i)) { result = i; return true; } return false; }
            if (targetType == typeof(float)) { if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f)) { result = f; return true; } return false; }
            if (targetType == typeof(double)) { if (double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d)) { result = d; return true; } return false; }
            if (targetType == typeof(Color))
            {
                string hex = raw.StartsWith("#") ? raw : "#" + raw;
                if (ColorUtility.TryParseHtmlString(hex, out var c)) { result = c; return true; }
                return false;
            }
            if (targetType == typeof(Vector2))
            {
                var parts = raw.Split(',');
                if (parts.Length == 2 &&
                    float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x) &&
                    float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var y))
                { result = new Vector2(x, y); return true; }
                return false;
            }
            if (targetType == typeof(Vector3))
            {
                var parts = raw.Split(',');
                if (parts.Length == 3 &&
                    float.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x) &&
                    float.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var y) &&
                    float.TryParse(parts[2], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var z))
                { result = new Vector3(x, y, z); return true; }
                return false;
            }

            // Last-ditch: System.Convert
            try { result = Convert.ChangeType(raw, targetType, System.Globalization.CultureInfo.InvariantCulture); return result != null; }
            catch { return false; }
        }

        static string SerializedPropToString(SerializedProperty p)
        {
            switch (p.propertyType)
            {
                case SerializedPropertyType.Integer:    return p.intValue.ToString();
                case SerializedPropertyType.Boolean:    return p.boolValue.ToString();
                case SerializedPropertyType.Float:      return p.floatValue.ToString("F4");
                case SerializedPropertyType.String:     return p.stringValue ?? "";
                case SerializedPropertyType.Color:      return $"#{ColorUtility.ToHtmlStringRGBA(p.colorValue)}";
                case SerializedPropertyType.ObjectReference:
                    return p.objectReferenceValue != null ? p.objectReferenceValue.name : "(null)";
                case SerializedPropertyType.Enum:       return p.enumDisplayNames != null && p.enumValueIndex >= 0 && p.enumValueIndex < p.enumDisplayNames.Length ? p.enumDisplayNames[p.enumValueIndex] : p.enumValueIndex.ToString();
                case SerializedPropertyType.Vector2:    return $"{p.vector2Value}";
                case SerializedPropertyType.Vector3:    return $"{p.vector3Value}";
                case SerializedPropertyType.LayerMask:  return $"mask={p.intValue}";
                case SerializedPropertyType.ArraySize:  return p.intValue.ToString();
                case SerializedPropertyType.Generic:    return p.isArray ? $"array[{p.arraySize}]" : "{...}";
                default:                                return p.propertyType.ToString();
            }
        }
    }
}
