using System.Collections.Generic;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter for converting from a string array to a string list.
    /// </summary>
    [Adapter(typeof(string[]), typeof(List<string>), null)]
    public class StringArrayToListAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            List<string> output = new List<string>();

            if(valueIn == null)
            {
                return output;
            }
            return new List<string>((string[])valueIn);
        }
    }
}