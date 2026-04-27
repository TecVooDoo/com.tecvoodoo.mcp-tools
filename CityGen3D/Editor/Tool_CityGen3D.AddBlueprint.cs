#nullable enable
using System.ComponentModel;
using System.Linq;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.CityGen3D.Editor
{
    public partial class Tool_CityGen3D
    {
        [McpPluginTool("cg-add-blueprint", Title = "CityGen3D / Add Blueprint")]
        [Description(@"Lists or adds a CityGen3D Blueprint asset to the scene Generator.

action options:
  list -- list all Blueprint assets in the project (Apartment, Church, Office variants, etc.)
  add  -- attach the named Blueprint to the active Generator's Blueprints list

Blueprints in CityGen3D ride under Assets/CityGen3D/Blueprints/ as 19 prefab assets (per ENTRY-340).
The Generator's editor module collects them and uses them to seed buildings on Generate.")]
        public string AddBlueprint(
            [Description("'list' | 'add'")]
            string action,
            [Description("Blueprint asset name (required for action='add').")]
            string? blueprintName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                RequireCityGen();
                action = (action ?? "").Trim().ToLowerInvariant();

                if (BlueprintType == null)
                    throw new System.Exception("CityGen3D.Blueprint type not found.");

                if (action == "list")
                {
                    var sb = new StringBuilder();
                    var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/CityGen3D/Blueprints" });
                    sb.AppendLine($"CityGen3D Blueprints in project ({guids.Length}):");
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (go == null) continue;
                        // Filter to those with the Blueprint component
                        var bp = go.GetComponent(BlueprintType);
                        if (bp == null) continue;
                        sb.AppendLine($"  - {go.name}  [{path}]");
                    }
                    return sb.ToString();
                }

                if (action == "add")
                {
                    if (string.IsNullOrEmpty(blueprintName))
                        throw new System.Exception("blueprintName is required for action='add'.");

                    var gen = FindGenerator() ?? throw new System.Exception("No CityGen3D Generator found in active scene.");

                    GameObject? bpPrefab = null;
                    var guids = AssetDatabase.FindAssets($"t:Prefab {blueprintName}", new[] { "Assets/CityGen3D/Blueprints" });
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                        if (go == null) continue;
                        if (go.GetComponent(BlueprintType) == null) continue;
                        if (string.Equals(go.name, blueprintName, System.StringComparison.OrdinalIgnoreCase))
                        { bpPrefab = go; break; }
                    }
                    if (bpPrefab == null && guids.Length > 0)
                        bpPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    if (bpPrefab == null) throw new System.Exception($"Blueprint '{blueprintName}' not found.");

                    var bpComp = bpPrefab.GetComponent(BlueprintType);
                    var blueprintsField = gen.GetType().GetField("blueprints", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (blueprintsField == null)
                        throw new System.Exception("Generator has no 'blueprints' field. CityGen3D version may differ.");

                    var existing = blueprintsField.GetValue(gen) as System.Collections.IList;
                    if (existing == null) throw new System.Exception("Generator.blueprints is not a list.");

                    if (existing.Cast<object>().Any(b => b == (object)bpComp))
                        return $"'{blueprintName}' is already in Generator.blueprints.";

                    existing.Add(bpComp);
                    EditorUtility.SetDirty(gen);
                    return $"OK: Added Blueprint '{blueprintName}' to Generator. List size: {existing.Count}.";
                }

                throw new System.Exception($"Unknown action '{action}'. Use 'list' or 'add'.");
            });
        }
    }
}
