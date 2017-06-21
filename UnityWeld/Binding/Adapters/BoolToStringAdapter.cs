namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a bool to a string.
    /// </summary>
    [Adapter(typeof(bool), typeof(string), typeof(BoolToStringAdapterOptions))]
    public class BoolToStringAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            var adapterOptions = (BoolToStringAdapterOptions)options;

            return (bool)valueIn ? adapterOptions.TrueValueString : adapterOptions.FalseValueString;
        }
    }
}