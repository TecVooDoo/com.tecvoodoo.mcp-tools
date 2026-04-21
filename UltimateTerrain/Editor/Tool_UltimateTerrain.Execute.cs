#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.UltimateTerrain.Editor
{
    public partial class Tool_UltimateTerrain
    {
        [McpPluginTool("ut-execute", Title = "Ultimate Terrain / Execute")]
        [Description(@"Triggers an action on an UltimateTerrain instance.
action options:
  Execute -- run all modules with animation
  ExecuteInstant -- run all modules instantly (no tween)
  ExecuteHeight -- height modules only
  ExecuteTextures -- texture layers only
  ExecuteDetails -- details layers only
  ExecuteTree -- tree layers only
  ExecutePrefab -- prefab layers only
  Pause -- pause active executions
  Resume -- resume paused executions
  Stop -- stop all executions
  Bake -- bake current state to UT_Storage
  ResetTerrain -- restore from last bake
  ResetHeight, ResetTexture, ResetDetails, ResetTrees, ResetPrefabs -- reset specific module
position/size override the configured values for this execution. strength is a multiplier (default 1).")]
        public string Execute(
            [Description("GameObject with UltimateTerrain.")] string gameObjectName,
            [Description("Action: Execute, ExecuteInstant, ExecuteHeight, ExecuteTextures, ExecuteDetails, ExecuteTree, ExecutePrefab, Pause, Resume, Stop, Bake, ResetTerrain, ResetHeight, ResetTexture, ResetDetails, ResetTrees, ResetPrefabs.")] string action,
            [Description("World position X (optional).")] float? worldX = null,
            [Description("World position Y (optional).")] float? worldY = null,
            [Description("World position Z (optional).")] float? worldZ = null,
            [Description("Size X (optional).")] float? sizeX = null,
            [Description("Size Y (optional).")] float? sizeY = null,
            [Description("Strength multiplier (default 1).")] float? strength = null,
            [Description("Rotation in degrees (Execute only).")] float? rotation = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var ut = GetUT(gameObjectName);
                bool hasPos = worldX.HasValue && worldY.HasValue && worldZ.HasValue;
                bool hasSize = sizeX.HasValue && sizeY.HasValue;
                Vector3 pos = hasPos ? new Vector3(worldX!.Value, worldY!.Value, worldZ!.Value) : Vector3.zero;
                Vector2 size = hasSize ? new Vector2(sizeX!.Value, sizeY!.Value) : Vector2.zero;
                float str = strength ?? 1f;

                switch (action.ToLowerInvariant())
                {
                    case "execute":
                        if (rotation.HasValue && hasPos && hasSize) ut.Execute(pos, size, str, rotation.Value);
                        else if (hasPos && hasSize) ut.Execute(pos, size, str);
                        else if (hasPos) ut.Execute(pos, str);
                        else ut.Execute();
                        break;
                    case "executeinstant":
                        if (hasPos && hasSize) ut.ExecuteInstant(pos, size);
                        else if (hasPos) ut.ExecuteInstant(pos);
                        else ut.ExecuteInstant();
                        break;
                    case "executeheight":
                        if (hasPos && hasSize) ut.ExecuteHeight(pos, size, ut.heightModules);
                        else throw new Exception("ExecuteHeight requires worldX/Y/Z and sizeX/Y.");
                        break;
                    case "executetextures":
                        if (hasPos && hasSize) ut.ExecuteTextures(pos, size, ut.textureLayers);
                        else throw new Exception("ExecuteTextures requires worldX/Y/Z and sizeX/Y.");
                        break;
                    case "pause": ut.Pause(); break;
                    case "resume": ut.Resume(); break;
                    case "stop": ut.Stop(); break;
                    case "bake": ut.BakeTerrainData(); break;
                    case "resetterrain": ut.ResetTerrain(); break;
                    case "resetheight": ut.ResetHeight(); break;
                    case "resettexture": ut.ResetTexture(); break;
                    case "resetdetails": ut.ResetDetails(); break;
                    case "resettrees": ut.ResetTrees(); break;
                    case "resetprefabs": ut.ResetPrefabs(); break;
                    default:
                        throw new Exception($"Unknown action '{action}'. Valid: Execute, ExecuteInstant, ExecuteHeight, ExecuteTextures, ExecuteDetails, ExecuteTree, ExecutePrefab, Pause, Resume, Stop, Bake, ResetTerrain, Reset[Height|Texture|Details|Trees|Prefabs].");
                }

                EditorUtility.SetDirty(ut);
                return $"OK: UltimateTerrain '{gameObjectName}' action='{action}' executed. IsExecuting={ut.IsExecuting()}";
            });
        }
    }
}
