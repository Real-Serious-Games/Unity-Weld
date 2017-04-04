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
    class PropertyBindingEditor : BaseBindingEditor
    {
        private TwoWayPropertyBinding targetScript;

        private AnimBool viewAdapterOptionsFade;
        private AnimBool viewModelAdapterOptionsFade;
        private AnimBool exceptionAdapterOptionsFade;

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
            ShowEventMenu(
                UnityEventWatcher.GetBindableEvents(targetScript.gameObject)
                    .OrderBy(evt => evt.Name)
                    .ToArray(),
                updatedValue => targetScript.uiEventName = updatedValue,
                targetScript.uiEventName
            );

            Type viewPropertyType;
            ShowViewPropertyMenu(
                new GUIContent("View property", "Property on the view to bind to"),
                PropertyFinder.GetBindableProperties(targetScript.gameObject)
                    .OrderBy(property => property.ReflectedType.Name)
                    .ThenBy(property => property.Name)
                    .ToArray(),
                updatedValue => targetScript.uiPropertyName = updatedValue,
                targetScript.uiPropertyName,
                out viewPropertyType
            );

            var viewAdapterTypeNames = GetAdapterTypeNames(
                type => viewPropertyType == null || 
                    TypeResolver.FindAdapterAttribute(type).OutputType == viewPropertyType
            );

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

            var adaptedViewPropertyType = AdaptTypeBackward(viewPropertyType, targetScript.viewAdapterTypeName);
            ShowViewModelPropertyMenu(
                new GUIContent("View-model property", "Property on the view-model to bind to."),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                property => property.PropertyType == adaptedViewPropertyType
            );

            var viewModelAdapterTypeNames = GetAdapterTypeNames(
                type => adaptedViewPropertyType == null || 
                    TypeResolver.FindAdapterAttribute(type).OutputType == adaptedViewPropertyType
            );

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

            Type exceptionAdapterType;
            exceptionAdapterOptionsFade.target = ShouldShowAdapterOptions(targetScript.exceptionAdapterTypeName, out exceptionAdapterType);
            ShowAdapterOptionsMenu(
                "Exception adapter options",
                exceptionAdapterType,
                options => targetScript.exceptionAdapterOptions = options,
                targetScript.exceptionAdapterOptions,
                exceptionAdapterOptionsFade.faded
            );

            var adaptedExceptionPropertyType = AdaptTypeForward(typeof(Exception), targetScript.exceptionAdapterTypeName);
            ShowViewModelPropertyMenu(
                new GUIContent("Exception property", "Property on the view-model to bind the exception to."),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.exceptionPropertyName = updatedValue,
                targetScript.exceptionPropertyName,
                property => property.PropertyType == adaptedExceptionPropertyType
            );
        }
    }
}
