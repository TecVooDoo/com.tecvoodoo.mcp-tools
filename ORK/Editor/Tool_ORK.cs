#nullable enable
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace MCPTools.ORK.Editor
{
    [McpPluginToolType]
    public partial class Tool_ORK
    {
        // Resolve key types lazily via reflection. ORK + Makinom ship as DLLs (with .pdb) under Assets/Gaming Is Love/.
        // Type names from source inspection:
        //   GamingIsLove.ORKFramework.ORK              (static facade -- ORK.Game, ORK.Battle, ORK.Combatants, etc.)
        //   GamingIsLove.ORKFramework.Combatant
        //   GamingIsLove.ORKFramework.Group
        //   GamingIsLove.ORKFramework.StatusValue
        //   GamingIsLove.ORKFramework.Inventory
        //   GamingIsLove.ORKFramework.QuestSetting / QuestStatusType
        //   GamingIsLove.Makinom.Maki                  (Makinom static facade)
        //   GamingIsLove.Makinom.MakinomSchematicAsset (schematic runner asset)

        static readonly Type? ORKType                 = FindType("GamingIsLove.ORKFramework.ORK");
        static readonly Type? CombatantType           = FindType("GamingIsLove.ORKFramework.Combatant");
        static readonly Type? GroupType               = FindType("GamingIsLove.ORKFramework.Group");
        static readonly Type? StatusValueType         = FindType("GamingIsLove.ORKFramework.StatusValue");
        static readonly Type? StatusValueSettingType  = FindType("GamingIsLove.ORKFramework.StatusValueSetting");
        static readonly Type? QuestSettingType        = FindType("GamingIsLove.ORKFramework.QuestSetting");
        static readonly Type? QuestStatusTypeType     = FindType("GamingIsLove.ORKFramework.QuestStatusType");
        static readonly Type? ItemType                = FindType("GamingIsLove.ORKFramework.Item");
        static readonly Type? MakiType                = FindType("GamingIsLove.Makinom.Maki");
        static readonly Type? MakinomSchematicAssetType = FindType("GamingIsLove.Makinom.MakinomSchematicAsset");

        static Type? FindType(string fullName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = asm.GetType(fullName);
                if (t != null) return t;
            }
            return null;
        }

        static void RequireORK()
        {
            if (ORKType == null)
                throw new Exception("GamingIsLove.ORKFramework.ORK type not found. Is ORK Framework installed?");
        }

        static object? GetStatic(Type type, string memberName)
        {
            var prop = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Static);
            if (prop != null) return prop.GetValue(null);
            var field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Static);
            if (field != null) return field.GetValue(null);
            return null;
        }

        static object? Get(object? target, string memberName)
        {
            if (target == null) return null;
            var type = target.GetType();
            var prop = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (prop != null) return prop.GetValue(target);
            var field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (field != null) return field.GetValue(target);
            return null;
        }

        static object? Call(object? target, string methodName, params object[] args)
        {
            if (target == null) return null;
            var argTypes = args.Select(a => a?.GetType() ?? typeof(object)).ToArray();
            var method = target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy, null, argTypes, null)
                         ?? target.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (method == null) return null;
            return method.Invoke(target, args);
        }

        static object? CallStatic(Type type, string methodName, params object[] args)
        {
            var argTypes = args.Select(a => a?.GetType() ?? typeof(object)).ToArray();
            var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy, null, argTypes, null)
                         ?? type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (method == null) return null;
            return method.Invoke(null, args);
        }

        // === ORK static accessors ===
        static object? GameHandler()    => ORKType != null ? GetStatic(ORKType, "Game") : null;
        static object? BattleHandler()  => ORKType != null ? GetStatic(ORKType, "Battle") : null;
        static object? Combatants()     => ORKType != null ? GetStatic(ORKType, "Combatants") : null;  // CombatantsSettings (DB)
        static object? Items()       => ORKType != null ? GetStatic(ORKType, "Items") : null;          // ItemsSettings (DB)
        static object? Abilities()   => ORKType != null ? GetStatic(ORKType, "Abilities") : null;
        static object? Classes()     => ORKType != null ? GetStatic(ORKType, "Classes") : null;
        static object? QuestsDB()    => ORKType != null ? GetStatic(ORKType, "Quests") : null;         // QuestsSettings
        static object? Equipment()   => ORKType != null ? GetStatic(ORKType, "Equipment") : null;
        static object? StatusValuesDB() => ORKType != null ? GetStatic(ORKType, "StatusValues") : null;

        // ORKGameHandler accessors
        static object? PlayerHandler() => Get(GameHandler(), "PlayerHandler");
        static object? ActiveGroup()   => Get(GameHandler(), "ActiveGroup");
        static object? QuestHandler()  => Get(GameHandler(), "Quests");
        static object? RuntimeCombatants() => Get(GameHandler(), "Combatants");

        // === Helpers ===

        /// <summary>
        /// List all combatants in the player's active group as runtime Combatant objects.
        /// </summary>
        static System.Collections.Generic.List<object> GetActiveGroupCombatants()
        {
            var list = new System.Collections.Generic.List<object>();
            var group = ActiveGroup();
            if (group == null) return list;
            var members = Call(group, "GetGroup");  // List<Combatant>
            if (members is IList ilist)
            {
                foreach (var m in ilist) if (m != null) list.Add(m);
            }
            return list;
        }

        /// <summary>
        /// Find a runtime Combatant by name (case-insensitive) within active group, falling back to scene combatants.
        /// </summary>
        static object? FindCombatantByName(string name)
        {
            foreach (var c in GetActiveGroupCombatants())
            {
                var cName = Call(c, "GetName") as string ?? Get(c, "GetName") as string ?? c.ToString();
                if (string.Equals(cName, name, StringComparison.OrdinalIgnoreCase))
                    return c;
            }
            // Try CombatantHandler.GetInBattle()
            var ch = RuntimeCombatants();
            var battleList = ch != null ? Call(ch, "GetInBattle") : null;
            if (battleList is IList iblist)
            {
                foreach (var c in iblist)
                {
                    if (c == null) continue;
                    var cName = Call(c, "GetName") as string ?? c.ToString();
                    if (string.Equals(cName, name, StringComparison.OrdinalIgnoreCase))
                        return c;
                }
            }
            return null;
        }

        /// <summary>
        /// Resolve a settings entry (Combatant/Item/Quest/etc.) by name from a Settings DB.
        /// Most ORK *Settings classes expose `Get(int id)` and a `Settings` array with ID + RenameableName.
        /// </summary>
        static object? FindSettingByName(object? settingsDB, string name)
        {
            if (settingsDB == null) return null;
            var settingsArr = Get(settingsDB, "Settings") as IList;
            if (settingsArr == null) return null;
            foreach (var s in settingsArr)
            {
                if (s == null) continue;
                var n = Call(s, "GetName") as string ?? Get(s, "RenameableName") as string ?? Get(s, "Name") as string ?? s.ToString();
                if (string.Equals(n, name, StringComparison.OrdinalIgnoreCase)) return s;
            }
            return null;
        }

        static int CountOf(object? settingsDB)
        {
            if (settingsDB == null) return 0;
            var arr = Get(settingsDB, "Settings") as IList;
            return arr?.Count ?? 0;
        }
    }
}
