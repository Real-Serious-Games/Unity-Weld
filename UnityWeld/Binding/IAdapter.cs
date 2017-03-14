namespace UnityWeld.Binding
{
    /// <summary>
    /// Base interface for all adapters. Combine with AdapterAttribute to specify 
    /// the types it supports converting to and from.
    /// </summary>
    public interface IAdapter
    {
        /// <summary>
        /// Convert from the source type to the output type. This should throw an exception 
        /// if the conversion fails or the input isn't valid.
        /// </summary>
        object Convert(object valueIn, AdapterOptions options);
    }
}
