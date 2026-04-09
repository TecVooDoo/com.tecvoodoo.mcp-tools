#nullable enable
using System;
using System.Collections;
using System.Reflection;
using com.IvanMurzak.McpPlugin;

namespace MCPTools.QuestForge.Editor
{
    [McpPluginToolType]
    public partial class Tool_QuestForge
    {
        const string QUEST_TYPE_NAME              = "MalbersAnimations.QuestForge.Quest";
        const string QUEST_MANAGER_TYPE_NAME      = "MalbersAnimations.QuestForge.QuestManager";
        const string QUEST_OBJECTIVE_TYPE_NAME    = "MalbersAnimations.QuestForge.QuestObjective";
        const string KILL_OBJECTIVE_TYPE_NAME     = "MalbersAnimations.QuestForge.KillObjective";
        const string COLLECT_OBJECTIVE_TYPE_NAME  = "MalbersAnimations.QuestForge.CollectObjective";
        const string TALKTO_OBJECTIVE_TYPE_NAME   = "MalbersAnimations.QuestForge.TalkToObjective";
        const string GOTO_OBJECTIVE_TYPE_NAME     = "MalbersAnimations.QuestForge.GoToLocationObjective";
        const string INTERACT_OBJECTIVE_TYPE_NAME = "MalbersAnimations.QuestForge.InteractObjective";
        const string POI_TYPE_NAME                = "MalbersAnimations.QuestForge.PointOfInterest";
        const string POI_CATEGORY_TYPE_NAME       = "MalbersAnimations.QuestForge.POICategory";

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

        static bool Set(object target, string name, object? value)
        {
            Type type = target.GetType();
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            PropertyInfo? prop = type.GetProperty(name, flags);
            if (prop != null && prop.CanWrite) { prop.SetValue(target, value); return true; }
            FieldInfo? field = type.GetField(name, flags);
            if (field != null) { field.SetValue(target, value); return true; }
            return false;
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
    }
}
