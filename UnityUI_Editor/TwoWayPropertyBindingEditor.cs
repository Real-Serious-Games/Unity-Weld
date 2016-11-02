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

            ShowEventMenu(
                targetScript,
                UnityEventWatcher.GetBindableEvents(targetScript.gameObject)
                .OrderBy(evt => evt.Name)
                    .ToArray(),
                    updatedValue => targetScript.uiEventName = updatedValue,
                targetScript.uiEventName
                );

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

            var adaptedViewPropertyType = AdaptType(viewPropertyType, targetScript.viewAdapterTypeName);
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

            var adaptedExceptionPropertyType = AdaptType(typeof(Exception), targetScript.exceptionAdapterTypeName);
            ShowViewModelPropertyMenu(
                "Exception property",
                targetScript, 
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.exceptionPropertyName = updatedValue,
                targetScript.exceptionPropertyName,
                property => property.PropertyType == adaptedExceptionPropertyType
            );

        }
    }
}
