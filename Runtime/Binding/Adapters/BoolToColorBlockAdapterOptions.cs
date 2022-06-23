using UnityEngine;
using UnityEngine.UI;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Options for converting from a bool to a Unity color.
    /// </summary>
    [CreateAssetMenu(menuName = "Unity Weld/Adapter options/Bool to ColorBlock adapter options")]
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
    public class BoolToColorBlockAdapterOptions : AdapterOptions
    {
        /// <summary>
        /// The value used when the bool is false.
        /// </summary>
        [Header("False colors")]
        public ColorBlock FalseColors;

        /// <summary>
        /// The value used when the bool is true.
        /// </summary>
        [Header("True colors")]
        public ColorBlock TrueColors;
    }
}