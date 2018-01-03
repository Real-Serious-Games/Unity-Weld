using System;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a DateTime to a string.
    /// </summary>
    [Adapter(typeof(DateTime), typeof(string), typeof(DateTimeToStringAdapterOptions))]
    public class DateTimeToStringAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            var dateTime = (DateTime) valueIn;
            var adapterOptions = (DateTimeToStringAdapterOptions) options;
            return dateTime.ToString(adapterOptions.Format);
        }
    }
}
