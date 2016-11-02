using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityUI.Binding;
using UnityUI.Internal;

namespace UnityUI_Editor
{
    [CustomEditor(typeof(OneWayPropertyBinding))]
    class OneWayPropertyBindingEditor : BaseBindingEditor
    {
        public override void OnInspectorGUI()
        {
            // Initialise reference to target script
            var targetScript = (OneWayPropertyBinding)target;

            var uiProperties = PropertyFinder
                .GetBindableProperties(targetScript.gameObject)
                .OrderBy(property => property.PropertyInfo.Name)
                .ToArray();

            var selectedPropertyIndex = ShowUIPropertySelector(targetScript, uiProperties);
            Type viewPropertyType = null;

            if (selectedPropertyIndex >= 0)
            {
                // Selected UI property
                UpdateProperty(
                    updatedValue => targetScript.uiPropertyName = updatedValue,
                    targetScript.uiPropertyName,
                    uiProperties[selectedPropertyIndex].Object.GetType().Name + "." + uiProperties[selectedPropertyIndex].PropertyInfo.Name
                );

                viewPropertyType = uiProperties[selectedPropertyIndex].PropertyInfo.PropertyType;
            }

            var viewAdapterTypeNames = TypeResolver.TypesWithAdapterAttribute
                .Where(type => viewPropertyType == null || TypeResolver.FindAdapterAttribute(type).OutputType == viewPropertyType)
                .Select(type => type.Name)
                .ToArray();

            ShowAdapterMenu(
                "View adaptor",
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

            Type adaptedViewPropertyType = viewPropertyType;
            if (!string.IsNullOrEmpty(targetScript.viewAdapterTypeName))
            {
                var adapterType = TypeResolver.FindAdapterType(targetScript.viewAdapterTypeName);
                if (adapterType != null)
                {
                    var adapterAttribute = TypeResolver.FindAdapterAttribute(adapterType);
                    if (adapterAttribute != null)
                    {
                        adaptedViewPropertyType = adapterAttribute.InputType;
                    }
                }               
            }

            ShowViewModelPropertyMenu(
                "View-model property",
                targetScript,
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                property => property.PropertyType == adaptedViewPropertyType
            );
        }

        /// <summary>
        /// Draws the dropdown menu for selectng a property in the UI to bind to.
        /// </summary>
        private int ShowUIPropertySelector(OneWayPropertyBinding targetScript, PropertyFinder.BindablePropertyInfo[] uiProperties)
        {
            var propertyNames = uiProperties
                .Select(prop => prop.PropertyInfo.ReflectedType.Name + "." + prop.PropertyInfo.Name)
                .ToArray();
            var selectedIndex = Array.IndexOf(propertyNames, targetScript.uiPropertyName);

            return EditorGUILayout.Popup(
                new GUIContent("View property"),
                selectedIndex,
                uiProperties
                    .Select(prop => new GUIContent(
                        prop.PropertyInfo.ReflectedType.Name + "/" + 
                        prop.PropertyInfo.Name + " : " +
                        prop.PropertyInfo.PropertyType.Name
                    ))
                    .ToArray()
            );
        }
    }
}
