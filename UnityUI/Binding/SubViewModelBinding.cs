using System;
using System.ComponentModel;
using System.Reflection;

namespace UnityUI.Binding
{
    /// <summary>
    /// Bind a sub-view model which is a property on another view model for use in the UI.
    /// </summary>
    public class SubViewModelBinding : AbstractMemberBinding, IViewModelBinding
    {
        /// <summary>
        /// Name of the property in the top-level view model that contains the sub-viewmodel that 
        /// we want to bind.
        /// </summary>
        public string boundPropertyName;

        private object boundViewModel;

        public object BoundViewModel
        {
            get
            {
                if (boundViewModel == null)
                {
                    UpdateViewModel();
                }
                return boundViewModel;
            }
        }

        /// <summary>
        /// Name of the type of the view model we're binding to. Set from the Unity inspector.
        /// </summary>
        public string viewModelTypeName;

        /// <summary>
        /// Lazily get the view model type name from the top level view model we've bound to.
        /// </summary>
        public string ViewModelTypeName
        {
            get
            {
                return viewModelTypeName;
            }
        }

        /// <summary>
        /// Watches the view-model proper for changes.
        /// </summary>
        private PropertyWatcher viewModelPropertyWatcher;

        /// <summary>
        /// Initialise the bound view model by getting the property from the parent view model.
        /// </summary>
        private void UpdateViewModel()
        {
            string propertyName;
            object viewModel;
            ParseViewModelEndPointReference(boundPropertyName, out propertyName, out viewModel);

            var propertyInfo = viewModel.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                throw new ApplicationException("Could not find property \"" + propertyName + "\" on view model \"" + viewModel.GetType().Name + "\"");
            }

            boundViewModel = propertyInfo.GetValue(viewModel, null);
        }

        public override void Connect()
        {
            string propertyName;
            object viewModel;
            ParseViewModelEndPointReference(boundPropertyName, out propertyName, out viewModel);

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
