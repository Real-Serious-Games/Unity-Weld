using UnityEngine;
using UnityWeld.Binding.Internal;

namespace UnityWeld.Binding
{
    /// <summary>
    /// Bind a property in the view model to one the UI, subscribing to OnPropertyChanged 
    /// and updating the UI accordingly. Also bind to a UnityEvent in the UI and update the
    /// view model when the event is triggered.
    /// </summary>
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
    public class TwoWayPropertyBinding : AbstractMemberBinding
    {
        /// <summary>
        /// Name of the property in the view model to bind.
        /// </summary>
        public string ViewModelPropertyName
        {
            get { return viewModelPropertyName; }
            set { viewModelPropertyName = value; }
        }

        [SerializeField]
        private string viewModelPropertyName;

        /// <summary>
        /// Event in the UI to bind to.
        /// </summary>
        public string UiEventName
        {
            get { return uiEventName; }
            set { uiEventName = value; }
        }

        [SerializeField]
        private string uiEventName;

        /// <summary>
        /// UI Property to update when value changes.
        /// </summary>
        public string UiPropertyName
        {
            get { return uiPropertyName; }
            set { uiPropertyName = value; }
        }

        [SerializeField]
        private string uiPropertyName;

        /// <summary>
        /// Name of the type of the adapter we're using to convert values from the 
        /// view model to the UI. Can be empty for no adapter.
        /// </summary>
        public string ViewAdapterTypeName
        {
            get { return viewAdapterTypeName; }
            set { viewAdapterTypeName = value; }
        }

        [SerializeField]
        private string viewAdapterTypeName;

        /// <summary>
        /// Options for the adapter from the view model to the UI.
        /// </summary>
        public AdapterOptions ViewAdapterOptions
        {
            get { return viewAdapterOptions; }
            set { viewAdapterOptions = value; }
        }

        [SerializeField]
        private AdapterOptions viewAdapterOptions;

        /// <summary>
        /// Name of the type of the adapter we're using to conver values from the
        /// UI back to the view model. Can be empty for no adapter.
        /// </summary>
        public string ViewModelAdapterTypeName
        {
            get { return viewModelAdapterTypeName; }
            set { viewModelAdapterTypeName = value; }
        }

        [SerializeField]
        private string viewModelAdapterTypeName;

        /// <summary>
        /// Options for the adapter from the UI to the view model.
        /// </summary>
        public AdapterOptions ViewModelAdapterOptions
        {
            get { return viewModelAdapterOptions; }
            set { viewModelAdapterOptions = value; }
        }

        [SerializeField]
        private AdapterOptions viewModelAdapterOptions;

        /// <summary>
        /// The name of the property to assign an exception to when adapter/validation fails.
        /// </summary>
        public string ExceptionPropertyName
        {
            get { return exceptionPropertyName; }
            set { exceptionPropertyName = value; }
        }

        [SerializeField]
        private string exceptionPropertyName;

        /// <summary>
        /// Adapter to apply to any adapter/validation exception that is assigned to the view model.
        /// </summary>
        public string ExceptionAdapterTypeName
        {
            get { return exceptionAdapterTypeName; }
            set { exceptionAdapterTypeName = value; }
        }

        [SerializeField]
        private string exceptionAdapterTypeName;

        /// <summary>
        /// Adapter options for an exception.
        /// </summary>
        public AdapterOptions ExceptionAdapterOptions
        {
            get { return exceptionAdapterOptions; }
            set { exceptionAdapterOptions = value; }
        }

        [SerializeField]
        private AdapterOptions exceptionAdapterOptions;

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
            string propertyName;
            Component view;
            ParseViewEndPointReference(uiPropertyName, out propertyName, out view);

            var viewModelEndPoint = MakeViewModelEndPoint(viewModelPropertyName, viewModelAdapterTypeName, viewModelAdapterOptions);

            var propertySync = new PropertySync(
                // Source
                viewModelEndPoint,

                // Dest
                new PropertyEndPoint(
                    view,
                    propertyName,
                    CreateAdapter(viewAdapterTypeName),
                    viewAdapterOptions,
                    "view",
                    this
                ),

                // Errors, exceptions and validation.
                !string.IsNullOrEmpty(exceptionPropertyName)
                    ? MakeViewModelEndPoint(exceptionPropertyName, exceptionAdapterTypeName, exceptionAdapterOptions)
                    : null
                    ,
                
                this
            );

            viewModelWatcher = viewModelEndPoint.Watch(
                () => propertySync.SyncFromSource()
            );

            string eventName;
            string eventComponentType;
            ParseEndPointReference(uiEventName, out eventName, out eventComponentType);

            var eventView = GetComponent(eventComponentType);

            unityEventWatcher = new UnityEventWatcher(
                eventView,
                eventName,
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
