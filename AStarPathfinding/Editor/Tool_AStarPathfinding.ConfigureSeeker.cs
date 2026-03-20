#nullable enable
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Pathfinding;
using UnityEditor;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    public partial class Tool_AStarPathfinding
    {
        [McpPluginTool("astar-configure-seeker", Title = "A* Pathfinding / Configure Seeker")]
        [Description(@"Configure a Seeker component on a GameObject.
Sets traversable tag bitmask and per-tag cost penalties.
traversableTags: bitmask where bit N enables tag N (e.g. -1 = all tags, 1 = only tag 0, 3 = tags 0+1).
tagCosts: comma-separated penalty values for tags 0-31 (e.g. '0,0,0,1000,0,5000' penalizes tags 3 and 5).")]
        public string ConfigureSeeker(
            [Description("Name of the GameObject with a Seeker component.")] string gameObjectName,
            [Description("Bitmask of traversable tags (-1 = all, 1 = tag0 only, etc.).")] int? traversableTags = null,
            [Description("Comma-separated cost penalties for tags 0-31 (e.g. '0,0,0,1000').")] string? tagCosts = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                GameObject go = FindGO(gameObjectName);
                Seeker seeker = go.GetComponent<Seeker>();
                if (seeker == null)
                    throw new System.Exception($"'{go.name}' has no Seeker component.");

                if (traversableTags.HasValue)
                    seeker.traversableTags = traversableTags.Value;

                if (tagCosts != null)
                {
                    string[] parts = tagCosts.Split(',');
                    int[] penalties = new int[32];
                    for (int i = 0; i < parts.Length && i < 32; i++)
                    {
                        string trimmed = parts[i].Trim();
                        if (!int.TryParse(trimmed, out int penalty))
                            throw new System.Exception($"Invalid tag cost at index {i}: '{trimmed}'. Must be an integer.");
                        penalties[i] = penalty;
                    }
                    seeker.tagPenalties = penalties;
                }

                EditorUtility.SetDirty(seeker);

                StringBuilder sb = new StringBuilder();
                sb.Append($"OK: Seeker on '{go.name}' configured.");
                sb.Append($" traversableTags={seeker.traversableTags} (0x{seeker.traversableTags:X8})");
                if (tagCosts != null)
                {
                    sb.Append(" tagPenalties set:");
                    int[] p = seeker.tagPenalties;
                    for (int i = 0; i < p.Length; i++)
                    {
                        if (p[i] != 0) sb.Append($" [{i}]={p[i]}");
                    }
                }
                return sb.ToString();
            });
        }
    }
}
