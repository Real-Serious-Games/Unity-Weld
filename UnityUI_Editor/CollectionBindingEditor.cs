using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;
using UnityWeld.Internal;
using UnityWeld_Editor;

namespace UnityWeld_Editor
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

            UpdateProperty(
                updatedValue => targetScript.template = updatedValue,
                targetScript.template,
                (TemplateBinding)EditorGUILayout.ObjectField(
                    new GUIContent("Collection template", "Object to create instances of for each item in the collection."), 
                    targetScript.template, 
                    typeof(TemplateBinding), 
                    true
                )
            );
        }
    }
}
