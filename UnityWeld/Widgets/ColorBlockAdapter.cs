using UnityEngine;
using UnityEngine.UI;

namespace UnityWeld.Widgets
{
    /// <summary>
    /// Adapter to assist with binding to individual colors within a color block.
    /// </summary>
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
    public class ColorBlockAdapter : MonoBehaviour
    {
        private Selectable selectable;

        void Awake()
        {
            selectable = GetComponent<Selectable>();
        }

        public Color DisabledColor
        {
            get
            {
                return selectable.colors.disabledColor;
            }
            set
            {
                var colors = selectable.colors;
                colors.disabledColor = value;
                selectable.colors = colors;
            }
        }

        public Color NormalColor
        {
            get
            {
                return selectable.colors.normalColor;
            }
            set
            {
                var colors = selectable.colors;
                colors.normalColor = value;
                selectable.colors = colors;
            }
        }

        public Color PressedColor
        {
            get
            {
                return selectable.colors.pressedColor;
            }
            set
            {
                var colors = selectable.colors;
                colors.pressedColor = value;
                selectable.colors = colors;
            }
        }

        public Color HighlightedColor
        {
            get
            {
                return selectable.colors.highlightedColor;
            }
            set
            {
                var colors = selectable.colors;
                colors.highlightedColor = value;
                selectable.colors = colors;
            }
        }
    }
}
