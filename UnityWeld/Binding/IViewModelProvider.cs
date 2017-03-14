namespace UnityWeld.Binding
{
    /// <summary>
    /// Interface for wiring view-models into the hierarchy.
    /// </summary>
    public interface IViewModelProvider
    {
        /// <summary>
        /// Get the view-model provided by this provider.
        /// </summary>
        object GetViewModel();

        /// <summary>
        /// Get the name of the view-model's type.
        /// </summary>
        string GetViewModelTypeName();
    }
}