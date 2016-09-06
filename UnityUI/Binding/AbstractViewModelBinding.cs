using UnityEngine;

namespace UnityUI.Binding
{
    public interface IViewModelBinding
    {
        /// <summary>
        /// Calls a method matching the specified string in the underlying view.
        /// </summary>
        void SendEvent(string methodName, params object[] arguments);

        /// <summary>
        /// The view model we have bound.
        /// </summary>
        object BoundViewModel { get; }

        /// <summary>
        /// Name of the view model type to bind this object to.
        /// </summary>
        string ViewModelTypeName { get; }
    }

    /// <summary>
    /// Base class with functionality that's shared between ViewModelBinding, TemplateBinding
    /// and potentially any other classes that support binding UI properties and events to properties
    /// and methods in a view model.
    /// </summary>
    public abstract class AbstractViewModelBinding : MonoBehaviour, IViewModelBinding
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

        [SerializeField]
        private string viewModelTypeName;

        public abstract object BoundViewModel { get; }

        /// <summary>
        /// Calls a method matching the specified string in the underlying view.
        /// </summary>
        public void SendEvent(string methodName, params object[] arguments)
        {
            ReflectionUtils.InvokeMethod(BoundViewModel, methodName, arguments);
        }
    }
}