using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityWeld;
using UnityWeld.Binding;
using UnityWeld.Internal;
using UnityWeld_Editor;

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

            Type viewPropertyType = null;
            ShowViewPropertyMenu(
                new GUIContent("View property", "Property on the view to bind to"),
                PropertyFinder.GetBindableProperties(targetScript.gameObject)
                    .OrderBy(property => property.PropertyInfo.ReflectedType.Name)
                    .ThenBy(property => property.PropertyInfo.Name)
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
                newValue => UpdateProperty(
                    updatedValue => targetScript.viewAdapterTypeName = updatedValue,
                    targetScript.viewAdapterTypeName,
                    newValue
                )
            );

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
                new GUIContent("View-model adaptor", "Adapter that converts from the view back to the view-model"),
                viewModelAdapterTypeNames,
                targetScript.viewModelAdapterTypeName,
                (newValue) => targetScript.viewModelAdapterTypeName = newValue
            );

            var expectionAdapterTypeNames = GetAdapterTypeNames(
                type => TypeResolver.FindAdapterAttribute(type).InputType == typeof(Exception)
            );

            ShowAdapterMenu(
                new GUIContent("Exception adaptor", "Adapter that handles exceptions thrown by the view-model adapter"),
                expectionAdapterTypeNames,
                targetScript.exceptionAdapterTypeName,
                (newValue) => targetScript.exceptionAdapterTypeName = newValue
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
