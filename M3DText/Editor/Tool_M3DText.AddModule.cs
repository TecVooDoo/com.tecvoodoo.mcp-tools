#nullable enable
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.M3DText.Editor
{
    public partial class Tool_M3DText
    {
        [McpPluginTool("m3dt-add-module", Title = "Modular 3D Text / Add or List Module")]
        [Description(@"Adds a module to a Modular3DText component's adding-effects or deleting-effects list.

action options:
  list          -- list all available Module subclasses (TinyGiantStudio.Modules.Module-derived ScriptableObject assets)
  list-attached -- list current adding/deleting modules on the named text component
  add           -- add a Module asset to addingModules (or deletingModules with list='deleting')
  clear         -- clear addingModules (or deletingModules) on the named text component

For 'add': moduleName resolves to a project asset of type Module (TinyGiantStudio.Modules.Module).
For 'add' / 'clear': pass list='adding' (default) or list='deleting'.")]
        public string AddModule(
            [Description("'list' | 'list-attached' | 'add' | 'clear'.")]
            string action,
            [Description("GameObject name with Modular3DText. Required for list-attached, add, clear.")]
            string? gameObjectName = null,
            [Description("Module asset name or full path. Required for add.")]
            string? moduleName = null,
            [Description("Which list: 'adding' (default) or 'deleting'.")]
            string list = "adding"
        )
        {
            return MainThread.Instance.Run(() =>
            {
                action = (action ?? "").Trim().ToLowerInvariant();
                bool deleting = string.Equals((list ?? "adding").Trim(), "deleting", StringComparison.OrdinalIgnoreCase);
                string listFieldName = deleting ? "deletingModules" : "addingModules";

                if (action == "list")
                {
                    if (ModuleType == null) throw new Exception("TinyGiantStudio.Modules.Module type not found. Modular 3D Text may not be installed.");
                    var sb0 = new StringBuilder();
                    var guids = AssetDatabase.FindAssets($"t:{ModuleType.Name}");
                    sb0.AppendLine($"Module assets in project ({guids.Length}):");
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var asset = AssetDatabase.LoadAssetAtPath(path, ModuleType) as UnityEngine.Object;
                        if (asset == null) continue;
                        sb0.AppendLine($"  - {asset.name}  ({asset.GetType().Name})  [{path}]");
                    }
                    return sb0.ToString();
                }

                if (string.IsNullOrEmpty(gameObjectName))
                    throw new Exception("'gameObjectName' is required for this action.");

                var t = GetText(gameObjectName);

                if (action == "list-attached")
                {
                    var sb1 = new StringBuilder();
                    sb1.AppendLine($"=== Modules on '{gameObjectName}' ===");
                    AppendModuleList(sb1, t, "addingModules");
                    AppendModuleList(sb1, t, "deletingModules");
                    return sb1.ToString();
                }

                if (action == "clear")
                {
                    if (Get(t, listFieldName) is IList ilist)
                    {
                        int n = ilist.Count;
                        ilist.Clear();
                        EditorUtility.SetDirty(t);
                        return $"OK: Cleared {n} entry/entries from {listFieldName} on '{gameObjectName}'.";
                    }
                    throw new Exception($"Field '{listFieldName}' not found or not a list.");
                }

                if (action == "add")
                {
                    if (string.IsNullOrEmpty(moduleName))
                        throw new Exception("'moduleName' is required for action='add'.");
                    if (ModuleType == null || ModuleContainerType == null)
                        throw new Exception("Module / ModuleContainer types not loaded. Modular 3D Text may not be installed.");

                    UnityEngine.Object? moduleAsset = null;
                    if (moduleName.Contains('/') || moduleName.EndsWith(".asset"))
                        moduleAsset = AssetDatabase.LoadAssetAtPath(moduleName, ModuleType) as UnityEngine.Object;
                    if (moduleAsset == null)
                    {
                        var guids = AssetDatabase.FindAssets($"t:{ModuleType.Name} {System.IO.Path.GetFileNameWithoutExtension(moduleName)}");
                        foreach (var guid in guids)
                        {
                            var path = AssetDatabase.GUIDToAssetPath(guid);
                            var asset = AssetDatabase.LoadAssetAtPath(path, ModuleType) as UnityEngine.Object;
                            if (asset == null) continue;
                            if (string.Equals(asset.name, System.IO.Path.GetFileNameWithoutExtension(moduleName), StringComparison.OrdinalIgnoreCase))
                            {
                                moduleAsset = asset; break;
                            }
                        }
                        if (moduleAsset == null && guids.Length > 0)
                            moduleAsset = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), ModuleType) as UnityEngine.Object;
                    }
                    if (moduleAsset == null)
                        throw new Exception($"Module asset '{moduleName}' not found in project.");

                    // Construct ModuleContainer { module = moduleAsset; UpdateVariableHolders(); }
                    var ctor = ModuleContainerType.GetConstructor(Type.EmptyTypes)
                               ?? throw new Exception("ModuleContainer has no default constructor.");
                    var container = ctor.Invoke(null);
                    var moduleField = ModuleContainerType.GetField("module", BindingFlags.Public | BindingFlags.Instance);
                    if (moduleField == null) throw new Exception("ModuleContainer.module field not found.");
                    moduleField.SetValue(container, moduleAsset);

                    var updateMethod = ModuleContainerType.GetMethod("UpdateVariableHolders", BindingFlags.Public | BindingFlags.Instance);
                    updateMethod?.Invoke(container, null);

                    if (Get(t, listFieldName) is IList ilist)
                    {
                        ilist.Add(container);
                        EditorUtility.SetDirty(t);
                        return $"OK: Added '{moduleAsset.name}' ({moduleAsset.GetType().Name}) to {listFieldName} on '{gameObjectName}'. List size now {ilist.Count}.";
                    }
                    throw new Exception($"Field '{listFieldName}' not found or not a list.");
                }

                throw new Exception($"Unknown action '{action}'. Use one of: list, list-attached, add, clear.");
            });
        }

        static void AppendModuleList(StringBuilder sb, UnityEngine.Component t, string fieldName)
        {
            var list = Get(t, fieldName) as IList;
            int count = list?.Count ?? 0;
            sb.AppendLine($"  {fieldName} ({count}):");
            if (list == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var entry = list[i];
                if (entry == null) { sb.AppendLine($"    [{i}] (null)"); continue; }
                var moduleField = entry.GetType().GetField("module", BindingFlags.Public | BindingFlags.Instance);
                var moduleObj = moduleField?.GetValue(entry) as UnityEngine.Object;
                var varField = entry.GetType().GetField("variableHolders", BindingFlags.Public | BindingFlags.Instance);
                var varArray = varField?.GetValue(entry) as Array;
                int vCount = varArray?.Length ?? 0;
                sb.AppendLine($"    [{i}] {(moduleObj != null ? moduleObj.name + " (" + moduleObj.GetType().Name + ")" : "(null)")}  variables={vCount}");
            }
        }
    }
}
