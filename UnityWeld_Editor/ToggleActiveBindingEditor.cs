using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(ToggleActiveBinding))]
    public class ToggleActiveBindingEditor : BaseBindingEditor
    {
        private ToggleActiveBinding targetScript;

        private AnimBool viewAdapterOptionsFade;

        private bool viewAdapterPrefabModified;
        private bool viewAdapterOptionsPrefabModified;
        private bool viewModelPropertyPrefabModified;

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
            if (CannotModifyInPlayMode())
            {
                GUI.enabled = false;
            }

            UpdatePrefabModifiedProperties();

            var defaultLabelStyle = EditorStyles.label.fontStyle;

            var viewPropertyType = typeof(bool);

            var viewAdapterTypeNames = GetAdapterTypeNames(
                type => TypeResolver.IsTypeCastableTo(
                    TypeResolver.FindAdapterAttribute(type).OutputType, viewPropertyType
                )
            );

            EditorStyles.label.fontStyle = viewAdapterPrefabModified
                ? FontStyle.Bold 
                : defaultLabelStyle;

            ShowAdapterMenu(
                new GUIContent(
                    "View adapter", 
                    "Adapter that converts values sent from the view-model to the view."
                ),
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

            Type adapterType;
            viewAdapterOptionsFade.target = ShouldShowAdapterOptions(
                targetScript.ViewAdapterTypeName,
                out adapterType
            );

            EditorStyles.label.fontStyle = viewAdapterOptionsPrefabModified
                ? FontStyle.Bold
                : defaultLabelStyle;

            ShowAdapterOptionsMenu(
                "View adapter options",
                adapterType,
                options => targetScript.ViewAdapterOptions = options,
                targetScript.ViewAdapterOptions,
                viewAdapterOptionsFade.faded
            );

            EditorGUILayout.Space();

            EditorStyles.label.fontStyle = viewModelPropertyPrefabModified
                ? FontStyle.Bold 
                : defaultLabelStyle;

            var adaptedViewPropertyType = AdaptTypeBackward(
                viewPropertyType, 
                targetScript.ViewAdapterTypeName
            );
            ShowViewModelPropertyMenu(
                new GUIContent(
                    "View-model property", 
                    "Property on the view-model to bind to."
                ),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.ViewModelPropertyName = updatedValue,
                targetScript.ViewModelPropertyName,
                property => TypeResolver.IsTypeCastableTo(property.PropertyType, adaptedViewPropertyType) 
            );

            EditorStyles.label.fontStyle = defaultLabelStyle;
        }

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
                    case "viewAdapterTypeName":
                        viewAdapterPrefabModified = property.prefabOverride;
                        break;

                    case "viewAdapterOptions":
                        viewAdapterOptionsPrefabModified = property.prefabOverride;
                        break;

                    case "viewModelPropertyName":
                        viewModelPropertyPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}
