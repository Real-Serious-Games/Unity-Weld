using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityUI.Binding;

namespace UnityUI_Editor
{
    /// <summary>
    /// Helper class for setting up the factory for use in the editor.
    /// </summary>
    internal static class TypeResolver
    {
        private static Type[] typesWithBindingAttribute;

        private static Type[] TypesWithBindingAttribute
        {
            get
            {
                if (typesWithBindingAttribute == null)
                {
                    typesWithBindingAttribute = FindTypesMarkedByBindingAttribute();
                }

                return typesWithBindingAttribute;
            }
        }

        /// <summary>
        /// Find all types marked with the [Binding] attribute.
        /// </summary>
        private static Type[] FindTypesMarkedByBindingAttribute()
        {
            return ReflectionUtils.FindTypesMarkedByAttribute(typeof(BindingAttribute));
        }

        /// <summary>
        /// Return the type of a view model bound by an IViewModelBinding
        /// </summary>
        private static Type GetBoundViewType(IViewModelBinding binding)
        {
            var type = TypesWithBindingAttribute
                .Where(t => t.Name == binding.ViewModelTypeName)
                .FirstOrDefault();

            if (type == null)
            {
                throw new ApplicationException("Could not find the specified view model \"" + binding.ViewModelTypeName + "\"");
            }

            return type;
        }

        /// <summary>
        /// Scan up the hierarchy and find all the types that can be bound to 
        /// a specified MemberBinding.
        /// </summary>
        public static IEnumerable<Type> GetAvailableViewModelTypes(this AbstractMemberBinding memberBinding)
        {
            bool foundAtLeastOneBinding = false;

            var trans = memberBinding.transform;
            while (trans != null)
            {
                var viewModels = trans.GetComponents<MonoBehaviour>();
                foreach (var viewModel in viewModels)
                {
                    // Can't bind to self
                    if (viewModel == memberBinding)
                    {
                        continue;
                    }

                    // Case where a ViewModelBinding is used to bind a non-MonoBehaviour class.
                    var viewModelBinding = viewModel as IViewModelBinding;
                    if (viewModelBinding != null)
                    {
                        foundAtLeastOneBinding = true;

                        yield return GetBoundViewType(viewModelBinding);
                    }
                    else if (viewModel.GetType().GetCustomAttributes(typeof(BindingAttribute), false).Any())
                    {
                        // Case where we are binding to an existing MonoBehaviour.
                        foundAtLeastOneBinding = true;

                        yield return viewModel.GetType();
                    }
                }

                // Stop at the top level
                if (trans.GetComponent<BindingRoot>() != null)
                {
                    break;
                }

                trans = trans.parent;
            }

            if (!foundAtLeastOneBinding)
            {
                Debug.LogError("UI binding " + memberBinding.gameObject.name +
                    " must be placed underneath at least one bindable component.", memberBinding);
            }
        }
    }
}
