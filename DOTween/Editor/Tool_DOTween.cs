#if HAS_DOTWEEN
#nullable enable
using System;
using System.Reflection;
using com.IvanMurzak.McpPlugin;
using UnityEngine;

namespace TecVooDoo.MCPTools.Editor
{
    /// <summary>
    /// Binds the DOTween tool group to DOTweenAnimation by reflection so the group compiles under either DOTween ASMDEF layout.
    /// </summary>
    /// <remarks>
    /// WHY REFLECTION: DOTween Pro ships DOTweenAnimation.cs as loose SOURCE, so which
    /// assembly ends up owning the type depends on whether the user ran DOTween's
    /// "Create ASMDEF" utility panel button:
    ///   ASMDEFs created     -> DG.Tweening.DOTweenAnimation lives in DOTweenPro.Scripts
    ///   ASMDEFs not created -> it lives in Assembly-CSharp
    /// An asmdef cannot reference Assembly-CSharp, so a compile-time reference would make
    /// MCPTools.DOTween.Editor uncompilable in the second layout. Reflection is what lets
    /// this group carry its own asmdef -- and therefore its own defineConstraints
    /// isolation -- in both layouts, instead of leaking into Assembly-CSharp-Editor.
    ///
    /// DOTween CORE is deliberately NOT reflected: DG.Tweening.DOTween / Ease / LoopType /
    /// Tween always ship as the precompiled DOTween.dll regardless of layout, so they are
    /// bound normally via precompiledReferences and stay strongly typed.
    /// </remarks>
    [AiToolType]
    public partial class Tool_DOTween
    {
        const string AnimTypeFullName = "DG.Tweening.DOTweenAnimation";

        static readonly Type? AnimType = FindType(AnimTypeFullName);
        static readonly Type? AnimationTypeEnum = AnimType == null ? null : AnimType.GetNestedType("AnimationType");

        static Type? FindType(string fullName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                Type? type = assemblies[i].GetType(fullName, false);
                if (type != null) return type;
            }
            return null;
        }

        static Type RequireAnimType()
        {
            if (AnimType == null)
                throw new InvalidOperationException($"'{AnimTypeFullName}' was not found in any loaded assembly. Is DOTween Pro installed?");
            return AnimType;
        }

        static Type RequireAnimationTypeEnum()
        {
            if (AnimationTypeEnum == null)
                throw new InvalidOperationException($"Nested enum '{AnimTypeFullName}+AnimationType' was not found. DOTween Pro version mismatch?");
            return AnimationTypeEnum;
        }

        static GameObject FindGO(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go == null) throw new ArgumentException($"GameObject '{name}' not found.", nameof(name));
            return go;
        }

        static Component[] GetAnims(GameObject go) => go.GetComponents(RequireAnimType());

        static Component AddAnim(GameObject go) => go.AddComponent(RequireAnimType());

        // Every member the tools touch is a public FIELD on DOTweenAnimation (or on its
        // ABSAnimationComponent base, which GetField finds via inherited-public lookup) --
        // DOTweenAnimation exposes no public properties.
        static FieldInfo RequireField(object target, string fieldName)
        {
            FieldInfo? field = target.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
            if (field == null)
                throw new InvalidOperationException($"Field '{fieldName}' not found on '{target.GetType().FullName}'. DOTween Pro version mismatch?");
            return field;
        }

        static object? GetField(object target, string fieldName) => RequireField(target, fieldName).GetValue(target);

        static T GetField<T>(object target, string fieldName)
        {
            object? value = RequireField(target, fieldName).GetValue(target);
            return value == null ? default! : (T)value;
        }

        static void SetField(object target, string fieldName, object value) => RequireField(target, fieldName).SetValue(target, value);

        // DOTweenAnimation overloads several DO* methods (DORestart() vs DORestart(bool)),
        // so the parameterless overload has to be selected by explicit signature -- a
        // name-only GetMethod throws AmbiguousMatchException.
        static void CallParameterless(object target, string methodName)
        {
            MethodInfo? method = target.GetType().GetMethod(
                methodName,
                BindingFlags.Public | BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null);
            if (method == null)
                throw new InvalidOperationException($"Parameterless method '{methodName}()' not found on '{target.GetType().FullName}'. DOTween Pro version mismatch?");
            method.Invoke(target, null);
        }

        static void CallCreateTween(object anim, bool regenerateIfExists, bool andPlay)
        {
            MethodInfo? method = anim.GetType().GetMethod(
                "CreateTween",
                BindingFlags.Public | BindingFlags.Instance,
                null,
                new Type[] { typeof(bool), typeof(bool) },
                null);
            if (method == null)
                throw new InvalidOperationException($"Method 'CreateTween(bool, bool)' not found on '{anim.GetType().FullName}'. DOTween Pro version mismatch?");
            method.Invoke(anim, new object[] { regenerateIfExists, andPlay });
        }

        // Returns the boxed nested-enum value; the enum type is not referenceable at
        // compile time, so callers hand it straight back to SetField.
        static object ParseAnimationType(string animationType)
        {
            Type enumType = RequireAnimationTypeEnum();
            try
            {
                return Enum.Parse(enumType, animationType, true);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException(
                    $"Invalid animationType '{animationType}'. Valid values: {string.Join(", ", Enum.GetNames(enumType))}.",
                    nameof(animationType));
            }
        }

        static string GetAnimationTypeName(object anim)
        {
            object? value = GetField(anim, "animationType");
            return value == null ? "None" : value.ToString() ?? "None";
        }
    }
}
#endif
