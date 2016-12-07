using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityUI;
using UnityUI.Binding;
using UnityUI.Internal;
using UnityUI_Editor;

namespace UnityUI_Editor
{
    [CustomEditor(typeof(CollectionBinding))]
    class CollectionBindingEditor : BaseBindingEditor
    {
        public override void OnInspectorGUI()
        {
            // Initialise everything
            var targetScript = (CollectionBinding)target;

            ShowViewModelPropertyMenu(
                new GUIContent("View-model property", "Property on the view-model to bind to."),
                targetScript, 
                TypeResolver.FindBindableCollectionProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                property => true
            );

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Collection template");

            UpdateProperty(
                updatedValue => targetScript.template = updatedValue,
                targetScript.template,
                (TemplateBinding)EditorGUILayout.ObjectField(targetScript.template, typeof(TemplateBinding), true)
            );

            EditorGUILayout.EndHorizontal();
        }
    }
}
