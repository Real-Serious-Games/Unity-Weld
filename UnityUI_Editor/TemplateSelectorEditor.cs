using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Internal;
using UnityWeld_Editor;

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

            UpdateProperty(
                updatedValue => targetScript.templates = updatedValue,
                targetScript.templates,
                (GameObject)EditorGUILayout.ObjectField(
                    new GUIContent("Templates root object", "Parent object to the objects we want to use as templates."),
                    targetScript.templates, 
                    typeof(GameObject), 
                    true
                )
            );
        }
    }
}
