#if HAS_PEGASUS
#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.Pegasus.Editor
{
    /// <summary>
    /// Pegasus (Procedural Worlds) flythrough tool group, driven by reflection because Pegasus ships
    /// no asmdef and compiles into Assembly-CSharp, which an asmdef cannot reference (Sandbox ENTRY-435).
    /// </summary>
    /// <remarks>
    /// Measured rules this group encodes (ENTRY-435): a scripted PegasusManager needs SetDefaults()
    /// before AddPOI (else NRE); with no m_target, CalculateTargetAtDistance silently returns (0,0,0);
    /// the manager renames itself "Pegasus Manager - &lt;target&gt;" when its inspector is drawn, so
    /// managers are found by type; it drives its target transform directly, so the group gives each
    /// flythrough its own camera instead of the Main Camera (which a CinemachineBrain would fight).
    /// </remarks>
    [AiToolType]
    public partial class Tool_Pegasus
    {
        const string ManagerTypeName = "Pegasus.PegasusManager";
        const BindingFlags Instance = BindingFlags.Public | BindingFlags.Instance;

        static Type ManagerType()
        {
#if UNITY_6000_4_OR_NEWER
            IReadOnlyList<Assembly> assemblies = UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies();
#else
            IReadOnlyList<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
#endif
            for (int i = 0; i < assemblies.Count; i++)
            {
                Type? t = assemblies[i].GetType(ManagerTypeName);
                if (t != null)
                    return t;
            }
            throw new Exception($"Type '{ManagerTypeName}' not found -- is Pegasus installed?");
        }

        static List<Component> AllManagers()
        {
            List<Component> result = new List<Component>();
            foreach (UnityEngine.Object found in UnityEngine.Object.FindObjectsByType(ManagerType(), FindObjectsInactive.Include))
            {
                if (found is Component component)
                    result.Add(component);
            }
            return result;
        }

        /// <summary>Manager by exact name, or by Pegasus's renamed form; the only manager when name is omitted.</summary>
        static Component FindManager(string? name)
        {
            List<Component> managers = AllManagers();
            if (string.IsNullOrWhiteSpace(name))
            {
                if (managers.Count == 1)
                    return managers[0];
                throw new Exception(managers.Count == 0
                    ? "No Pegasus manager in the open scenes. Create one with pegasus-create."
                    : $"{managers.Count} Pegasus managers exist -- pass 'manager'. Use pegasus-query to list them.");
            }

            string wanted = name!.Trim();
            Component? match = null;
            foreach (Component manager in managers)
            {
                string n = manager.gameObject.name;
                if (n != wanted && n != "Pegasus Manager - " + wanted)
                    continue;
                if (match != null)
                    throw new Exception($"Two Pegasus managers match '{wanted}'. Rename one.");
                match = manager;
            }
            if (match == null)
                throw new Exception($"No Pegasus manager named '{wanted}'. Use pegasus-query to list them.");
            return match;
        }

        static object? GetField(object target, string name)
        {
            FieldInfo? field = target.GetType().GetField(name, Instance);
            if (field == null)
                throw new Exception($"Pegasus field '{name}' not found -- API changed?");
            return field.GetValue(target);
        }

        static void SetField(object target, string name, object? value)
        {
            FieldInfo? field = target.GetType().GetField(name, Instance);
            if (field == null)
                throw new Exception($"Pegasus field '{name}' not found -- API changed?");
            field.SetValue(target, value);
        }

        static void SetEnumField(object target, string name, string value)
        {
            FieldInfo? field = target.GetType().GetField(name, Instance);
            if (field == null)
                throw new Exception($"Pegasus field '{name}' not found -- API changed?");
            field.SetValue(target, Enum.Parse(field.FieldType, value, true));
        }

        static object? Call(object target, string method, params object?[] args)
        {
            Type[] types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
                types[i] = args[i]?.GetType() ?? typeof(object);
            MethodInfo? info = target.GetType().GetMethod(method, Instance, null, types, null);
            if (info == null)
                throw new Exception($"Pegasus method '{method}' not found -- API changed?");
            return info.Invoke(target, args);
        }

        /// <summary>Position/rotation at a distance along the path (CalculateTargetAtDistance has out params).</summary>
        static void PoseAtDistance(Component manager, float distance, out Vector3 position, out Quaternion rotation)
        {
            MethodInfo? info = manager.GetType().GetMethod("CalculateTargetAtDistance", Instance);
            if (info == null)
                throw new Exception("Pegasus method 'CalculateTargetAtDistance' not found -- API changed?");
            object?[] args = { distance, null, null, null };
            info.Invoke(manager, args);
            position = (Vector3)args[1]!;
            rotation = (Quaternion)args[2]!;
        }

        static Vector3 ParseVector(string text, string role)
        {
            string[] parts = text.Split(',');
            if (parts.Length != 3
                || !float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
                || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y)
                || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
                throw new Exception($"{role} '{text}' is not 'x,y,z'.");
            return new Vector3(x, y, z);
        }

        /// <summary>Height of the ground under a point (world Y), or null when no terrain covers it.</summary>
        static float? GroundHeight(Vector3 position)
        {
            foreach (Terrain terrain in Terrain.activeTerrains)
            {
                if (terrain == null || terrain.terrainData == null)
                    continue;
                Vector3 origin = terrain.GetPosition();
                Vector3 size = terrain.terrainData.size;
                if (position.x < origin.x || position.z < origin.z || position.x > origin.x + size.x || position.z > origin.z + size.z)
                    continue;
                return terrain.SampleHeight(position) + origin.y;
            }
            return null;
        }
    }
}
#endif
