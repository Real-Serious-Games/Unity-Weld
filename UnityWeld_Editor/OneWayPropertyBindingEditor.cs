using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(OneWayPropertyBinding))]
    class OneWayPropertyBindingEditor : BaseBindingEditor
    {
        public override void OnInspectorGUI()
        {
            // Initialise reference to target script
            var targetScript = (OneWayPropertyBinding)target;

            Type viewPropertyType = null;
            ShowViewPropertyMenu(
                new GUIContent("View property", "Property on the view to bind to"),
                targetScript,
                PropertyFinder.GetBindableProperties(targetScript.gameObject)
                    .OrderBy(property => property.PropertyInfo.ReflectedType.Name)
                    .ThenBy(property => property.PropertyInfo.Name)
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
                new GUIContent("View adapter", "Adapter that converts values sent from the view-model to the view."),
                viewAdapterTypeNames,
                targetScript.viewAdapterTypeName,
                newValue =>
                {
                    UpdateProperty(
                        updatedValue => targetScript.viewAdapterTypeName = updatedValue,
                        targetScript.viewAdapterTypeName,
                        newValue
                    );
                }
            );

            var adaptedViewPropertyType = AdaptTypeBackward(viewPropertyType, targetScript.viewAdapterTypeName);
            ShowViewModelPropertyMenu(
                new GUIContent("View-model property", "Property on the view-model to bind to."),
                targetScript,
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                property => property.PropertyType == adaptedViewPropertyType
            );
        }
    }
}
