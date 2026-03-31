#nullable enable
using System.ComponentModel;
using System.Globalization;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    [McpPluginToolType]
    public partial class Tool_FMOD
    {
        [McpPluginTool("fmod-query", Title = "FMOD / Query State")]
        [Description(@"Returns FMOD Studio runtime state: initialization status, bank load status, mute state.
Lists all loaded banks (Master bank is always loaded).
Use bank paths from the FMOD Studio project (e.g. 'Master', 'Music', 'SFX').")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                if (!RuntimeManager.IsInitialized)
                    return "ERROR: FMOD RuntimeManager not initialized. Ensure FMOD Studio is configured.";

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== FMOD Studio State ===");
                sb.AppendLine($"  Initialized:         {RuntimeManager.IsInitialized}");
                sb.AppendLine($"  AllBanksLoaded:      {RuntimeManager.HaveAllBanksLoaded}");
                sb.AppendLine($"  MasterBanksLoaded:   {RuntimeManager.HaveMasterBanksLoaded}");
                sb.AppendLine($"  IsMuted:             {RuntimeManager.IsMuted}");
                sb.AppendLine($"  AnyBankLoading:      {RuntimeManager.AnyBankLoading()}");

                return sb.ToString();
            });
        }

        [McpPluginTool("fmod-play", Title = "FMOD / Play One Shot")]
        [Description(@"Plays an FMOD Studio event as a one-shot (fire and forget).
eventPath: FMOD Studio event path, e.g. 'event:/Music/MainTheme' or 'event:/SFX/Explosion'.
Optionally provide position as 'x,y,z' for 3D spatialized playback.")]
        public string PlayOneShot(
            [Description("FMOD Studio event path, e.g. 'event:/Music/MainTheme'.")]
            string eventPath,
            [Description("World position as 'x,y,z' for 3D audio. Omit for 2D.")]
            string? position = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!RuntimeManager.IsInitialized)
                    return "ERROR: FMOD RuntimeManager not initialized.";

                Vector3 pos = Vector3.zero;
                if (!string.IsNullOrEmpty(position))
                {
                    string[] parts = position!.Split(',');
                    if (parts.Length != 3)
                        throw new System.ArgumentException("Position must be 'x,y,z'.", nameof(position));
                    pos = new Vector3(
                        float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture),
                        float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture));
                }

                RuntimeManager.PlayOneShot(eventPath, pos);
                return $"OK: PlayOneShot '{eventPath}' at ({pos.x:F1},{pos.y:F1},{pos.z:F1})";
            });
        }

        [McpPluginTool("fmod-parameter", Title = "FMOD / Set Global Parameter")]
        [Description(@"Sets a global FMOD Studio parameter by name.
Global parameters affect all event instances. Use this to drive music layers, intensity, wetness, etc.
parameterName must match the global parameter name in FMOD Studio exactly.")]
        public string SetGlobalParameter(
            [Description("Global parameter name as defined in FMOD Studio.")]
            string parameterName,
            [Description("Parameter value to set.")]
            float value,
            [Description("Ignore seek speed (apply immediately). Default true.")]
            bool ignoreSeekSpeed = true
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!RuntimeManager.IsInitialized)
                    return "ERROR: FMOD RuntimeManager not initialized.";

                FMOD.RESULT result = RuntimeManager.StudioSystem.setParameterByName(
                    parameterName, value, ignoreSeekSpeed);

                if (result == FMOD.RESULT.OK)
                    return $"OK: Global parameter '{parameterName}' = {value:F4}";
                else
                    return $"ERROR: setParameterByName failed: {result}. Check parameter name exists as a global parameter.";
            });
        }

        [McpPluginTool("fmod-vca", Title = "FMOD / Set VCA Volume")]
        [Description(@"Sets the volume of an FMOD Studio VCA (Voltage Controlled Amplifier).
VCAs group buses for high-level volume control (e.g. 'vca:/Master', 'vca:/Music', 'vca:/SFX').
Volume is 0.0 to 1.0.")]
        public string SetVCAVolume(
            [Description("VCA path, e.g. 'vca:/Master' or 'vca:/Music'.")]
            string vcaPath,
            [Description("Volume 0.0 to 1.0.")]
            float volume
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!RuntimeManager.IsInitialized)
                    return "ERROR: FMOD RuntimeManager not initialized.";

                VCA vca = RuntimeManager.GetVCA(vcaPath);
                vca.getVolume(out float current);
                FMOD.RESULT result = vca.setVolume(Mathf.Clamp01(volume));

                if (result == FMOD.RESULT.OK)
                    return $"OK: VCA '{vcaPath}' volume {current:F2} -> {volume:F2}";
                else
                    return $"ERROR: setVolume failed: {result}. Check VCA path is correct.";
            });
        }

        [McpPluginTool("fmod-bus", Title = "FMOD / Set Bus Volume")]
        [Description(@"Sets the fader level of an FMOD Studio Bus.
Bus paths follow FMOD Studio routing: 'bus:/' for master bus, 'bus:/Music' for music bus.
Volume is 0.0 to 1.0.")]
        public string SetBusVolume(
            [Description("Bus path, e.g. 'bus:/' or 'bus:/Music'.")]
            string busPath,
            [Description("Fader level 0.0 to 1.0.")]
            float volume
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (!RuntimeManager.IsInitialized)
                    return "ERROR: FMOD RuntimeManager not initialized.";

                Bus bus = RuntimeManager.GetBus(busPath);
                bus.getVolume(out float current);
                FMOD.RESULT result = bus.setVolume(Mathf.Clamp01(volume));

                if (result == FMOD.RESULT.OK)
                    return $"OK: Bus '{busPath}' volume {current:F2} -> {volume:F2}";
                else
                    return $"ERROR: setVolume failed: {result}. Check bus path is correct.";
            });
        }
    }
}
