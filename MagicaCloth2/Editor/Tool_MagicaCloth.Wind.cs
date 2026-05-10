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
using MagicaCloth2;
using UnityEditor;
using UnityEngine;

namespace MCPTools.MagicaCloth2.Editor
{
    public partial class Tool_MagicaCloth
    {
        [McpPluginTool("magica-add-wind", Title = "Magica Cloth / Add Wind Zone")]
        [Description(@"Creates a new GameObject with a MagicaWindZone component.
Wind zones affect all MagicaCloth in the scene (GlobalDirection) or within an area.
Modes: GlobalDirection (whole scene), SphereDirection, BoxDirection, SphereRadial.")]
        public AddWindResponse AddWind(
            [Description("Name for the wind zone GameObject. Default 'MagicaWind'.")]
            string name = "MagicaWind",
            [Description("Wind mode: 'GlobalDirection', 'SphereDirection', 'BoxDirection', 'SphereRadial'. Default 'GlobalDirection'.")]
            string mode = "GlobalDirection",
            [Description("Wind strength in m/s (0-30). Default 5.")]
            float strength = 5f,
            [Description("Turbulence rate (0-1). Default 1.")]
            float turbulence = 1f,
            [Description("Wind direction X angle (-180 to 180). Default 0.")]
            float directionAngleX = 0f,
            [Description("Wind direction Y angle (-180 to 180). Default 0.")]
            float directionAngleY = 0f,
            [Description("Sphere radius (for sphere modes). Default 10.")]
            float radius = 10f,
            [Description("Box size (for BoxDirection). Default (10,10,10).")]
            Vector3? boxSize = null,
            [Description("World position. Default (0,0,0).")]
            Vector3? position = null,
            [Description("Parent GameObject reference. Null for scene root.")]
            GameObjectRef? parentGameObjectRef = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject? parentGo = null;
                if (parentGameObjectRef?.IsValid(out _) == true)
                {
                    parentGo = parentGameObjectRef.FindGameObject(out var error);
                    if (error != null) throw new System.Exception(error);
                }

                var go = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(go, "Create Magica Wind Zone");

                if (parentGo != null)
                    go.transform.SetParent(parentGo.transform, false);
                go.transform.position = position ?? Vector3.zero;

                var wind = go.AddComponent<MagicaWindZone>();

                switch (mode.ToLower().Replace(" ", ""))
                {
                    case "spheredirection": wind.mode = MagicaWindZone.Mode.SphereDirection; break;
                    case "boxdirection":    wind.mode = MagicaWindZone.Mode.BoxDirection; break;
                    case "sphereradial":    wind.mode = MagicaWindZone.Mode.SphereRadial; break;
                    default:                wind.mode = MagicaWindZone.Mode.GlobalDirection; break;
                }

                wind.main = Mathf.Clamp(strength, 0f, 30f);
                wind.turbulence = Mathf.Clamp01(turbulence);
                wind.directionAngleX = Mathf.Clamp(directionAngleX, -180f, 180f);
                wind.directionAngleY = Mathf.Clamp(directionAngleY, -180f, 180f);
                wind.radius = radius;
                if (boxSize.HasValue) wind.size = boxSize.Value;

                EditorUtility.SetDirty(go);

                return new AddWindResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    windMode = wind.mode.ToString(),
                    strength = wind.main,
                    turbulence = wind.turbulence
                };
            });
        }

        public class AddWindResponse
        {
            [Description("Name of the wind zone GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Wind mode")] public string windMode = "";
            [Description("Wind strength (m/s)")] public float strength;
            [Description("Turbulence rate")] public float turbulence;
        }
    }
}
