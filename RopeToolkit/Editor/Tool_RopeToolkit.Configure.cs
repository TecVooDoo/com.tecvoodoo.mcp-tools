#if HAS_ROPE_TOOLKIT
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.RopeToolkit.Editor
{
    public partial class Tool_RopeToolkit
    {
        [McpPluginTool("rope-configure-simulation", Title = "Rope Toolkit / Configure Simulation")]
        [Description(@"Sets simulation parameters on a Rope component.
All parameters are optional -- only provided values are changed.
stiffness [0.01-1.0]: rope rigidity (higher = stiffer, less droopy).
energyLoss [0-1.0]: damping/air resistance (higher = settles faster).
gravityMultiplier [0-1.0]: gravity influence scale (0=weightless).
substeps [1-10]: physics substeps per frame (higher = more stable but slower).
solverIterations [1-32]: constraint solver passes (higher = less stretching).
lengthMultiplier [0-2.0]: dynamic extension/retraction of rope length.")]
        public string ConfigureSimulation(
            [Description("Name of the GameObject with the Rope component.")] string gameObjectName,
            [Description("Rope stiffness [0.01-1.0]. Higher = less droopy.")] float? stiffness = null,
            [Description("Energy loss / damping [0-1.0]. Higher = settles faster.")] float? energyLoss = null,
            [Description("Gravity influence scale [0-1.0]. 0 = weightless.")] float? gravityMultiplier = null,
            [Description("Physics substeps per frame [1-10].")] int? substeps = null,
            [Description("Constraint solver iterations [1-32].")] int? solverIterations = null,
            [Description("Rope length multiplier [0-2.0]. 1 = normal length.")] float? lengthMultiplier = null,
            [Description("Particles per meter. Higher = smoother, more expensive.")] float? resolution = null,
            [Description("Enable/disable simulation independently of rendering.")] bool? enabled = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var rope = GetRope(gameObjectName);
                var sim = rope.simulation;

                if (stiffness.HasValue) sim.stiffness = Mathf.Clamp(stiffness.Value, 0.01f, 1.0f);
                if (energyLoss.HasValue) sim.energyLoss = Mathf.Clamp01(energyLoss.Value);
                if (gravityMultiplier.HasValue) sim.gravityMultiplier = Mathf.Clamp01(gravityMultiplier.Value);
                if (substeps.HasValue) sim.substeps = Mathf.Clamp(substeps.Value, 1, 10);
                if (solverIterations.HasValue) sim.solverIterations = Mathf.Clamp(solverIterations.Value, 1, 32);
                if (lengthMultiplier.HasValue) sim.lengthMultiplier = Mathf.Clamp(lengthMultiplier.Value, 0f, 2.0f);
                if (resolution.HasValue) sim.resolution = Mathf.Max(0.1f, resolution.Value);
                if (enabled.HasValue) sim.enabled = enabled.Value;

                rope.simulation = sim;
                EditorUtility.SetDirty(rope);

                return $"OK: Rope '{gameObjectName}' simulation updated. stiffness={sim.stiffness:F3} energyLoss={sim.energyLoss:F3} substeps={sim.substeps} solverIterations={sim.solverIterations}";
            });
        }

        [McpPluginTool("rope-configure-collision", Title = "Rope Toolkit / Configure Collision")]
        [Description(@"Sets collision parameters on a Rope component.
enabled: toggles collision detection entirely.
influenceRigidbodies: whether rope particles push attached rigidbodies.
friction [0-20]: friction coefficient against surfaces.
stride [1-20]: check every Nth particle (higher = faster but less accurate).
collisionMargin [0-1.0]: extra buffer around rope particles for collision.")]
        public string ConfigureCollision(
            [Description("Name of the GameObject with the Rope component.")] string gameObjectName,
            [Description("Enable collision detection.")] bool? enabled = null,
            [Description("Whether rope can push rigidbodies.")] bool? influenceRigidbodies = null,
            [Description("Friction coefficient [0-20].")] float? friction = null,
            [Description("Check every Nth particle [1-20]. Higher = faster, less precise.")] int? stride = null,
            [Description("Extra collision margin [0-1.0].")] float? collisionMargin = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var rope = GetRope(gameObjectName);
                var col = rope.collisions;

                if (enabled.HasValue) col.enabled = enabled.Value;
                if (influenceRigidbodies.HasValue) col.influenceRigidbodies = influenceRigidbodies.Value;
                if (friction.HasValue) col.friction = Mathf.Clamp(friction.Value, 0f, 20f);
                if (stride.HasValue) col.stride = Mathf.Clamp(stride.Value, 1, 20);
                if (collisionMargin.HasValue) col.collisionMargin = Mathf.Clamp01(collisionMargin.Value);

                rope.collisions = col;
                EditorUtility.SetDirty(rope);

                return $"OK: Rope '{gameObjectName}' collision updated. enabled={col.enabled} friction={col.friction:F3} stride={col.stride}";
            });
        }

        [McpPluginTool("rope-configure-appearance", Title = "Rope Toolkit / Configure Appearance")]
        [Description(@"Sets visual properties on a Rope component.
radius [0.001-1.0]: visual rope thickness in world units.
radialVertices [3-32]: mesh segments around the rope circumference (higher = rounder, more expensive).
isLoop: connect end point back to start point.
materialName: name of a material already in the scene or project to assign (optional).")]
        public string ConfigureAppearance(
            [Description("Name of the GameObject with the Rope component.")] string gameObjectName,
            [Description("Rope visual radius in world units [0.001-1.0].")] float? radius = null,
            [Description("Radial mesh vertex count [3-32]. Higher = rounder.")] int? radialVertices = null,
            [Description("Whether rope forms a loop (end connects to start).")] bool? isLoop = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var rope = GetRope(gameObjectName);

                if (radius.HasValue) rope.radius = Mathf.Clamp(radius.Value, 0.001f, 1.0f);
                if (radialVertices.HasValue) rope.radialVertices = Mathf.Clamp(radialVertices.Value, 3, 32);
                if (isLoop.HasValue) rope.isLoop = isLoop.Value;

                EditorUtility.SetDirty(rope);

                return $"OK: Rope '{gameObjectName}' appearance updated. radius={rope.radius:F4} radialVertices={rope.radialVertices} isLoop={rope.isLoop}";
            });
        }
    }
}
#endif
