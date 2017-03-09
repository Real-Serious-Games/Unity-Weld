using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityWeld.Binding;

namespace UnityUI.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a DateTime to a string.
    /// </summary>
    [Adapter(typeof(DateTime), typeof(string), typeof(DateTimeToStringAdapterOptions))]
    class DateTimeToStringAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            var dateTime = (DateTime) valueIn;
            var adapterOptions = (DateTimeToStringAdapterOptions) options;
            return dateTime.ToString(adapterOptions.Format);
        }
    }
}
