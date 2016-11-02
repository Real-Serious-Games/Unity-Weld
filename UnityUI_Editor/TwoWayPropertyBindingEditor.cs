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
            ShowViewPropertyMenu(
                "View property",
                targetScript,
                PropertyFinder.GetBindableProperties(targetScript.gameObject)
                    .OrderBy(property => property.PropertyInfo.Name)
                    .ToArray(),
                    updatedValue => targetScript.uiPropertyName = updatedValue,
                    targetScript.uiPropertyName,
                out viewPropertyType
                );

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

            ShowViewModelPropertyMenu(
                "View-model property",
                targetScript, 
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                property => property.PropertyType == adaptedViewPropertyType
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

            ShowViewModelPropertyMenu(
                "Exception property",
                targetScript, 
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.exceptionPropertyName = updatedValue,
                targetScript.exceptionPropertyName,
                property => property.PropertyType == adaptedExceptionPropertyType
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


    }
}
