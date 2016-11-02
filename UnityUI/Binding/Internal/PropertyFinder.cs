using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityUI.Binding
{
    /// <summary>
    /// Helper to find bindable properties.
    /// </summary>
    public class PropertyFinder
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
        /// Information needed to bind to a property on a component. 
        /// </summary>
        public struct BindablePropertyInfo
        {
            /// <summary>
            /// PropertyInfo of the property to bind to.
            /// </summary>
            public PropertyInfo PropertyInfo { get; set; }

            /// <summary>
            /// Object the property belongs to.
            /// </summary>
            public UnityEngine.Component Object { get; set; }

            public BindablePropertyInfo(PropertyInfo propertyInfo, UnityEngine.Component obj)
                : this()
            {
                PropertyInfo = propertyInfo;
                Object = obj;
            }
        }

        /// <summary>
        /// Use reflection to find all components with properties we can bind to.
        /// </summary>
        public static IEnumerable<BindablePropertyInfo> GetBindableProperties(GameObject gameObject) //todo: Maybe move this to the TypeResolver.
        {
            return gameObject.GetComponents<UnityEngine.Component>()
                .SelectMany(component =>
                {
                    var propertiesOnComponent = new List<BindablePropertyInfo>();
                    foreach (var propertyInfo in component.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    {
                        propertiesOnComponent.Add(
                            new BindablePropertyInfo(propertyInfo, component));
                    }
                    return propertiesOnComponent;
                })
                .Where(prop => !hiddenTypes.Contains(prop.PropertyInfo.ReflectedType))
                .Where(prop => !prop.PropertyInfo.GetCustomAttributes(typeof(ObsoleteAttribute), true).Any());
        }
    }
}
