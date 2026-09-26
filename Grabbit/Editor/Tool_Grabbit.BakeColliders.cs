#if HAS_GRABBIT
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Grabbit2;
using UnityEngine;

namespace MCPTools.Grabbit.Editor
{
    public partial class Tool_Grabbit
    {
        [AiTool("grabbit-bake-colliders", Title = "Grabbit / Bake Colliders")]
        [Description(@"Bakes colliders onto meshes with Grabbit 2: convex decomposition (balance / precision) or primitive fitting (performance) -- concave meshes Unity cannot collide on its own.
Acts on 'targets' (names / hierarchy paths), or the editor selection when omitted. MeshFilter and SkinnedMeshRenderer meshes are bakeable.
action='info' reports the bake defaults and whether the targets carry a bakeable mesh (read-only) -- run it before a long bake.
A bake that adds 0 colliders usually means Read/Write is disabled on the mesh import, or the target is a read-only model prefab.")]
        public string BakeColliders(
            [Description("'bake' (default) or 'info'.")] string? action = null,
            [Description("GameObject names / hierarchy paths. Omit to use the editor selection.")] string[]? targets = null,
            [Description("balance | precision | performance. Omit for the Grabbit settings default.")] string? strategy = null,
            [Description("Also bake child meshes. Default true.")] bool includeChildren = true,
            [Description("Max convex pieces per mesh (>= 1). Omit for the settings default.")] int? maxPieces = null,
            [Description("Bake each child separately. Omit for the settings default.")] bool? perChild = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string a = string.IsNullOrWhiteSpace(action) ? "bake" : action!.Trim().ToLowerInvariant();

                if (a == "info")
                {
                    List<GameObject> infoTargets;
                    try { infoTargets = ResolveTargets(targets, "target"); }
                    catch (Exception) when (targets == null || targets.Length == 0) { infoTargets = new List<GameObject>(); }

                    var info = GrabbitColliderBakeApi.GetInfo(infoTargets);
                    var sb = new StringBuilder();
                    sb.AppendLine($"Default strategy: {info.DefaultStrategy} | maxPieces: {info.DefaultMaxPieces} | perChild: {info.DefaultPerChild}");
                    sb.AppendLine($"Strategies: {string.Join(", ", info.Strategies)}");
                    sb.AppendLine($"Targets ({info.TargetCount}): {string.Join(", ", info.TargetNames)}");
                    sb.AppendLine($"TargetsHaveBakeableMesh: {info.TargetsHaveBakeableMesh}");
                    return sb.ToString();
                }
                if (a != "bake")
                    throw new Exception($"Unknown action '{action}'. Use: bake, info.");

                var resolved = ResolveTargets(targets, "target");
                var parsed = GrabbitColliderBakeApi.ParseStrategy(strategy, out string strategyError);
                if (strategyError != null)
                    throw new Exception(strategyError);

                var r = GrabbitColliderBakeApi.Bake(resolved, includeChildren, parsed, maxPieces, perChild);
                if (!r.Success)
                    throw new Exception(r.Message);
                return $"{r.Message}\nCollidersAdded: {r.CollidersAdded} | Strategy: {r.StrategyUsed} | IncludeChildren: {r.IncludeChildren} | Targets: {string.Join(", ", r.TargetNames)}";
            });
        }
    }
}
#endif
