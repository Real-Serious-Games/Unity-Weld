using UnityEngine;
using System.Collections;
using UnityUI.Binding;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Reflection;

namespace UnityUI.Binding
{
    /// <summary>
    /// Binds to a view and instantiates a template based on the view type.
    /// </summary>
    public class TemplateSelector : AbstractMemberBinding
    {
        /// <summary>
        /// The name of the property we are binding to on the view model.
        /// </summary>
        public string viewModelPropertyName = string.Empty;

        /// <summary>
        /// The gameobject in the scene that is the parent object for the tenplates.
        /// </summary>
        public GameObject templates;

        /// <summary>
        /// Cached view model binding, set on connection.
        /// </summary>
        private IViewModelBinding viewModelBinding;

        /// <summary>
        /// All available templates indexed by the view model the are for.
        /// </summary>
        private IDictionary<string, TemplateBinding> availableTemplates = new Dictionary<string, TemplateBinding>();

        /// <summary>
        /// The template that has been instantiated.
        /// </summary>
        private TemplateBinding initalizedTemplate = null;

        /// <summary>
        /// The property of the view model that is being bound to
        /// </summary>
        private PropertyInfo viewModelProperty = null;

        public override void Connect()
        {
            // Cache available templates.
            var templatesInScene = templates.GetComponentsInChildren<TemplateBinding>();
            foreach (var template in templatesInScene)
            {
                availableTemplates.Add(template.ViewModelTypeName, template);
            }

            this.viewModelBinding = GetViewModelBinding();

            // Subscribe to property changed events.
            var notifyPropertyChanged = viewModelBinding.BoundViewModel as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += NotifyPropertyChanged_PropertyChanged;
            }

            // Get property from view model.
            viewModelProperty = viewModelBinding
                .BoundViewModel.GetType()
                .GetProperty(viewModelPropertyName);
            if (viewModelProperty == null)
            {
                throw new ApplicationException("Expected property " + viewModelPropertyName + " not found on type " + viewModelName);
            }

            InitalizeTemplate();
        }

        public override void Disconnect()
        {
            DestroyTemplate();

            if (viewModelBinding != null)
            {
                // Unsubscribe from property changed events.
                var notifyPropertyChanged = viewModelBinding.BoundViewModel as INotifyPropertyChanged;
                if (notifyPropertyChanged != null)
                {
                    notifyPropertyChanged.PropertyChanged -= NotifyPropertyChanged_PropertyChanged;
                }
            }
        }

        private void NotifyPropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == viewModelPropertyName)
            {
                InitalizeTemplate();
            }
        }

        /// <summary>
        /// Initalized the correct tempalte based on the type.
        /// </summary>
        private void InitalizeTemplate()
        {
            DestroyTemplate();

            // Get value from view model.
            var viewModelPropertyValue = viewModelProperty.GetValue(viewModelBinding.BoundViewModel, null);
            if (viewModelPropertyValue == null)
            {
                throw new ApplicationException("Cannot bind to null property in view: " + viewModelName + "." + viewModelPropertyName);
            }

            // Select template.
            var viewModelValueType = viewModelPropertyValue.GetType().Name;
            TemplateBinding selectedTemplate = null;
            if (!availableTemplates.TryGetValue(viewModelValueType, out selectedTemplate))
            {
                throw new ApplicationException("Cannot find matching template for: " + viewModelValueType);
            }

            // Setup selected template.
            initalizedTemplate = Instantiate(selectedTemplate);

            initalizedTemplate.transform.SetParent(transform, false);
            initalizedTemplate.gameObject.SetActive(true);

            initalizedTemplate.InitChildBindings(viewModelPropertyValue);
        }

        /// <summary>
        /// Destroys the instantiated template.
        /// </summary>
        private void DestroyTemplate()
        {
            if (initalizedTemplate == null)
            {
                return;
            }

            Destroy(initalizedTemplate.gameObject);
            initalizedTemplate = null;
        }
    }
}
