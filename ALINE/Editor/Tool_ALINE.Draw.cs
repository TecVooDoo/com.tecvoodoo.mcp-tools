#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using com.IvanMurzak.Unity.MCP.Editor.Utils;
using Drawing;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace MCPTools.ALINE.Editor
{
    public partial class Tool_ALINE
    {
        [McpPluginTool("aline-draw-line", Title = "ALINE / Draw Line")]
        [Description(@"Draws a line in the Scene View using ALINE.
The line persists for 'duration' seconds, then disappears.
Useful for visualizing directions, distances, or connections between scene objects.
Can draw from explicit coordinates or between two named scene GameObjects.
After calling, move the mouse in the Scene View to trigger a repaint.")]
        public string DrawLine(
            [Description("Start X position (world space).")] float fromX = 0f,
            [Description("Start Y position (world space).")] float fromY = 0f,
            [Description("Start Z position (world space).")] float fromZ = 0f,
            [Description("End X position (world space).")] float toX = 0f,
            [Description("End Y position (world space).")] float toY = 1f,
            [Description("End Z position (world space).")] float toZ = 0f,
            [Description("Name of the start GameObject (overrides fromX/Y/Z if set).")] string? fromObjectName = null,
            [Description("Name of the end GameObject (overrides toX/Y/Z if set).")] string? toObjectName = null,
            [Description("Duration in seconds the line persists (default 10s).")] float duration = 10f,
            [Description("Color red [0-1].")] float colorR = 1f,
            [Description("Color green [0-1].")] float colorG = 0.3f,
            [Description("Color blue [0-1].")] float colorB = 0f,
            [Description("Color alpha [0-1].")] float colorA = 1f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                Vector3 from = new Vector3(fromX, fromY, fromZ);
                Vector3 to   = new Vector3(toX, toY, toZ);

                if (fromObjectName != null)
                {
                    var go = GameObject.Find(fromObjectName);
                    if (go == null) throw new System.Exception($"GameObject '{fromObjectName}' not found.");
                    from = go.transform.position;
                }
                if (toObjectName != null)
                {
                    var go = GameObject.Find(toObjectName);
                    if (go == null) throw new System.Exception($"GameObject '{toObjectName}' not found.");
                    to = go.transform.position;
                }

                Color color = ParseColor(colorR, colorG, colorB, colorA);
                using (Draw.WithDuration(Mathf.Max(0.1f, duration)))
                using (Draw.WithColor(color))
                {
                    Draw.Line(from, to);
                }

                SceneView.RepaintAll();
                return $"OK: Line drawn from ({from.x:F2},{from.y:F2},{from.z:F2}) to ({to.x:F2},{to.y:F2},{to.z:F2}) for {duration:F1}s.";
            });
        }

        [McpPluginTool("aline-draw-sphere", Title = "ALINE / Draw Sphere")]
        [Description(@"Draws a wire sphere in the Scene View using ALINE.
Persists for 'duration' seconds. Useful for visualizing trigger zones, ranges, or spawn points.
Can position at a named GameObject's world position.")]
        public string DrawSphere(
            [Description("Center X position (world space).")] float x = 0f,
            [Description("Center Y position (world space).")] float y = 0f,
            [Description("Center Z position (world space).")] float z = 0f,
            [Description("Sphere radius.")] float radius = 1f,
            [Description("Name of a GameObject to use as center position (overrides x/y/z).")] string? centerObjectName = null,
            [Description("Duration in seconds (default 10s).")] float duration = 10f,
            [Description("Color red [0-1].")] float colorR = 0f,
            [Description("Color green [0-1].")] float colorG = 0.8f,
            [Description("Color blue [0-1].")] float colorB = 1f,
            [Description("Color alpha [0-1].")] float colorA = 1f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                Vector3 center = new Vector3(x, y, z);
                if (centerObjectName != null)
                {
                    var go = GameObject.Find(centerObjectName);
                    if (go == null) throw new System.Exception($"GameObject '{centerObjectName}' not found.");
                    center = go.transform.position;
                }

                Color color = ParseColor(colorR, colorG, colorB, colorA);
                using (Draw.WithDuration(Mathf.Max(0.1f, duration)))
                using (Draw.WithColor(color))
                {
                    Draw.WireSphere(new float3(center.x, center.y, center.z), radius);
                }

                SceneView.RepaintAll();
                return $"OK: Sphere drawn at ({center.x:F2},{center.y:F2},{center.z:F2}) r={radius:F2} for {duration:F1}s.";
            });
        }

        [McpPluginTool("aline-draw-box", Title = "ALINE / Draw Box")]
        [Description(@"Draws a wire box in the Scene View using ALINE.
Persists for 'duration' seconds. Useful for visualizing areas, volumes, or bounding regions.
Can position at a named GameObject's world position and use its scale as the box size.")]
        public string DrawBox(
            [Description("Center X position (world space).")] float x = 0f,
            [Description("Center Y position (world space).")] float y = 0f,
            [Description("Center Z position (world space).")] float z = 0f,
            [Description("Box size X.")] float sizeX = 1f,
            [Description("Box size Y.")] float sizeY = 1f,
            [Description("Box size Z.")] float sizeZ = 1f,
            [Description("Name of a GameObject to use as center (overrides x/y/z). If set, uses the GO's scale as size unless sizeX/Y/Z specified.")] string? centerObjectName = null,
            [Description("Duration in seconds (default 10s).")] float duration = 10f,
            [Description("Color red [0-1].")] float colorR = 1f,
            [Description("Color green [0-1].")] float colorG = 1f,
            [Description("Color blue [0-1].")] float colorB = 0f,
            [Description("Color alpha [0-1].")] float colorA = 1f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                Vector3 center = new Vector3(x, y, z);
                Vector3 size   = new Vector3(sizeX, sizeY, sizeZ);

                if (centerObjectName != null)
                {
                    var go = GameObject.Find(centerObjectName);
                    if (go == null) throw new System.Exception($"GameObject '{centerObjectName}' not found.");
                    center = go.transform.position;
                    if (sizeX == 1f && sizeY == 1f && sizeZ == 1f)
                        size = go.transform.localScale;
                }

                Color color = ParseColor(colorR, colorG, colorB, colorA);
                using (Draw.WithDuration(Mathf.Max(0.1f, duration)))
                using (Draw.WithColor(color))
                {
                    Draw.WireBox(new float3(center.x, center.y, center.z), new float3(size.x, size.y, size.z));
                }

                SceneView.RepaintAll();
                return $"OK: Box drawn at ({center.x:F2},{center.y:F2},{center.z:F2}) size=({size.x:F2},{size.y:F2},{size.z:F2}) for {duration:F1}s.";
            });
        }

        [McpPluginTool("aline-label", Title = "ALINE / Draw Label")]
        [Description(@"Draws a 2D text label at a world position in the Scene View using ALINE.
Persists for 'duration' seconds. Useful for annotating objects, coordinates, or debug info.
Can position at a named GameObject's world position.")]
        public string DrawLabel(
            [Description("Label text to display.")] string text,
            [Description("Position X (world space).")] float x = 0f,
            [Description("Position Y (world space).")] float y = 0f,
            [Description("Position Z (world space).")] float z = 0f,
            [Description("Name of a GameObject to use as position (overrides x/y/z).")] string? objectName = null,
            [Description("Font size in pixels (default 14).")] float fontSize = 14f,
            [Description("Duration in seconds (default 10s).")] float duration = 10f,
            [Description("Color red [0-1].")] float colorR = 1f,
            [Description("Color green [0-1].")] float colorG = 1f,
            [Description("Color blue [0-1].")] float colorB = 1f,
            [Description("Color alpha [0-1].")] float colorA = 1f
        )
        {
            return MainThread.Instance.Run(() =>
            {
                Vector3 pos = new Vector3(x, y, z);
                if (objectName != null)
                {
                    var go = GameObject.Find(objectName);
                    if (go == null) throw new System.Exception($"GameObject '{objectName}' not found.");
                    pos = go.transform.position;
                }

                Color color = ParseColor(colorR, colorG, colorB, colorA);
                using (Draw.WithDuration(Mathf.Max(0.1f, duration)))
                using (Draw.WithColor(color))
                {
                    Draw.Label2D(new float3(pos.x, pos.y, pos.z), text, fontSize);
                }

                SceneView.RepaintAll();
                return $"OK: Label '{text}' drawn at ({pos.x:F2},{pos.y:F2},{pos.z:F2}) for {duration:F1}s.";
            });
        }
    }
}
