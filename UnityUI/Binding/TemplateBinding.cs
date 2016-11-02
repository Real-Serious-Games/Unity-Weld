using UnityEngine;

namespace UnityUI.Binding
{
    /// <summary>
    /// Template for use in collection bindings.
    /// </summary>
    public interface ITemplateBinding
    {
        /// <summary>
        /// Set the view model and initialise all binding objects down the hierarchy.
        /// </summary>
        void InitChildBindings(object viewModel);
    }

    /// <summary>
    /// Template for use in collection bindings.
    /// </summary>
    public class TemplateBinding : MonoBehaviour, IViewModelProvider, ITemplateBinding
    {
        /// <summary>
        /// View to bind the UI to.
        /// </summary>
        public string ViewModelTypeName
        {
            get
            {
                return viewModelTypeName;
            }
        }

        private string viewModelTypeName = string.Empty;

        public object BoundViewModel
        {
            get
            {
                return boundViewModel;
            }
        }
        private object boundViewModel;

        /// <summary>
        /// Set the view model and initialise all binding objects down the hierarchy.
        /// </summary>
        public void InitChildBindings(object viewModel)
        {
            // Set the bound view to the new view model.
            boundViewModel = viewModel;

            foreach (var binding in GetComponentsInChildren<AbstractMemberBinding>())
            {
                binding.Init();
            }
        }
    }
}
