using System;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a DateTime to an OADate as a float.
    /// </summary>
    [Adapter(typeof(DateTime), typeof(float))]
    public class DateTimeToFloatAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            var date = (DateTime)valueIn;

            return (float)date.ToOADate();
        }
    }
}
