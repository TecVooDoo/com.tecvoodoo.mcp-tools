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
using Flexalon;
using UnityEditor;
using UnityEngine;

namespace MCPTools.Flexalon.Editor
{
    public partial class Tool_Flexalon
    {
        [McpPluginTool("flexalon-create-random-layout", Title = "Flexalon / Create Random Layout")]
        [Description(@"Creates a new GameObject with a Flexalon Random Layout component.
Children are placed at random positions within the layout bounds.
Great for scattering objects like trees, rocks, debris, etc.")]
        public RandomLayoutResponse CreateRandomLayout(
            [Description("Name for the layout GameObject.")]
            string name = "FlexalonRandom",
            [Description("Random seed for reproducible layouts. Default 1.")]
            int randomSeed = 1,
            [Description("Whether to randomize child rotations. Default false.")]
            bool randomizeRotation = false,
            [Description("Size of the random area (width, height, depth). Default (10, 1, 10).")]
            Vector3? size = null,
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
                Undo.RegisterCreatedObjectUndo(go, "Create Flexalon Random");

                if (parentGo != null)
                    go.transform.SetParent(parentGo.transform, false);

                go.transform.position = position ?? Vector3.zero;

                var random = go.AddComponent<FlexalonRandomLayout>();
                random.RandomSeed = randomSeed;
                random.RandomizeRotationX = randomizeRotation;
                random.RandomizeRotationY = randomizeRotation;
                random.RandomizeRotationZ = randomizeRotation;

                // FlexalonLayoutBase auto-adds FlexalonObject via [RequireComponent] — use GetComponent
                var s = size ?? new Vector3(10f, 1f, 10f);
                var fObj = go.GetComponent<FlexalonObject>();
                if (fObj != null)
                {
                    fObj.Width = s.x;
                    fObj.Height = s.y;
                    fObj.Depth = s.z;
                }

                EditorUtility.SetDirty(go);

                return new RandomLayoutResponse
                {
                    gameObjectName = go.name,
                    instanceId = go.GetInstanceID(),
                    randomSeed = random.RandomSeed,
                    position = FormatVector3(go.transform.position)
                };
            });
        }

        public class RandomLayoutResponse
        {
            [Description("Name of the created GameObject")]
            public string gameObjectName = "";
            [Description("Instance ID")]
            public int instanceId;
            [Description("Random seed used")]
            public int randomSeed;
            [Description("World position")]
            public string position = "";
        }
    }
}
