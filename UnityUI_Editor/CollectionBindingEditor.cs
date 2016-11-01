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

            var bindableCollections = GetBindableViewModelCollections(targetScript);
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

        private PropertyInfo[] GetBindableViewModelCollections(CollectionBinding target)
        {
            return TypeResolver.GetAvailableViewModelTypes(target)
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
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
            var propertyNames = bindableProperties
                .Select(property => property.ReflectedType.Name + "." + property.Name)
                .ToArray();
            var selectedIndex = Array.IndexOf(propertyNames, targetScript.viewModelPropertyName);

            var options = bindableProperties.Select(m =>
                new InspectorUtils.MenuItem(
                    new GUIContent(m.ReflectedType + "/" + m.Name),
                    true
                )
            ).ToArray();

            InspectorUtils.ShowCustomSelectionMenu(
                index => SetViewModelProperty(targetScript, bindableProperties[index]),
                options,
                selectedIndex,
                position);
        }

        /// <summary>
        /// Set up the viewModelName and viewModelPropertyname in the TwoWayPropertyBinding we're editing.
        /// </summary>
        private void SetViewModelProperty(CollectionBinding target, PropertyInfo property)
        {
            UpdateProperty(
                updatedValue => target.viewModelPropertyName = updatedValue,
                target.viewModelPropertyName,
                property.ReflectedType.Name + "." + property.Name
            );
        }
    }
}
