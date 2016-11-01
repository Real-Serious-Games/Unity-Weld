using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
    [CustomEditor(typeof(TwoWayPropertyBinding))]
    class PropertyBindingEditor : BaseBindingEditor
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
                UpdateProperty(
                    updatedValue => targetScript.uiEventName = updatedValue,
                    targetScript.uiEventName,
                    events[selectedEventIndex].ComponentType.Name + "." + events[selectedEventIndex].Name
                );
            }

            Type viewPropertyType = null;
            var selectedPropertyIndex = ShowUIPropertySelector(targetScript, properties);
            if (selectedPropertyIndex >= 0)
            {
                UpdateProperty(
                    updatedValue => targetScript.uiPropertyName = updatedValue,
                    targetScript.uiPropertyName,
                    properties[selectedPropertyIndex].PropertyInfo.ReflectedType.Name + "." + properties[selectedPropertyIndex].PropertyInfo.Name
                );
                viewPropertyType = properties[selectedPropertyIndex].PropertyInfo.PropertyType;
            }

            var viewAdapterTypeNames = TypeResolver.TypesWithAdapterAttribute
                .Where(type => viewPropertyType == null || TypeResolver.FindAdapterAttribute(type).OutputType == viewPropertyType)
                .Select(type => type.Name)
                .ToArray();

            ShowAdapterMenu(
                "View adaptor", 
                viewAdapterTypeNames, 
                targetScript.viewAdapterTypeName,
                newValue => UpdateProperty(
                    updatedValue => targetScript.viewAdapterTypeName = updatedValue,
                    targetScript.viewAdapterTypeName,
                    newValue
                )
            );

            Type adaptedViewPropertyType = viewPropertyType;
            if (!string.IsNullOrEmpty(targetScript.viewAdapterTypeName))
            {
                var adapterType = TypeResolver.FindAdapterType(targetScript.viewAdapterTypeName);
                if (adapterType != null)
                {
                    var adapterAttribute = TypeResolver.FindAdapterAttribute(adapterType);
                    if (adapterAttribute != null)
                    {
                        adaptedViewPropertyType = adapterAttribute.InputType;
                    }
                }
            }

            var bindableViewModelProperties = GetBindableViewModelProperties(targetScript);
            ShowViewModelPropertySelector(
                "View-model property",
                targetScript, 
                bindableViewModelProperties,
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                adaptedViewPropertyType
            );

            var viewModelAdapterTypeNames = TypeResolver.TypesWithAdapterAttribute
                .Where(type => adaptedViewPropertyType == null || TypeResolver.FindAdapterAttribute(type).OutputType == adaptedViewPropertyType)
                .Select(type => type.Name)
                .ToArray();

            ShowAdapterMenu(
                "View-model adaptor", 
                viewModelAdapterTypeNames, 
                targetScript.viewModelAdapterTypeName,
                (newValue) => targetScript.viewModelAdapterTypeName = newValue
            );

            var expectionAdapterTypeNames = TypeResolver.TypesWithAdapterAttribute
                .Where(type => TypeResolver.FindAdapterAttribute(type).InputType == typeof(Exception))
                .Select(type => type.Name)
                .ToArray();

            ShowAdapterMenu(
                "Exception adaptor",
                expectionAdapterTypeNames,
                targetScript.exceptionAdapterTypeName,
                (newValue) => targetScript.exceptionAdapterTypeName = newValue
            );

            var adaptedExceptionPropertyType = typeof(Exception);
            if (!string.IsNullOrEmpty(targetScript.exceptionAdapterTypeName))
            {
                var adapterType = TypeResolver.FindAdapterType(targetScript.exceptionAdapterTypeName);
                if (adapterType != null)
                {
                    var adapterAttribute = TypeResolver.FindAdapterAttribute(adapterType);
                    if (adapterAttribute != null)
                    {
                        adaptedExceptionPropertyType = adapterAttribute.OutputType;
                    }
                }
            }

            var exceptionViewModelProperties = GetBindableViewModelProperties(targetScript);
            ShowViewModelPropertySelector(
                "Exception property",
                targetScript, 
                exceptionViewModelProperties,
                updatedValue => targetScript.exceptionPropertyName = updatedValue,
                targetScript.exceptionPropertyName,
                adaptedExceptionPropertyType
            );

        }

        /// <summary>
        /// Show dropdown for selecting a UnityEvent to bind to.
        /// </summary>
        private int ShowEventSelector(TwoWayPropertyBinding targetScript, UnityEventWatcher.BindableEvent[] events)
        {
            var eventNames = events
                .Select(evt => evt.ComponentType.Name + "." + evt.Name)
                .ToArray();
            var selectedIndex = Array.IndexOf(eventNames, targetScript.uiEventName);

            return EditorGUILayout.Popup(
                new GUIContent("View event"),
                selectedIndex,
                events.Select(evt => new GUIContent(evt.ComponentType.Name + "." + evt.Name))
                .ToArray()
            );
        }

        /// <summary>
        /// Shows a dropdown for selecting a property in the UI to bind to.
        /// </summary>
        private int ShowUIPropertySelector(TwoWayPropertyBinding targetScript, PropertyFinder.BindablePropertyInfo[] properties)
        {
            var propertyNames = properties
                .Select(prop => prop.PropertyInfo.ReflectedType.Name + "." + prop.PropertyInfo.Name)
                .ToArray();
            var selectedIndex = Array.IndexOf(propertyNames, targetScript.uiPropertyName);

            return EditorGUILayout.Popup(
                new GUIContent("View property"),
                selectedIndex,
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
            return TypeResolver.GetAvailableViewModelTypes(target)
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                .ToArray();
        }

        private void ShowViewModelPropertySelector(
            string label,
            TwoWayPropertyBinding target, 
            PropertyInfo[] bindableProperties,
            Action<string> propertyNameSetter,
            string propertyName,
            Type viewPropertyType
            )
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            var dropdownPosition = GUILayoutUtility.GetLastRect();
            dropdownPosition.x += dropdownPosition.width;

            if (GUILayout.Button(new GUIContent(propertyName), EditorStyles.popup))
            {
                ShowViewModelPropertyMenu(
                    target, 
                    bindableProperties,
                    propertyNameSetter,
                    propertyName,
                    viewPropertyType, 
                    dropdownPosition
                );
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the dropdown menu for picking a property in the view model to bind to.
        /// </summary>
        private void ShowViewModelPropertyMenu(
            TwoWayPropertyBinding target, 
            PropertyInfo[] bindableProperties,
            Action<string> propertyNameSetter,
            string viewModelPropertyName,
            Type viewPropertyType, 
            Rect position
        )
        {
            var propertyNames = bindableProperties
                .Select(property => property.ReflectedType.Name + "." + property.Name)
                .ToArray();
            var selectedIndex = Array.IndexOf(propertyNames, viewModelPropertyName);

            var options = bindableProperties
                .Select(p => new InspectorUtils.MenuItem(
                    new GUIContent(p.ReflectedType + "/" + p.Name + " : " + p.PropertyType.Name),
                    p.PropertyType == viewPropertyType
                ))
                .ToArray();

            InspectorUtils.ShowCustomSelectionMenu(
                index => SetViewModelProperty(
                    target, 
                    bindableProperties[index], 
                    propertyNameSetter,
                    viewModelPropertyName
                ), 
                options, 
                selectedIndex, 
                position
            );
        }

        /// <summary>
        /// Set up the viewModelName and viewModelPropertyname in the TwoWayPropertyBinding we're editing.
        /// </summary>
        private void SetViewModelProperty(
            TwoWayPropertyBinding target, 
            PropertyInfo propertyInfo,
            Action<string> propertyNameSetter,
            string propertyName
        )
        {
            UpdateProperty(
                propertyNameSetter,
                propertyName,
                propertyInfo.ReflectedType.Name + "." + propertyInfo.Name
            );
        }
    }
}
