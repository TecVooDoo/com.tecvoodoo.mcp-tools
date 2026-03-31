#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using global::Naninovel;

namespace MCPTools.Naninovel.Editor
{
    public partial class Tool_Naninovel
    {
        [McpPluginTool("nani-list-commands", Title = "Naninovel / List Commands")]
        [Description(@"Lists all available Naninovel script commands with their aliases and parameter info.
Shows command name, alias (used in .nani scripts with @), and parameters.
Useful for understanding what commands are available when writing .nani scripts.")]
        public string ListCommands(
            [Description("Optional filter to search commands by name or alias. Leave empty to list all.")]
            string? filter = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                var sb = new StringBuilder();
                sb.AppendLine("=== NANINOVEL COMMANDS ===");

                // Find all Command subclasses via reflection
                var commandBaseType = typeof(Command);
                var commandTypes = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                    {
                        try { return a.GetTypes(); }
                        catch { return Array.Empty<Type>(); }
                    })
                    .Where(t => !t.IsAbstract && commandBaseType.IsAssignableFrom(t))
                    .OrderBy(t => t.Name)
                    .ToList();

                string? filterLower = filter?.ToLowerInvariant();

                var results = new List<(string name, string? alias, string summary)>();

                foreach (var cmdType in commandTypes)
                {
                    string name = cmdType.Name;

                    // Get command alias from attribute
                    string? alias = null;
                    var aliasAttr = cmdType.GetCustomAttribute<Command.AliasAttribute>();
                    if (aliasAttr != null)
                        alias = aliasAttr.Alias;

                    // Apply filter
                    if (filterLower != null)
                    {
                        bool matches = name.ToLowerInvariant().Contains(filterLower);
                        if (!matches && alias != null)
                            matches = alias.ToLowerInvariant().Contains(filterLower);
                        if (!matches) continue;
                    }

                    // Get summary from Description attribute or XML docs
                    string summary = "";
                    var descAttr = cmdType.GetCustomAttribute<DescriptionAttribute>();
                    if (descAttr != null)
                        summary = descAttr.Description;

                    results.Add((name, alias, summary));
                }

                sb.AppendLine($"  Total: {results.Count} commands{(filterLower != null ? $" matching '{filter}'" : "")}");
                sb.AppendLine();

                foreach (var (name, alias, summary) in results)
                {
                    string aliasStr = alias != null ? $" (@{alias})" : "";
                    sb.AppendLine($"  {name}{aliasStr}");
                    if (!string.IsNullOrEmpty(summary))
                        sb.AppendLine($"    {summary}");
                }

                return sb.ToString();
            });
        }
    }
}
