using System;
using UnityEngine;

namespace UnityUI.Binding
{
    /// <summary>
    /// Adapter for binding MonoBehaviours as IViewModelBindings.
    /// </summary>
    public class MonoBehaviourBinding : IViewModelBinding
    {
        public MonoBehaviourBinding(MonoBehaviour viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel", "MonoBehaviourBinding cannot be created with a null MonoBehaviour");
            }
            BoundViewModel = viewModel;
        }

        public object BoundViewModel
        {
            get;
            private set;
        }

        public string ViewModelTypeName
        {
            get
            {
                return BoundViewModel.GetType().Name;
            }
        }
    }
}
