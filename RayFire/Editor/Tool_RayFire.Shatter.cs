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
        [McpPluginTool("rayfire-add-shatter", Title = "RayFire / Add Shatter")]
        [Description(@"Adds a RayfireShatter component for pre-fragmenting a mesh.
Choose a fragmentation algorithm and configure its parameters.
After adding, call Fragment() in the editor to generate fragments.")]
        public AddShatterResponse AddShatter(
            [Description("Reference to the target GameObject.")]
            GameObjectRef targetRef,
            [Description("Fragmentation type: 'Voronoi', 'Splinters', 'Slabs', 'Radial', 'Hexagon', 'Bricks', 'Voxels', 'Slices', 'Decompose'. Default 'Voronoi'.")]
            string fragmentType = "Voronoi",
            [Description("Number of fragments (for Voronoi). Default 10.")]
            int fragmentCount = 10,
            [Description("For Bricks: number of columns. Default 3.")]
            int brickColumns = 3,
            [Description("For Bricks: number of rows. Default 3.")]
            int brickRows = 3,
            [Description("For Radial: number of rings. Default 3.")]
            int radialRings = 3,
            [Description("For Radial: number of rays. Default 6.")]
            int radialRays = 6,
            [Description("Enable fragment preview coloring. Default false.")]
            bool colorPreview = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var shatter = go.GetComponent<RayfireShatter>();
                if (shatter == null)
                    shatter = Undo.AddComponent<RayfireShatter>(go);

                shatter.type = ParseEnum<FragType>(fragmentType, FragType.Voronoi);
                shatter.colorPreview = colorPreview;

                switch (shatter.type)
                {
                    case FragType.Voronoi:
                        shatter.voronoi.amount = fragmentCount;
                        break;
                    case FragType.Splinters:
                        shatter.splinters.amount = fragmentCount;
                        break;
                    case FragType.Slabs:
                        shatter.slabs.amount = fragmentCount;
                        break;
                    case FragType.Bricks:
                        shatter.bricks.amount_X = brickColumns;
                        shatter.bricks.amount_Y = brickRows;
                        break;
                    case FragType.Radial:
                        shatter.radial.rings = radialRings;
                        shatter.radial.rays = radialRays;
                        break;
                }

                EditorUtility.SetDirty(go);

                return new AddShatterResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    fragmentType = shatter.type.ToString(),
                    configured = true
                };
            });
        }

        [McpPluginTool("rayfire-fragment", Title = "RayFire / Fragment Object")]
        [Description(@"Executes fragmentation on a RayfireShatter component, generating fragment meshes in the editor.
The object must have a RayfireShatter component. This is an editor-time operation.")]
        public FragmentResponse Fragment(
            [Description("Reference to the GameObject with RayfireShatter.")]
            GameObjectRef targetRef
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var go = targetRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (go == null) throw new System.Exception("GameObject not found.");

                var shatter = go.GetComponent<RayfireShatter>();
                if (shatter == null)
                    throw new System.Exception($"'{go.name}' has no RayfireShatter component.");

                shatter.Fragment();

                int batchCount = shatter.batches != null ? shatter.batches.Count : 0;

                EditorUtility.SetDirty(go);

                return new FragmentResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    fragmentType = shatter.type.ToString(),
                    batchCount = batchCount,
                    hasFragments = shatter.HasBatches
                };
            });
        }

        public class AddShatterResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Fragment type configured")] public string fragmentType = "";
            [Description("Configuration applied")] public bool configured;
        }

        public class FragmentResponse
        {
            [Description("Name of the GameObject")] public string gameObjectName = "";
            [Description("Instance ID")] public int instanceId;
            [Description("Fragment type used")] public string fragmentType = "";
            [Description("Number of fragment batches")] public int batchCount;
            [Description("Has generated fragments")] public bool hasFragments;
        }
    }
}
