using System;
using System.Reflection;
using UnityWeld.Binding.Internal;

namespace UnityWeld.Binding
{
    /// <summary>
    /// Binds to a view and instantiates a template based on the view type.
    /// </summary>
    public class TemplateBinding : AbstractTemplateSelector
    {
        /// <summary>
        /// The property of the view model that is being bound to
        /// </summary>
        private PropertyInfo viewModelProperty;
        
        /// <summary>
        /// Connect to the attached view model.
        /// </summary>
        public override void Connect()
        {
            Disconnect();

            string propertyName;
            ParseViewModelEndPointReference(viewModelPropertyName, out propertyName, out viewModel);

            viewModelPropertyWatcher = new PropertyWatcher(viewModel, propertyName, InitalizeTemplate);

            // Get property from view model.
            viewModelProperty = viewModel.GetType().GetProperty(propertyName);
            if (viewModelProperty == null)
            {
                throw new ApplicationException("Expected property " + viewModelPropertyName + ", but was not found.");
            }

            InitalizeTemplate();
        }


        /// <summary>
        /// Disconnect from the attached view model.
        /// </summary>
        public override void Disconnect()
        {
            DestroyAllTemplates();

            if (viewModelPropertyWatcher != null)
            {
                viewModelPropertyWatcher.Dispose();
                viewModelPropertyWatcher = null;
            }

            viewModel = null;
        }

        /// <summary>
        /// Initalized the correct tempalte based on the type.
        /// </summary>
        private void InitalizeTemplate()
        {
            DestroyAllTemplates();

            // Get value from view model.
            var viewModelPropertyValue = viewModelProperty.GetValue(viewModel, null);
            if (viewModelPropertyValue == null)
            {
                throw new ApplicationException("Cannot bind to null property in view: " + viewModelPropertyName);
            }

            InstantiateTemplate(viewModelPropertyValue);
        }
    }
}
