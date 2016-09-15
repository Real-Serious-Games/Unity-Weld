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

        private PropertyBinder propertyBinder;

        public override void Connect()
        {
            propertyBinder = new PropertyBinder(this.gameObject,
                viewModelPropertyName,
                uiPropertyName,
                boundComponentType,
                CreateAdapter(adapterTypeName),
                GetViewModel());
        }

        public override void Disconnect()
        {
            if (propertyBinder != null)
            {
                propertyBinder.Dispose();
                propertyBinder = null;
            }
        }

        void OnDestroy()
        {
            Disconnect();
        }
    }
}
