#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Unity.Physics.Authoring;
using UnityEditor;
using UnityEngine;

namespace MCPTools.UnityPhysics.Editor
{
    public partial class Tool_UnityPhysics
    {
        [McpPluginTool("uphys-configure-step", Title = "Unity Physics / Configure Step")]
        [Description(@"Adds (if missing) and configures a PhysicsStepAuthoring component on a GameObject.
PhysicsStepAuthoring controls global ECS physics simulation parameters: gravity, solver iterations,
substep count, multi-threading, collision tolerance, broadphase settings, and depenetration velocity.
Only one PhysicsStepAuthoring should exist in a scene. Only provided parameters are applied;
omitted parameters keep their current values.")]
        public string ConfigureStep(
            [Description("Name of the GameObject with (or to add) PhysicsStepAuthoring.")]
            string gameObjectName,
            [Description("Gravity X component.")] float gravityX = float.NaN,
            [Description("Gravity Y component (default -9.81).")] float gravityY = float.NaN,
            [Description("Gravity Z component.")] float gravityZ = float.NaN,
            [Description("Number of solver iterations. Higher = more stable, worse performance. Min 1.")] int solverIterationCount = -1,
            [Description("Number of substeps per frame. Higher = smaller timesteps, more stable. Min 1.")] int substepCount = -1,
            [Description("Enable multi-threaded physics processing.")] bool? multiThreaded = null,
            [Description("Collision tolerance. Minimum distance for contact creation. Increase if tunneling occurs.")] float collisionTolerance = float.NaN,
            [Description("Enable solver stabilization heuristic for better stacking behavior.")] bool? enableSolverStabilization = null,
            [Description("Synchronize collision world after step for more precise queries.")] bool? synchronizeCollisionWorld = null,
            [Description("Enable incremental dynamic broadphase for scenes with many sleeping dynamic bodies.")] bool? incrementalDynamicBroadphase = null,
            [Description("Enable incremental static broadphase for scenes with many static bodies.")] bool? incrementalStaticBroadphase = null,
            [Description("Max velocity for separating intersecting dynamic bodies.")] float maxDynamicDepenetrationVelocity = float.NaN,
            [Description("Max velocity for separating dynamic bodies intersecting with static bodies.")] float maxStaticDepenetrationVelocity = float.NaN
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = GameObject.Find(gameObjectName) ?? throw new System.Exception($"GameObject '{gameObjectName}' not found.");
                PhysicsStepAuthoring step = go.GetComponent<PhysicsStepAuthoring>();
                if (step == null) step = go.AddComponent<PhysicsStepAuthoring>();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"=== PhysicsStepAuthoring configured: {go.name} ===");

                Undo.RecordObject(step, "MCP Configure PhysicsStep");

                // Handle gravity: read current, apply per-axis overrides
                Unity.Mathematics.float3 currentGravity = step.Gravity;
                bool gravityChanged = false;
                if (!float.IsNaN(gravityX)) { currentGravity.x = gravityX; gravityChanged = true; }
                if (!float.IsNaN(gravityY)) { currentGravity.y = gravityY; gravityChanged = true; }
                if (!float.IsNaN(gravityZ)) { currentGravity.z = gravityZ; gravityChanged = true; }
                if (gravityChanged)
                {
                    step.Gravity = currentGravity;
                    Unity.Mathematics.float3 g = step.Gravity;
                    sb.AppendLine($"  Gravity -> ({g.x:F3}, {g.y:F3}, {g.z:F3})");
                }

                if (solverIterationCount >= 1)
                {
                    step.SolverIterationCount = solverIterationCount;
                    sb.AppendLine($"  SolverIterationCount -> {step.SolverIterationCount}");
                }
                if (substepCount >= 1)
                {
                    step.SubstepCount = substepCount;
                    sb.AppendLine($"  SubstepCount -> {step.SubstepCount}");
                }
                if (multiThreaded.HasValue)
                {
                    step.MultiThreaded = multiThreaded.Value;
                    sb.AppendLine($"  MultiThreaded -> {step.MultiThreaded}");
                }
                if (!float.IsNaN(collisionTolerance))
                {
                    step.CollisionTolerance = collisionTolerance;
                    sb.AppendLine($"  CollisionTolerance -> {step.CollisionTolerance:F4}");
                }
                if (enableSolverStabilization.HasValue)
                {
                    step.EnableSolverStabilizationHeuristic = enableSolverStabilization.Value;
                    sb.AppendLine($"  EnableSolverStabilization -> {step.EnableSolverStabilizationHeuristic}");
                }
                if (synchronizeCollisionWorld.HasValue)
                {
                    step.SynchronizeCollisionWorld = synchronizeCollisionWorld.Value;
                    sb.AppendLine($"  SynchronizeCollisionWorld -> {step.SynchronizeCollisionWorld}");
                }
                if (incrementalDynamicBroadphase.HasValue)
                {
                    step.IncrementalDynamicBroadphase = incrementalDynamicBroadphase.Value;
                    sb.AppendLine($"  IncrementalDynamicBroadphase -> {step.IncrementalDynamicBroadphase}");
                }
                if (incrementalStaticBroadphase.HasValue)
                {
                    step.IncrementalStaticBroadphase = incrementalStaticBroadphase.Value;
                    sb.AppendLine($"  IncrementalStaticBroadphase -> {step.IncrementalStaticBroadphase}");
                }
                if (!float.IsNaN(maxDynamicDepenetrationVelocity))
                {
                    step.MaxDynamicDepenetrationVelocity = maxDynamicDepenetrationVelocity;
                    sb.AppendLine($"  MaxDynamicDepenetrationVel -> {step.MaxDynamicDepenetrationVelocity:F3}");
                }
                if (!float.IsNaN(maxStaticDepenetrationVelocity))
                {
                    step.MaxStaticDepenetrationVelocity = maxStaticDepenetrationVelocity;
                    sb.AppendLine($"  MaxStaticDepenetrationVel -> {step.MaxStaticDepenetrationVelocity:F3}");
                }

                EditorUtility.SetDirty(step);
                return sb.ToString();
            });
        }
    }
}
