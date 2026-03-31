#nullable enable
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Febucci.TextAnimatorForUnity;

namespace MCPTools.TextAnimator.Editor
{
    public partial class Tool_TextAnimator
    {
        [McpPluginTool("ta-list-effects", Title = "Text Animator / List Effects")]
        [Description(@"Lists all available Text Animator effects (behavior and appearance animations).
Discovers effects via the [EffectInfo] attribute across all loaded assemblies.
Shows tag ID, category (Behavior/Appearance), and class name.
Use these tag IDs in rich text tags like <wave>, <shake>, etc.")]
        public string ListEffects(
            [Description("Optional filter: 'behaviors', 'appearances', or a tag name to search. Leave empty for all.")]
            string? filter = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== TEXT ANIMATOR EFFECTS ===");

                var effectTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Array.Empty<Type>(); }
                    })
                    .Select(t => new { Type = t, Attr = t.GetCustomAttribute<EffectInfoAttribute>() })
                    .Where(x => x.Attr != null)
                    .OrderBy(x => x.Attr!.category)
                    .ThenBy(x => x.Attr!.tagID)
                    .ToList();

                string? filterLower = filter?.ToLowerInvariant();
                EffectCategory? categoryFilter = null;
                if (filterLower == "behaviors" || filterLower == "behavior")
                    categoryFilter = EffectCategory.Behaviors;
                else if (filterLower == "appearances" || filterLower == "appearance")
                    categoryFilter = EffectCategory.Appearances;

                var filtered = effectTypes.Where(x =>
                {
                    if (categoryFilter.HasValue)
                        return x.Attr!.category == categoryFilter.Value;
                    if (filterLower != null && categoryFilter == null)
                        return x.Attr!.tagID.ToLowerInvariant().Contains(filterLower) ||
                               x.Type.Name.ToLowerInvariant().Contains(filterLower);
                    return true;
                }).ToList();

                // Group by category
                var behaviors = filtered.Where(x => x.Attr!.category == EffectCategory.Behaviors).ToList();
                var appearances = filtered.Where(x => x.Attr!.category == EffectCategory.Appearances).ToList();

                sb.AppendLine($"  Total: {filtered.Count} effects");
                sb.AppendLine();

                if (behaviors.Count > 0)
                {
                    sb.AppendLine("  --- BEHAVIOR EFFECTS (looping/continuous) ---");
                    foreach (var e in behaviors)
                        sb.AppendLine($"    <{e.Attr!.tagID}> ({e.Type.Name})");
                    sb.AppendLine();
                }

                if (appearances.Count > 0)
                {
                    sb.AppendLine("  --- APPEARANCE EFFECTS (show/hide animations) ---");
                    foreach (var e in appearances)
                        sb.AppendLine($"    <{e.Attr!.tagID}> ({e.Type.Name})");
                }

                return sb.ToString();
            });
        }
    }
}
