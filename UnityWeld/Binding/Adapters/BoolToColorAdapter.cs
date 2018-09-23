using UnityEngine;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a bool to a Unity color.
    /// </summary>
    [Adapter(typeof(bool), typeof(Color), typeof(BoolToColorAdapterOptions))]
    public class BoolToColorAdapter : IAdapter
    {
        /// <summary>
        /// Returns eithet the value of TrueColor from the specified adapter options if
        /// the input value was true, or FalseColor if it was false.
        /// </summary>
        public object Convert(object valueIn, AdapterOptions options)
        {
            var adapterOptions = (BoolToColorAdapterOptions)options;

            return (bool)valueIn ? adapterOptions.TrueColor : adapterOptions.FalseColor;
        }
    }
}
