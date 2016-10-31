using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityUI.Binding;

namespace UnityUI.Internal
{
    /// <summary>
    /// Helper class for setting up the factory for use in the editor.
    /// </summary>
    public static class TypeResolver
    {
        private static Type[] typesWithBindingAttribute;

        public static Type[] TypesWithBindingAttribute
        {
            get
            {
                if (typesWithBindingAttribute == null)
                {
                    typesWithBindingAttribute = FindTypesMarkedByAttribute(typeof(BindingAttribute));
                }

                return typesWithBindingAttribute;
            }
        }

        private static Type[] typesWithAdapterAttribute;

        public static Type[] TypesWithAdapterAttribute
        {
            get
            {
                if (typesWithAdapterAttribute == null)
                {
                    typesWithAdapterAttribute = FindTypesMarkedByAttribute(typeof(AdapterAttribute));
                }

                return typesWithAdapterAttribute;
            }
        }

        /// <summary>
        /// Find all types marked with the [Binding] attribute.
        /// </summary>
        private static Type[] FindTypesMarkedByAttribute(Type attributeType)
        {
            var typesFound = new List<Type>();

            foreach (var type in GetAllTypes())
            {
                try
                {
                    if (type.GetCustomAttributes(attributeType, false).Any())
                    {
                        typesFound.Add(type);
                    }
                }
                catch (Exception)
                {
                    continue; // Some tyupes throw an exception when we try to use reflection on them.
                }
            }

            return typesFound.ToArray();
        }

        /// <summary>
        /// Returns an enumerable of all known types.
        /// </summary>
        private static IEnumerable<Type> GetAllTypes()
        {
            var assemblies =
                AppDomain.CurrentDomain.GetAssemblies()
                    // Automatically exclude the Unity assemblies, which throw exceptions when we try to access them.
                    .Where(a =>
                        !a.FullName.StartsWith("UnityEngine") &&
                        !a.FullName.StartsWith("UnityEditor"));

            foreach (var assembly in assemblies)
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (Exception)
                {
                    // Ignore assemblies that can't be loaded.
                    continue;
                }

                foreach (var type in types)
                {
                    yield return type;
                }
            }
        }

        /// <summary>
        /// Find a particular type by its short name.
        /// </summary>
        public static Type FindAdapterType(string typeName)
        {
            var matchingTypes = TypesWithAdapterAttribute.Where(type => type.Name == typeName);
            if (!matchingTypes.Any())
            {
                return null;
            }

            if (matchingTypes.Skip(1).Any())
            {
                throw new ApplicationException("Multiple types match: " + typeName);
            }

            return matchingTypes.First();               
        }

        /// <summary>
        /// Find the [Adapter] attribute for a particular type.
        /// Returns null if there is no such attribute.
        /// </summary>
        public static AdapterAttribute FindAdapterAttribute(Type adapterType)
        {
            return adapterType
                .GetCustomAttributes(typeof(AdapterAttribute), false)
                .Cast<AdapterAttribute>()
                .FirstOrDefault();
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
        public static IEnumerable<Type> GetAvailableViewModelTypes(AbstractMemberBinding memberBinding)
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
