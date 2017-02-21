using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Internal;
using UnityWeld_Editor;

/// <summary>
/// Editor for template bindings with a dropdown for selecting what view model
/// to bind to.
/// </summary>
[CustomEditor(typeof(TemplateBinding))]
public class TemplateBindingEditor : BaseBindingEditor
{
    public override void OnInspectorGUI()
    {
        var targetScript = (TemplateBinding)target;

        var availableViewModels = TypeResolver.TypesWithBindingAttribute
            .Select(type => type.Name)
            .OrderBy(name => name)
            .ToArray();

        var selectedIndex = Array.IndexOf(availableViewModels, targetScript.viewModelTypeName);

        var newSelectedIndex = EditorGUILayout.Popup(
            new GUIContent("Template view model", "Type of the view model that this template will be bound to when it is instantiated."),
            selectedIndex,
            availableViewModels.Select(viewModel => new GUIContent(viewModel)).ToArray()
        );

        UpdateProperty(newValue => targetScript.viewModelTypeName = newValue,
            selectedIndex < 0 ? string.Empty : availableViewModels[selectedIndex],
            newSelectedIndex < 0 ? string.Empty : availableViewModels[newSelectedIndex]
        );
    }
}
