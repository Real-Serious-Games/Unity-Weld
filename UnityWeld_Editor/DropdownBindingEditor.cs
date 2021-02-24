using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(DropdownBinding))]
    public class DropdownBindingEditor : BaseBindingEditor
    {
        private DropdownBinding targetScript;

        private readonly bool viewPropertyPrefabModified;
        private AnimBool optionsAdapterOptionsFade;

        private void OnEnable()
        {
            // Initialise reference to target script
            targetScript = (DropdownBinding)target;

            Type adapterType;

            optionsAdapterOptionsFade = new AnimBool(
                ShouldShowAdapterOptions(targetScript.optionsViewAdapterTypeName, out adapterType)
            );

            optionsAdapterOptionsFade.valueChanged.AddListener(Repaint);
        }

        private void OnDisable()
        {
            optionsAdapterOptionsFade.valueChanged.RemoveListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            UpdatePrefabModifiedProperties();

            var defaultLabelStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = viewPropertyPrefabModified
                ? FontStyle.Bold
                : defaultLabelStyle;

            ShowViewModelPropertyMenu(
                new GUIContent(
                    "Options: View-model property",
                    "Property on the view-model to bind Options to."
                ),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.ViewModelOptionsPropertyName = updatedValue,
                targetScript.ViewModelOptionsPropertyName,
                property => property.PropertyType == typeof(string[]) || property.PropertyType == typeof(System.Collections.Generic.List<string>)
            );

            Type viewPropertyType = typeof(string[]);

            var viewAdapterTypeNames = GetAdapterTypeNames(
                type =>
                {
                    return viewPropertyType == null ||
                                        TypeResolver.FindAdapterAttribute(type).OutputType == viewPropertyType;
                });

            var guiPreviouslyEnabled = GUI.enabled;

            ShowAdapterMenu(
                new GUIContent(
                    "View adapter",
                    "Adapter that converts values sent from the view-model to the view."
                ),
                viewAdapterTypeNames,
                targetScript.optionsViewAdapterTypeName,
                newValue =>
                {
                    // Get rid of old adapter options if we changed the type of the adapter.
                    if (newValue != targetScript.optionsViewAdapterTypeName)
                    {
                        Undo.RecordObject(targetScript, "Set view adapter options");
                        targetScript.optionsViewAdapterTypeName = null;
                    }

                    UpdateProperty(
                        updatedValue => targetScript.optionsViewAdapterTypeName = updatedValue,
                        targetScript.optionsViewAdapterTypeName,
                        newValue,
                        "Set view adapter"
                    );
                }
            );

            Type adapterType;
            optionsAdapterOptionsFade.target = ShouldShowAdapterOptions(
                targetScript.optionsViewAdapterTypeName,
                out adapterType
            );

            ShowAdapterOptionsMenu(
                "View adapter options",
                adapterType,
                options => targetScript.optionsViewAdapterOptions = options,
                targetScript.optionsViewAdapterOptions,
                optionsAdapterOptionsFade.faded
            );

            

            ShowViewModelPropertyMenu(
                new GUIContent(
                    "Selected Item : View-model property",
                    "Property on the view-model to bind the selected item to."
                ),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.ViewModelSelectionPropertyName = updatedValue,
                targetScript.ViewModelSelectionPropertyName,
                property => property.PropertyType == typeof(string)
            );

            GUI.enabled = guiPreviouslyEnabled;

            EditorStyles.label.fontStyle = defaultLabelStyle;
        }

        /// <summary>
        /// Check whether each of the properties on the object have been changed 
        /// from the value in the prefab.
        /// </summary>
        private void UpdatePrefabModifiedProperties()
        {
            var property = serializedObject.GetIterator();
            // Need to call Next(true) to get the first child. Once we have it, Next(false)
            // will iterate through the properties.
            property.Next(true);
            do
            {
                switch (property.name)
                {

                    default:
                        //Debug.Log(property.name);
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}