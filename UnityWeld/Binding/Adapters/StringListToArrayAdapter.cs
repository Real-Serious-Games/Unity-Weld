using System.Collections.Generic;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a string list to a string array.
    /// </summary>
    [Adapter(typeof(List<string>), typeof(string[]), null)]
    public class StringListToArrayAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {

            if (valueIn == null)
            {
                return new string[0];
            }

            return ((List<string>)valueIn).ToArray();
        }
    }
}