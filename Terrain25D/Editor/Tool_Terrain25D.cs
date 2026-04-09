#if HAS_TERRAIN25D
#nullable enable
using com.IvanMurzak.McpPlugin;
using Kamgam.Terrain25DLib;
using UnityEngine;

namespace MCPTools.Terrain25D.Editor
{
    [McpPluginToolType]
    public partial class Tool_Terrain25D
    {
        static global::Kamgam.Terrain25DLib.Terrain25D GetTerrain(string gameObjectName)
        {
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new System.Exception($"GameObject '{gameObjectName}' not found.");
            var terrain = go.GetComponent<global::Kamgam.Terrain25DLib.Terrain25D>();
            if (terrain == null)
                throw new System.Exception($"'{gameObjectName}' has no Terrain25D component.");
            return terrain;
        }

        static MeshGenerator GetMeshGenerator(string gameObjectName)
        {
            var terrain = GetTerrain(gameObjectName);
            var mg = terrain.MeshGenerator;
            if (mg == null)
                throw new System.Exception($"'{gameObjectName}' has no MeshGenerator child.");
            return mg;
        }
    }
}
#endif
