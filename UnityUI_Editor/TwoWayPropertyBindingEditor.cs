using System;
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
    [CustomEditor(typeof(TwoWayPropertyBinding))]
    class PropertyBindingEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var targetScript = (TwoWayPropertyBinding)target;

            var events = UnityEventWatcher
                .GetBindableEvents(targetScript.gameObject)
                .Where(evt => evt.GetEventTypes().Length == 1) // Only select events that can be bound directly to properties
                .OrderBy(evt => evt.Name)
                .ToArray();

            var properties = PropertyFinder
                .GetBindableProperties(targetScript.gameObject)
                .OrderBy(property => property.PropertyInfo.Name)
                .ToArray();

            var selectedEventIndex = ShowEventSelector(targetScript, events);
            if (selectedEventIndex >= 0)
            {
                targetScript.uiEventName = events[selectedEventIndex].Name;
                targetScript.boundComponentType = events[selectedEventIndex].ComponentType.Name;
            }

            Type viewPropertyType = null;
            var selectedPropertyIndex = ShowUIPropertySelector(targetScript, properties);
            if (selectedPropertyIndex >= 0)
            {
                targetScript.uiPropertyName = properties[selectedPropertyIndex].PropertyInfo.Name;
                viewPropertyType = properties[selectedPropertyIndex].PropertyInfo.PropertyType;
            }

            var adapterTypeNames = TypeResolver.TypesWithAdapterAttribute
                .Select(type => type.Name)
                .ToArray();

            ShowAdapterMenu(
                "View adaptor", 
                adapterTypeNames, 
                targetScript.viewAdapterTypeName,
                (newValue) => targetScript.viewAdapterTypeName = newValue
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

            var bindableViewModelProperties = GetBindableViewModelProperties(targetScript);
            ShowViewModelPropertySelector(targetScript, bindableViewModelProperties, adaptedViewPropertyType);

            ShowAdapterMenu(
                "View-model adaptor", 
                adapterTypeNames, 
                targetScript.viewModelAdapterTypeName,
                (newValue) => targetScript.viewModelAdapterTypeName = newValue
            );
        }

        /// <summary>
        /// Show dropdown for selecting a UnityEvent to bind to.
        /// </summary>
        private int ShowEventSelector(TwoWayPropertyBinding targetScript, UnityEventWatcher.BindableEvent[] events)
        {
            var eventNames = events
                .Select(evt => evt.Name)
                .ToArray();

            return EditorGUILayout.Popup(
                new GUIContent("View event"),
                Array.IndexOf(eventNames, targetScript.uiEventName),
                events.Select(evt => new GUIContent(
                    evt.DeclaringType + "." + evt.Name + 
                    "(" + evt.GetEventTypes()[0].ToString() + ")")
                )
                .ToArray()
            );
        }

        /// <summary>
        /// Shows a dropdown for selecting a property in the UI to bind to.
        /// </summary>
        private int ShowUIPropertySelector(TwoWayPropertyBinding targetScript, PropertyFinder.BindablePropertyInfo[] properties)
        {
            var propertyNames = properties
                .Select(prop => prop.PropertyInfo.Name)
                .ToArray();

            return EditorGUILayout.Popup(
                new GUIContent("View property"),
                Array.IndexOf(propertyNames, targetScript.uiPropertyName),
                properties.Select(prop => new GUIContent(
                        prop.PropertyInfo.ReflectedType.Name + "/" +
                        prop.PropertyInfo.Name + " : " +
                        prop.PropertyInfo.PropertyType.Name
                    ))
                    .ToArray()
            );
        }

        /// <summary>
        /// Get a list of all the methods in the bound view model of a specific type that we can bind to.
        /// </summary>
        private PropertyInfo[] GetBindableViewModelProperties(TwoWayPropertyBinding target)
        {
            return target.GetAvailableViewModelTypes()
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                .ToArray();
        }

        private void ShowViewModelPropertySelector(TwoWayPropertyBinding target, PropertyInfo[] bindableProperties, Type viewPropertyType)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("View-model property");

            var dropdownPosition = GUILayoutUtility.GetLastRect();
            dropdownPosition.x += dropdownPosition.width;

            if (GUILayout.Button(new GUIContent(target.viewModelPropertyName), EditorStyles.popup))
            {
                ShowViewModelPropertyMenu(target, bindableProperties, viewPropertyType, dropdownPosition);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the dropdown menu for picking a property in the view model to bind to.
        /// </summary>
        private void ShowViewModelPropertyMenu(TwoWayPropertyBinding target, PropertyInfo[] bindableProperties, Type viewPropertyType, Rect position)
        {
            var selectedIndex = Array.IndexOf(
                bindableProperties.Select(p => p.ReflectedType + p.Name).ToArray(),
                target.viewModelName + target.viewModelPropertyName
            );

            var options = bindableProperties
                .Select(p => new InspectorUtils.MenuItem(
                    new GUIContent(p.ReflectedType + "/" + p.Name + " : " + p.PropertyType.Name),
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
        /// Set up the viewModelName and viewModelPropertyname in the TwoWayPropertyBinding we're editing.
        /// </summary>
        private void SetViewModelProperty(TwoWayPropertyBinding target, PropertyInfo propertyInfo)
        {
            target.viewModelName = propertyInfo.ReflectedType.Name;
            target.viewModelPropertyName = propertyInfo.Name;
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
