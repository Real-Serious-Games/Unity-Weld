using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityWeld.Binding.Internal;

namespace UnityWeld.Binding
{
    /// <summary>
    /// Bind to a boolean property on the view model and turn all child objects on
    /// or off based on its value.
    /// </summary>
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
    public class ToggleActiveBinding : AbstractMemberBinding
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
        /// Watcher the view-model for changes that must be propagated to the view.
        /// </summary>
        private PropertyWatcher viewModelWatcher;

        /// <summary>
        /// Adapter for converting values that are set on the property.
        /// </summary>
        private IAdapter adapter;


        public override void Connect()
        {
            var viewModelEndPoint = MakeViewModelEndPoint(viewModelPropertyName, null, null);

            adapter = CreateAdapter(viewAdapterTypeName);

            Assert.IsTrue(
                viewModelEndPoint.GetValue() is bool,
                "ToggleActiveBinding can only be bound to a boolean property."
            );

            viewModelWatcher = viewModelEndPoint.Watch(() => SyncFromSource(viewModelEndPoint));

            SyncFromSource(viewModelEndPoint);
        }

        public override void Disconnect()
        {
            if (viewModelWatcher != null)
            {
                viewModelWatcher.Dispose();
                viewModelWatcher = null;
            }
        }

        private void SyncFromSource(PropertyEndPoint viewModelEndPoint)
        {
            bool input = (bool)viewModelEndPoint.GetValue();
            if (adapter != null)
            {
                input = (bool)adapter.Convert(input, viewAdapterOptions);
            }
            gameObject.SetActive(input);
        }
    }
}
