using UnityEngine;
using UnityEditor;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;
using System.Linq;
using System.Reflection;

namespace UnityWeld_Editor
{
    /// <summary>
    /// Inspector window for SubViewModelBinding
    /// </summary>
    [CustomEditor(typeof(SubViewModelBinding))]
    public class SubViewModelBindingEditor : BaseBindingEditor
    {
        private SubViewModelBinding targetScript;

        private void OnEnable()
        {
            targetScript = (SubViewModelBinding)target;
        }

        public override void OnInspectorGUI()
        {
            var bindableProperties = FindBindableProperties();

            ShowViewModelPropertyMenu(
                new GUIContent("Sub view-model property"),
                bindableProperties,
                updatedValue => 
                {
                    targetScript.viewModelPropertyName = updatedValue;

                    targetScript.viewModelTypeName = bindableProperties
                        .Where(prop => MemberInfoToString(prop) == updatedValue)
                        .Single()
                        .PropertyType.ToString();
                },
                targetScript.viewModelPropertyName,
                p => true
            );
        }

        private PropertyInfo[] FindBindableProperties()
        {
            return TypeResolver.FindBindableProperties(targetScript)
                .Where(property => property
                    .PropertyType
                    .GetCustomAttributes(typeof(BindingAttribute), false)
                    .Any()
                )
                .ToArray();
        }
    }
}