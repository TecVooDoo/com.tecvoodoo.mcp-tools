#if HAS_BEHAVIOR_DESIGNER
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Opsive.BehaviorDesigner.Runtime;
using UnityEngine;

namespace MCPTools.BehaviorDesigner.Editor
{
    public partial class Tool_BehaviorDesigner
    {
        [McpPluginTool("bd-control", Title = "Behavior Designer / Control Tree")]
        [Description("Control behavior tree execution: start, stop, pause, unpause, or restart a BehaviorTree on a GameObject.")]
        public string Control(
            [Description("Name of the GameObject with BehaviorTree component(s).")]
            string gameObjectName,
            [Description("Action to perform: 'start', 'stop', 'pause', 'unpause', or 'restart'.")]
            string action,
            [Description("Index of the tree if multiple BehaviorTree components exist. Default 0.")]
            int? treeIndex = 0
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                BehaviorTree[] trees = go.GetComponents<BehaviorTree>();
                if (trees.Length == 0)
                    throw new Exception($"'{gameObjectName}' has no BehaviorTree component.");

                int idx = treeIndex ?? 0;
                if (idx < 0 || idx >= trees.Length)
                    throw new Exception($"Tree index {idx} out of range. '{gameObjectName}' has {trees.Length} tree(s) (0-{trees.Length - 1}).");

                BehaviorTree tree = trees[idx];
                string actionLower = action.ToLower().Trim();

                switch (actionLower)
                {
                    case "start":
                    case "enable":
                        bool started = tree.StartBehavior();
                        return started
                            ? $"Started BehaviorTree [{idx}] on '{go.name}'."
                            : $"BehaviorTree [{idx}] on '{go.name}' could not start (may already be running or missing data).";

                    case "stop":
                    case "disable":
                        bool stopped = tree.StopBehavior(false);
                        return stopped
                            ? $"Stopped BehaviorTree [{idx}] on '{go.name}'."
                            : $"BehaviorTree [{idx}] on '{go.name}' could not stop (may not be running).";

                    case "pause":
                        bool paused = tree.StopBehavior(true);
                        return paused
                            ? $"Paused BehaviorTree [{idx}] on '{go.name}'."
                            : $"BehaviorTree [{idx}] on '{go.name}' could not pause (may not be running).";

                    case "unpause":
                    case "resume":
                        if (!tree.IsPaused())
                            return $"BehaviorTree [{idx}] on '{go.name}' is not paused.";
                        bool resumed = tree.StartBehavior();
                        return resumed
                            ? $"Resumed BehaviorTree [{idx}] on '{go.name}'."
                            : $"BehaviorTree [{idx}] on '{go.name}' could not resume.";

                    case "restart":
                        bool restarted = tree.RestartBehavior();
                        return restarted
                            ? $"Restarted BehaviorTree [{idx}] on '{go.name}'."
                            : $"BehaviorTree [{idx}] on '{go.name}' could not restart (may not be active).";

                    default:
                        throw new Exception($"Unknown action '{action}'. Use 'start', 'stop', 'pause', 'unpause', or 'restart'.");
                }
            });
        }
    }
}
#endif
