#if HAS_MALBERS_AC
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
#if MCP_HAS_AIGD
using AIGD;
#else
using com.IvanMurzak.Unity.MCP.Runtime.Data;
#endif
using com.IvanMurzak.Unity.MCP.Runtime.Extensions;
using MalbersAnimations;
using UnityEditor;
using UnityEngine;

namespace MCPTools.MalbersAC.Editor
{
    public partial class Tool_MalbersAC
    {
        [McpPluginTool("ac-add-lock-axis", Title = "Malbers AC / Add Lock Axis")]
        [Description(@"Adds or configures a LockAxis component on a GameObject for 2.5D gameplay.
LockAxis constrains the animal's movement to specific axes.
For 2.5D side-scrolling: lock Z axis (LockZ=true) so movement stays on the XY plane.
For 2.5D top-down: lock Y axis (LockY=true) so movement stays on the XZ plane.")]
        public LockAxisResponse AddLockAxis(
            [Description("Reference to the target GameObject (should have MAnimal).")]
            GameObjectRef targetRef,
            [Description("Lock X axis position. Default false.")]
            bool lockX = false,
            [Description("Lock Y axis position. Default false.")]
            bool lockY = false,
            [Description("Lock Z axis position. Default true (standard 2.5D side-scroller).")]
            bool lockZ = true,
            [Description("X offset value when locked. Default 0.")]
            float offsetX = 0f,
            [Description("Y offset value when locked. Default 0.")]
            float offsetY = 0f,
            [Description("Z offset value when locked. Default 0.")]
            float offsetZ = 0f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var lockAxis = go.GetComponent<LockAxis>();
                if (lockAxis == null)
                    lockAxis = Undo.AddComponent<LockAxis>(go);

                lockAxis.LockX = lockX;
                lockAxis.LockY = lockY;
                lockAxis.LockZ = lockZ;
                lockAxis.LockOffset = new Vector3(offsetX, offsetY, offsetZ);

                EditorUtility.SetDirty(go);

                return new LockAxisResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    lockX = lockAxis.LockX,
                    lockY = lockAxis.LockY,
                    lockZ = lockAxis.LockZ,
                    offset = FormatVector3(lockAxis.LockOffset)
                };
            });
        }

        public class LockAxisResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("X axis locked")] public bool lockX;
            [Description("Y axis locked")] public bool lockY;
            [Description("Z axis locked")] public bool lockZ;
            [Description("Lock offset values")] public string offset = "";
        }
    }
}
#endif
