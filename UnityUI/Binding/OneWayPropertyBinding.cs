using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityUI.Binding
{
    /// <summary>
    /// Bind a property in the view model to one on the UI, subscribing to OnPropertyChanged 
    /// and updating the UI accordingly (note that this does not update the view model when
    /// the UI changes).
    /// </summary>
    public class OneWayPropertyBinding : AbstractMemberBinding
    {
        /// <summary>
        /// Type of the adapter we're using to adapt between the view model property and UI property.
        /// </summary>
        public string adapterTypeName;

        /// <summary>
        /// Name of the property in the view model to bind.
        /// </summary>
        public string viewModelPropertyName;

        /// <summary>
        /// UI Property to update when value changes.
        /// </summary>
        public string uiPropertyName;

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

        public override void Connect()
        {
            var viewModelBinding = GetViewModelBinding();
            var viewModel = viewModelBinding.BoundViewModel;
            var view = GetComponent(boundComponentType);

            propertySync = new PropertySync(
                // Source
                new PropertyEndPoint(
                    viewModel,
                viewModelPropertyName,
                    null, // One-way only. No dest-to source adapter required.
                    "view-model",
                    this
                ),

                // Dest
                new PropertyEndPoint(
                    view,
                uiPropertyName,
                CreateAdapter(adapterTypeName),
                    "view",
                    this
                ),

                // Errors, exceptions and validation.
                null, // Validation not needed

                this
            );

            viewModelWatcher = new PropertyWatcher(
                viewModel,
                viewModelPropertyName,
                () => propertySync.SyncFromSource()
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
        }
    }
}
