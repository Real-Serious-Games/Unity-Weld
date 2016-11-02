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

            var bindableViews = TypeResolver.FindBindableProperties(targetScript);
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
