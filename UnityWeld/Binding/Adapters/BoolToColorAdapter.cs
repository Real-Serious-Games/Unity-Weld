using UnityEngine;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a bool to a Unity color.
    /// </summary>
    [Adapter(typeof(bool), typeof(Color), typeof(BoolToColorAdapterOptions))]
    public class BoolToColorAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            var adapterOptions = (BoolToColorAdapterOptions)options;

            return (bool)valueIn ? adapterOptions.TrueColor : adapterOptions.FalseColor;
        }
    }
}
