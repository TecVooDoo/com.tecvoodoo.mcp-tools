#nullable enable
using com.IvanMurzak.McpPlugin;
using DamageNumbersPro;
using UnityEngine;

namespace MCPTools.DamageNumbersPro.Editor
{
    [McpPluginToolType]
    public partial class Tool_DamageNumbersPro
    {
        static DamageNumber GetDN(string gameObjectName)
        {
            var go = GameObject.Find(gameObjectName);
            if (go == null)
                throw new System.Exception($"GameObject '{gameObjectName}' not found.");
            var dn = go.GetComponent<DamageNumber>();
            if (dn == null)
                throw new System.Exception($"'{gameObjectName}' has no DamageNumber component (DamageNumberMesh or DamageNumberGUI).");
            return dn;
        }
    }
}
