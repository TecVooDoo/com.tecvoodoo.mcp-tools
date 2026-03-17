#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using RayFire;
using UnityEditor;
using UnityEngine;

namespace MCPTools.RayFire.Editor
{
    public partial class Tool_RayFire
    {
        [McpPluginTool("rayfire-add-rigid", Title = "RayFire / Add Rigid")]
        [Description(@"Adds a RayfireRigid component to a GameObject, making it destructible.
Configure simulation type, demolition type, physics material, and damage settings.
Use 'rayfire-add-shatter' afterward to configure fragmentation.")]
        public AddRigidResponse AddRigid(
            [Description("Reference to the target GameObject.")]
            GameObjectRef targetRef,
            [Description("Simulation type: 'Dynamic', 'Sleeping', 'Inactive', 'Kinematic', 'Static'. Default 'Dynamic'.")]
            string simulationType = "Dynamic",
            [Description("Demolition type: 'None', 'Runtime', 'AwakePrecache', 'AwakePrefragment'. Default 'Runtime'.")]
            string demolitionType = "Runtime",
            [Description("Object type: 'Mesh', 'MeshRoot', 'ConnectedCluster', 'NestedCluster', 'SkinnedMesh'. Default 'Mesh'.")]
            string objectType = "Mesh",
            [Description("Physics material: 'HeavyMetal', 'LightMetal', 'DenseRock', 'PorousRock', 'Concrete', 'Brick', 'Glass', 'Rubber', 'Ice', 'Wood'. Default 'Concrete'.")]
            string physicsMaterial = "Concrete",
            [Description("Mass value. Default 1.")]
            float mass = 1f,
            [Description("Enable damage tracking. Default true.")]
            bool enableDamage = true,
            [Description("Maximum damage before destruction. Default 100.")]
            float maxDamage = 100f,
            [Description("Enable collision-based damage. Default true.")]
            bool collisionDamage = true,
            [Description("Use gravity. Default true.")]
            bool useGravity = true,
            [Description("Maximum demolition depth. Default 1.")]
            int maxDepth = 1,
            [Description("Fading type for fragments: 'None', 'Destroy', 'MoveDown', 'ScaleDown', 'FallDown'. Default 'None'.")]
            string fadeType = "None",
            [Description("Fragment fade lifetime in seconds. Default 5.")]
            float fadeLifetime = 5f,
            [Description("Initialize component immediately. Default true.")]
            bool initialize = true
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var rigid = go.GetComponent<RayfireRigid>();
                if (rigid == null)
                {
                    rigid = Undo.AddComponent<RayfireRigid>(go);
                }

                rigid.simTp = ParseEnum<SimType>(simulationType, SimType.Dynamic);
                rigid.dmlTp = ParseEnum<DemolitionType>(demolitionType, DemolitionType.Runtime);
                rigid.objTp = ParseEnum<ObjectType>(objectType, ObjectType.Mesh);

                // Physics
                rigid.physics.mt = ParseEnum<MaterialType>(physicsMaterial, MaterialType.Concrete);
                rigid.physics.ms = mass;
                rigid.physics.gr = useGravity;

                // Damage
                rigid.damage.en = enableDamage;
                rigid.damage.max = maxDamage;
                rigid.damage.col = collisionDamage;

                // Limitations
                rigid.lim.depth = maxDepth;

                // Fading
                rigid.fading.fadeType = ParseEnum<FadeType>(fadeType, FadeType.None);
                rigid.fading.lifeTime = fadeLifetime;

                if (initialize)
                    rigid.Initialize();

                EditorUtility.SetDirty(go);

                return new AddRigidResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    simulationType = rigid.simTp.ToString(),
                    demolitionType = rigid.dmlTp.ToString(),
                    physicsMaterial = rigid.physics.mt.ToString(),
                    damageEnabled = rigid.damage.en,
                    maxDamage = rigid.damage.max
                };
            });
        }

        [McpPluginTool("rayfire-configure-rigid", Title = "RayFire / Configure Rigid")]
        [Description(@"Modifies properties on an existing RayfireRigid component.
Only provided parameters are changed; others are left as-is.")]
        public ConfigureRigidResponse ConfigureRigid(
            [Description("Reference to the target GameObject with RayfireRigid.")]
            GameObjectRef targetRef,
            [Description("Simulation type. Null to keep current.")]
            string? simulationType = null,
            [Description("Demolition type. Null to keep current.")]
            string? demolitionType = null,
            [Description("Physics material type. Null to keep current.")]
            string? physicsMaterial = null,
            [Description("Mass value. Null to keep current.")]
            float? mass = null,
            [Description("Enable/disable damage. Null to keep current.")]
            bool? enableDamage = null,
            [Description("Max damage. Null to keep current.")]
            float? maxDamage = null,
            [Description("Use gravity. Null to keep current.")]
            bool? useGravity = null,
            [Description("Max demolition depth. Null to keep current.")]
            int? maxDepth = null,
            [Description("Fade type. Null to keep current.")]
            string? fadeType = null,
            [Description("Fade lifetime. Null to keep current.")]
            float? fadeLifetime = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var rigid = go.GetComponent<RayfireRigid>();
                if (rigid == null)
                    throw new System.Exception($"'{go.name}' has no RayfireRigid component.");

                if (simulationType != null) rigid.simTp = ParseEnum<SimType>(simulationType, rigid.simTp);
                if (demolitionType != null) rigid.dmlTp = ParseEnum<DemolitionType>(demolitionType, rigid.dmlTp);
                if (physicsMaterial != null) rigid.physics.mt = ParseEnum<MaterialType>(physicsMaterial, rigid.physics.mt);
                if (mass.HasValue) rigid.physics.ms = mass.Value;
                if (enableDamage.HasValue) rigid.damage.en = enableDamage.Value;
                if (maxDamage.HasValue) rigid.damage.max = maxDamage.Value;
                if (useGravity.HasValue) rigid.physics.gr = useGravity.Value;
                if (maxDepth.HasValue) rigid.lim.depth = maxDepth.Value;
                if (fadeType != null) rigid.fading.fadeType = ParseEnum<FadeType>(fadeType, rigid.fading.fadeType);
                if (fadeLifetime.HasValue) rigid.fading.lifeTime = fadeLifetime.Value;

                EditorUtility.SetDirty(go);

                return new ConfigureRigidResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    simulationType = rigid.simTp.ToString(),
                    demolitionType = rigid.dmlTp.ToString(),
                    physicsMaterial = rigid.physics.mt.ToString(),
                    mass = rigid.physics.ms,
                    damageEnabled = rigid.damage.en,
                    maxDamage = rigid.damage.max
                };
            });
        }

        static T ParseEnum<T>(string value, T defaultValue) where T : struct, System.Enum
        {
            if (System.Enum.TryParse<T>(value, true, out var result))
                return result;
            return defaultValue;
        }

        public class AddRigidResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Simulation type set")] public string simulationType = "";
            [Description("Demolition type set")] public string demolitionType = "";
            [Description("Physics material type")] public string physicsMaterial = "";
            [Description("Damage tracking enabled")] public bool damageEnabled;
            [Description("Maximum damage")] public float maxDamage;
        }

        public class ConfigureRigidResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Simulation type")] public string simulationType = "";
            [Description("Demolition type")] public string demolitionType = "";
            [Description("Physics material type")] public string physicsMaterial = "";
            [Description("Mass")] public float mass;
            [Description("Damage enabled")] public bool damageEnabled;
            [Description("Max damage")] public float maxDamage;
        }
    }
}
