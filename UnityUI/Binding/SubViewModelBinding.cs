using System;
using System.ComponentModel;
using System.Reflection;

namespace UnityUI.Binding
{
    /// <summary>
    /// Bind a sub-view model which is a property on another view model for use in the UI.
    /// </summary>
    public class SubViewModelBinding : AbstractMemberBinding, IViewModelProvider
    {
        /// <summary>
        /// Get the view-model provided by this provider.
        /// </summary>
        public object GetViewModel()
        {
            return viewModel;
        }

        /// <summary>
        /// Get the name of the view-model's type.
        /// </summary>
        public string GetViewModelTypeName()
        {
            return viewModelTypeName;
        }

        /// <summary>
        /// Name of the property in the view-model that contains the sub-viewmodel.
        /// </summary>
        public string viewModelPropertyName;

        /// <summary>
        /// Name of the type of the view model we're binding to. Set from the Unity inspector.
        /// </summary>
        public string viewModelTypeName;

        /// <summary>
        /// Watches the view-model proper for changes.
        /// </summary>
        private PropertyWatcher viewModelPropertyWatcher;

        /// <summary>
        /// Cached view-model object.
        /// </summary>
        private object viewModel;

        /// <summary>
        /// Initialise the bound view model by getting the property from the parent view model.
        /// </summary>
        private void UpdateViewModel()
        {
            string propertyName;
            object viewModel;
            ParseViewModelEndPointReference(viewModelPropertyName, out propertyName, out viewModel);

            var propertyInfo = viewModel.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ApplicationException("Could not find property \"" + propertyName + "\" on view model \"" + viewModel.GetType().Name + "\"");
            }

            this.viewModel = propertyInfo.GetValue(viewModel, null);
        }

        public override void Connect()
        {
            string propertyName;
            object viewModel;
            ParseViewModelEndPointReference(viewModelPropertyName, out propertyName, out viewModel);

            viewModelPropertyWatcher = new PropertyWatcher(viewModel, propertyName, NotifyPropertyChanged_PropertyChanged);

            UpdateViewModel();
        }

        public override void Disconnect()
        {
            if (viewModelPropertyWatcher != null)
            {
                viewModelPropertyWatcher.Dispose();
                viewModelPropertyWatcher = null;
            }
        }

        private void NotifyPropertyChanged_PropertyChanged()
        {
            UpdateViewModel();

            // Rebind all children.
            foreach (var memberBinding in GetComponentsInChildren<AbstractMemberBinding>())
            {
                if (memberBinding == this)
                {
                    continue;
                }

                memberBinding.Init();
            }
        }
    }
}
