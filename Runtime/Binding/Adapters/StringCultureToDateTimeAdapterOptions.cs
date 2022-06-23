using UnityEngine;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Options for converting from a string to a DateTime using format from a specified
    /// culture.
    /// </summary>
    [CreateAssetMenu(menuName = "Unity Weld/Adapter options/String to DateTime adapter (using culture specifier)")]
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
    public class StringCultureToDateTimeAdapterOptions : AdapterOptions
    {
        public string CultureName;
    }
}
