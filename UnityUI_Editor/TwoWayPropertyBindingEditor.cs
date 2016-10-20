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
        UnityEventWatcher.BindableEvent[] events;

        PropertyFinder.BindablePropertyInfo[] properties;

        public override void OnInspectorGUI()
        {
            // Initialise everything
            var targetScript = (TwoWayPropertyBinding)target;

            events = UnityEventWatcher
                .GetBindableEvents(targetScript.gameObject)
                .Where(evt => evt.GetEventTypes().Length == 1) // Only select events that can be bound directly to properties
                .OrderBy(evt => evt.Name)
                .ToArray();

            properties = PropertyFinder
                .GetBindableProperties(targetScript.gameObject)
                .OrderBy(property => property.PropertyInfo.Name)
                .ToArray();

            Type selectedEventType = null;
            var selectedEventIndex = ShowEventSelector(targetScript);
            if (selectedEventIndex >= 0)
            {
                selectedEventType = events[selectedEventIndex].GetEventTypes().Single(); ;

                targetScript.uiEventName = events[selectedEventIndex].Name;
                targetScript.boundComponentType = events[selectedEventIndex].ComponentType.Name;
            }

            var selectedPropertyIndex = ShowUIPropertySelector(targetScript);

            if (selectedPropertyIndex >= 0)
            {
                targetScript.uiPropertyName = properties[selectedPropertyIndex].PropertyInfo.Name;
            }

            var bindableViewModelProperties = GetBindableViewModelProperties(targetScript);
            ShowViewModelPropertySelector(targetScript, bindableViewModelProperties, selectedEventType);
        }

        /// <summary>
        /// Show dropdown for selecting a UnityEvent to bind to.
        /// </summary>
        private int ShowEventSelector(TwoWayPropertyBinding targetScript)
        {
            return EditorGUILayout.Popup(
                new GUIContent("Event to bind to"),
                events.Select(evt => evt.Name)
                    .ToList()
                    .IndexOf(targetScript.uiEventName),
                events.Select(evt => 
                    new GUIContent(evt.DeclaringType + "." + evt.Name + 
                        "(" + evt.GetEventTypes()[0].ToString() + ")")).ToArray());
        }

        /// <summary>
        /// Shows a dropdown for selecting a property in the UI to bind to.
        /// </summary>
        private int ShowUIPropertySelector(TwoWayPropertyBinding targetScript)
        {
            return EditorGUILayout.Popup(
                new GUIContent("Property to bind to"),
                properties.Select(prop => prop.PropertyInfo.Name)
                    .ToList()
                    .IndexOf(targetScript.uiPropertyName),
                properties.Select(prop =>
                    new GUIContent(prop.PropertyInfo.ReflectedType.Name + "/" +
                        prop.PropertyInfo.Name + " : " +
                        prop.PropertyInfo.PropertyType.Name)).ToArray());
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
            EditorGUILayout.PrefixLabel("View model property");

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

            var options = bindableProperties.Select(p =>
                new InspectorUtils.MenuItem(
                    new GUIContent(p.ReflectedType + "/" + p.Name + " : " + p.PropertyType.Name),
                    p.PropertyType == viewPropertyType
                )
            ).ToArray();

            InspectorUtils.ShowCustomSelectionMenu(index =>
                SetViewModelProperty(target, bindableProperties[index]), options, selectedIndex, position);
        }

        /// <summary>
        /// Set up the viewModelName and viewModelPropertyname in the TwoWayPropertyBinding we're editing.
        /// </summary>
        private void SetViewModelProperty(TwoWayPropertyBinding target, PropertyInfo propertyInfo)
        {
            target.viewModelName = propertyInfo.ReflectedType.Name;
            target.viewModelPropertyName = propertyInfo.Name;
        }
    }
}
