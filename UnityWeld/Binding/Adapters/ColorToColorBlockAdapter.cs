using UnityEngine;
using UnityEngine.UI;

namespace UnityWeld.Binding.Adapters
{
    /// <summary>
    /// Adapter that converts a single Color to one of the colors inside a ColorBlock
    /// </summary>
    [Adapter(typeof(Color), typeof(ColorBlock), typeof(ColorToColorBlockAdapterOptions))]
    public class ColorToColorBlockAdapter : IAdapter
    {
        public object Convert(object valueIn, AdapterOptions options)
        {
            var adapterOptions = (ColorToColorBlockAdapterOptions)options;
            var color = (Color)valueIn;

            var colorBlock = adapterOptions.DefaultColors;
            switch (adapterOptions.OverrideColor)
            {
                case ColorToColorBlockAdapterOptions.Role.Disabled:
                    colorBlock.disabledColor = color;
                    break;
                case ColorToColorBlockAdapterOptions.Role.Highlighed:
                    colorBlock.highlightedColor = color;
                    break;
                case ColorToColorBlockAdapterOptions.Role.Normal:
                    colorBlock.normalColor = color;
                    break;
                case ColorToColorBlockAdapterOptions.Role.Pressed:
                    colorBlock.pressedColor = color;
                    break;
            }

            return colorBlock;
        }
    }
}
