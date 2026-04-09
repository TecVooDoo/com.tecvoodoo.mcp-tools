#if HAS_LUMEN
#nullable enable
using System.Collections;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace MCPTools.Lumen.Editor
{
    public partial class Tool_Lumen
    {
        [McpPluginTool("lumen-query", Title = "Lumen / Query Effect Player")]
        [Description(@"Reports the LumenEffectPlayer configuration on a GameObject.
Returns: scale, brightness, color, range, update frequency, fading time,
initialization/deinitialization behaviors, profile name, and layer count.
Use to inspect current Lumen light effect setup before configuring.")]
        public string QueryLumen(
            [Description("Name of the GameObject with LumenEffectPlayer component.")]
            string gameObjectName
        )
        {
            return MainThread.Instance.Run(() =>
            {
                UnityEngine.Component player = GetPlayer(gameObjectName);
                StringBuilder sb = new StringBuilder();

                sb.AppendLine($"=== LumenEffectPlayer on '{gameObjectName}' ===");
                sb.AppendLine($"  scale:                    {Get(player, "scale")}");
                sb.AppendLine($"  brightness:               {Get(player, "brightness")}");
                sb.AppendLine($"  color:                    {FormatColor(Get(player, "color"))}");
                sb.AppendLine($"  range:                    {Get(player, "range")}");
                sb.AppendLine($"  updateFrequency:          {Get(player, "updateFrequency")}");
                sb.AppendLine($"  fadingTime:               {Get(player, "fadingTime")}");
                sb.AppendLine($"  initializationBehavior:   {Get(player, "initializationBehavior")}");
                sb.AppendLine($"  deinitializationBehavior: {Get(player, "deinitializationBehavior")}");
                sb.AppendLine($"  autoAssignSun:            {Get(player, "autoAssignSun")}");
                sb.AppendLine($"  useLumenSunScript:        {Get(player, "useLumenSunScript")}");

                object? profile = Get(player, "profile");
                if (profile is ScriptableObject so)
                    sb.AppendLine($"  profile:                  {so.name}");
                else
                    sb.AppendLine($"  profile:                  (none)");

                object? layers = Get(player, "instantiatedLumenLayers");
                int layerCount = 0;
                if (layers is ICollection col)
                    layerCount = col.Count;
                sb.AppendLine($"  instantiatedLayers:       {layerCount}");

                return sb.ToString();
            });
        }
    }
}
#endif
