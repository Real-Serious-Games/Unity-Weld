using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnityUI.Binding
{
    /// <summary>
    /// Bind a property in the view model to a property in the UI and subscribe to NotifyPropertyChanged
    /// so that it gets updated when the view model changes (1-way data binding).
    /// </summary>
    public class PropertyBinder : IDisposable
    {
        /// <summary>
        /// Name of the property in the view model to bind.
        /// </summary>
        public string viewModelPropertyName;

        /// <summary>
        /// UI Property to update when value changes.
        /// </summary>
        public string uiPropertyName;

        /// <summary>
        /// Property in the view model that we have bound to.
        /// </summary>
        private PropertyInfo boundViewModelProperty;

        /// <summary>
        /// Property on the UI that we have bound to.
        /// </summary>
        private BindablePropertyInfo boundUiProperty;

        private IAdapter adapter;

        private object boundViewModel;

        private GameObject gameObject;

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
        /// Set up the property binder for a specified property in the bound view model and
        /// a specified property in a component on our game object.
        /// </summary>
        public PropertyBinder(GameObject gameObject, string viewModelPropertyName, string uiPropertyName, IAdapter adapter, object boundViewModel)
        {
            this.gameObject = gameObject;
            this.viewModelPropertyName = viewModelPropertyName;
            this.uiPropertyName = uiPropertyName;
            this.adapter = adapter;
            this.boundViewModel = boundViewModel;

            // Find bound UI property
            var matchingProperties = GetBindableProperties()
                .Where(property => property.PropertyInfo.Name == uiPropertyName);

            if (!matchingProperties.Any())
            { 
                throw new ApplicationException(
                    string.Format("Could not find property {0} on {1} component on object {2}", uiPropertyName, this.adapter, gameObject.name)
                );
            }

            boundUiProperty = matchingProperties.First();

            // Bind view model
            if (!String.IsNullOrEmpty(viewModelPropertyName))
            {
                BindViewModel(viewModelPropertyName, boundViewModel);
            }
        }

        private void BindViewModel(string viewModelPropertyName, object viewModel)
        {
            var viewModelType = viewModel.GetType();
            boundViewModelProperty = viewModelType.GetProperty(viewModelPropertyName);
            if (boundViewModelProperty == null)
            {
                throw new ApplicationException("Expected property " + viewModelPropertyName + " not found on type " + viewModelType.Name + ".");
            }

            // Update the widget with the initial value from the bound property.
            var widgetValue = GetValueFromViewModel();
            UpdateUI(widgetValue);

            // Bind the property so that the widget gets updated when the view model changes.
            var notifyPropertyChanged = viewModel as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                // Make sure we listen for property changes on the view model.
                notifyPropertyChanged.PropertyChanged += viewModel_PropertyChanged;
            }
        }

        void viewModel_PropertyChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName == viewModelPropertyName)
            {
                UpdateUI(GetValueFromViewModel());
            }
        }

        /// <summary>
        /// Returns the current value of the bound property.
        /// </summary>
        private object GetValueFromViewModel()
        {
            return boundViewModelProperty.GetValue(boundViewModel, null);
        }

        /// <summary>
        /// Updates the UI widget when the value of the bound property has changed.
        /// </summary>
        private void UpdateUI(object widgetValue)
        {
            // Setting a UI property which is a value type to null can cause issues.
            if (widgetValue == null && boundUiProperty.PropertyInfo.PropertyType.IsValueType)
            {
                boundUiProperty.PropertyInfo.GetSetMethod()
                    .Invoke(boundUiProperty.Object, new object[] { Activator.CreateInstance(boundUiProperty.PropertyInfo.PropertyType) });
            }
            // Setting a UI property which is a string to null can also cause issues.
            else if (widgetValue == null && boundUiProperty.PropertyInfo.PropertyType == typeof(string))
            {
                boundUiProperty.PropertyInfo.GetSetMethod()
                    .Invoke(boundUiProperty.Object, new object[] { String.Empty });
            }
            else
            {
                var value = adapter == null ? widgetValue : adapter.Convert(widgetValue);

                boundUiProperty.PropertyInfo.GetSetMethod()
                    .Invoke(boundUiProperty.Object, new object[] { value });
            }
        }

        /// <summary>
        /// Use reflection to find all components with properties we can bind to.
        /// </summary>
        public static IEnumerable<BindablePropertyInfo> GetBindableProperties(GameObject gameObject)
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
        
        private IEnumerable<BindablePropertyInfo> GetBindableProperties()
        {
            return GetBindableProperties(gameObject);
        }

        /// <summary>
        /// Deregister property changed listener if necessary.
        /// </summary>
        public void Dispose()
        {
            var notifyPropertyChanged = boundViewModel as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                // Make sure we listen for property changes on the view model.
                notifyPropertyChanged.PropertyChanged -= viewModel_PropertyChanged;
            }
        }
    }
}
