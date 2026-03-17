#nullable enable
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using RayFire;
using UnityEngine;

namespace MCPTools.RayFire.Editor
{
    public partial class Tool_RayFire
    {
        [McpPluginTool("rayfire-list-rigid", Title = "RayFire / List Rigid Objects")]
        [Description("Lists all RayfireRigid components in the current scene with their configuration summary.")]
        public ListRigidResponse ListRigid()
        {
            return MainThread.Instance.Run(() =>
            {
                var rigids = Object.FindObjectsByType<RayfireRigid>(FindObjectsSortMode.None);
                var lines = rigids.Select(r =>
                    $"[{r.gameObject.GetInstanceID()}] {r.gameObject.name} " +
                    $"(Sim:{r.simTp}, Dml:{r.dmlTp}, Obj:{r.objTp}, " +
                    $"Mat:{r.physics.mt}, Dmg:{(r.damage.en ? $"{r.damage.cur}/{r.damage.max}" : "off")})"
                ).ToArray();

                return new ListRigidResponse
                {
                    count = rigids.Length,
                    details = string.Join("\n", lines)
                };
            });
        }

        [McpPluginTool("rayfire-demolish", Title = "RayFire / Demolish Object")]
        [Description(@"Forces immediate demolition of a RayfireRigid object at runtime or in play mode.
The object must have a RayfireRigid component with demolition enabled.")]
        public DemolishResponse Demolish(
            [Description("Reference to the GameObject with RayfireRigid.")]
            GameObjectRef targetRef
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

                rigid.DemolishForced();

                int fragCount = rigid.HasFragments ? rigid.fragments.Count : 0;

                return new DemolishResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    fragmentCount = fragCount,
                    demolished = true
                };
            });
        }

        [McpPluginTool("rayfire-apply-damage", Title = "RayFire / Apply Damage")]
        [Description(@"Applies damage to a RayfireRigid object. If accumulated damage exceeds max, the object is demolished.
Damage must be enabled on the component.")]
        public ApplyDamageResponse ApplyDamage(
            [Description("Reference to the target GameObject with RayfireRigid.")]
            GameObjectRef targetRef,
            [Description("Amount of damage to apply.")]
            float damageAmount,
            [Description("World position of the damage point. Default is object center.")]
            Vector3? damagePoint = null,
            [Description("Damage radius (0 for point damage). Default 0.")]
            float damageRadius = 0f
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

                if (!rigid.damage.en)
                    throw new System.Exception($"Damage is not enabled on '{go.name}'.");

                var point = damagePoint ?? go.transform.position;
                bool demolished = rigid.ApplyDamage(damageAmount, point, damageRadius);

                return new ApplyDamageResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    damageApplied = damageAmount,
                    currentDamage = rigid.damage.cur,
                    maxDamage = rigid.damage.max,
                    demolished = demolished
                };
            });
        }

        public class ListRigidResponse
        {
            [Description("Number of RayfireRigid objects found")] public int count;
            [Description("Details of each rigid object")] public string details = "";
        }

        public class DemolishResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Number of fragments created")] public int fragmentCount;
            [Description("Whether demolition occurred")] public bool demolished;
        }

        public class ApplyDamageResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Damage applied")] public float damageApplied;
            [Description("Current accumulated damage")] public float currentDamage;
            [Description("Maximum damage threshold")] public float maxDamage;
            [Description("Whether the object was demolished")] public bool demolished;
        }
    }
}
