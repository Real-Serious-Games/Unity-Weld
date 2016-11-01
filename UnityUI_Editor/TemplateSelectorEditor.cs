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

            var bindableViews = GetBindableViews(targetScript);
            ShowPropertySelector(targetScript, bindableViews);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Templates root object");

            UpdateProperty(
                updatedValue => targetScript.templates = updatedValue,
                targetScript.templates,
                (GameObject)EditorGUILayout.ObjectField(targetScript.templates, typeof(GameObject), true)
            );

            EditorGUILayout.EndHorizontal();
        }

        private PropertyInfo[] GetBindableViews(TemplateSelector target)
        {
            return TypeResolver.GetAvailableViewModelTypes(target)
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                .ToArray();
        }

        /// <summary>
        /// Show dropdown for selecting a collection to bind to.
        /// </summary>
        private void ShowPropertySelector(TemplateSelector targetScript, PropertyInfo[] bindableViews)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("View to bind to");

            var dropdownPosition = GUILayoutUtility.GetLastRect();
            dropdownPosition.x += dropdownPosition.width;

            if (GUILayout.Button(new GUIContent(targetScript.viewModelPropertyName), EditorStyles.popup))
            {
                ShowPropertySelectorDropdown(targetScript, bindableViews, dropdownPosition);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the dropdown for selecting a method from bindableViewModelProperties
        /// </summary>
        private void ShowPropertySelectorDropdown(TemplateSelector targetScript, PropertyInfo[] bindableProperties, Rect position)
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
        /// Set up the viewModelName and viewModelPropertyname in the TemplateSelector we're editing.
        /// </summary>
        private void SetViewModelProperty(TemplateSelector target, PropertyInfo property)
        {
            UpdateProperty(
                updatedValue => target.viewModelPropertyName = updatedValue,
                target.viewModelPropertyName,
                property.ReflectedType.Name + "." + property.Name
            );
        }
    }
}
