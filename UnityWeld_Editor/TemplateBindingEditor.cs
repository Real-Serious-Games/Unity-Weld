using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    [CustomEditor(typeof(TemplateBinding))]
    class TemplateBindingEditor : BaseBindingEditor
    {

        public override void OnInspectorGUI()
        {
            // Initialise everything
            var targetScript = (TemplateBinding)target;

            ShowViewModelPropertyMenu(
                new GUIContent("Template property", "Property on the view model to use for selecting templates."),
                TypeResolver.FindBindableProperties(targetScript),
                updatedValue => targetScript.viewModelPropertyName = updatedValue,
                targetScript.viewModelPropertyName,
                property => true
            );

            UpdateProperty(
                updatedValue => targetScript.templatesRoot = updatedValue,
                targetScript.templatesRoot,
                (GameObject)EditorGUILayout.ObjectField(
                    new GUIContent("Templates root object", "Parent object to the objects we want to use as templates."),
                    targetScript.templatesRoot, 
                    typeof(GameObject), 
                    true
                ),
                "Set template binding root object"
            );
        }
    }
}
