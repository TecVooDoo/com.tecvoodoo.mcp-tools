#nullable enable
using System;
using com.IvanMurzak.McpPlugin;
using Technie.PhysicsCreator;
using Technie.PhysicsCreator.Rigid;
using UnityEditor;
using UnityEngine;

namespace MCPTools.TCC.Editor
{
    [McpPluginToolType]
    public partial class Tool_TCC
    {
        static GameObject FindGO(string name)
        {
            var go = GameObject.Find(name);
            if (go == null) throw new Exception($"GameObject '{name}' not found.");
            return go;
        }

        static RigidColliderCreator GetCreator(string gameObjectName)
        {
            var go = FindGO(gameObjectName);
            var creator = go.GetComponent<RigidColliderCreator>();
            if (creator == null)
                throw new Exception($"'{gameObjectName}' has no RigidColliderCreator component. Use tcc-create first.");
            return creator;
        }

        static Mesh GetSourceMesh(GameObject go)
        {
            var mf = go.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null) return mf.sharedMesh;
            var smr = go.GetComponent<SkinnedMeshRenderer>();
            if (smr != null && smr.sharedMesh != null) return smr.sharedMesh;
            throw new Exception($"'{go.name}' has no MeshFilter.sharedMesh or SkinnedMeshRenderer.sharedMesh.");
        }

        static HullType ParseHullType(string s) =>
            (HullType)Enum.Parse(typeof(HullType), s, true);

        static AutoHullPreset ParsePreset(string s) =>
            (AutoHullPreset)Enum.Parse(typeof(AutoHullPreset), s, true);

        static PhysicsMaterial? FindPhysicsMaterial(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            string[] guids = AssetDatabase.FindAssets($"{name} t:PhysicsMaterial");
            if (guids.Length == 0) return null;
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(path);
        }

        static RigidColliderCreatorWindow EnsureWindow()
        {
            RigidColliderCreatorWindow.ShowWindow();
            var window = RigidColliderCreatorWindow.instance;
            if (window == null)
                throw new Exception("Failed to open RigidColliderCreatorWindow.");
            return window;
        }
    }
}
