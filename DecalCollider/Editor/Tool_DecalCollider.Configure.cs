#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.DecalCollider.Editor
{
    public partial class Tool_DecalCollider
    {
        [McpPluginTool("decal-configure", Title = "Decal Collider / Configure")]
        [Description(@"Sets configuration properties on a DecalCollider component. All parameters optional.
decalMode: GridProjection or MeshProjection.
projectionDirection: Up, Down, Forward, Back, Right, Left, ForwardUp, ForwardDown, BackUp, BackDown,
  RightUp, RightDown, LeftUp, LeftDown, ForwardRight, ForwardLeft, BackRight, BackLeft.
projectionSpace: Local or World.
After configuring, call decal-rebuild to regenerate the mesh.")]
        public string Configure(
            [Description("Name of the GameObject with the DecalCollider component.")] string gameObjectName,
            [Description("Decal mode: GridProjection or MeshProjection.")] string? decalMode = null,
            [Description("Projection direction enum name (e.g. Forward, Down, BackUp).")] string? projectionDirection = null,
            [Description("Projection space: Local or World.")] string? projectionSpace = null,
            [Description("Decal size X (width).")] float? sizeX = null,
            [Description("Decal size Y (height).")] float? sizeY = null,
            [Description("Max raycast distance.")] float? maxDistance = null,
            [Description("Mesh subdivisions [1-128]. Higher = smoother, more expensive.")] int? meshSubdivisions = null,
            [Description("Collider subdivisions [1-64].")] int? colliderSubdivisions = null,
            [Description("Surface offset to avoid z-fighting.")] float? surfaceOffset = null,
            [Description("Alpha threshold [0-1] for masking.")] float? alphaThreshold = null,
            [Description("Always rebuild when transform changes.")] bool? alwaysRebuild = null,
            [Description("Ignore self in raycasts.")] bool? ignoreSelf = null,
            [Description("Cull mesh when not visible to camera.")] bool? cullIfInvisible = null,
            [Description("Enable dynamic LOD based on camera distance.")] bool? useDynamicLOD = null,
            [Description("LOD falloff distance.")] float? lodDistance = null,
            [Description("Live update interval in seconds.")] float? liveUpdateInterval = null,
            [Description("Update collider during live updates.")] bool? updateColliderOnLive = null,
            [Description("Center offset X.")] float? centerX = null,
            [Description("Center offset Y.")] float? centerY = null,
            [Description("Center offset Z.")] float? centerZ = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var dc = GetDecal(gameObjectName);

                if (decalMode != null) SetEnum(dc, "decalMode", decalMode);
                if (projectionDirection != null) SetEnum(dc, "projectionDirection", projectionDirection);
                if (projectionSpace != null) SetEnum(dc, "projectionSpace", projectionSpace);

                if (sizeX.HasValue || sizeY.HasValue)
                {
                    var s = (Vector2)(Get(dc, "size") ?? Vector2.one);
                    if (sizeX.HasValue) s.x = sizeX.Value;
                    if (sizeY.HasValue) s.y = sizeY.Value;
                    Set(dc, "size", s);
                }

                if (maxDistance.HasValue) Set(dc, "maxDistance", maxDistance.Value);
                if (meshSubdivisions.HasValue) Set(dc, "meshSubdivisions", Mathf.Clamp(meshSubdivisions.Value, 1, 128));
                if (colliderSubdivisions.HasValue) Set(dc, "colliderSubdivisions", Mathf.Clamp(colliderSubdivisions.Value, 1, 64));
                if (surfaceOffset.HasValue) Set(dc, "surfaceOffset", surfaceOffset.Value);
                if (alphaThreshold.HasValue) Set(dc, "alphaThreshold", Mathf.Clamp01(alphaThreshold.Value));
                if (alwaysRebuild.HasValue) Set(dc, "alwaysRebuild", alwaysRebuild.Value);
                if (ignoreSelf.HasValue) Set(dc, "ignoreSelf", ignoreSelf.Value);
                if (cullIfInvisible.HasValue) Set(dc, "cullIfInvisible", cullIfInvisible.Value);
                if (useDynamicLOD.HasValue) Set(dc, "useDynamicLOD", useDynamicLOD.Value);
                if (lodDistance.HasValue) Set(dc, "lodDistance", lodDistance.Value);
                if (liveUpdateInterval.HasValue) Set(dc, "liveUpdateInterval", liveUpdateInterval.Value);
                if (updateColliderOnLive.HasValue) Set(dc, "updateColliderOnLive", updateColliderOnLive.Value);

                if (centerX.HasValue || centerY.HasValue || centerZ.HasValue)
                {
                    var c = (Vector3)(Get(dc, "center") ?? Vector3.zero);
                    if (centerX.HasValue) c.x = centerX.Value;
                    if (centerY.HasValue) c.y = centerY.Value;
                    if (centerZ.HasValue) c.z = centerZ.Value;
                    Set(dc, "center", c);
                }

                EditorUtility.SetDirty(dc);
                return $"OK: DecalCollider on '{gameObjectName}' configured. mode={Get(dc, "decalMode")} dir={Get(dc, "projectionDirection")} size={Get(dc, "size")} subdivs={Get(dc, "meshSubdivisions")}/{Get(dc, "colliderSubdivisions")}";
            });
        }
    }
}
