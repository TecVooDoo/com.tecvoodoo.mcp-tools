#nullable enable
using System;
using System.ComponentModel;
using System.Linq;
using com.IvanMurzak.McpPlugin;
using com.IvanMurzak.ReflectorNet.Utils;
using UnityEditor;
using UnityEngine;

namespace MCPTools.M3DText.Editor
{
    public partial class Tool_M3DText
    {
        [McpPluginTool("m3dt-create-control", Title = "Modular 3D Text / Create UI Control")]
        [Description(@"Creates a new GameObject with a Modular 3D Text UI control component attached.

controlType options:
  Text        -- bare Modular3DText component (with required TextUpdater)
  Button      -- TinyGiantStudio.Text.Button (interactable text button with state events)
  Slider      -- TinyGiantStudio.Text.Slider
  InputField  -- TinyGiantStudio.Text.InputField (text input)
  Toggle      -- TinyGiantStudio.Text.Toggle
  HorizontalSelector -- TinyGiantStudio.Text.HorizontalSelector (left/right value picker)
  List        -- TinyGiantStudio.Text.List (vertical list)

Optional: parentName attaches under the named GameObject; initialText sets Text on the underlying Modular3DText.
font / material apply to the underlying Modular3DText (asset name or path).

Returns the created GameObject's name and the components attached.")]
        public string CreateControl(
            [Description("Control type: Text | Button | Slider | InputField | Toggle | HorizontalSelector | List.")]
            string controlType,
            [Description("Name for the new GameObject (default: '<ControlType> (M3D)').")]
            string? gameObjectName = null,
            [Description("Optional parent GameObject name to attach the new GO under.")]
            string? parentName = null,
            [Description("Initial text content (sets Modular3DText.Text after creation).")]
            string? initialText = null,
            [Description("Font asset name or path (applied to underlying Modular3DText).")]
            string? font = null,
            [Description("Material asset name or path (applied to underlying Modular3DText).")]
            string? material = null
        )
        {
            return MainThread.Instance.Run(() =>
            {
                if (Modular3DTextType == null)
                    throw new Exception("Modular3DText type not found. Modular 3D Text may not be installed.");

                Type? controlClass = ResolveControlType(controlType);
                if (controlClass == null && !string.Equals(controlType, "Text", StringComparison.OrdinalIgnoreCase))
                    throw new Exception($"Control type '{controlType}' not found. Use one of: Text, Button, Slider, InputField, Toggle, HorizontalSelector, List.");

                string goName = gameObjectName ?? $"{char.ToUpper(controlType[0]) + controlType.Substring(1)} (M3D)";
                var go = new GameObject(goName);

                if (parentName != null)
                {
                    var parent = GameObject.Find(parentName);
                    if (parent == null) throw new Exception($"Parent '{parentName}' not found.");
                    go.transform.SetParent(parent.transform, worldPositionStays: false);
                }

                // Add the control component first when applicable; the [RequireComponent(typeof(TextUpdater))]
                // on Modular3DText pulls in TextUpdater automatically.
                UnityEngine.Component? control = null;
                if (controlClass != null)
                    control = go.AddComponent(controlClass);

                // Ensure Modular3DText is present (control classes that include a text label still expect it).
                var textComp = go.GetComponent(Modular3DTextType) ?? go.AddComponent(Modular3DTextType);

                if (initialText != null) Set(textComp, "Text", initialText);
                if (font != null)
                {
                    var fontAsset = FindFontByName(font);
                    if (fontAsset == null) throw new Exception($"Font '{font}' not found.");
                    Set(textComp, "Font", fontAsset);
                }
                if (material != null)
                {
                    var matAsset = FindMaterial(material);
                    if (matAsset == null) throw new Exception($"Material '{material}' not found.");
                    Set(textComp, "Material", matAsset);
                }

                Selection.activeGameObject = go;
                Undo.RegisterCreatedObjectUndo(go, $"Create M3D {controlType}");
                EditorUtility.SetDirty(go);

                var components = string.Join(", ", go.GetComponents<UnityEngine.Component>().Where(c => c != null).Select(c => c.GetType().Name));
                return $"OK: Created '{goName}' with components: {components}\n  text='{initialText ?? ""}'  font={(font ?? "(default)")}  material={(material ?? "(default)")}";
            });
        }

        static Type? ResolveControlType(string name)
        {
            switch ((name ?? "").Trim().ToLowerInvariant())
            {
                case "text":               return null; // Modular3DText only -- handled below
                case "button":             return ButtonType;
                case "slider":             return SliderType;
                case "inputfield":         return InputFieldType;
                case "toggle":             return ToggleType;
                case "horizontalselector": return HorizontalSelectorType;
                case "list":               return ListType;
                default:                   return null;
            }
        }
    }
}
