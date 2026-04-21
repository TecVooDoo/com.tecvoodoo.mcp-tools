#if HAS_ULIPSYNC
#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.uLipSync.Editor
{
    [McpPluginToolType]
    public partial class Tool_uLipSync
    {
        static readonly Type? ULipSyncType           = FindType("uLipSync.uLipSync");
        static readonly Type? BlendShapeType          = FindType("uLipSync.uLipSyncBlendShape");
        static readonly Type? BakedDataPlayerType     = FindType("uLipSync.uLipSyncBakedDataPlayer");
        static readonly Type? TimelineEventType       = FindType("uLipSync.Timeline.uLipSyncTimelineEvent");
        static readonly Type? TextureType             = FindType("uLipSync.uLipSyncTexture");
        static readonly Type? AnimatorType            = FindType("uLipSync.uLipSyncAnimator");
        static readonly Type? ProfileType             = FindType("uLipSync.Profile");
        static readonly Type? BakedDataType           = FindType("uLipSync.BakedData");
        static readonly Type? MfccDataType            = FindType("uLipSync.MfccData");

        static Type? FindType(string name)
        {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type? t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        static UnityEngine.Component? GetComponent(GameObject go, Type? type, string typeName)
        {
            if (type == null)
                throw new Exception($"{typeName} type not found in loaded assemblies.");
            return go.GetComponent(type);
        }

        static UnityEngine.Component GetRequiredComponent(GameObject go, Type? type, string typeName)
        {
            UnityEngine.Component? comp = GetComponent(go, type, typeName);
            if (comp == null)
                throw new Exception($"'{go.name}' has no {typeName} component.");
            return comp;
        }

        static object? Get(object target, string name)
        {
            Type type = target.GetType();
            PropertyInfo? prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(target);
            FieldInfo? field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) return field.GetValue(target);
            return null;
        }

        static bool Set(object target, string name, object value)
        {
            Type type = target.GetType();
            FieldInfo? field = type.GetField(name, BindingFlags.Public | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, Convert.ChangeType(value, field.FieldType)); return true; }
            PropertyInfo? prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, Convert.ChangeType(value, prop.PropertyType)); return true; }
            return false;
        }

        static List<UnityEngine.Component> FindComponentsInHierarchy(GameObject root, Type? type)
        {
            List<UnityEngine.Component> results = new List<UnityEngine.Component>();
            if (type == null) return results;
            UnityEngine.Component[] found = root.GetComponentsInChildren(type, true);
            results.AddRange(found);
            return results;
        }
    }
}
#endif
