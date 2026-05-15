#nullable enable
using System;
using com.IvanMurzak.McpPlugin;
using UMotionEditor.API;

namespace MCPTools.UMotionPro.Editor
{
    [McpPluginToolType]
    public partial class Tool_UMotionPro
    {
        static void RequireClipEditorWindow()
        {
            if (!ClipEditor.IsWindowOpened)
                throw new Exception("UMotion Clip Editor window is not open. Call 'umotion-project' operation='open-windows' first, then retry.");
        }

        static void RequireProjectLoaded()
        {
            RequireClipEditorWindow();
            if (!ClipEditor.IsProjectLoaded)
                throw new Exception("No UMotion project is loaded. Call 'umotion-project' operation='load' with a projectPath first.");
        }
    }
}
