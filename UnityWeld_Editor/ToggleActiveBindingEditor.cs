using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(ToggleActiveBinding))]
    public class ToggleActiveBindingEditor : BaseBindingEditor
    {
        private ToggleActiveBinding targetScript;

        private void OnEnable()
        {
            targetScript = (ToggleActiveBinding)target;
        }

        public override void OnInspectorGUI()
        {
            var defaultLabelStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = DoesFieldOverridePrefab() ? FontStyle.Bold : defaultLabelStyle;

            ShowViewModelPropertyMenu(
                new GUIContent("View-model property", "Property on the view-model to bind to."),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
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
                if (property.name != "viewModelPropertyName")
                {
                    return property.prefabOverride;
                }
            }
            while (property.Next(false));

            return false;
        }
    }
}
