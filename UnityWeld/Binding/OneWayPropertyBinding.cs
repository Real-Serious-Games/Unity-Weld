using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityWeld.Binding
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
        public string viewAdapterTypeName;

        /// <summary>
        /// Options for adapting from the view model to the UI property.
        /// </summary>
        public AdapterOptions viewAdapterOptions;

        /// <summary>
        /// Name of the property in the view model to bind.
        /// </summary>
        public string viewModelPropertyName;

        /// <summary>
        /// UI Property to update when value changes.
        /// </summary>
        public string uiPropertyName;

        /// <summary>
        /// Watches the view-model for changes that must be propagated to the view.
        /// </summary>
        private PropertyWatcher viewModelWatcher;

        public override void Connect()        
        {
            string propertyName;
            Component view;
            ParseViewEndPointReference(uiPropertyName, out propertyName, out view);

            var viewModelEndPoint = MakeViewModelEndPoint(viewModelPropertyName, null, null);

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
                null, // Validation not needed

                this
            );

            viewModelWatcher = viewModelEndPoint.Watch(
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
