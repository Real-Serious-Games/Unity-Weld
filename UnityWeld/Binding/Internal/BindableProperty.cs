using System;
using System.Reflection;

namespace UnityWeld.Binding.Internal
{
    /// <summary>
    /// Data structure combining a bindable property with the view model it belongs to.
    /// This is needed because we can't always rely on PropertyInfo.ReflectedType
    /// returning the type of the view model if the property was declared in an interface
    /// that the view model inherits from.
    /// </summary>
    public class BindableProperty
    {
        /// <summary>
        /// The bindable property info.
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// View model that the property belongs to.
        /// </summary>
        public Type ViewModelType { get; private set; }

        public BindableProperty(PropertyInfo property, Type viewModelType)
        {
            Property = property;
            ViewModelType = viewModelType;
        }

        public override string ToString()
        {
            return string.Concat(ViewModelType.ToString(), ".", Property.Name);
        }
    }
}
