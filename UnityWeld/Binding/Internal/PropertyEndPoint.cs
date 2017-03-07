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
        private readonly object propertyOwner;

        /// <summary>
        /// The name of the property.
        /// </summary>
        private readonly string propertyName;

        /// <summary>
        /// Cached reference to the property.
        /// </summary>
        private readonly PropertyInfo property;

        /// <summary>
        /// Adapter for converting values that are set on the property.
        /// </summary>
        private readonly IAdapter adaptor;

        /// <summary>
        /// Options for using the adapter to convert values.
        /// </summary>
        private readonly AdapterOptions adapterOptions;

        public PropertyEndPoint(object propertyOwner, string propertyName, IAdapter adaptor, AdapterOptions options, string endPointType, Component context)
        {
            this.propertyOwner = propertyOwner;
            this.adaptor = adaptor;
            this.adapterOptions = options;
            var type = propertyOwner.GetType();

            if (string.IsNullOrEmpty(propertyName))
            {
                Debug.LogError("Property not specified for type '" + type + "'.", context);
                return;
            }

            this.propertyName = propertyName;
            this.property = type.GetProperty(propertyName);
            if (this.property == null)
            {
                Debug.LogError("Property '" + propertyName + "' not found on " + endPointType  + " '" + type + "'.", context);
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
                input = adaptor.Convert(input, adapterOptions);
            }

            property.SetValue(propertyOwner, input, null);
        }

        public override string ToString()
        {
            if (property == null)
            {
                return "!! property not found !!";
            }

            return string.Concat(propertyOwner.GetType(), ".", property.Name, " (", property.PropertyType.Name, ")");
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
