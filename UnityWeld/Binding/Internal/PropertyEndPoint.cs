using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace UnityWeld.Binding
{
    /// <summary>
    /// Represents an attachment to a property via reflection.
    /// </summary>
    public class PropertyEndPoint
    {
        /// <summary>
        /// The object that owns the property.
        /// </summary>
        private object propertyOwner;

        /// <summary>
        /// The name of the property.
        /// </summary>
        private string propertyName;

        /// <summary>
        /// Cached reference to the property.
        /// </summary>
        private PropertyInfo property;

        /// <summary>
        /// Adapter for converting values that are set on the property.
        /// </summary>
        private IAdapter adaptor;

        public PropertyEndPoint(object propertyOwner, string propertyName, IAdapter adaptor, string endPointType, Component context)
        {
            this.propertyOwner = propertyOwner;
            this.adaptor = adaptor;
            var type = propertyOwner.GetType();

            if (string.IsNullOrEmpty(propertyName))
            {
                Debug.LogError("Property not specified for type '" + type.Name + "'.", context);
                return;
            }

            this.propertyName = propertyName;
            this.property = type.GetProperty(propertyName);
            if (this.property == null)
            {
                Debug.LogError("Property '" + propertyName + "' not found on " + endPointType  + " '" + type.Name + "'.", context);
            }
        }

        /// <summary>
        /// Get the value of the property.
        /// </summary>
        public object GetValue()
        {
            return property != null ? property.GetValue(propertyOwner, null) : null;
        }

        /// <summary>
        /// Set the value of the property.
        /// </summary>
        public void SetValue(object input)
        {
            if (property == null)
            {
                return;
            }

            if (adaptor != null)
            {
                input = adaptor.Convert(input);
            }

            property.SetValue(propertyOwner, input, null);
        }

        public override string ToString()
        {
            if (property != null)
            {
                return "!! property not found !!";
            }

            return propertyOwner.GetType().Name + "." + property.Name + " (" + property.PropertyType.Name + ")";
        }

        /// <summary>
        /// Watch the property for changes.
        /// </summary>
        public PropertyWatcher Watch(Action changed)
        {
            return new PropertyWatcher(propertyOwner, propertyName, changed);
        }
    }
}
