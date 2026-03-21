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
        [McpPluginTool("uphys-configure-body", Title = "Unity Physics / Configure Rigidbody")]
        [Description(@"Adds (if missing) and configures a Rigidbody component on a GameObject.
The Rigidbody is the standard Unity component that Unity.Physics bakes into ECS PhysicsMass,
PhysicsVelocity, PhysicsDamping, and PhysicsGravityFactor at bake time. Only provided
parameters are applied; omitted parameters keep their current values.")]
        public string ConfigureBody(
            [Description("Name of the GameObject to configure.")]
            string gameObjectName,
            [Description("Mass in kg. Min 0.0001.")] float mass = -1f,
            [Description("Linear drag (baked as PhysicsDamping.Linear).")] float drag = -1f,
            [Description("Angular drag (baked as PhysicsDamping.Angular).")] float angularDrag = -1f,
            [Description("Whether gravity applies to this body.")] bool? useGravity = null,
            [Description("If true, body is kinematic (infinite mass, not affected by forces).")] bool? isKinematic = null,
            [Description("Interpolation mode: 0=None, 1=Interpolate, 2=Extrapolate.")] int interpolation = -1,
            [Description("Collision detection mode: 0=Discrete, 1=Continuous, 2=ContinuousDynamic, 3=ContinuousSpeculative.")] int collisionDetection = -1,
            [Description("Rigidbody constraints bitmask (0=None, 2=FreezePositionX, 4=FreezePositionY, 8=FreezePositionZ, 16=FreezeRotationX, 32=FreezeRotationY, 64=FreezeRotationZ, 126=FreezeAll).")] int constraints = -1,
            [Description("Max linear velocity cap.")] float maxLinearVelocity = -1f,
            [Description("Max angular velocity cap.")] float maxAngularVelocity = -1f,
            [Description("If true, auto-compute center of mass from colliders.")] bool? automaticCenterOfMass = null,
            [Description("If true, auto-compute inertia tensor from colliders.")] bool? automaticInertiaTensor = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = GameObject.Find(gameObjectName) ?? throw new System.Exception($"GameObject '{gameObjectName}' not found.");
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb == null) rb = go.AddComponent<Rigidbody>();
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"=== Rigidbody configured: {go.name} ===");

                Undo.RecordObject(rb, "MCP Configure Rigidbody");

                if (mass >= 0f)
                {
                    rb.mass = Mathf.Max(0.0001f, mass);
                    sb.AppendLine($"  Mass -> {rb.mass:F3}");
                }
                if (drag >= 0f)
                {
                    rb.linearDamping = drag;
                    sb.AppendLine($"  Drag -> {rb.linearDamping:F3}");
                }
                if (angularDrag >= 0f)
                {
                    rb.angularDamping = angularDrag;
                    sb.AppendLine($"  AngularDrag -> {rb.angularDamping:F3}");
                }
                if (useGravity.HasValue)
                {
                    rb.useGravity = useGravity.Value;
                    sb.AppendLine($"  UseGravity -> {rb.useGravity}");
                }
                if (isKinematic.HasValue)
                {
                    rb.isKinematic = isKinematic.Value;
                    sb.AppendLine($"  IsKinematic -> {rb.isKinematic}");
                }
                if (interpolation >= 0 && interpolation <= 2)
                {
                    rb.interpolation = (RigidbodyInterpolation)interpolation;
                    sb.AppendLine($"  Interpolation -> {rb.interpolation}");
                }
                if (collisionDetection >= 0 && collisionDetection <= 3)
                {
                    rb.collisionDetectionMode = (CollisionDetectionMode)collisionDetection;
                    sb.AppendLine($"  CollisionDetection -> {rb.collisionDetectionMode}");
                }
                if (constraints >= 0)
                {
                    rb.constraints = (RigidbodyConstraints)constraints;
                    sb.AppendLine($"  Constraints -> {rb.constraints}");
                }
                if (maxLinearVelocity >= 0f)
                {
                    rb.maxLinearVelocity = maxLinearVelocity;
                    sb.AppendLine($"  MaxLinearVelocity -> {rb.maxLinearVelocity:F3}");
                }
                if (maxAngularVelocity >= 0f)
                {
                    rb.maxAngularVelocity = maxAngularVelocity;
                    sb.AppendLine($"  MaxAngularVelocity -> {rb.maxAngularVelocity:F3}");
                }
                if (automaticCenterOfMass.HasValue)
                {
                    rb.automaticCenterOfMass = automaticCenterOfMass.Value;
                    sb.AppendLine($"  AutoCenterOfMass -> {rb.automaticCenterOfMass}");
                }
                if (automaticInertiaTensor.HasValue)
                {
                    rb.automaticInertiaTensor = automaticInertiaTensor.Value;
                    sb.AppendLine($"  AutoInertiaTensor -> {rb.automaticInertiaTensor}");
                }

                EditorUtility.SetDirty(rb);
                return sb.ToString();
            });
        }
    }
}
