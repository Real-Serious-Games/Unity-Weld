using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityWeld;
using UnityWeld.Binding;
using UnityWeld.Internal;

namespace UnityWeld_Editor
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

            ShowMethodMenu(targetScript, TypeResolver.FindBindableMethods(targetScript));
        }

        /// <summary>
        /// Draws the dropdown for selecting a method from bindableViewModelMethods
        /// </summary>
        private void ShowMethodMenu(EventBinding targetScript, MethodInfo[] bindableMethods)
        {
            var tooltip = "Method on the view-model to bind to.";

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("View-model method", tooltip));

            var dropdownPosition = GUILayoutUtility.GetLastRect();
            dropdownPosition.x += dropdownPosition.width;

            if (GUILayout.Button(new GUIContent(targetScript.viewModelMethodName, tooltip), EditorStyles.popup))
            {
                InspectorUtils.ShowMenu<MethodInfo>(
                    method => method.ReflectedType + "/" + method.Name,
                    method => true,
                    method => method.ReflectedType.Name + "." + method.Name == targetScript.viewModelMethodName,
                    method => UpdateProperty(
                        updatedValue => targetScript.viewModelMethodName = updatedValue,
                        targetScript.viewModelMethodName,
                        method.ReflectedType.Name + "." + method.Name
                    ),
                    bindableMethods
                        .OrderBy(method => method.ReflectedType.Name)
                        .ThenBy(method => method.Name)
                        .ToArray(),
                    dropdownPosition
                );
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
