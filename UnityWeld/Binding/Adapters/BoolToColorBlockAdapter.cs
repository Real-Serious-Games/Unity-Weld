using UnityEngine.UI;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a bool to a Unity color.
    /// </summary>
    [Adapter(typeof(bool), typeof(ColorBlock), typeof(BoolToColorBlockAdapterOptions))]
    public class BoolToColorBlockAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            var adapterOptions = (BoolToColorBlockAdapterOptions)options;

            return (bool)valueIn ? adapterOptions.TrueColors : adapterOptions.FalseColors;
        }
    }
}
