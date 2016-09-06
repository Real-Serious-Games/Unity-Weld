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
                    SetUpBoundViewModel();
                }
                return boundViewModel;
            }
        }

        /// <summary>
        /// Initialise the bound view model by getting the property from the parent view model.
        /// </summary>
        private void SetUpBoundViewModel()
        {
            boundViewModel = GetBoundPropertyInfo()
                .GetValue(GetViewModel(), null);
        }

        /// <summary>
        /// Return the PropertyInfo of the property we're binding to.
        /// </summary>
        private PropertyInfo GetBoundPropertyInfo()
        {
            var propertyInfo = GetViewModelBinding()
                .BoundViewModel.GetType()
                .GetProperty(boundPropertyName);

            if (propertyInfo == null)
            {
                throw new ApplicationException("Could not find property \"" + boundPropertyName +
                    "\" on view model \"" + GetViewModelBinding().ViewModelTypeName + "\"");
            }

            return propertyInfo;
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

        public override void Connect()
        {
            var notifyPropertyChanged = GetViewModel() as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += NotifyPropertyChanged_PropertyChanged;
            }
        }

        public override void Disconnect()
        {
            var notifyPropertyChanged = GetViewModel() as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged -= NotifyPropertyChanged_PropertyChanged;
            }
        }

        private void NotifyPropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != boundPropertyName)
            {
                return;
            }

            SetUpBoundViewModel();

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

        /// <summary>
        /// Calls a method matching the specified string in the underlying view.
        /// </summary>
        public void SendEvent(string methodName, params object[] arguments)
        {
            ReflectionUtils.InvokeMethod(BoundViewModel, methodName, arguments);
        }
    }
}
