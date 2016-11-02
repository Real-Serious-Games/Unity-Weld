using UnityEngine;

namespace UnityUI.Binding
{
    public interface IViewModelProvider
    {
        /// <summary>
        /// The view model we have bound.
        /// </summary>
        object BoundViewModel { get; }

        /// <summary>
        /// Name of the view model type to bind this object to.
        /// </summary>
        string ViewModelTypeName { get; }
    }
}