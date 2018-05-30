using System.Reflection;
using UnityEngine;
using UnityWeld.Binding.Exceptions;
using UnityWeld.Binding.Internal;

namespace UnityWeld.Binding
{
    /// <summary>
    /// Binds to a view and instantiates a template based on the view type.
    /// </summary>
    [AddComponentMenu("Unity Weld/Template Binding")]
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
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
            ParseViewModelEndPointReference(
                ViewModelPropertyName, 
                out propertyName, 
                out viewModel
            );

            viewModelPropertyWatcher = new PropertyWatcher(
                viewModel, 
                propertyName, 
                InitalizeTemplate
            );

            // Get property from view model.
            viewModelProperty = viewModel.GetType().GetProperty(propertyName);
            if (viewModelProperty == null)
            {
                throw new MemberNotFoundException(string.Format(
                    "Expected property {0} on type {1}, but was not found.", 
                    propertyName, 
                    viewModel.GetType().Name
                ));
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
                throw new PropertyNullException(string.Format(
                    "TemplateBinding cannot bind to null property in view: {0}.", 
                    ViewModelPropertyName
                ));
            }

            InstantiateTemplate(viewModelPropertyValue);
        }
    }
}
