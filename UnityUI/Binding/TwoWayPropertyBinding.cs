using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityUI.Binding
{
    /// <summary>
    /// Bind a property in the view model to one the UI, subscribing to OnPropertyChanged 
    /// and updating the UI accordingly. Also bind to a UnityEvent in the UI and update the
    /// view model when the event is triggered.
    /// </summary>
    public class TwoWayPropertyBinding : AbstractMemberBinding
    {
        /// <summary>
        /// Name of the property in the view model to bind.
        /// </summary>
        public string viewModelPropertyName;

        /// <summary>
        /// Event in the UI to bind to.
        /// </summary>
        public string uiEventName;

        /// <summary>
        /// UI Property to update when value changes.
        /// </summary>
        public string uiPropertyName;

        /// <summary>
        /// Name of the type of the adapter we're using to convert values from the 
        /// view model to the UI. Can be empty for no adapter.
        /// </summary>
        public string viewModelToUIAdapterName;

        /// <summary>
        /// Name of the type of the adapter we're using to conver values from the
        /// UI back to the view model. Can be empty for no adapter.
        /// </summary>
        public string uiToViewModelAdapterName;

        /// <summary>
        /// Type of the component we're binding to.
        /// Must be a string so because Types can't be serialised in the scene.
        /// </summary>
        public string boundComponentType;

        private PropertyBinder propertyBinder;
        private EventBinder eventBinder;

        public override void Connect()
        {
            var viewModelBinding = GetViewModelBinding();

            propertyBinder = new PropertyBinder(this.gameObject,
                viewModelPropertyName,
                uiPropertyName,
                boundComponentType,
                CreateAdapter(viewModelToUIAdapterName),
                viewModelBinding.BoundViewModel);

            eventBinder = new EventBinder(this.gameObject,
                "set_" + viewModelPropertyName, // Call the setter on the bound property
                uiEventName,
                boundComponentType,
                CreateAdapter(uiToViewModelAdapterName),
                viewModelBinding);
        }

        public override void Disconnect()
        {
            if (propertyBinder != null)
            {
                propertyBinder.Dispose();
                propertyBinder = null;
            }

            if (eventBinder != null)
            {
                eventBinder.Dispose();
                eventBinder = null;
            }
        }
    }
}
