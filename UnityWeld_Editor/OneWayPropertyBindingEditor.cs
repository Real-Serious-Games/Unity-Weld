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
                newValue =>
                {
                    UpdateProperty(
                        updatedValue => targetScript.viewAdapterTypeName = updatedValue,
                        targetScript.viewAdapterTypeName,
                        newValue
                    );
                }
            );

            if (!string.IsNullOrEmpty(targetScript.viewAdapterTypeName))
            {
                var adapterOptionsType = TypeResolver.FindAdapterAttribute(
                    TypeResolver.FindAdapterType(targetScript.viewAdapterTypeName)
                ).OptionsType;

                if (adapterOptionsType != typeof(AdapterOptions))
                {
                    var oldAdapterOptions = targetScript.viewAdapterOptions;
                    var adapterOptionsName = "View adapter options";
                    var newAdapterOptions = (AdapterOptions)EditorGUILayout.ObjectField(adapterOptionsName, oldAdapterOptions, adapterOptionsType, false);
                    if (newAdapterOptions != oldAdapterOptions)
                    {
                        targetScript.viewAdapterOptions = newAdapterOptions;
                        InspectorUtils.MarkSceneDirty(targetScript.gameObject);
                    }
                }
            }

            var adaptedViewPropertyType = AdaptTypeBackward(viewPropertyType, targetScript.viewAdapterTypeName);
            ShowViewModelPropertyMenu(
                new GUIContent("View-model property", "Property on the view-model to bind to."),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                property => property.PropertyType == adaptedViewPropertyType
            );
        }
    }
}
