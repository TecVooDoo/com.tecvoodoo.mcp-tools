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
        [McpPluginTool("flexalon-add-child", Title = "Flexalon / Add Child to Layout")]
        [Description(@"Adds a child GameObject to an existing Flexalon layout. The child can be:
- A new primitive (cube, sphere, cylinder, etc.)
- An existing GameObject (moved into the layout)
- A prefab instance (instantiated from an asset path)
The layout will automatically arrange the child.")]
        public AddChildResponse AddChild(
            [Description("Reference to the Flexalon layout GameObject to add the child to.")]
            GameObjectRef layoutRef,
            [Description("Type of child to add: 'cube', 'sphere', 'cylinder', 'capsule', 'quad', 'empty', 'existing', or 'prefab'. Default 'cube'.")]
            string childType = "cube",
            [Description("Name for the new child. Default auto-generated.")]
            string? childName = null,
            [Description("When childType is 'existing', reference to the existing GameObject to move into the layout.")]
            GameObjectRef? existingChildRef = null,
            [Description("When childType is 'prefab', the asset path to the prefab (e.g. 'Assets/Synty/.../.prefab').")]
            string? prefabPath = null,
            [Description("Scale of the child. Default (1,1,1).")]
            Vector3? scale = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var layoutGo = layoutRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (layoutGo == null) throw new System.Exception("Layout GameObject not found.");

                // Verify it has a Flexalon layout
                var hasLayout = layoutGo.GetComponent<LayoutBase>() != null;
                if (!hasLayout)
                    throw new System.Exception($"GameObject '{layoutGo.name}' does not have a Flexalon layout component.");

                GameObject child;
                string typeUsed;

                switch (childType.ToLower())
                {
                    case "sphere":
                        child = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        typeUsed = "Sphere";
                        break;
                    case "cylinder":
                        child = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        typeUsed = "Cylinder";
                        break;
                    case "capsule":
                        child = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                        typeUsed = "Capsule";
                        break;
                    case "quad":
                        child = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        typeUsed = "Quad";
                        break;
                    case "empty":
                        child = new GameObject();
                        typeUsed = "Empty";
                        break;
                    case "existing":
                        if (existingChildRef == null)
                            throw new System.ArgumentException("existingChildRef is required when childType is 'existing'.");
                        child = existingChildRef.FindGameObject(out var existErr);
                        if (existErr != null) throw new System.Exception(existErr);
                        if (child == null) throw new System.Exception("Existing child GameObject not found.");
                        typeUsed = "Existing";
                        Undo.SetTransformParent(child.transform, layoutGo.transform, "Move to Flexalon Layout");
                        if (childName != null) child.name = childName;
                        if (scale != null) child.transform.localScale = scale.Value;
                        EditorUtility.SetDirty(layoutGo);
                        return new AddChildResponse
                        {
                            gameObjectName = child.name,
                            instanceId = child.GetInstanceID(),
                            childType = typeUsed,
                            layoutName = layoutGo.name,
                            childCount = layoutGo.transform.childCount
                        };
                    case "prefab":
                        if (string.IsNullOrEmpty(prefabPath))
                            throw new System.ArgumentException("prefabPath is required when childType is 'prefab'.");
                        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                        if (prefab == null)
                            throw new System.Exception($"Prefab not found at path: {prefabPath}");
                        child = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                        typeUsed = "Prefab";
                        break;
                    default: // cube
                        child = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        typeUsed = "Cube";
                        break;
                }

                Undo.RegisterCreatedObjectUndo(child, "Add Flexalon Child");
                child.name = childName ?? $"{typeUsed}_{layoutGo.transform.childCount}";
                child.transform.SetParent(layoutGo.transform);
                child.transform.localPosition = Vector3.zero;
                child.transform.localRotation = Quaternion.identity;
                child.transform.localScale = scale ?? Vector3.one;

                EditorUtility.SetDirty(layoutGo);

                return new AddChildResponse
                {
                    gameObjectName = child.name,
                    instanceId = child.GetInstanceID(),
                    childType = typeUsed,
                    layoutName = layoutGo.name,
                    childCount = layoutGo.transform.childCount
                };
            });
        }

        [McpPluginTool("flexalon-add-prefab-children", Title = "Flexalon / Add Multiple Prefab Children")]
        [Description(@"Adds multiple instances of a prefab as children of a Flexalon layout.
Useful for quickly populating a grid or circle with identical objects.")]
        public AddMultipleResponse AddPrefabChildren(
            [Description("Reference to the Flexalon layout GameObject.")]
            GameObjectRef layoutRef,
            [Description("Asset path to the prefab (e.g. 'Assets/Synty/.../SM_Something.prefab').")]
            string prefabPath,
            [Description("Number of instances to add. Default 1.")]
            int count = 1,
            [Description("Scale for each instance. Default (1,1,1).")]
            Vector3? scale = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var layoutGo = layoutRef.FindGameObject(out var error);
                if (error != null) throw new System.Exception(error);
                if (layoutGo == null) throw new System.Exception("Layout GameObject not found.");

                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null)
                    throw new System.Exception($"Prefab not found at path: {prefabPath}");

                int added = 0;
                for (int i = 0; i < count; i++)
                {
                    var child = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                    Undo.RegisterCreatedObjectUndo(child, "Add Flexalon Prefab Child");
                    child.transform.SetParent(layoutGo.transform);
                    child.transform.localPosition = Vector3.zero;
                    child.transform.localRotation = Quaternion.identity;
                    child.transform.localScale = scale ?? Vector3.one;
                    added++;
                }

                EditorUtility.SetDirty(layoutGo);

                return new AddMultipleResponse
                {
                    layoutName = layoutGo.name,
                    prefabName = prefab.name,
                    addedCount = added,
                    totalChildCount = layoutGo.transform.childCount
                };
            });
        }

        public class AddChildResponse
        {
            [Description("Name of the child GameObject")]
            public string gameObjectName = "";
            [Description("Instance ID of the child")]
            public int instanceId;
            [Description("Type of child added")]
            public string childType = "";
            [Description("Name of the layout it was added to")]
            public string layoutName = "";
            [Description("Total number of children in the layout")]
            public int childCount;
        }

        public class AddMultipleResponse
        {
            [Description("Name of the layout")]
            public string layoutName = "";
            [Description("Name of the prefab used")]
            public string prefabName = "";
            [Description("Number of instances added")]
            public int addedCount;
            [Description("Total children in the layout")]
            public int totalChildCount;
        }
    }
}
