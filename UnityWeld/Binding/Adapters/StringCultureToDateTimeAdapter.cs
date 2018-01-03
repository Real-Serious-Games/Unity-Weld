using System;
using System.Globalization;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a string to a DateTime, using a specified culture.
    /// </summary>
    [Adapter(typeof(string), typeof(DateTime), typeof(StringCultureToDateTimeAdapterOptions))]
    public class StringCultureToDateTimeAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            var str = (string)valueIn;
            var adapterOptions = (StringCultureToDateTimeAdapterOptions) options;
            var culture = new CultureInfo(adapterOptions.CultureName);

            return DateTime.Parse(str, culture);
        }
    }
}
