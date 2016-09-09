using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityUI.Binding.Adapters
{
    /// <summary>
    /// Adapter that converts a float to a string.
    /// </summary>
    [Adapter(typeof(float), typeof(string))]
    public class FloatToStringAdapter : IAdapter
    {
        public object Convert(object valueIn)
        {
            return ((float)valueIn).ToString();
        }
    }
}
