#if HAS_CHUNITY
#nullable enable
using System;
using System.ComponentModel;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    [McpPluginToolType]
    public partial class Tool_Chunity
    {
        [McpPluginTool("chuck-query", Title = "Chunity / Query Instances")]
        [Description(@"Lists all ChuckMainInstance components in the scene.
Reports the GameObject name and whether each instance is active.
Use the GameObject name as 'instanceName' in other chuck-* tools.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                ChuckMainInstance[] instances =
                    UnityEngine.Object.FindObjectsByType<ChuckMainInstance>(FindObjectsSortMode.None);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== Chunity Instances ===");
                sb.AppendLine($"  ChuckMainInstance count: {instances.Length}");

                for (int i = 0; i < instances.Length; i++)
                {
                    ChuckMainInstance inst = instances[i];
                    sb.AppendLine($"  [{i}] \"{inst.gameObject.name}\"  Active: {inst.gameObject.activeInHierarchy}");
                }

                if (instances.Length == 0)
                    sb.AppendLine("  (no ChuckMainInstance found -- add one to the scene)");

                return sb.ToString();
            });
        }

        [McpPluginTool("chuck-run", Title = "Chunity / Run ChucK Code")]
        [Description(@"Runs a ChucK code string on a named ChuckMainInstance.
instanceName: the GameObject name of the ChuckMainInstance.
code: valid ChucK DSP code. The code runs as a new shred on the ChucK VM.
Example code: 'SinOsc s => dac; 440 => s.freq; 1 => s.gain; 2::second => now;'")]
        public string RunCode(
            [Description("GameObject name of the ChuckMainInstance.")]
            string instanceName,
            [Description("ChucK code string to execute.")]
            string code
        )
        {
            return MainThread.Instance.Run(() =>
            {
                ChuckMainInstance? inst = FindInstance(instanceName);
                if (inst == null)
                    return $"ERROR: ChuckMainInstance '{instanceName}' not found. Use chuck-query to list instances.";

                bool success = inst.RunCode(code);
                if (success)
                    return $"OK: ChucK code submitted to '{instanceName}'";
                else
                    return $"ERROR: RunCode returned false for '{instanceName}'. Check ChucK code syntax.";
            });
        }

        [McpPluginTool("chuck-set-float", Title = "Chunity / Set Global Float")]
        [Description(@"Sets a global float variable in a ChuckMainInstance's VM.
The variable must be declared globally in the ChucK code as 'global float myVar;'.
This allows real-time parameter control of running ChucK shreds.")]
        public string SetFloat(
            [Description("GameObject name of the ChuckMainInstance.")]
            string instanceName,
            [Description("Name of the global float variable declared in ChucK code.")]
            string variableName,
            [Description("Value to set.")]
            double value
        )
        {
            return MainThread.Instance.Run(() =>
            {
                ChuckMainInstance? inst = FindInstance(instanceName);
                if (inst == null)
                    return $"ERROR: ChuckMainInstance '{instanceName}' not found.";

                bool success = Chuck.Manager.SetFloat(instanceName, variableName, value);
                if (success)
                    return $"OK: '{instanceName}'.{variableName} = {value:F4}";
                else
                    return $"ERROR: SetFloat failed. Ensure '{variableName}' is declared as 'global float {variableName};' in running ChucK code.";
            });
        }

        [McpPluginTool("chuck-set-int", Title = "Chunity / Set Global Int")]
        [Description(@"Sets a global int variable in a ChuckMainInstance's VM.
The variable must be declared globally in the ChucK code as 'global int myVar;'.")]
        public string SetInt(
            [Description("GameObject name of the ChuckMainInstance.")]
            string instanceName,
            [Description("Name of the global int variable declared in ChucK code.")]
            string variableName,
            [Description("Integer value to set.")]
            long value
        )
        {
            return MainThread.Instance.Run(() =>
            {
                ChuckMainInstance? inst = FindInstance(instanceName);
                if (inst == null)
                    return $"ERROR: ChuckMainInstance '{instanceName}' not found.";

                bool success = Chuck.Manager.SetInt(instanceName, variableName, value);
                if (success)
                    return $"OK: '{instanceName}'.{variableName} = {value}";
                else
                    return $"ERROR: SetInt failed. Ensure '{variableName}' is declared as 'global int {variableName};' in running ChucK code.";
            });
        }

        private static ChuckMainInstance? FindInstance(string name)
        {
            ChuckMainInstance[] all =
                UnityEngine.Object.FindObjectsByType<ChuckMainInstance>(FindObjectsSortMode.None);
            foreach (ChuckMainInstance inst in all)
            {
                if (inst.gameObject.name == name)
                    return inst;
            }
            return null;
        }
    }
}
#endif
