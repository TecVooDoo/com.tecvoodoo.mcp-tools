#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.DialogueSystem.Editor
{
    [McpPluginToolType]
    public partial class Tool_DialogueSystem
    {
        // --- Cached types ---
        static Type? _dmType;
        static Type? DmType => _dmType ??= FindType("PixelCrushers.DialogueSystem.DialogueManager");

        static Type? _luaType;
        static Type? LuaType => _luaType ??= FindType("PixelCrushers.DialogueSystem.Lua");

        static Type? _questLogType;
        static Type? QuestLogType => _questLogType ??= FindType("PixelCrushers.DialogueSystem.QuestLog");

        static Type? _questStateType;
        static Type? QuestStateType => _questStateType ??= FindType("PixelCrushers.DialogueSystem.QuestState");

        static Type? _dialogueDatabaseType;
        static Type? DialogueDatabaseType => _dialogueDatabaseType ??= FindType("PixelCrushers.DialogueSystem.DialogueDatabase");

        // --- Reflection helpers ---

        static Type? FindType(string name)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(name);
                if (t != null) return t;
            }
            return null;
        }

        static object? Get(object target, string name)
        {
            if (target == null) return null;
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null) return prop.GetValue(target);
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.GetValue(target);
        }

        static bool Set(object target, string name, object value)
        {
            if (target == null) return false;
            var type = target.GetType();
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop != null) { prop.SetValue(target, value); return true; }
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null) { field.SetValue(target, value); return true; }
            return false;
        }

        static object? GetStatic(Type? type, string name)
        {
            if (type == null) return null;
            var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (prop != null) return prop.GetValue(null);
            var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return field?.GetValue(null);
        }

        static object? Call(object target, string methodName, params object[] args)
        {
            if (target == null) return null;
            var type = target.GetType();
            var types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
                types[i] = args[i]?.GetType() ?? typeof(object);
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);
            if (method == null)
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return method?.Invoke(target, args);
        }

        static object? CallStatic(Type? type, string methodName, params object[] args)
        {
            if (type == null) return null;
            var types = new Type[args.Length];
            for (int i = 0; i < args.Length; i++)
                types[i] = args[i]?.GetType() ?? typeof(object);
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, types, null);
            if (method == null)
                method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            return method?.Invoke(null, args);
        }

        static object? CallStaticExplicit(Type? type, string methodName, Type[] paramTypes, params object[] args)
        {
            if (type == null) return null;
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, paramTypes, null);
            return method?.Invoke(null, args);
        }

        // --- Scene helpers ---

        static GameObject? FindGameObjectByName(string? name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            GameObject go = GameObject.Find(name);
            return go;
        }

        static Transform? FindTransformByName(string? name)
        {
            GameObject? go = FindGameObjectByName(name);
            return go != null ? go.transform : null;
        }

        // --- DS convenience helpers ---

        static bool HasDialogueManager()
        {
            if (DmType == null) return false;
            var result = GetStatic(DmType, "hasInstance");
            return result is true;
        }

        static object? GetMasterDatabase()
        {
            return GetStatic(DmType, "masterDatabase");
        }

        static object? ParseQuestStateEnum(string stateStr)
        {
            if (QuestStateType == null) return null;
            string lower = stateStr.ToLowerInvariant();
            string enumName = lower switch
            {
                "unassigned" => "Unassigned",
                "active" => "Active",
                "success" => "Success",
                "failure" => "Failure",
                _ => "Unassigned"
            };
            return Enum.Parse(QuestStateType, enumName);
        }

        /// <summary>
        /// Combines all QuestState flags for listing all quests.
        /// </summary>
        static object? AllQuestStateFlags()
        {
            if (QuestStateType == null) return null;
            int combined = (int)Enum.Parse(QuestStateType, "Unassigned")
                         | (int)Enum.Parse(QuestStateType, "Active")
                         | (int)Enum.Parse(QuestStateType, "Success")
                         | (int)Enum.Parse(QuestStateType, "Failure");
            return Enum.ToObject(QuestStateType, combined);
        }
    }
}
