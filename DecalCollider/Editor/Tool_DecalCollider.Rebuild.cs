#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.DecalCollider.Editor
{
    public partial class Tool_DecalCollider
    {
        [McpPluginTool("decal-rebuild", Title = "Decal Collider / Rebuild")]
        [Description(@"Triggers a rebuild of the DecalCollider mesh and optionally sets dynamic content before rebuilding.
Use after decal-configure to see changes take effect, or to update sprite/text/color on the fly.
spriteName: assigns a Sprite by asset name (searched via AssetDatabase).
text: assigns text content (requires TextMeshPro source).
vertexColorHex: sets vertex color as hex (e.g. FF0000 for red).
lookAtX/Y/Z: orients the decal to face a world position before rebuilding.
saveMeshPath: optionally saves the generated mesh as an asset (e.g. Assets/DecalMeshes/).")]
        public string Rebuild(
            [Description("Name of the GameObject with the DecalCollider component.")] string gameObjectName,
            [Description("Optional: Sprite asset name to assign before rebuild.")] string? spriteName = null,
            [Description("Optional: Text to assign before rebuild (needs TMP source).")] string? text = null,
            [Description("Optional: Vertex color as hex string (e.g. FF0000).")] string? vertexColorHex = null,
            [Description("Optional: Look-at target X position.")] float? lookAtX = null,
            [Description("Optional: Look-at target Y position.")] float? lookAtY = null,
            [Description("Optional: Look-at target Z position.")] float? lookAtZ = null,
            [Description("Optional: Path to save mesh asset (e.g. Assets/DecalMeshes/).")] string? saveMeshPath = null,
            [Description("Use ForceRebuild (synchronous) instead of RebuildSafe.")] bool forceSync = false
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var dc = GetDecal(gameObjectName);
                var sb = new StringBuilder();

                if (spriteName != null)
                {
                    string[] guids = AssetDatabase.FindAssets($"{spriteName} t:Sprite");
                    if (guids.Length == 0)
                        throw new System.Exception($"Sprite '{spriteName}' not found in AssetDatabase.");
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                    if (sprite == null)
                        throw new System.Exception($"Failed to load Sprite at '{path}'.");
                    CallMethod(dc, "SetSprite", sprite);
                    sb.AppendLine($"  Sprite set: {sprite.name} ({path})");
                }

                if (text != null)
                {
                    CallMethod(dc, "SetText", text);
                    sb.AppendLine($"  Text set: \"{text}\"");
                }

                if (vertexColorHex != null)
                {
                    if (ColorUtility.TryParseHtmlString($"#{vertexColorHex}", out Color color))
                    {
                        CallMethod(dc, "SetVertexColor", color);
                        sb.AppendLine($"  VertexColor set: #{vertexColorHex}");
                    }
                    else
                        throw new System.Exception($"Invalid hex color '{vertexColorHex}'. Use format: FF0000");
                }

                if (lookAtX.HasValue && lookAtY.HasValue && lookAtZ.HasValue)
                {
                    CallMethod(dc, "LookAtTarget", new Vector3(lookAtX.Value, lookAtY.Value, lookAtZ.Value));
                    sb.AppendLine($"  LookAt: ({lookAtX.Value}, {lookAtY.Value}, {lookAtZ.Value})");
                }

                if (forceSync)
                    CallMethod(dc, "ForceRebuild", true);
                else
                    CallMethod(dc, "RebuildSafe");

                var stats = Get(dc, "LastRebuildStats");
                sb.Insert(0, $"OK: DecalCollider '{gameObjectName}' rebuilt.\n");
                if (stats != null)
                    sb.AppendLine($"  Stats: trisVisual={Get(stats, "TrianglesVisual")} trisCollider={Get(stats, "TrianglesCollider")} rays={Get(stats, "RaysCast")} hits={Get(stats, "RaysHit")} time={Get(stats, "BuildTimeMS")}ms memory={Get(stats, "MemoryKb")}KB");

                if (saveMeshPath != null)
                    CallMethod(dc, "SaveMeshToAsset", saveMeshPath);

                EditorUtility.SetDirty(dc);
                return sb.ToString();
            });
        }
    }
}
