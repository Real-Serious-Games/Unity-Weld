using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityUI;
using UnityUI.Binding;
using UnityUI.Internal;

namespace UnityUI_Editor
{
    [CustomEditor(typeof(EventBinding))]
    public class EventBindingEditor : BaseBindingEditor
    {
        public override void OnInspectorGUI()
        {
            var targetScript = (EventBinding)target;

            // Get list of events we can bind to.
            var events = UnityEventWatcher.GetBindableEvents(targetScript.gameObject)
                .OrderBy(evt => evt.Name)
                .ToArray();

            // Popup for the user to pick a UnityEvent on the UI to bind to.
            var selectedEventIndex = ShowEventSelector(targetScript, events);

            Type[] eventType = null;
            if (selectedEventIndex >= 0)
            {
                eventType = events[selectedEventIndex].GetEventTypes();

                // Save properties on the target script so they'll be serialised into the scene
                UpdateProperty(
                    updatedValue => targetScript.uiEventName = updatedValue,
                    targetScript.uiEventName,
                    events[selectedEventIndex].ComponentType.Name + "." + events[selectedEventIndex].Name
                );
            }

            var bindableViewModelMethods = GetBindableViewModelMethods(targetScript);

            // Show a popup for selecting which method to bind to.
            ShowMethodSelector(targetScript, bindableViewModelMethods, eventType);
        }

        /// <summary>
        /// Show dropdown for selecting a UnityEvent to bind to.
        /// </summary>
        private int ShowEventSelector(EventBinding targetScript, UnityEventWatcher.BindableEvent[] events)
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
        /// Draws the dropdown for selecting a method from bindableViewModelMethods
        /// </summary>
        private void ShowMethodSelector(EventBinding targetScript, MethodInfo[] bindableViewModelMethods, Type[] viewEventArgs)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("View model method");

            var dropdownPosition = GUILayoutUtility.GetLastRect();
            dropdownPosition.x += dropdownPosition.width;

            if (GUILayout.Button(new GUIContent(targetScript.viewModelMethodName), EditorStyles.popup))
            {
                ShowViewModelMethodDropdown(targetScript, bindableViewModelMethods, viewEventArgs, dropdownPosition);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ShowViewModelMethodDropdown(EventBinding target, MethodInfo[] bindableMethods, Type[] viewEventArgs, Rect position)
        {
            var propertyNames = bindableMethods
                .Select(method => method.ReflectedType.Name + "." + method.Name)
                .ToArray();
            var selectedIndex = Array.IndexOf(propertyNames, target.viewModelMethodName);

            var options = bindableMethods.Select(m =>
                new InspectorUtils.MenuItem(
                    new GUIContent(m.ReflectedType + "/" + m.Name + "(" + ParameterInfoToString(m.GetParameters()) + ")"),
                    MethodMatchesSignature(m, viewEventArgs)
                )
            ).ToArray();

            InspectorUtils.ShowCustomSelectionMenu(
                index => SetBoundMethod(target, bindableMethods[index]),
                options,
                selectedIndex,
                position);
        }

        /// <summary>
        /// Get a list of methods in the view model that we can bind to.
        /// </summary>
        private MethodInfo[] GetBindableViewModelMethods(EventBinding targetScript)
        {
            return TypeResolver.GetAvailableViewModelTypes(targetScript)
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                .Where(m => m.GetCustomAttributes(typeof(BindingAttribute), false).Any() && !m.Name.StartsWith("get_")) // Exclude property getters, since we aren't doing anything with the return value of the bound method anyway.
                .ToArray();
        }

        /// <summary>
        /// Check that a method matches the specified array of types for its the calling convention.
        /// </summary>
        private bool MethodMatchesSignature(MethodInfo method, Type[] callingConvention)
        {
            var methodParameters = method.GetParameters().Select(p => p.ParameterType).ToArray();

            // Check that the calling convention and methodParameters are equal
            return callingConvention != null
                && callingConvention.Length == methodParameters.Length
                && !callingConvention.Where((type, index) => methodParameters[index] != type).Any();
        }

        /// <summary>
        /// Convert an array of ParameterInfo objects to a nicely formatted string with their
        /// types and names delimited by commas.
        /// </summary>
        private string ParameterInfoToString(ParameterInfo[] info)
        {
            return string.Join(", ", info.Select(parameterInfo => parameterInfo.ToString()).ToArray());
        }

        /// <summary>
        /// Set up the viewModelName and viewModelMethodName in the EventBinding we're editing.
        /// </summary>
        private void SetBoundMethod(EventBinding target, MethodInfo method)
        {
            UpdateProperty(
                updatedValue => target.viewModelMethodName = updatedValue,
                target.viewModelMethodName,
                method.ReflectedType.Name + "." + method.Name
            );
        }
    }
}
