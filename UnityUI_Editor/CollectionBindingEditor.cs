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
using UnityUI_Editor;

namespace UnityTools.UnityUI_Editor
{
    [CustomEditor(typeof(CollectionBinding))]
    class CollectionBindingEditor : Editor
    {

        public override void OnInspectorGUI()
        {
            // Initialise everything
            var targetScript = (CollectionBinding)target;

            var bindableCollections = GetBindableViewModelCollections(targetScript);
            ShowCollectionSelector(targetScript, bindableCollections);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Collection template");

            var newTemplate = (TemplateBinding)EditorGUILayout.ObjectField(targetScript.template, typeof(TemplateBinding), true);
            if (targetScript.template != newTemplate)
            {
                targetScript.template = newTemplate;
                InspectorUtils.MarkSceneDirty(targetScript.gameObject);
            }

            EditorGUILayout.EndHorizontal();
        }

        private PropertyInfo[] GetBindableViewModelCollections(CollectionBinding target)
        {
            return target.GetAvailableViewModelTypes()
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
        private void ShowCollectionSelectorDropdown(CollectionBinding targetScript, PropertyInfo[] bindableViewModelCollections, Rect position)
        {
            var selectedIndex = Array.IndexOf(
                bindableViewModelCollections.Select(m => m.ReflectedType + m.Name).ToArray(),
                targetScript.viewModelName + targetScript.viewModelPropertyName
            );

            var options = bindableViewModelCollections.Select(m =>
                new InspectorUtils.MenuItem(
                    new GUIContent(m.ReflectedType + "/" + m.Name),
                    true
                )
            ).ToArray();

            InspectorUtils.ShowCustomSelectionMenu(
                index => SetViewModelProperty(targetScript, bindableViewModelCollections[index]),
                options,
                selectedIndex,
                position);
        }

        /// <summary>
        /// Set up the viewModelName and viewModelPropertyname in the TwoWayPropertyBinding we're editing.
        /// </summary>
        private void SetViewModelProperty(CollectionBinding target, PropertyInfo property)
        {
            var viewModelName = property.ReflectedType.Name;
            var viewModelPropertyName = property.Name;
            var dirty = false;

            if (target.viewModelName != viewModelName)
            {
                dirty = true;
                target.viewModelName = viewModelName;
            }
            if (target.viewModelPropertyName != viewModelPropertyName)
            {
                dirty = true;
                target.viewModelPropertyName = viewModelPropertyName;
            }

            if (dirty)
            {
                InspectorUtils.MarkSceneDirty(target.gameObject);
            }
        }
    }
}
