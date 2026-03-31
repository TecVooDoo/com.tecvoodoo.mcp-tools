#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using AC;
using UnityEngine;

namespace MCPTools.AdventureCreator.Editor
{
    public partial class Tool_AdventureCreator
    {
        [McpPluginTool("ac-find-scene-objects", Title = "Adventure Creator / Find Scene Objects")]
        [Description(@"Finds Adventure Creator objects in the current scene: Hotspots, NPCs, Players,
Markers, Triggers, and ActionLists. Shows their names, positions, and key properties.
Requires a scene to be open.")]
        public string FindSceneObjects()
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== AC SCENE OBJECTS ===");

                // Hotspots
                var hotspots = Object.FindObjectsByType<Hotspot>(FindObjectsSortMode.None);
                sb.AppendLine($"\n  --- Hotspots ({hotspots.Length}) ---");
                foreach (var h in hotspots)
                {
                    var pos = h.transform.position;
                    sb.AppendLine($"    \"{h.gameObject.name}\" at ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
                }

                // NPCs
                var npcs = Object.FindObjectsByType<NPC>(FindObjectsSortMode.None);
                sb.AppendLine($"\n  --- NPCs ({npcs.Length}) ---");
                foreach (var npc in npcs)
                {
                    var pos = npc.transform.position;
                    sb.AppendLine($"    \"{npc.gameObject.name}\" at ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
                }

                // Players
                var players = Object.FindObjectsByType<Player>(FindObjectsSortMode.None);
                sb.AppendLine($"\n  --- Players ({players.Length}) ---");
                foreach (var p in players)
                {
                    var pos = p.transform.position;
                    sb.AppendLine($"    \"{p.gameObject.name}\" at ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
                }

                // Markers
                var markers = Object.FindObjectsByType<Marker>(FindObjectsSortMode.None);
                sb.AppendLine($"\n  --- Markers ({markers.Length}) ---");
                foreach (var m in markers)
                {
                    var pos = m.transform.position;
                    sb.AppendLine($"    \"{m.gameObject.name}\" at ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
                }

                // ActionLists
                var actionLists = Object.FindObjectsByType<ActionList>(FindObjectsSortMode.None);
                sb.AppendLine($"\n  --- ActionLists ({actionLists.Length}) ---");
                foreach (var al in actionLists)
                {
                    int actionCount = al.actions != null ? al.actions.Count : 0;
                    sb.AppendLine($"    \"{al.gameObject.name}\" ({actionCount} actions)");
                }

                return sb.ToString();
            });
        }
    }
}
