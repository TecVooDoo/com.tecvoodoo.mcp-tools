#nullable enable
using System;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    [McpPluginToolType]
    public partial class Tool_Chunity
    {
        const string CHUCK_MAIN_INSTANCE_TYPE_NAME = "ChuckMainInstance";
        const string CHUCK_TYPE_NAME               = "Chuck";

        static Type? _chuckInstTypeCached;
        static Type ChuckMainInstanceType
        {
            get
            {
                if (_chuckInstTypeCached == null)
                    _chuckInstTypeCached = FindType(CHUCK_MAIN_INSTANCE_TYPE_NAME);
                if (_chuckInstTypeCached == null)
                    throw new InvalidOperationException($"Type '{CHUCK_MAIN_INSTANCE_TYPE_NAME}' not found. Is Chunity installed?");
                return _chuckInstTypeCached;
            }
        }

        static Type? FindType(string fullTypeName)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? type = asm.GetType(fullTypeName);
                if (type != null) return type;
            }
            return null;
        }

        static object? Get(object target, string name)
        {
            Type type = target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            PropertyInfo? prop = type.GetProperty(name, flags);
            if (prop != null) return prop.GetValue(target);
            FieldInfo? field = type.GetField(name, flags);
            if (field != null) return field.GetValue(target);
            return null;
        }

        static object? GetStatic(Type type, string name)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            PropertyInfo? prop = type.GetProperty(name, flags);
            if (prop != null) return prop.GetValue(null);
            FieldInfo? field = type.GetField(name, flags);
            if (field != null) return field.GetValue(null);
            return null;
        }

        static object? Call(object target, string methodName, params object[] args)
        {
            Type type = target.GetType();
            Type[] argTypes = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
                argTypes[i] = args[i].GetType();
            MethodInfo? method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, null, argTypes, null);
            if (method == null)
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
                throw new Exception($"Method '{methodName}' not found on {type.Name}.");
            return method.Invoke(target, args);
        }

        static object? CallStatic(Type type, string methodName, params object[] args)
        {
            Type[] argTypes = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
                argTypes[i] = args[i].GetType();

            MethodInfo? method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static, null, argTypes, null);
            if (method == null)
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
            if (method == null)
                throw new Exception($"Static method '{methodName}' not found on {type.Name}.");
            return method.Invoke(null, args);
        }

        /// <summary>
        /// Find all ChuckMainInstance components in scene via reflection.
        /// Returns Component[] (typed as Array).
        /// </summary>
        private static Array FindAllInstances()
        {
            var findMethod = typeof(UnityEngine.Object).GetMethod(
                "FindObjectsByType",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(FindObjectsSortMode) },
                null
            );
            if (findMethod != null)
            {
                var generic = findMethod.MakeGenericMethod(ChuckMainInstanceType);
                return (Array)generic.Invoke(null, new object[] { FindObjectsSortMode.None })!;
            }

            // Fallback: FindObjectsOfType
            var fallback = typeof(UnityEngine.Object).GetMethod(
                "FindObjectsOfType",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(Type) },
                null
            );
            return (Array)fallback!.Invoke(null, new object[] { ChuckMainInstanceType })!;
        }

        /// <summary>
        /// Find a specific ChuckMainInstance by GameObject name.
        /// </summary>
        private static Component? FindInstance(string name)
        {
            var all = FindAllInstances();
            foreach (var obj in all)
            {
                var comp = obj as Component;
                if (comp != null && comp.gameObject.name == name)
                    return comp;
            }
            return null;
        }

        [McpPluginTool("chuck-query", Title = "Chunity / Query Instances")]
        [Description(@"Lists all ChuckMainInstance components in the scene.
Reports the GameObject name and whether each instance is active.
Use the GameObject name as 'instanceName' in other chuck-* tools.")]
        public string Query()
        {
            return MainThread.Instance.Run(() =>
            {
                var instances = FindAllInstances();

                StringBuilder sb = new StringBuilder();
                sb.AppendLine("=== Chunity Instances ===");
                sb.AppendLine($"  ChuckMainInstance count: {instances.Length}");

                for (int i = 0; i < instances.Length; i++)
                {
                    var comp = (Component)instances.GetValue(i)!;
                    sb.AppendLine($"  [{i}] \"{comp.gameObject.name}\"  Active: {comp.gameObject.activeInHierarchy}");
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
                var inst = FindInstance(instanceName);
                if (inst == null)
                    return $"ERROR: ChuckMainInstance '{instanceName}' not found. Use chuck-query to list instances.";

                bool success = (bool)(Call(inst, "RunCode", code) ?? false);
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
                var inst = FindInstance(instanceName);
                if (inst == null)
                    return $"ERROR: ChuckMainInstance '{instanceName}' not found.";

                // Chuck.Manager.SetFloat(instanceName, variableName, value)
                var chuckType = FindType(CHUCK_TYPE_NAME);
                if (chuckType == null)
                    return "ERROR: Chuck type not found.";

                var manager = GetStatic(chuckType, "Manager");
                if (manager == null)
                    return "ERROR: Chuck.Manager is null.";

                bool success = (bool)(Call(manager, "SetFloat", instanceName, variableName, value) ?? false);
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
                var inst = FindInstance(instanceName);
                if (inst == null)
                    return $"ERROR: ChuckMainInstance '{instanceName}' not found.";

                var chuckType = FindType(CHUCK_TYPE_NAME);
                if (chuckType == null)
                    return "ERROR: Chuck type not found.";

                var manager = GetStatic(chuckType, "Manager");
                if (manager == null)
                    return "ERROR: Chuck.Manager is null.";

                bool success = (bool)(Call(manager, "SetInt", instanceName, variableName, value) ?? false);
                if (success)
                    return $"OK: '{instanceName}'.{variableName} = {value}";
                else
                    return $"ERROR: SetInt failed. Ensure '{variableName}' is declared as 'global int {variableName};' in running ChucK code.";
            });
        }
    }
}
