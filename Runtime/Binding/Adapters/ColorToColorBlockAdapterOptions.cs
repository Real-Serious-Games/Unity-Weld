using UnityEngine;
using UnityEngine.UI;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter that converts a single Color to one of the colors inside a ColorBlock
    /// </summary>
    [CreateAssetMenu(menuName = "Unity Weld/Adapter options/Color to ColorBlock adapter")]
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
    public class ColorToColorBlockAdapterOptions : AdapterOptions
    {
        public enum Role
        {
            Disabled,
            Highlighed,
            Normal,
            Pressed
        }

        /// <summary>
        /// Which color to override.
        /// </summary>
        public Role OverrideColor;

        /// <summary>
        /// Default colors for the other roles.
        /// </summary>
        public ColorBlock DefaultColors;
    }
}
