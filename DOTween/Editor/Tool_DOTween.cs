#if HAS_DOTWEEN
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;
using UnityEditor;
using DG.Tweening;

namespace TecVooDoo.MCPTools.Editor
{
    [McpPluginToolType]
    public partial class Tool_DOTween
    {
        static GameObject FindGO(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) throw new ArgumentException($"GameObject '{name}' not found.", nameof(name));
            return go;
        }
    }
}
#endif
