using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityWeld.Binding.Internal
{
    /// <summary>
    /// Helper class for setting up the factory for use in the editor.
    /// </summary>
    public static class TypeResolver
    {
        private static Type[] typesWithBindingAttribute;

        public static IEnumerable<Type> TypesWithBindingAttribute
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

        public static IEnumerable<Type> TypesWithAdapterAttribute
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
        /// Find all types marked with the specified attribute.
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
                    // Some types throw an exception when we try to use reflection on them.
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
            var matchingTypes = TypesWithAdapterAttribute.Where(type => type.ToString() == typeName);
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
        private static Type GetViewModelType(string viewModelTypeName)
        {
            var type = TypesWithBindingAttribute
                .FirstOrDefault(t => t.ToString() == viewModelTypeName);

            if (type == null)
            {
                throw new ApplicationException("Could not find the specified view model \"" + viewModelTypeName + "\"");
            }

            return type;
        }

        /// <summary>
        /// Scan up the hierarchy and find all the types that can be bound to 
        /// a specified MemberBinding.
        /// </summary>
        private static IEnumerable<Type> FindAvailableViewModelTypes(AbstractMemberBinding memberBinding)
        {
            var foundAtLeastOneBinding = false;

            var trans = memberBinding.transform;
            while (trans != null)
            {
                var components = trans.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    // Can't bind to self or null
                    if (component == null || component == memberBinding)
                    {
                        continue;
                    }

                    // Case where a ViewModelBinding is used to bind a non-MonoBehaviour class.
                    var viewModelBinding = component as IViewModelProvider;
                    if (viewModelBinding != null)
                    {
                        foundAtLeastOneBinding = true;

                        yield return GetViewModelType(viewModelBinding.GetViewModelTypeName());
                    }
                    else if (component.GetType().GetCustomAttributes(typeof(BindingAttribute), false).Any())
                    {
                        // Case where we are binding to an existing MonoBehaviour.
                        foundAtLeastOneBinding = true;

                        yield return component.GetType();
                    }
                }

                trans = trans.parent;
            }

            if (!foundAtLeastOneBinding)
            {
                Debug.LogError("UI binding " + memberBinding.gameObject.name + " must be placed underneath at least one bindable component.", memberBinding);
            }
        }

        /// <summary>
        /// Find bindable properties in available view models.
        /// </summary>
        public static PropertyInfo[] FindBindableProperties(AbstractMemberBinding target)
        {
            return FindAvailableViewModelTypes(target)
                .SelectMany(type => type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                .Where(property => property
                    .GetCustomAttributes(false)
                    .Any(attribute => attribute is BindingAttribute) // Filter out properties that don't have [Binding].
                )
                .ToArray();
        }

        /// <summary>
        /// Get a list of methods in the view model that we can bind to.
        /// </summary>
        public static MethodInfo[] FindBindableMethods(EventBinding targetScript)
        {
            return FindAvailableViewModelTypes(targetScript)
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                .Where(method => method.GetParameters().Length == 0)
                .Where(method => method.GetCustomAttributes(typeof(BindingAttribute), false).Any() && !method.Name.StartsWith("get_")) // Exclude property getters, since we aren't doing anything with the return value of the bound method anyway.
                .ToArray();
        }

        /// <summary>
        /// Find collection properties that can be data-bound.
        /// </summary>
        public static PropertyInfo[] FindBindableCollectionProperties(CollectionBinding target)
        {
            return FindBindableProperties(target)
                .Where(property => typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                .Where(property => !typeof(string).IsAssignableFrom(property.PropertyType))
                .ToArray();
        }
    }
}
