using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityUI.Binding
{
    /// <summary>
    /// Adapter that does not convert values and does no validation.
    /// </summary>
    [Adapter(typeof(object), typeof(object))]
    public class NoAdapter : IAdapter
    {
        public object Convert(object valueIn)
        {
            return valueIn;
        }
    }
}
