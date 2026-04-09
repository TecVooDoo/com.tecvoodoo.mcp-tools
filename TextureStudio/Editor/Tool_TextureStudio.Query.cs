#nullable enable
using System.Collections;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.TextureStudio.Editor
{
    public partial class Tool_TextureStudio
    {
        [McpPluginTool("texstudio-query", Title = "Texture Studio / Query CompositeMap")]
        [Description(@"Inspects a CompositeMap asset: lists all layers with hierarchy, blend mode, active state,
transform, parameters, and linked materials. Also reports map dimensions, state snapshots, and output settings.
assetName: name of the CompositeMap ScriptableObject (searched via AssetDatabase).")]
        public string Query(
            [Description("Name of the CompositeMap asset to inspect.")] string assetName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var map = GetMap(assetName);
                var sb = new StringBuilder();

                sb.AppendLine($"CompositeMap: {Get(map, "mapName") ?? map.name}");
                sb.AppendLine($"  Size: {Get(map, "width")}x{Get(map, "height")} (aspect={Get(map, "aspect")})");
                sb.AppendLine($"  Format: {Get(map, "format")}  sRGB={Get(map, "srgb")}  MipMaps={Get(map, "mipMaps")}");
                sb.AppendLine($"  MapMode: {Get(map, "mapMode")}  ClearColor: {Get(map, "clearCol")}");

                var adjustOutput = Get(map, "adjustOutput");
                if (adjustOutput is true)
                {
                    sb.AppendLine($"  OutputAdjust: hue={Get(map, "hue")} sat={Get(map, "saturation")} contrast={Get(map, "contrast")} brightness={Get(map, "brightness")}");
                    sb.AppendLine($"    Levels: min={Get(map, "levelsMin")} max={Get(map, "levelsMax")}");
                }

                // Materials
                var mtms = Get(map, "mapToMaterials") as IList;
                if (mtms != null && mtms.Count > 0)
                {
                    sb.AppendLine($"  LinkedMaterials ({mtms.Count}):");
                    foreach (var mtm in mtms)
                    {
                        var mat = Get(mtm!, "material") as Material;
                        string matName = mat != null ? mat.name : "null";
                        sb.AppendLine($"    {matName} -> {Get(mtm!, "matName")}");
                    }
                }

                // States
                var states = Get(map, "mapStates") as IList;
                if (states != null && states.Count > 0)
                {
                    sb.Append($"  SavedStates ({states.Count}): ");
                    for (int i = 0; i < states.Count; i++)
                    {
                        if (i > 0) sb.Append(", ");
                        sb.Append($"[{i}]{Get(states[i]!, "name")}");
                    }
                    sb.AppendLine();
                    sb.AppendLine($"  CurrentState: {Get(map, "currentState")}");
                }

                // Layers
                var allLayers = GetAllLayers(map);
                if (allLayers != null)
                {
                    sb.AppendLine($"  Layers ({allLayers.Count}):");
                    foreach (var layer in allLayers)
                    {
                        if (layer == null) continue;
                        string indent = Get(layer, "parent") != null ? "      " : "    ";
                        string activeStr = (Get(layer, "active") is true) ? "ON" : "off";
                        var layerMap = Get(layer, "map") as Texture;
                        var sprite = Get(layer, "sprite") as Sprite;
                        string mapStr = layerMap != null ? layerMap.name : (sprite != null ? $"sprite:{sprite.name}" : "none");

                        sb.AppendLine($"{indent}[{activeStr}] \"{Get(layer, "name")}\" blend={Get(layer, "blendMode")} opacity={Get(layer, "blendAmount")} map={mapStr}");

                        var pos = Get(layer, "pos");
                        var scale = Get(layer, "scale");
                        sb.AppendLine($"{indent}  pos={pos} scale={scale} angle={Get(layer, "angle")}");

                        var str = Get(layer, "str") as string;
                        if (!string.IsNullOrEmpty(str))
                            sb.AppendLine($"{indent}  text=\"{str}\" fontSize={Get(layer, "fontSize")} color={Get(layer, "textCol")}");

                        var children = Get(layer, "children") as IList;
                        if (children != null && children.Count > 0)
                            sb.AppendLine($"{indent}  children: {children.Count}");
                    }
                }

                return sb.ToString();
            });
        }
    }
}
