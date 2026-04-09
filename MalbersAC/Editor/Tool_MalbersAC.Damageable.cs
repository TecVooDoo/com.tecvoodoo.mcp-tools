#if HAS_MALBERS_AC
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using com.IvanMurzak.Unity.MCP.Runtime.Data;
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using MalbersAnimations;
using UnityEditor;
using UnityEngine;

namespace MCPTools.MalbersAC.Editor
{
    public partial class Tool_MalbersAC
    {
        [McpPluginTool("ac-configure-damageable", Title = "Malbers AC / Configure Damageable")]
        [Description(@"Configures an MDamageable component on a GameObject.
MDamageable handles how the animal receives damage, including multipliers and alignment.
Only provided parameters are changed; others are left as-is.")]
        public ConfigureDamageableResponse ConfigureDamageable(
            [Description("Reference to the GameObject with MDamageable component.")]
            GameObjectRef targetRef,
            [Description("Damage multiplier (1.0 = normal, 2.0 = double damage). Null to keep current.")]
            float? multiplier = null,
            [Description("Auto-rotate to face damage source. Null to keep current.")]
            bool? alignToDamage = null,
            [Description("Alignment time in seconds. Null to keep current.")]
            float? alignTime = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var damageable = go.GetComponent<MDamageable>();
                if (damageable == null)
                    throw new System.Exception($"'{go.name}' has no MDamageable component.");

                if (multiplier.HasValue) damageable.multiplier.Value = multiplier.Value;
                if (alignToDamage.HasValue) damageable.AlignToDamage.Value = alignToDamage.Value;
                if (alignTime.HasValue) damageable.AlignTime.Value = alignTime.Value;

                EditorUtility.SetDirty(go);

                return new ConfigureDamageableResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    multiplier = damageable.multiplier.Value,
                    alignToDamage = damageable.AlignToDamage.Value,
                    alignTime = damageable.AlignTime.Value
                };
            });
        }

        public class ConfigureDamageableResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Damage multiplier")] public float multiplier;
            [Description("Align to damage source")] public bool alignToDamage;
            [Description("Alignment time in seconds")] public float alignTime;
        }
    }
}
#endif
