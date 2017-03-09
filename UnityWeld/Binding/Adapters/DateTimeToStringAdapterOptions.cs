using UnityEngine;
using UnityWeld.Binding;

namespace UnityUI.Binding.Adapters
{
    /// <summary>
    /// Options for converting a DateTime to a string.
    /// </summary>
    [CreateAssetMenu(menuName = "Unity Weld/Adapter options/DateTime to string adapter")]
    public class DateTimeToStringAdapterOptions : AdapterOptions
    {
        /// <summary>
        /// Format passed in to the DateTime.ToString method. 
        /// See this page for details on usage: https://msdn.microsoft.com/en-us/library/zdtaw1bw(v=vs.110).aspx
        /// </summary>
        public string Format;
    }
}