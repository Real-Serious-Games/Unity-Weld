using System;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(OneWayPropertyBinding))]
    class OneWayPropertyBindingEditor : BaseBindingEditor
    {
        private OneWayPropertyBinding targetScript;

        private AnimBool viewAdapterOptionsFade;

        // Whether each property in the target differs from the prefab it uses.
        private bool viewAdapterPrefabModified;
        private bool viewAdapterOptionsPrefabModified;
        private bool viewModelPropertyPrefabModified;
        private bool viewPropertyPrefabModified;

        private void OnEnable()
        {
            // Initialise reference to target script
            targetScript = (OneWayPropertyBinding)target;

            Type adapterType;

            viewAdapterOptionsFade = new AnimBool(
                ShouldShowAdapterOptions(targetScript.viewAdapterTypeName, out adapterType)
            );

            viewAdapterOptionsFade.valueChanged.AddListener(Repaint);
        }

        private void OnDisable()
        {
            viewAdapterOptionsFade.valueChanged.RemoveListener(Repaint);
        }

        public override void OnInspectorGUI()
        {
            UpdatePrefabModifiedProperties();

            var defaultLabelStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = viewPropertyPrefabModified ? FontStyle.Bold : defaultLabelStyle;

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

            // Don't let the user set anything else until they've chosen a view property.
            var guiPreviouslyEnabled = GUI.enabled;
            if (string.IsNullOrEmpty(targetScript.uiPropertyName))
            {
                GUI.enabled = false;
            }

            var viewAdapterTypeNames = GetAdapterTypeNames(
                type => viewPropertyType == null || 
                    TypeResolver.FindAdapterAttribute(type).OutputType == viewPropertyType
            );

            EditorStyles.label.fontStyle = viewAdapterPrefabModified ? FontStyle.Bold : defaultLabelStyle;

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

            Type adapterType;
            viewAdapterOptionsFade.target = ShouldShowAdapterOptions(targetScript.viewAdapterTypeName, out adapterType);

            EditorStyles.label.fontStyle = viewAdapterOptionsPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            ShowAdapterOptionsMenu(
                "View adapter options", 
                adapterType, 
                options => targetScript.viewAdapterOptions = options,
                targetScript.viewAdapterOptions,
                viewAdapterOptionsFade.faded
            );

            EditorGUILayout.Space();

            EditorStyles.label.fontStyle = viewModelPropertyPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            var adaptedViewPropertyType = AdaptTypeBackward(viewPropertyType, targetScript.viewAdapterTypeName);
            ShowViewModelPropertyMenu(
                new GUIContent("View-model property", "Property on the view-model to bind to."),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                property => property.PropertyType == adaptedViewPropertyType
            );

            GUI.enabled = guiPreviouslyEnabled;

            EditorStyles.label.fontStyle = defaultLabelStyle;
        }

        /// <summary>
        /// Check whether each of the properties on the object have been changed from the value in the prefab.
        /// </summary>
        private void UpdatePrefabModifiedProperties()
        {
            var property = serializedObject.GetIterator();
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

                    case "uiPropertyName":
                        viewPropertyPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}
