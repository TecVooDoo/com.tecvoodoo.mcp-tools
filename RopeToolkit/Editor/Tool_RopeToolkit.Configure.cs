#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
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

                // simulation is a struct -- read-modify-write
                var sim = GetStruct(rope, "simulation")
                          ?? throw new Exception("Could not read rope.simulation.");

                if (stiffness.HasValue)    SetStructField(sim, "stiffness", Mathf.Clamp(stiffness.Value, 0.01f, 1.0f));
                if (energyLoss.HasValue)   SetStructField(sim, "energyLoss", Mathf.Clamp01(energyLoss.Value));
                if (gravityMultiplier.HasValue) SetStructField(sim, "gravityMultiplier", Mathf.Clamp01(gravityMultiplier.Value));
                if (substeps.HasValue)     SetStructField(sim, "substeps", Mathf.Clamp(substeps.Value, 1, 10));
                if (solverIterations.HasValue) SetStructField(sim, "solverIterations", Mathf.Clamp(solverIterations.Value, 1, 32));
                if (lengthMultiplier.HasValue) SetStructField(sim, "lengthMultiplier", Mathf.Clamp(lengthMultiplier.Value, 0f, 2.0f));
                if (resolution.HasValue)   SetStructField(sim, "resolution", Mathf.Max(0.1f, resolution.Value));
                if (enabled.HasValue)      SetStructField(sim, "enabled", enabled.Value);

                SetStruct(rope, "simulation", sim);
                EditorUtility.SetDirty(rope);

                var stiffVal = GetStructField(sim, "stiffness");
                var elVal    = GetStructField(sim, "energyLoss");
                var subVal   = GetStructField(sim, "substeps");
                var siVal    = GetStructField(sim, "solverIterations");

                return $"OK: Rope '{gameObjectName}' simulation updated. stiffness={stiffVal:F3} energyLoss={elVal:F3} substeps={subVal} solverIterations={siVal}";
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

                // collisions is a struct -- read-modify-write
                var col = GetStruct(rope, "collisions")
                          ?? throw new Exception("Could not read rope.collisions.");

                if (enabled.HasValue)            SetStructField(col, "enabled", enabled.Value);
                if (influenceRigidbodies.HasValue) SetStructField(col, "influenceRigidbodies", influenceRigidbodies.Value);
                if (friction.HasValue)           SetStructField(col, "friction", Mathf.Clamp(friction.Value, 0f, 20f));
                if (stride.HasValue)             SetStructField(col, "stride", Mathf.Clamp(stride.Value, 1, 20));
                if (collisionMargin.HasValue)    SetStructField(col, "collisionMargin", Mathf.Clamp01(collisionMargin.Value));

                SetStruct(rope, "collisions", col);
                EditorUtility.SetDirty(rope);

                var enVal  = GetStructField(col, "enabled");
                var frVal  = GetStructField(col, "friction");
                var stVal  = GetStructField(col, "stride");

                return $"OK: Rope '{gameObjectName}' collision updated. enabled={enVal} friction={frVal:F3} stride={stVal}";
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

                if (radius.HasValue)         Set(rope, "radius", Mathf.Clamp(radius.Value, 0.001f, 1.0f));
                if (radialVertices.HasValue)  Set(rope, "radialVertices", Mathf.Clamp(radialVertices.Value, 3, 32));
                if (isLoop.HasValue)          Set(rope, "isLoop", isLoop.Value);

                EditorUtility.SetDirty(rope);

                var r  = Get(rope, "radius");
                var rv = Get(rope, "radialVertices");
                var il = Get(rope, "isLoop");

                return $"OK: Rope '{gameObjectName}' appearance updated. radius={r:F4} radialVertices={rv} isLoop={il}";
            });
        }
    }
}
