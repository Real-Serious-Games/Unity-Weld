using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityUI.Binding;

namespace UnityUI_Editor
{
    [CustomEditor(typeof(OneWayPropertyBinding))]
    class OneWayPropertyBindingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Initialise reference to target script
            var targetScript = (OneWayPropertyBinding)target;

            var uiProperties = PropertyFinder
                .GetBindableProperties(targetScript.gameObject)
                .OrderBy(property => property.PropertyInfo.Name)
                .ToArray();

            var selectedPropertyIndex = ShowUIPropertySelector(targetScript, uiProperties);
            Type viewPropertyType = null;

            if (selectedPropertyIndex >= 0)
            {
                // Selected UI property
                targetScript.uiPropertyName = uiProperties[selectedPropertyIndex].PropertyInfo.Name;
                targetScript.boundComponentType = uiProperties[selectedPropertyIndex].Object.GetType().Name;

                viewPropertyType = uiProperties[selectedPropertyIndex].PropertyInfo.PropertyType;
            }

            var adapterTypeNames = TypeResolver.TypesWithAdapterAttribute
                .Select(type => type.Name)
                .ToArray();

            ShowAdapterMenu(
                "View adaptor",
                adapterTypeNames,
                targetScript.viewAdapterTypeName,
                newValue => targetScript.viewAdapterTypeName = newValue
            );

            Type adaptedViewPropertyType = viewPropertyType;
            if (!string.IsNullOrEmpty(targetScript.viewAdapterTypeName))
            {
                var adapterType = Type.GetType(targetScript.viewAdapterTypeName);
                if (adapterType != null)
                {
                    var adapterAttribute = adapterType
                        .GetCustomAttributes(typeof(AdapterAttribute), false)
                        .Cast<AdapterAttribute>()
                        .FirstOrDefault();
                    if (adapterAttribute != null)
                    {
                        adaptedViewPropertyType = adapterAttribute.InputType;
                    }
                }               
            }

            // Show selector for property in the view model.
            var bindableViewModelProperties = GetBindableViewModelProperties(targetScript);
            ShowViewModelPropertySelector(targetScript, bindableViewModelProperties, adaptedViewPropertyType);
        }

        /// <summary>
        /// Draws the dropdown menu for selectng a property in the UI to bind to.
        /// </summary>
        private int ShowUIPropertySelector(OneWayPropertyBinding targetScript, PropertyFinder.BindablePropertyInfo[] uiProperties)
        {
            var propertyNames = uiProperties
                .Select(prop => prop.PropertyInfo.Name)
                .ToArray();

            return EditorGUILayout.Popup(
                new GUIContent("View property"),
                Array.IndexOf(propertyNames, targetScript.uiPropertyName),
                uiProperties
                    .Select(prop => new GUIContent(
                        prop.PropertyInfo.ReflectedType.Name + "/" + 
                        prop.PropertyInfo.Name + " : " +
                        prop.PropertyInfo.PropertyType.Name
                    ))
                    .ToArray()
            );
        }

        private void ShowViewModelPropertySelector(OneWayPropertyBinding target, PropertyInfo[] bindableProperties, Type viewPropertyType)
        {
            var buttonContent = new GUIContent(target.viewModelPropertyName);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("View-model property");

            var dropdownPosition = GUILayoutUtility.GetLastRect();
            dropdownPosition.x += dropdownPosition.width;

            if (GUILayout.Button(buttonContent, EditorStyles.popup))
            {
                ShowViewModelPropertyDropdown(target, bindableProperties, viewPropertyType, dropdownPosition);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the dropdown menu for picking a property in the view model to bind to.
        /// </summary>
        private void ShowViewModelPropertyDropdown(OneWayPropertyBinding target, PropertyInfo[] bindableProperties, Type viewPropertyType, Rect position)
        { 
            var selectedIndex = Array.IndexOf(
                bindableProperties.Select(p => p.ReflectedType.Name + p.Name).ToArray(),
                target.viewModelName + target.viewModelPropertyName
            );

            var options = bindableProperties
                .Select(p => new InspectorUtils.MenuItem(
                    new GUIContent(p.ReflectedType.Name + "/" + p.Name + " : " + p.PropertyType.Name),
                    p.PropertyType == viewPropertyType
                ))
                .ToArray();

            InspectorUtils.ShowCustomSelectionMenu(
                index => SetViewModelProperty(target, bindableProperties[index]), 
                options, 
                selectedIndex, 
                position
            );
        }

        /// <summary>
        /// Get a list of properties in the bound view model that match the type of the selected property in the UI.
        /// </summary>
        private PropertyInfo[] GetBindableViewModelProperties(OneWayPropertyBinding target)
        {
            return target.GetAvailableViewModelTypes()
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                .ToArray();
        }

        /// <summary>
        /// Set up the viewModelName and viewModelPropertyName in the OneWayPropertyBinding we're editing.
        /// </summary>
        private void SetViewModelProperty(OneWayPropertyBinding target, PropertyInfo property)
        {
            target.viewModelName = property.ReflectedType.Name;
            target.viewModelPropertyName = property.Name;
        }

        /// <summary>
        /// Display the adapters popup menu.
        /// </summary>
        private static void ShowAdapterMenu(
            string label,
            string[] adapterTypeNames,
            string curValue,
            Action<string> valueUpdated
        )
        {
            var adapterMenu = new string[] { "None" }
                .Concat(adapterTypeNames)
                .Select(typeName => new GUIContent(typeName))
                .ToArray();

            var curSelectionIndex = Array.IndexOf(adapterTypeNames, curValue) + 1; // +1 to account for 'None'.
            var newSelectionIndex = EditorGUILayout.Popup(
                    new GUIContent(label),
                    curSelectionIndex,
                    adapterMenu
                );

            if (newSelectionIndex != curSelectionIndex)
            {
                if (newSelectionIndex == 0)
                {
                    valueUpdated(null); // No adapter selected.
                }
                else
                {
                    valueUpdated(adapterTypeNames[newSelectionIndex - 1]); // -1 to account for 'None'.
                }
            }
        }
    }
}
