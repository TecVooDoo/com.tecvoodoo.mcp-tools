#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;

namespace MCPTools.M3DText.Editor
{
    public partial class Tool_M3DText
    {
        [McpPluginTool("m3dt-set-text", Title = "Modular 3D Text / Set Text")]
        [Description(@"Sets the Text property on a Modular3DText component.
The setter triggers an end-of-frame mesh rebuild (mesh updates are batched per frame).
For numeric overloads, use UpdateText(int) or UpdateText(float) via m3dt-configure.

forceUpdate=true clears `oldText` and forces a full character recreate, which is
useful when the existing characters are out of sync (e.g. font swap mid-frame).")]
        public string SetText(
            [Description("GameObject name with Modular3DText.")]
            string gameObjectName,
            [Description("New text content.")]
            string text,
            [Description("If true, force full character recreate (clears oldText).")]
            bool forceUpdate = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var t = GetText(gameObjectName);
                string? prev = Get(t, "Text") as string;

                if (forceUpdate)
                {
                    // Clear oldText so the diff sees every character as new.
                    Set(t, "oldText", "");
                }

                if (!Set(t, "Text", text))
                    throw new System.Exception("Failed to set Text property.");

                EditorUtility.SetDirty(t);
                return $"OK: '{gameObjectName}'.Text = \"{text}\"  (was: \"{prev}\"{(forceUpdate ? ", force-rebuilt" : "")})";
            });
        }
    }
}
