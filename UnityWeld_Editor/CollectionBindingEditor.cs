using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(CollectionBinding))]
    class CollectionBindingEditor : BaseBindingEditor
    {
        private CollectionBinding targetScript;

        private bool viewModelPrefabModified;
        private bool templatesRootPrefabModified;

        private void OnEnable()
        {
            // Initialise everything
            targetScript = (CollectionBinding)target;
        }

        public override void OnInspectorGUI()
        {
            UpdatePrefabModifiedProperties();

            var defaultLabelStyle = EditorStyles.label.fontStyle;
            EditorStyles.label.fontStyle = viewModelPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            ShowViewModelPropertyMenu(
                new GUIContent("View-model property", "Property on the view-model to bind to."),
                TypeResolver.FindBindableCollectionProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                property => true
            );

            EditorStyles.label.fontStyle = templatesRootPrefabModified ? FontStyle.Bold : defaultLabelStyle;

            UpdateProperty(
                updatedValue => targetScript.templatesRoot = updatedValue,
                targetScript.templatesRoot,
                (GameObject)EditorGUILayout.ObjectField(
                    new GUIContent("Collection templates", "Parent object for all templates to copy and bind to items in the collection."), 
                    targetScript.templatesRoot, 
                    typeof(GameObject), 
                    true
                ),
                "Set collection templates root"
            );

            EditorStyles.label.fontStyle = defaultLabelStyle;
        }

        /// <summary>
        /// Check whether each of the properties on the object have been changed from the value in the prefab.
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
                    case "viewModelPropertyName":
                        viewModelPrefabModified = property.prefabOverride;
                        break;

                    case "templatesRoot":
                        templatesRootPrefabModified = property.prefabOverride;
                        break;
                }
            }
            while (property.Next(false));
        }
    }
}
