using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Exceptions;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(ToggleActiveBinding))]
    public class ToggleActiveBindingEditor : BaseBindingEditor
    {
        private ToggleActiveBinding targetScript;

        private AnimBool viewAdapterOptionsFade;

        private void OnEnable()
        {
            targetScript = (ToggleActiveBinding)target;

            Type adapterType;

            viewAdapterOptionsFade = new AnimBool(
                ShouldShowAdapterOptions(targetScript.ViewAdapterTypeName, out adapterType)
            );

            viewAdapterOptionsFade.valueChanged.AddListener(Repaint);
        }

        private void OnDisable()
        {
            viewAdapterOptionsFade.valueChanged.RemoveListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            var defaultLabelStyle = EditorStyles.label.fontStyle;

            Type viewPropertyType = typeof(bool);

            var viewAdapterTypeNames = GetAdapterTypeNames(
                type => viewPropertyType == null ||
                    TypeResolver.FindAdapterAttribute(type).OutputType == viewPropertyType
            );

            EditorStyles.label.fontStyle = DoesFieldOverridePrefab() ? FontStyle.Bold : defaultLabelStyle;

            ShowAdapterMenu(
                new GUIContent("View adapter", "Adapter that converts values sent from the view-model to the view."),
                viewAdapterTypeNames,
                targetScript.ViewAdapterTypeName,
                newValue =>
                {
                    // Get rid of old adapter options if we changed the type of the adapter.
                    if (newValue != targetScript.ViewAdapterTypeName)
                    {
                        Undo.RecordObject(targetScript, "Set view adapter options");
                        targetScript.ViewAdapterOptions = null;
                    }

                    UpdateProperty(
                        updatedValue => targetScript.ViewAdapterTypeName = updatedValue,
                        targetScript.ViewAdapterTypeName,
                        newValue,
                        "Set view adapter"
                    );
                }
            );

            EditorStyles.label.fontStyle = DoesFieldOverridePrefab() ? FontStyle.Bold : defaultLabelStyle;

            ShowViewModelPropertyMenu(
                new GUIContent("View-model property", "Property on the view-model to bind to."),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.ViewModelPropertyName = updatedValue,
                targetScript.ViewModelPropertyName,
                property => property.PropertyType == typeof(bool)
            );

            EditorStyles.label.fontStyle = defaultLabelStyle;
        }

        private bool DoesFieldOverridePrefab()
        {
            var property = serializedObject.GetIterator();
            // Need to call Next(true) to get the first child. Once we have it, Next(false)
            // will iterate through the properties.
            property.Next(true);
            do
            {
                if (property.name == "viewModelPropertyName")
                {
                    return property.prefabOverride;
                }
            }
            while (property.Next(false));

            throw new MemberNotFoundException("Field \"viewModelPropertyName\" not found on ToggleActiveBindingEditor.");
        }
    }
}
