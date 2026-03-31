#if HAS_BEHAVIOR_DESIGNER
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Opsive.BehaviorDesigner.Runtime;
using Opsive.BehaviorDesigner.Runtime.Components;
using UnityEngine;

namespace MCPTools.BehaviorDesigner.Editor
{
    public partial class Tool_BehaviorDesigner
    {
        [McpPluginTool("bd-tick", Title = "Behavior Designer / Manual Tick")]
        [Description("Manually ticks a BehaviorTree. Only works in play mode with UpdateMode set to Manual.")]
        public string Tick(
            [Description("Name of the GameObject with BehaviorTree component(s).")]
            string gameObjectName,
            [Description("Number of ticks to perform. Default 1.")]
            int? count = 1,
            [Description("Index of the tree if multiple BehaviorTree components exist. Default 0.")]
            int? treeIndex = 0
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!Application.isPlaying)
                    throw new Exception("bd-tick requires play mode.");

                GameObject go = FindGO(gameObjectName);
                BehaviorTree[] trees = go.GetComponents<BehaviorTree>();
                if (trees.Length == 0)
                    throw new Exception($"'{gameObjectName}' has no BehaviorTree component.");

                int idx = treeIndex ?? 0;
                if (idx < 0 || idx >= trees.Length)
                    throw new Exception($"Tree index {idx} out of range. '{gameObjectName}' has {trees.Length} tree(s) (0-{trees.Length - 1}).");

                BehaviorTree tree = trees[idx];

                if (tree.UpdateMode != UpdateMode.Manual)
                    return $"Warning: BehaviorTree [{idx}] on '{go.name}' has UpdateMode={tree.UpdateMode}, not Manual. Tick may have no effect if the tree is already auto-updating.";

                if (!tree.IsActive())
                    return $"BehaviorTree [{idx}] on '{go.name}' is not active. Start it first.";

                int tickCount = Math.Max(1, count ?? 1);
                for (int i = 0; i < tickCount; i++)
                {
                    tree.Tick();
                }

                return $"Ticked BehaviorTree [{idx}] on '{go.name}' {tickCount} time(s). Status: {tree.Status}";
            });
        }
    }
}
#endif
