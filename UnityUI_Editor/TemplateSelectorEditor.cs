using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityUI.Binding;
using UnityUI.Internal;
using UnityUI_Editor;

namespace UnityTools.UnityUI_Editor
{
    [CustomEditor(typeof(TemplateSelector))]
    class TemplateSelectorEditor : BaseBindingEditor
    {

        public override void OnInspectorGUI()
        {
            // Initialise everything
            var targetScript = (TemplateSelector)target;

            ShowViewModelPropertyMenu(
                new GUIContent("Template property", "Property on the view model to use for selecting templates."),
                targetScript,
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                property => true
            );

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Templates root object");

            UpdateProperty(
                updatedValue => targetScript.templates = updatedValue,
                targetScript.templates,
                (GameObject)EditorGUILayout.ObjectField(targetScript.templates, typeof(GameObject), true)
            );

            EditorGUILayout.EndHorizontal();
        }
    }
}
