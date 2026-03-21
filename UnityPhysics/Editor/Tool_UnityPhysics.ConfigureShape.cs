#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.UnityPhysics.Editor
{
    public partial class Tool_UnityPhysics
    {
        [McpPluginTool("uphys-configure-shape", Title = "Unity Physics / Configure Collider")]
        [Description(@"Configures collider components on a GameObject for Unity.Physics baking.
Works with standard Unity colliders (BoxCollider, SphereCollider, CapsuleCollider, MeshCollider)
which Unity.Physics bakes into ECS PhysicsCollider at bake time.
Can set: isTrigger, physics material properties (friction, restitution, combine modes).
To add a collider, specify shapeType. To configure an existing collider at a specific index, use colliderIndex.
Only provided parameters are applied; omitted parameters keep their current values.")]
        public string ConfigureShape(
            [Description("Name of the GameObject to configure.")]
            string gameObjectName,
            [Description("Collider index to configure if multiple exist (0-based). Default 0.")] int colliderIndex = 0,
            [Description("Add a new collider of this type: Box, Sphere, Capsule, Mesh. Leave empty to configure existing.")] string shapeType = "",
            [Description("Set collider as trigger.")] bool? isTrigger = null,
            [Description("Static friction coefficient (0-1+). Creates/assigns a PhysicsMaterial if needed.")] float staticFriction = float.NaN,
            [Description("Dynamic friction coefficient (0-1+). Creates/assigns a PhysicsMaterial if needed.")] float dynamicFriction = float.NaN,
            [Description("Bounciness / restitution (0-1). Creates/assigns a PhysicsMaterial if needed.")] float bounciness = float.NaN,
            [Description("Friction combine mode: 0=Average, 1=Minimum, 2=Multiply, 3=Maximum.")] int frictionCombine = -1,
            [Description("Bounce combine mode: 0=Average, 1=Minimum, 2=Multiply, 3=Maximum.")] int bounceCombine = -1,
            [Description("Box/Capsule/Sphere center X.")] float centerX = float.NaN,
            [Description("Box/Capsule/Sphere center Y.")] float centerY = float.NaN,
            [Description("Box/Capsule/Sphere center Z.")] float centerZ = float.NaN,
            [Description("Box size X.")] float sizeX = float.NaN,
            [Description("Box size Y.")] float sizeY = float.NaN,
            [Description("Box size Z.")] float sizeZ = float.NaN,
            [Description("Sphere/Capsule radius.")] float radius = float.NaN,
            [Description("Capsule height.")] float height = float.NaN,
            [Description("Capsule direction axis: 0=X, 1=Y, 2=Z.")] int direction = -1,
            [Description("MeshCollider convex flag.")] bool? convex = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = GameObject.Find(gameObjectName) ?? throw new System.Exception($"GameObject '{gameObjectName}' not found.");
                StringBuilder sb = new StringBuilder();
                Collider col;

                // Add new collider if shapeType specified
                if (!string.IsNullOrEmpty(shapeType))
                {
                    string lower = shapeType.ToLowerInvariant();
                    if (lower == "box")
                        col = Undo.AddComponent<BoxCollider>(go);
                    else if (lower == "sphere")
                        col = Undo.AddComponent<SphereCollider>(go);
                    else if (lower == "capsule")
                        col = Undo.AddComponent<CapsuleCollider>(go);
                    else if (lower == "mesh")
                        col = Undo.AddComponent<MeshCollider>(go);
                    else
                        throw new System.Exception($"Unknown shapeType '{shapeType}'. Use: Box, Sphere, Capsule, Mesh.");

                    sb.AppendLine($"  Added {col.GetType().Name}");
                }
                else
                {
                    // Get existing collider at index
                    Collider[] colliders = go.GetComponents<Collider>();
                    if (colliders.Length == 0)
                        throw new System.Exception($"'{gameObjectName}' has no Collider components. Use shapeType to add one.");
                    if (colliderIndex < 0 || colliderIndex >= colliders.Length)
                        throw new System.Exception($"colliderIndex {colliderIndex} out of range. '{gameObjectName}' has {colliders.Length} collider(s).");
                    col = colliders[colliderIndex];
                }

                sb.Insert(0, $"=== Collider configured: {go.name} [{col.GetType().Name}] ===\n");

                Undo.RecordObject(col, "MCP Configure Collider");

                if (isTrigger.HasValue)
                {
                    col.isTrigger = isTrigger.Value;
                    sb.AppendLine($"  IsTrigger -> {col.isTrigger}");
                }

                // Physics material configuration
                bool needsMaterial = !float.IsNaN(staticFriction) || !float.IsNaN(dynamicFriction) ||
                                     !float.IsNaN(bounciness) || frictionCombine >= 0 || bounceCombine >= 0;

                if (needsMaterial)
                {
                    PhysicsMaterial mat = col.sharedMaterial;
                    if (mat == null)
                    {
                        mat = new PhysicsMaterial($"{go.name}_PhysMat");
                        col.sharedMaterial = mat;
                        sb.AppendLine($"  Created PhysicsMaterial: {mat.name}");
                    }
                    else
                    {
                        // Clone to avoid modifying shared asset
                        PhysicsMaterial clone = new PhysicsMaterial(mat.name + "_Copy");
                        clone.staticFriction = mat.staticFriction;
                        clone.dynamicFriction = mat.dynamicFriction;
                        clone.bounciness = mat.bounciness;
                        clone.frictionCombine = mat.frictionCombine;
                        clone.bounceCombine = mat.bounceCombine;
                        mat = clone;
                        col.sharedMaterial = mat;
                        sb.AppendLine($"  Cloned PhysicsMaterial: {mat.name}");
                    }

                    if (!float.IsNaN(staticFriction))
                    {
                        mat.staticFriction = staticFriction;
                        sb.AppendLine($"  StaticFriction -> {mat.staticFriction:F3}");
                    }
                    if (!float.IsNaN(dynamicFriction))
                    {
                        mat.dynamicFriction = dynamicFriction;
                        sb.AppendLine($"  DynamicFriction -> {mat.dynamicFriction:F3}");
                    }
                    if (!float.IsNaN(bounciness))
                    {
                        mat.bounciness = bounciness;
                        sb.AppendLine($"  Bounciness -> {mat.bounciness:F3}");
                    }
                    if (frictionCombine >= 0 && frictionCombine <= 3)
                    {
                        mat.frictionCombine = (PhysicsMaterialCombine)frictionCombine;
                        sb.AppendLine($"  FrictionCombine -> {mat.frictionCombine}");
                    }
                    if (bounceCombine >= 0 && bounceCombine <= 3)
                    {
                        mat.bounceCombine = (PhysicsMaterialCombine)bounceCombine;
                        sb.AppendLine($"  BounceCombine -> {mat.bounceCombine}");
                    }
                }

                // Shape-specific configuration
                BoxCollider box = col as BoxCollider;
                if (box != null)
                {
                    Vector3 center = box.center;
                    if (!float.IsNaN(centerX)) center.x = centerX;
                    if (!float.IsNaN(centerY)) center.y = centerY;
                    if (!float.IsNaN(centerZ)) center.z = centerZ;
                    if (!float.IsNaN(centerX) || !float.IsNaN(centerY) || !float.IsNaN(centerZ))
                    {
                        box.center = center;
                        sb.AppendLine($"  Center -> {box.center}");
                    }

                    Vector3 size = box.size;
                    if (!float.IsNaN(sizeX)) size.x = sizeX;
                    if (!float.IsNaN(sizeY)) size.y = sizeY;
                    if (!float.IsNaN(sizeZ)) size.z = sizeZ;
                    if (!float.IsNaN(sizeX) || !float.IsNaN(sizeY) || !float.IsNaN(sizeZ))
                    {
                        box.size = size;
                        sb.AppendLine($"  Size -> {box.size}");
                    }
                }

                SphereCollider sphere = col as SphereCollider;
                if (sphere != null)
                {
                    Vector3 center = sphere.center;
                    if (!float.IsNaN(centerX)) center.x = centerX;
                    if (!float.IsNaN(centerY)) center.y = centerY;
                    if (!float.IsNaN(centerZ)) center.z = centerZ;
                    if (!float.IsNaN(centerX) || !float.IsNaN(centerY) || !float.IsNaN(centerZ))
                    {
                        sphere.center = center;
                        sb.AppendLine($"  Center -> {sphere.center}");
                    }
                    if (!float.IsNaN(radius))
                    {
                        sphere.radius = radius;
                        sb.AppendLine($"  Radius -> {sphere.radius:F3}");
                    }
                }

                CapsuleCollider capsule = col as CapsuleCollider;
                if (capsule != null)
                {
                    Vector3 center = capsule.center;
                    if (!float.IsNaN(centerX)) center.x = centerX;
                    if (!float.IsNaN(centerY)) center.y = centerY;
                    if (!float.IsNaN(centerZ)) center.z = centerZ;
                    if (!float.IsNaN(centerX) || !float.IsNaN(centerY) || !float.IsNaN(centerZ))
                    {
                        capsule.center = center;
                        sb.AppendLine($"  Center -> {capsule.center}");
                    }
                    if (!float.IsNaN(radius))
                    {
                        capsule.radius = radius;
                        sb.AppendLine($"  Radius -> {capsule.radius:F3}");
                    }
                    if (!float.IsNaN(height))
                    {
                        capsule.height = height;
                        sb.AppendLine($"  Height -> {capsule.height:F3}");
                    }
                    if (direction >= 0 && direction <= 2)
                    {
                        capsule.direction = direction;
                        sb.AppendLine($"  Direction -> {capsule.direction}");
                    }
                }

                MeshCollider mesh = col as MeshCollider;
                if (mesh != null)
                {
                    if (convex.HasValue)
                    {
                        mesh.convex = convex.Value;
                        sb.AppendLine($"  Convex -> {mesh.convex}");
                    }
                }

                EditorUtility.SetDirty(col);
                return sb.ToString();
            });
        }
    }
}
