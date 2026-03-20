#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Unity.Physics.Authoring;
using UnityEngine;

namespace MCPTools.UnityPhysics.Editor
{
    public partial class Tool_UnityPhysics
    {
        [McpPluginTool("uphys-query", Title = "Unity Physics / Query Authoring")]
        [Description(@"Reads all Unity Physics authoring components on a GameObject.
Reports: Rigidbody (mass, drag, angularDrag, useGravity, isKinematic, interpolation, constraints),
Collider (type, isTrigger, material, bounds), and PhysicsStepAuthoring (gravity, solver iterations,
substep count, multi-threaded, collision tolerance). Works with the standard Unity Physics hybrid
workflow that bakes from built-in Rigidbody and Collider components.")]
        public string QueryAuthoring(
            [Description("Name of the GameObject to query for physics authoring components.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                StringBuilder sb = new StringBuilder();
                bool found = false;

                // --- Rigidbody (standard Unity component, baked by Unity.Physics.Hybrid) ---
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    found = true;
                    sb.AppendLine($"=== Rigidbody: {go.name} ===");
                    sb.AppendLine($"  Mass:             {rb.mass:F3}");
                    sb.AppendLine($"  Drag:             {rb.linearDamping:F3}");
                    sb.AppendLine($"  AngularDrag:      {rb.angularDamping:F3}");
                    sb.AppendLine($"  UseGravity:       {rb.useGravity}");
                    sb.AppendLine($"  IsKinematic:      {rb.isKinematic}");
                    sb.AppendLine($"  Interpolation:    {rb.interpolation}");
                    sb.AppendLine($"  CollisionDetect:  {rb.collisionDetectionMode}");
                    sb.AppendLine($"  Constraints:      {rb.constraints}");
                    sb.AppendLine($"  AutoCenterOfMass: {rb.automaticCenterOfMass}");
                    sb.AppendLine($"  CenterOfMass:     {FormatV3(rb.centerOfMass)}");
                    sb.AppendLine($"  AutoInertiaTensor:{rb.automaticInertiaTensor}");
                    sb.AppendLine($"  InertiaTensor:    {FormatV3(rb.inertiaTensor)}");
                    sb.AppendLine($"  MaxLinearVelocity:{rb.maxLinearVelocity:F3}");
                    sb.AppendLine($"  MaxAngularVelocity:{rb.maxAngularVelocity:F3}");
                }

                // --- Colliders (standard Unity components, baked by Unity.Physics.Hybrid) ---
                Collider[] colliders = go.GetComponents<Collider>();
                if (colliders.Length > 0)
                {
                    found = true;
                    sb.AppendLine($"\n=== Colliders: {go.name} ({colliders.Length} found) ===");
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        Collider col = colliders[i];
                        sb.AppendLine($"\n  -- Collider [{i}]: {col.GetType().Name} --");
                        sb.AppendLine($"    Enabled:    {col.enabled}");
                        sb.AppendLine($"    IsTrigger:  {col.isTrigger}");
                        sb.AppendLine($"    Bounds:     center={FormatV3(col.bounds.center)}, size={FormatV3(col.bounds.size)}");

                        PhysicsMaterial mat = col.sharedMaterial;
                        if (mat != null)
                        {
                            sb.AppendLine($"    Material:   {mat.name}");
                            sb.AppendLine($"      StaticFriction:  {mat.staticFriction:F3}");
                            sb.AppendLine($"      DynamicFriction: {mat.dynamicFriction:F3}");
                            sb.AppendLine($"      Bounciness:      {mat.bounciness:F3}");
                            sb.AppendLine($"      FrictionCombine: {mat.frictionCombine}");
                            sb.AppendLine($"      BounceCombine:   {mat.bounceCombine}");
                        }
                        else
                        {
                            sb.AppendLine($"    Material:   (none / default)");
                        }

                        BoxCollider box = col as BoxCollider;
                        if (box != null)
                        {
                            sb.AppendLine($"    Center: {FormatV3(box.center)}");
                            sb.AppendLine($"    Size:   {FormatV3(box.size)}");
                        }

                        SphereCollider sphere = col as SphereCollider;
                        if (sphere != null)
                        {
                            sb.AppendLine($"    Center: {FormatV3(sphere.center)}");
                            sb.AppendLine($"    Radius: {sphere.radius:F3}");
                        }

                        CapsuleCollider capsule = col as CapsuleCollider;
                        if (capsule != null)
                        {
                            sb.AppendLine($"    Center:    {FormatV3(capsule.center)}");
                            sb.AppendLine($"    Radius:    {capsule.radius:F3}");
                            sb.AppendLine($"    Height:    {capsule.height:F3}");
                            sb.AppendLine($"    Direction: {capsule.direction}");
                        }

                        MeshCollider mesh = col as MeshCollider;
                        if (mesh != null)
                        {
                            sb.AppendLine($"    Convex:     {mesh.convex}");
                            sb.AppendLine($"    SharedMesh: {(mesh.sharedMesh != null ? mesh.sharedMesh.name : "none")}");
                            sb.AppendLine($"    CookingOptions: {mesh.cookingOptions}");
                        }
                    }
                }

                // --- PhysicsStepAuthoring (Unity.Physics.Hybrid) ---
                PhysicsStepAuthoring step = go.GetComponent<PhysicsStepAuthoring>();
                if (step != null)
                {
                    found = true;
                    sb.AppendLine($"\n=== PhysicsStepAuthoring: {go.name} ===");
                    sb.AppendLine($"  SimulationType:   {step.SimulationType}");
                    sb.AppendLine($"  Gravity:          {FormatFloat3(step.Gravity)}");
                    sb.AppendLine($"  SubstepCount:     {step.SubstepCount}");
                    sb.AppendLine($"  SolverIterations: {step.SolverIterationCount}");
                    sb.AppendLine($"  MultiThreaded:    {step.MultiThreaded}");
                    sb.AppendLine($"  CollisionTolerance:          {step.CollisionTolerance:F4}");
                    sb.AppendLine($"  SynchronizeCollisionWorld:   {step.SynchronizeCollisionWorld}");
                    sb.AppendLine($"  EnableSolverStabilization:   {step.EnableSolverStabilizationHeuristic}");
                    sb.AppendLine($"  IncrementalDynamicBroadphase:{step.IncrementalDynamicBroadphase}");
                    sb.AppendLine($"  IncrementalStaticBroadphase: {step.IncrementalStaticBroadphase}");
                    sb.AppendLine($"  MaxDynamicDepenetrationVel:  {step.MaxDynamicDepenetrationVelocity:F3}");
                    sb.AppendLine($"  MaxStaticDepenetrationVel:   {step.MaxStaticDepenetrationVelocity:F3}");
                }

                if (!found)
                    throw new System.Exception($"'{gameObjectName}' has no Rigidbody, Collider, or PhysicsStepAuthoring components.");

                return sb.ToString();
            });
        }
    }
}
