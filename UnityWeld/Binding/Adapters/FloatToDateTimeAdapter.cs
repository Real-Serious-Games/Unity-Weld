using System;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a float as an OADate to a DateTime.
    /// </summary>
    [Adapter(typeof(float), typeof(DateTime))]
    public class FloatToDateTimeAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            var value = (float)valueIn;

            return DateTime.FromOADate(value);
        }
    }
}
