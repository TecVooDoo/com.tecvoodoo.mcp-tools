#if HAS_MUDBUN
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.MudBun.Editor
{
    public partial class Tool_MudBun
    {
        [McpPluginTool("mudbun-query", Title = "MudBun / Query Renderer & Brushes")]
        [Description(@"Reports MudBun renderer settings and brush hierarchy on a GameObject.
Returns: RenderMode, MeshingMode, VoxelDensity, MaxVoxelsK, MaxChunks, MasterColor,
MasterMetallic, MasterSmoothness, SurfaceShift, Enable2dMode.
Lists all child brushes with type, Operator, Blend, type-specific params, and material info.")]
        public string QueryMudBun(
            [Description("Name of the GameObject with the MudRenderer component.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var renderer = GetRenderer(gameObjectName);
                var sb = new StringBuilder();

                sb.AppendLine($"=== MudBun Renderer: {gameObjectName} ===");

                sb.AppendLine("\n-- Renderer Settings --");
                sb.AppendLine($"  RenderMode:        {Get(renderer, "RenderMode")}");
                sb.AppendLine($"  MeshingMode:       {Get(renderer, "MeshingMode")}");
                sb.AppendLine($"  VoxelDensity:      {Get(renderer, "VoxelDensity")}");
                sb.AppendLine($"  MaxVoxelsK:        {Get(renderer, "MaxVoxelsK")}");
                sb.AppendLine($"  MaxChunks:         {Get(renderer, "MaxChunks")}");
                sb.AppendLine($"  MasterColor:       {FormatColor(Get(renderer, "MasterColor"))}");
                sb.AppendLine($"  MasterMetallic:    {Get(renderer, "MasterMetallic")}");
                sb.AppendLine($"  MasterSmoothness:  {Get(renderer, "MasterSmoothness")}");
                sb.AppendLine($"  SurfaceShift:      {Get(renderer, "SurfaceShift")}");
                sb.AppendLine($"  Enable2dMode:      {Get(renderer, "Enable2dMode")}");
                sb.AppendLine($"  CastShadows:       {Get(renderer, "CastShadows")}");
                sb.AppendLine($"  ReceiveShadows:    {Get(renderer, "ReceiveShadows")}");

                // List child brushes
                if (MudSolidType == null)
                {
                    sb.AppendLine("\n[Warning] MudSolid type not found; cannot list brushes.");
                    return sb.ToString();
                }

                var go = ((UnityEngine.Component)renderer).gameObject;
                var brushes = go.GetComponentsInChildren(MudSolidType);
                sb.AppendLine($"\n-- Brushes ({brushes.Length}) --");

                foreach (var brush in brushes)
                {
                    var brushGo = ((UnityEngine.Component)brush).gameObject;
                    var brushType = GetBrushTypeName((UnityEngine.Component)brush);
                    sb.AppendLine($"\n  [{brushGo.name}] ({brushType})");
                    sb.AppendLine($"    Operator:    {Get(brush, "Operator")}");
                    sb.AppendLine($"    Blend:       {Get(brush, "Blend")}");

                    // Type-specific parameters
                    if (MudSphereType != null && MudSphereType.IsInstanceOfType(brush))
                    {
                        sb.AppendLine($"    Radius:      {Get(brush, "Radius")}");
                    }
                    else if (MudBoxType != null && MudBoxType.IsInstanceOfType(brush))
                    {
                        sb.AppendLine($"    Round:       {Get(brush, "Round")}");
                    }
                    else if (MudCylinderType != null && MudCylinderType.IsInstanceOfType(brush))
                    {
                        sb.AppendLine($"    Radius:      {Get(brush, "Radius")}");
                        sb.AppendLine($"    Round:       {Get(brush, "Round")}");
                    }

                    // Material info on the brush
                    sb.AppendLine($"    Color:       {FormatColor(Get(brush, "Color"))}");
                    sb.AppendLine($"    Emission:    {FormatColor(Get(brush, "Emission"))}");
                    sb.AppendLine($"    Metallic:    {Get(brush, "Metallic")}");
                    sb.AppendLine($"    Smoothness:  {Get(brush, "Smoothness")}");
                }

                return sb.ToString();
            });
        }
    }
}
#endif
