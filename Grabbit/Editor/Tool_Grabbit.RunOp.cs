#if HAS_GRABBIT
#nullable enable
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Grabbit2;

namespace MCPTools.Grabbit.Editor
{
    public partial class Tool_Grabbit
    {
        [AiTool("grabbit-run-op", Title = "Grabbit / Run Operation")]
        [Description(@"Runs a Grabbit 2 physics operation headlessly until the targets settle -- physics-accurate placement, no play mode.
mode: place | arrange | scatter.
operation: place = drop / move; arrange = align / distribute / level / orient; scatter = randomize / explode / cramp / nudge.
Box / Point / Grab are interactive cursor ops and are NOT available headlessly.
Acts on 'targets' (GameObject names or hierarchy paths), or the current editor selection when omitted.
Returns each target's final position + rotation. Pass action='info' to list modes and operations (read-only).")]
        public string RunOp(
            [Description("'run' (default) or 'info'.")] string? action = null,
            [Description("place | arrange | scatter. Required for 'run'.")] string? mode = null,
            [Description("Operation token for the mode. Omit for the mode default (place=drop, arrange=align, scatter=randomize).")] string? operation = null,
            [Description("GameObject names or hierarchy paths. Omit to use the editor selection.")] string[]? targets = null,
            [Description("Simulation step cap. Omit for Grabbit's default (1500).")] int? maxSteps = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                string a = string.IsNullOrWhiteSpace(action) ? "run" : action!.Trim().ToLowerInvariant();
                if (a == "info")
                {
                    return "Grabbit headless operations:\n" +
                           $"  modes:   {string.Join(", ", GrabbitOps.Modes)}\n" +
                           $"  place:   {string.Join(", ", GrabbitOps.PlaceOps)}\n" +
                           $"  arrange: {string.Join(", ", GrabbitOps.ArrangeOps)} (distribute = align)\n" +
                           $"  scatter: {string.Join(", ", GrabbitOps.ScatterOps)}";
                }
                if (a != "run")
                    throw new System.Exception($"Unknown action '{action}'. Use: run, info.");
                if (string.IsNullOrWhiteSpace(mode) || System.Array.IndexOf(GrabbitOps.Modes, mode!.Trim().ToLowerInvariant()) < 0)
                    throw new System.Exception($"'mode' must be one of: {string.Join(", ", GrabbitOps.Modes)} (got '{mode}').");

                var resolved = ResolveTargets(targets, "target");
                return FormatRun(GrabbitOps.Run(mode!, operation ?? string.Empty, resolved, MakeOptions(maxSteps)));
            });
        }
    }
}
#endif
