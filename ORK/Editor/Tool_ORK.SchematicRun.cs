#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.ORK.Editor
{
    public partial class Tool_ORK
    {
        [McpPluginTool("ork-schematic-run", Title = "ORK / Run Schematic")]
        [Description(@"Loads a Makinom schematic asset by name and starts running it on the Maki.MachineHandler.

action options:
  list  -- list all MakinomSchematicAsset assets in the project
  run   -- start the named schematic on Maki.MachineHandler.Instance
  stop  -- stop the named schematic if running

Schematics in ORK / Makinom drive most game logic (combatant init, ability execution, cutscenes, etc.).
Requires Play mode for run/stop.")]
        public string SchematicRun(
            [Description("'list' | 'run' | 'stop'")]
            string action,
            [Description("Schematic asset name or path (required for run/stop).")]
            string? schematicName = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (MakinomSchematicAssetType == null)
                    throw new System.Exception("MakinomSchematicAsset type not found. Is Makinom 2 installed?");

                action = (action ?? "").Trim().ToLowerInvariant();

                if (action == "list")
                {
                    var sb = new System.Text.StringBuilder();
                    var guids = AssetDatabase.FindAssets($"t:{MakinomSchematicAssetType.Name}");
                    sb.AppendLine($"Makinom schematics in project ({guids.Length}):");
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        var asset = AssetDatabase.LoadAssetAtPath(path, MakinomSchematicAssetType) as UnityEngine.Object;
                        if (asset != null) sb.AppendLine($"  - {asset.name}  [{path}]");
                    }
                    return sb.ToString();
                }

                if (string.IsNullOrEmpty(schematicName))
                    throw new System.Exception("schematicName is required for this action.");

                Object? asset_ = null;
                if (schematicName.Contains('/') || schematicName.EndsWith(".asset"))
                    asset_ = AssetDatabase.LoadAssetAtPath(schematicName, MakinomSchematicAssetType) as UnityEngine.Object;
                if (asset_ == null)
                {
                    var guids = AssetDatabase.FindAssets($"t:{MakinomSchematicAssetType.Name} {System.IO.Path.GetFileNameWithoutExtension(schematicName)}");
                    foreach (var guid in guids)
                    {
                        var p = AssetDatabase.GUIDToAssetPath(guid);
                        var a = AssetDatabase.LoadAssetAtPath(p, MakinomSchematicAssetType) as UnityEngine.Object;
                        if (a != null && string.Equals(a.name, System.IO.Path.GetFileNameWithoutExtension(schematicName), System.StringComparison.OrdinalIgnoreCase))
                        { asset_ = a; break; }
                    }
                    if (asset_ == null && guids.Length > 0)
                        asset_ = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]), MakinomSchematicAssetType) as UnityEngine.Object;
                }
                if (asset_ == null)
                    throw new System.Exception($"Schematic '{schematicName}' not found.");

                if (MakiType == null) throw new System.Exception("Maki type not found. Makinom may not be initialized.");
                var mh = GetStatic(MakiType, "MachineHandler") ?? throw new System.Exception("Maki.MachineHandler not available.");

                if (action == "stop")
                {
                    Call(mh, "Stop", asset_, null!);
                    return $"OK: Stop('{asset_.name}') called on MachineHandler.";
                }

                if (action == "run")
                {
                    if (!Application.isPlaying)
                        return "Schematic run requires play mode -- skipping. (Found asset: " + asset_.name + ")";

                    // Standard runtime path is MakinomSchematicAsset.Run() / Use(...). Try common method names.
                    foreach (var m in new[] { "Run", "Use", "PlaySchematic", "Execute" })
                    {
                        var r = Call(asset_, m);
                        if (r != null) return $"OK: {asset_.name}.{m}() invoked.";
                    }

                    // Fallback: AddMachine via MachineHandler
                    var mhAdd = Call(mh, "AddMachine", asset_, null!);
                    return mhAdd != null
                        ? $"OK: MachineHandler.AddMachine('{asset_.name}') invoked."
                        : $"WARN: Could not find a runtime entry point on '{asset_.name}'. Try invoking via game logic.";
                }

                throw new System.Exception($"Unknown action '{action}'. Use one of: list, run, stop.");
            });
        }
    }
}
