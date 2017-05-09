using System;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(TwoWayPropertyBinding))]
    class TwoWayPropertyBindingEditor : BaseBindingEditor
    {
        private TwoWayPropertyBinding targetScript;

        private AnimBool viewAdapterOptionsFade;
        private AnimBool viewModelAdapterOptionsFade;
        private AnimBool exceptionAdapterOptionsFade;

        // Whether properties in the target script differ from the value in the prefab.
        // Needed to know which ones to display as bold in the inspector.
        bool viewEventPrefabModified;
        bool viewPropertyPrefabModified;
        bool viewAdapterPrefabModified;
        bool viewAdapterOptionsPrefabModified;

        bool viewModelPropertyPrefabModified;
        bool viewModelAdapterPrefabModified;
        bool viewModelAdapterOptionsPrefabModified;

        bool exceptionPropertyPrefabModified;
        bool exceptionAdapterPrefabModified;
        bool exceptionAdapterOptionsPrefabModified;

        private void OnEnable()
        {
            targetScript = (TwoWayPropertyBinding)target;

            Type adapterType;
            viewAdapterOptionsFade = new AnimBool(
                ShouldShowAdapterOptions(targetScript.viewAdapterTypeName, out adapterType)
            );
            viewModelAdapterOptionsFade = new AnimBool(
                ShouldShowAdapterOptions(targetScript.viewModelAdapterTypeName, out adapterType)
            );
            exceptionAdapterOptionsFade = new AnimBool(
                ShouldShowAdapterOptions(targetScript.exceptionAdapterTypeName, out adapterType)
            );

            viewAdapterOptionsFade.valueChanged.AddListener(Repaint);
            viewModelAdapterOptionsFade.valueChanged.AddListener(Repaint);
            exceptionAdapterOptionsFade.valueChanged.AddListener(Repaint);
        }

        private void OnDisable()
        {
            viewAdapterOptionsFade.valueChanged.RemoveListener(Repaint);
            viewModelAdapterOptionsFade.valueChanged.RemoveListener(Repaint);
            exceptionAdapterOptionsFade.valueChanged.RemoveListener(Repaint);
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

            EditorStyles.label.fontStyle = viewPropertyPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            Type viewPropertyType;
            ShowViewPropertyMenu(
                new GUIContent("View property", "Property on the view to bind to"),
                PropertyFinder.GetBindableProperties(targetScript.gameObject)
                    .OrderBy(prop => prop.ReflectedType.Name)
                    .ThenBy(prop => prop.Name)
                    .ToArray(),
                updatedValue => targetScript.uiPropertyName = updatedValue,
                targetScript.uiPropertyName,
                out viewPropertyType
            );

            // Don't let the user set other options until they've set the event and view property.
            var guiPreviouslyEnabled = GUI.enabled;
            if (string.IsNullOrEmpty(targetScript.uiEventName) || string.IsNullOrEmpty(targetScript.uiPropertyName))
            {
                GUI.enabled = false;
            }

            var viewAdapterTypeNames = GetAdapterTypeNames(
                type => viewPropertyType == null ||
                    TypeResolver.FindAdapterAttribute(type).OutputType == viewPropertyType
            );

            EditorStyles.label.fontStyle = viewAdapterPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            ShowAdapterMenu(
                new GUIContent("View adapter", "Adapter that converts values sent from the view-model to the view."),
                viewAdapterTypeNames,
                targetScript.viewAdapterTypeName,
                newValue =>
                {
                    // Get rid of old adapter options if we changed the type of the adapter.
                    if (newValue != targetScript.viewAdapterTypeName)
                    {
                        Undo.RecordObject(targetScript, "Set view adapter options");
                        targetScript.viewAdapterOptions = null;
                    }

                    UpdateProperty(
                        updatedValue => targetScript.viewAdapterTypeName = updatedValue,
                        targetScript.viewAdapterTypeName,
                        newValue,
                        "Set view adapter"
                    );
                }
            );

            EditorStyles.label.fontStyle = viewAdapterOptionsPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            Type viewAdapterType;
            viewAdapterOptionsFade.target = ShouldShowAdapterOptions(targetScript.viewAdapterTypeName, out viewAdapterType);
            ShowAdapterOptionsMenu(
                "View adapter options",
                viewAdapterType,
                options => targetScript.viewAdapterOptions = options,
                targetScript.viewAdapterOptions,
                viewAdapterOptionsFade.faded
            );

            EditorGUILayout.Space();

            EditorStyles.label.fontStyle = viewModelPropertyPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            var adaptedViewPropertyType = AdaptTypeBackward(viewPropertyType, targetScript.viewAdapterTypeName);
            ShowViewModelPropertyMenu(
                new GUIContent("View-model property", "Property on the view-model to bind to."),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                prop => prop.PropertyType == adaptedViewPropertyType
            );

            var viewModelAdapterTypeNames = GetAdapterTypeNames(
                type => adaptedViewPropertyType == null ||
                    TypeResolver.FindAdapterAttribute(type).OutputType == adaptedViewPropertyType
            );

            EditorStyles.label.fontStyle = viewModelAdapterPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            ShowAdapterMenu(
                new GUIContent("View-model adapter", "Adapter that converts from the view back to the view-model"),
                viewModelAdapterTypeNames,
                targetScript.viewModelAdapterTypeName,
                newValue =>
                {
                    if (newValue != targetScript.viewModelAdapterTypeName)
                    {
                        Undo.RecordObject(targetScript, "Set view-model adapter options");
                        targetScript.viewModelAdapterOptions = null;
                    }

                    UpdateProperty(
                        updatedValue => targetScript.viewModelAdapterTypeName = updatedValue,
                        targetScript.viewModelAdapterTypeName,
                        newValue,
                        "Set view-model adapter"
                    );
                }
            );

            EditorStyles.label.fontStyle = viewModelAdapterOptionsPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            Type viewModelAdapterType;
            viewModelAdapterOptionsFade.target = ShouldShowAdapterOptions(targetScript.viewModelAdapterTypeName, out viewModelAdapterType);
            ShowAdapterOptionsMenu(
                "View-model adapter options",
                viewModelAdapterType,
                options => targetScript.viewModelAdapterOptions = options,
                targetScript.viewModelAdapterOptions,
                viewModelAdapterOptionsFade.faded
            );

            EditorGUILayout.Space();

            var expectionAdapterTypeNames = GetAdapterTypeNames(
                type => TypeResolver.FindAdapterAttribute(type).InputType == typeof(Exception)
            );

            EditorStyles.label.fontStyle = exceptionPropertyPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            var adaptedExceptionPropertyType = AdaptTypeForward(typeof(Exception), targetScript.exceptionAdapterTypeName);
            ShowViewModelPropertyMenu(
                new GUIContent("Exception property", "Property on the view-model to bind the exception to."),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.exceptionPropertyName = updatedValue,
                targetScript.exceptionPropertyName,
                prop => prop.PropertyType == adaptedExceptionPropertyType
            );

            EditorStyles.label.fontStyle = exceptionAdapterPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            ShowAdapterMenu(
                new GUIContent("Exception adapter", "Adapter that handles exceptions thrown by the view-model adapter"),
                expectionAdapterTypeNames,
                targetScript.exceptionAdapterTypeName,
                newValue =>
                {
                    if (newValue != targetScript.exceptionAdapterTypeName)
                    {
                        Undo.RecordObject(targetScript, "Set exception adapter options");
                        targetScript.exceptionAdapterOptions = null;
                    }

                    UpdateProperty(
                        updatedValue => targetScript.exceptionAdapterTypeName = updatedValue,
                        targetScript.exceptionAdapterTypeName,
                        newValue,
                        "Set exception adapter"
                    );
                }
            );

            EditorStyles.label.fontStyle = exceptionAdapterOptionsPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            Type exceptionAdapterType;
            exceptionAdapterOptionsFade.target = ShouldShowAdapterOptions(targetScript.exceptionAdapterTypeName, out exceptionAdapterType);
            ShowAdapterOptionsMenu(
                "Exception adapter options",
                exceptionAdapterType,
                options => targetScript.exceptionAdapterOptions = options,
                targetScript.exceptionAdapterOptions,
                exceptionAdapterOptionsFade.faded
            );

            EditorStyles.label.fontStyle = defaultLabelStyle;

            GUI.enabled = guiPreviouslyEnabled;
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
                        viewPropertyPrefabModified = property.prefabOverride;
                        break;

                    case "uiPropertyName":
                        viewPropertyPrefabModified = property.prefabOverride;
                        break;

                    case "viewAdapterTypeName":
                        viewAdapterPrefabModified = property.prefabOverride;
                        break;

                    case "viewAdapterOptions":
                        viewAdapterOptionsPrefabModified = property.prefabOverride;
                        break;

                    case "viewModelPropertyName":
                        viewModelPropertyPrefabModified = property.prefabOverride;
                        break;

                    case "viewModelAdapterTypeName":
                        viewModelAdapterPrefabModified = property.prefabOverride;
                        break;

                    case "viewModelAdapterOptions":
                        viewAdapterPrefabModified = property.prefabOverride;
                        break;

                    case "exceptionPropertyName":
                        exceptionPropertyPrefabModified = property.prefabOverride;
                        break;

                    case "exceptionAdapterTypeName":
                        exceptionAdapterPrefabModified = property.prefabOverride;
                        break;

                    case "exceptionAdapterOptions":
                        exceptionAdapterOptionsPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}
