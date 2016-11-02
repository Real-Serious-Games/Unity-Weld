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

            var bindableCollections = FindBindableCollectionProperties(targetScript);
            ShowCollectionSelector(targetScript, bindableCollections);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Collection template");

            UpdateProperty(
                updatedValue => targetScript.template = updatedValue,
                targetScript.template,
                (TemplateBinding)EditorGUILayout.ObjectField(targetScript.template, typeof(TemplateBinding), true)
            );

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Find collection properties that can be data-bound.
        /// </summary>
        private PropertyInfo[] FindBindableCollectionProperties(CollectionBinding target)
        {
            return TypeResolver.FindBindableProperties(target)
                .Where(property => typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                .Where(property => !typeof(string).IsAssignableFrom(property.PropertyType))
                .ToArray();
        }

        /// <summary>
        /// Show dropdown for selecting a collection to bind to.
        /// </summary>
        private void ShowCollectionSelector(CollectionBinding targetScript, PropertyInfo[] bindableCollections)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Collection to bind to");

            var dropdownPosition = GUILayoutUtility.GetLastRect();
            dropdownPosition.x += dropdownPosition.width;

            if (GUILayout.Button(new GUIContent(targetScript.viewModelPropertyName), EditorStyles.popup))
            {
                ShowCollectionSelectorDropdown(targetScript, bindableCollections, dropdownPosition);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the dropdown for selecting a method from bindableViewModelCollections
        /// </summary>
        private void ShowCollectionSelectorDropdown(CollectionBinding targetScript, PropertyInfo[] bindableProperties, Rect position)
        {
            InspectorUtils.ShowMenu<PropertyInfo>(
                property => property.ReflectedType + "/" + property.Name,
                property => true,
                property => property.ReflectedType.Name + "." + property.Name == targetScript.viewModelPropertyName,
                property => UpdateProperty(
                    updatedValue => targetScript.viewModelPropertyName = updatedValue,
                    targetScript.viewModelPropertyName,
                property.ReflectedType.Name + "." + property.Name
                ),
                bindableProperties,
                position
            );
        }
    }
}
