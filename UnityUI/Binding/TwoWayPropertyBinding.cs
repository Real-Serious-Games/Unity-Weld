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
        public string viewAdapterTypeName;

        /// <summary>
        /// Name of the type of the adapter we're using to conver values from the
        /// UI back to the view model. Can be empty for no adapter.
        /// </summary>
        public string viewModelAdapterTypeName;

        /// <summary>
        /// The name of the property to assign an exception to when adapter/validation fails.
        /// </summary>
        public string exceptionPropertyName;

        /// <summary>
        /// Adapter to apply to any adapter/validation exception that is assigned to the view model.
        /// </summary>
        public string exceptionAdapterTypeName;

        /// <summary>
        /// Type of the component we're binding to.
        /// Must be a string so because Types can't be serialised in the scene.
        /// </summary>
        public string boundComponentType;

        /// <summary>
        /// Syncronizes the property in the view-model with the property in the view.
        /// </summary>
        private PropertySync propertySync;

        /// <summary>
        /// Watches the view-model for changes that must be propagated to the view.
        /// </summary>
        private PropertyWatcher viewModelWatcher;

        /// <summary>
        /// Watches the view for changes that must be propagated to the view-model.
        /// </summary>
        private UnityEventWatcher unityEventWatcher;

        public override void Connect()
        {
            var view = GetComponent(boundComponentType);

            var viewModelEndPoint = MakeViewModelEndPoint(viewModelPropertyName, viewModelAdapterTypeName);

            propertySync = new PropertySync(
                // Source
                viewModelEndPoint,

                // Dest
                new PropertyEndPoint(
                    view,
                    uiPropertyName,
                    CreateAdapter(viewAdapterTypeName),
                    "view",
                    this
                ),

                // Errors, exceptions and validation.
                !string.IsNullOrEmpty(exceptionPropertyName)
                    ? MakeViewModelEndPoint(exceptionPropertyName, exceptionAdapterTypeName)
                    : null
                    ,
                
                this
            );

            viewModelWatcher = viewModelEndPoint.Watch(
                () => propertySync.SyncFromSource()
            );

            unityEventWatcher = new UnityEventWatcher(
                view,
                uiEventName,
                () => propertySync.SyncFromDest()
            );

            // Copy the initial value over from the view-model.
            propertySync.SyncFromSource();
        }

        public override void Disconnect()
        {
            if (viewModelWatcher != null)
            {
                viewModelWatcher.Dispose();
                viewModelWatcher = null;
            }

            if (unityEventWatcher != null)
            {
                unityEventWatcher.Dispose();
                unityEventWatcher = null;
            }
        }
    }
}
