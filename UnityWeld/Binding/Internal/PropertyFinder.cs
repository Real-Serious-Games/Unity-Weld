using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityWeld.Binding.Internal
{
    /// <summary>
    /// Helper to find bindable properties.
    /// </summary>
    public static class PropertyFinder
    {
        /// <summary>
        /// List of types to exclude from the types of components in the UI we can bind to.
        /// </summary>
        private static readonly HashSet<Type> hiddenTypes = new HashSet<Type>{
            typeof(AbstractMemberBinding),
            typeof(OneWayPropertyBinding),
            typeof(TwoWayPropertyBinding)
        };

        /// <summary>
        /// Use reflection to find all components with properties we can bind to.
        /// </summary>
        public static IEnumerable<PropertyInfo> GetBindableProperties(GameObject gameObject) //todo: Maybe move this to the TypeResolver.
        {
            return gameObject.GetComponents<Component>()
                .SelectMany(component => component.GetType()
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public))
                .Where(prop => !hiddenTypes.Contains(prop.ReflectedType))
                .Where(prop => !prop.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any());
        }
    }
}
