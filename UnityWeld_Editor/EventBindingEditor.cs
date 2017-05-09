using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(EventBinding))]
    public class EventBindingEditor : BaseBindingEditor
    {
        private EventBinding targetScript;

        // Whether or not the values on our target match its prefab.
        private bool viewEventPrefabModified;
        private bool viewModelMethodPrefabModified;

        private void OnEnable()
        {
            targetScript = (EventBinding)target;
        }

        public override void OnInspectorGUI()
        {
            UpdatePrefabModifiedProperties();

            var defaultLabelStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = viewEventPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            ShowEventMenu(
                UnityEventWatcher.GetBindableEvents(targetScript.gameObject)
                    .OrderBy(evt => evt.Name)
                    .ToArray(),
                updatedValue => targetScript.uiEventName = updatedValue,
                targetScript.uiEventName
            );

            EditorStyles.label.fontStyle = viewModelMethodPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            ShowMethodMenu(targetScript, TypeResolver.FindBindableMethods(targetScript));

            EditorStyles.label.fontStyle = defaultLabelStyle;
        }

        /// <summary>
        /// Draws the dropdown for selecting a method from bindableViewModelMethods
        /// </summary>
        private void ShowMethodMenu(EventBinding targetScript, MethodInfo[] bindableMethods)
        {
            var tooltip = "Method on the view-model to bind to.";

            InspectorUtils.DoPopup(
                new GUIContent(targetScript.viewModelMethodName),
                new GUIContent("View-model method", tooltip),
                method => method.ReflectedType + "/" + method.Name,
                method => true,
                method => MemberInfoToString(method) == targetScript.viewModelMethodName,
                method => UpdateProperty(
                    updatedValue => targetScript.viewModelMethodName = updatedValue,
                    targetScript.viewModelMethodName,
                    MemberInfoToString(method),
                    "Set bound view-model method"
                ),
                bindableMethods
                    .OrderBy(method => method.ReflectedType.Name)
                    .ThenBy(method => method.Name)
                    .ToArray()
            );
        }

        /// <summary>
        /// Check whether each of the properties on the object have been changed from the value in the prefab.
        /// </summary>
        private void UpdatePrefabModifiedProperties()
        {
            var property = serializedObject.GetIterator();
            property.Next(true);
            do
            {
                switch (property.name)
                {
                    case "uiEventName":
                        viewEventPrefabModified = property.prefabOverride;
                        break;

                    case "viewModelMethodName":
                        viewModelMethodPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}
