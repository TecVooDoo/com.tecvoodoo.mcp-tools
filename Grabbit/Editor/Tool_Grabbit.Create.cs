#if HAS_GRABBIT
#nullable enable
using System;
using System.ComponentModel;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using Grabbit2;
using UnityEngine;

namespace MCPTools.Grabbit.Editor
{
    public partial class Tool_Grabbit
    {
        [AiTool("grabbit-create", Title = "Grabbit / Create Around")]
        [Description(@"Spawns duplicates of template objects and physically settles them, using Grabbit 2's Create mode (e.g. 'scatter 10 rocks around the well').
'templates' = the object(s) to duplicate (names / hierarchy paths; omit for the editor selection). 'count' = how many to spawn.
Surround EITHER an 'around' list of objects (spread = 'padding') OR a 'center' point (spread = 'radius'). 'height' = drop height above them.
Returns each spawned object's final position + rotation.")]
        public string Create(
            [Description("Number of objects to spawn (>= 1).")] int count,
            [Description("Template GameObject names / hierarchy paths. Omit to use the editor selection.")] string[]? templates = null,
            [Description("Objects to surround. Takes precedence over 'center'.")] string[]? around = null,
            [Description("Centre X (used when 'around' is omitted).")] float? centerX = null,
            [Description("Centre Y (used when 'around' is omitted).")] float? centerY = null,
            [Description("Centre Z (used when 'around' is omitted).")] float? centerZ = null,
            [Description("Spread beyond the 'around' objects' bounds. Default 1.")] float padding = 1f,
            [Description("Spread radius around 'center'. Default 2.")] float radius = 2f,
            [Description("Drop height. Default 3.")] float height = 3f,
            [Description("Simulation step cap. Omit for Grabbit's default (1500).")] int? maxSteps = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (count < 1)
                    throw new Exception("'count' must be at least 1.");

                var templateObjects = ResolveTargets(templates, "template");
                var options = MakeOptions(maxSteps);

                if (around != null && around.Length > 0)
                {
                    var aroundObjects = ResolveTargets(around, "'around' object");
                    return FormatRun(GrabbitOps.CreateAroundObjects(templateObjects, count, aroundObjects, padding, height, options));
                }

                if (centerX.HasValue && centerY.HasValue && centerZ.HasValue)
                {
                    var center = new Vector3(centerX.Value, centerY.Value, centerZ.Value);
                    return FormatRun(GrabbitOps.CreateAround(templateObjects, count, center, radius, height, options));
                }

                throw new Exception("Provide either 'around' (objects to surround) or all of centerX / centerY / centerZ.");
            });
        }
    }
}
#endif
