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

            ShowEventMenu(
                targetScript,
                UnityEventWatcher.GetBindableEvents(targetScript.gameObject)
                .OrderBy(evt => evt.Name)
                    .ToArray(),
                    updatedValue => targetScript.uiEventName = updatedValue,
                targetScript.uiEventName
                );

            var bindableViewModelMethods = GetBindableViewModelMethods(targetScript);

            // Show a popup for selecting which method to bind to.
            ShowMethodSelector(targetScript, bindableViewModelMethods);
        }

        /// <summary>
        /// Draws the dropdown for selecting a method from bindableViewModelMethods
        /// </summary>
        private void ShowMethodSelector(EventBinding targetScript, MethodInfo[] bindableViewModelMethods)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("View model method");

            var dropdownPosition = GUILayoutUtility.GetLastRect();
            dropdownPosition.x += dropdownPosition.width;

            if (GUILayout.Button(new GUIContent(targetScript.viewModelMethodName), EditorStyles.popup))
            {
                ShowViewModelMethodDropdown(targetScript, bindableViewModelMethods, dropdownPosition);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ShowViewModelMethodDropdown(EventBinding target, MethodInfo[] bindableMethods, Rect position)
        {
            InspectorUtils.ShowMenu<MethodInfo>(
                method => method.ReflectedType + "/" + method.Name,
                method => true,
                method => method.ReflectedType.Name + "." + method.Name == target.viewModelMethodName,
                method => UpdateProperty(
                    updatedValue => target.viewModelMethodName = updatedValue,
                    target.viewModelMethodName,
                    method.ReflectedType.Name + "." + method.Name
                ),
                bindableMethods,
                position
            );

        }

        /// <summary>
        /// Get a list of methods in the view model that we can bind to.
        /// </summary>
        private MethodInfo[] GetBindableViewModelMethods(EventBinding targetScript)
        {
            return TypeResolver.FindAvailableViewModelTypes(targetScript)
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                .Where(method => method.GetParameters().Length == 0)
                .Where(method => method.GetCustomAttributes(typeof(BindingAttribute), false).Any() && !method.Name.StartsWith("get_")) // Exclude property getters, since we aren't doing anything with the return value of the bound method anyway.
                .ToArray();
        }
    }
}
