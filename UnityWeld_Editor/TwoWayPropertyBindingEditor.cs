using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(TwoWayPropertyBinding))]
    class PropertyBindingEditor : BaseBindingEditor
    {
        public override void OnInspectorGUI()
        {
            var targetScript = (TwoWayPropertyBinding)target;

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

            ShowAdapterOptionsMenu(
                "View adapter options",
                targetScript.viewAdapterTypeName,
                options => targetScript.viewAdapterOptions = options,
                targetScript.viewAdapterOptions
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

            ShowAdapterOptionsMenu(
                "View-model adapter options",
                targetScript.viewModelAdapterTypeName,
                options => targetScript.viewModelAdapterOptions = options,
                targetScript.viewModelAdapterOptions
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

            ShowAdapterOptionsMenu(
                "Exception adapter options",
                targetScript.exceptionAdapterTypeName,
                options => targetScript.exceptionAdapterOptions = options,
                targetScript.exceptionAdapterOptions
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
