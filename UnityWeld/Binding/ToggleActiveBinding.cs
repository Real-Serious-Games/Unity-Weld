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
        /// Name of the property in the view model to bind.
        /// </summary>
        public string viewModelPropertyName;

        /// <summary>
        /// Watcher the view-model for changes that must be propagated to the view.
        /// </summary>
        private PropertyWatcher viewModelWatcher;

        public override void Connect()
        {
            var viewModelEndPoint = MakeViewModelEndPoint(viewModelPropertyName, null, null);

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
            SetAllChildrenActive((bool)viewModelEndPoint.GetValue());
        }

        private void SetAllChildrenActive(bool active)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(active);
            }
        }
    }
}
