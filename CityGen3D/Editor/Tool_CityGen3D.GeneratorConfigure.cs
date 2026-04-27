#nullable enable
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.CityGen3D.Editor
{
    public partial class Tool_CityGen3D
    {
        [McpPluginTool("cg-generator-configure", Title = "CityGen3D / Configure Generator")]
        [Description(@"Sets serialized fields on the scene's CityGen3D Generator (CityGen3D.EditorExtension.Generator).

fieldAssignments format: comma-separated 'field=value' pairs, e.g.
  'mapSize=2000,resolution=512,seed=123,generateBuildings=true'.

Booleans, ints, floats, strings supported. Use action='list' to dump every public field's
current value before deciding what to change.

action options:
  list -- list every public field with current value (helps discover field names)
  set  -- apply fieldAssignments")]
        public string GeneratorConfigure(
            [Description("'list' | 'set'")]
            string action,
            [Description("For action='set': comma-separated 'field=value' pairs.")]
            string? fieldAssignments = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireCityGen();
                action = (action ?? "").Trim().ToLowerInvariant();

                var gen = FindGenerator() ?? throw new System.Exception("No CityGen3D Generator in active scene.");

                if (action == "list")
                {
                    var sb = new StringBuilder();
                    sb.AppendLine($"=== Generator '{gen.gameObject.name}' fields ===");
                    var fields = gen.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
                    foreach (var f in fields)
                    {
                        object? v;
                        try { v = f.GetValue(gen); } catch { v = "(error)"; }
                        sb.AppendLine($"  {f.FieldType.Name} {f.Name} = {v}");
                    }
                    return sb.ToString();
                }

                if (action == "set")
                {
                    if (string.IsNullOrEmpty(fieldAssignments))
                        throw new System.Exception("fieldAssignments required for action='set'.");

                    var changes = new StringBuilder();
                    int changeCount = 0;
                    foreach (var pair in fieldAssignments.Split(','))
                    {
                        var trimmed = pair.Trim();
                        if (trimmed.Length == 0) continue;
                        int eq = trimmed.IndexOf('=');
                        if (eq <= 0) { changes.AppendLine($"  [skip] '{trimmed}' is not 'field=value'"); continue; }

                        string fieldName = trimmed.Substring(0, eq).Trim();
                        string rawValue = trimmed.Substring(eq + 1).Trim();

                        var f = gen.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                        if (f == null) { changes.AppendLine($"  [skip] field '{fieldName}' not found"); continue; }

                        if (TryConvert(rawValue, f.FieldType, out object? converted))
                        {
                            f.SetValue(gen, converted);
                            changes.AppendLine($"  {fieldName} = {converted}");
                            changeCount++;
                        }
                        else
                        {
                            changes.AppendLine($"  [skip] could not convert '{rawValue}' to {f.FieldType.Name}");
                        }
                    }

                    if (changeCount > 0) EditorUtility.SetDirty(gen);
                    return $"OK: Generator updated ({changeCount} change(s)):\n{changes}";
                }

                throw new System.Exception($"Unknown action '{action}'. Use 'list' or 'set'.");
            });
        }

        static bool TryConvert(string raw, System.Type t, out object? result)
        {
            result = null;
            if (t == typeof(string)) { result = raw; return true; }
            if (t == typeof(bool)) { if (bool.TryParse(raw, out var b)) { result = b; return true; } return false; }
            if (t.IsEnum) { try { result = System.Enum.Parse(t, raw, ignoreCase: true); return true; } catch { return false; } }
            if (t == typeof(int)) { if (int.TryParse(raw, out var i)) { result = i; return true; } return false; }
            if (t == typeof(float)) { if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f)) { result = f; return true; } return false; }
            if (t == typeof(double)) { if (double.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var d)) { result = d; return true; } return false; }
            try { result = System.Convert.ChangeType(raw, t, System.Globalization.CultureInfo.InvariantCulture); return result != null; }
            catch { return false; }
        }
    }
}
