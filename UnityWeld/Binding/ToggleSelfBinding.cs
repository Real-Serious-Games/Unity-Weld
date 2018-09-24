using UnityEngine;
using UnityWeld.Binding.Internal;

namespace UnityWeld.Binding
{
    /// <summary>
    /// Bind to a boolean property on the view model and turn itself on
    /// or off based on its value.
    /// </summary>
    [AddComponentMenu("Unity Weld/ToggleSelf Binding")]
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
    public class ToggleSelfBinding : AbstractMemberBinding
    {
        /// <summary>
        /// Type of the adapter we're using to adapt between the view model property 
        /// and view property.
        /// </summary>
        public string ViewAdapterTypeName
        {
            get { return viewAdapterTypeName; }
            set { viewAdapterTypeName = value; }
        }

        [SerializeField]
        private string viewAdapterTypeName;

        /// <summary>
        /// Options for adapting from the view model to the view property.
        /// </summary>
        public AdapterOptions ViewAdapterOptions
        {
            get { return viewAdapterOptions; }
            set { viewAdapterOptions = value; }
        }

        [SerializeField]
        private AdapterOptions viewAdapterOptions;

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
        /// Watcher the view-model for changes that must be propagated to the view.
        /// </summary>
        private PropertyWatcher viewModelWatcher;

        /// <summary>
        /// Property for the propertySync to set in order to activate and deactivate all children
        /// </summary>
        public bool SelfActive
        {
            set
            {
                SetActive(value);
            }
        }
        
        /// <inheritdoc />
        public override void Connect()
        {
            var viewModelEndPoint = MakeViewModelEndPoint(viewModelPropertyName, null, null);

            var propertySync = new PropertySync(
                // Source
                viewModelEndPoint,

                // Dest
                new PropertyEndPoint(
                    this,
                    "SelfActive",
                    CreateAdapter(viewAdapterTypeName),
                    viewAdapterOptions,
                    "view",
                    this
                ),

                // Errors, exceptions and validation.
                null, // Validation not needed

                this
            );

            viewModelWatcher = viewModelEndPoint.Watch(
                () => propertySync.SyncFromSource()
            );

            // Copy the initial value over from the view-model.
            propertySync.SyncFromSource();
        }

        /// <inheritdoc />
        public override void Disconnect()
        {
            if (viewModelWatcher != null)
            {
                viewModelWatcher.Dispose();
                viewModelWatcher = null;
            }
        }

        private void SetActive(bool active)
        {
            transform.gameObject.SetActive(active);
        }
    }
}
