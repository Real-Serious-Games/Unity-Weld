using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace UnityUI_Editor
{
    /// <summary>
    /// Common utilities for custom inspectors.
    /// </summary>
    internal class InspectorUtils
    {
        /// <summary>
        /// Item to be shown in our custom menu.
        /// </summary>
        public struct MenuItem
        {
            public MenuItem(GUIContent content, bool enabled)
            {
                this.content = content;
                this.enabled = enabled;
            }

            public GUIContent content;

            public bool enabled;
        }

        /// <summary>
        /// Show a menu with some items disabled. Has a callback that will be called when an item is selected with the index of the selected item.
        /// Takes a dictionary of options and whether or not they should be enabled.
        /// </summary>
        public static void ShowCustomSelectionMenu(Action<int> callback, MenuItem[] options, int selectedIndex, Rect position)
        {
            var menu = new GenericMenu();
            for (var i = 0; i < options.Length; i++)
            {
                // Need to cache index so that it doesn't get passed through to the callback by reference.
                int index = i;

                var option = options.ElementAt(index);

                if (option.enabled)
                {
                    menu.AddItem(option.content, selectedIndex == index, () => callback(index));
                }
                else
                {
                    menu.AddDisabledItem(option.content);
                }
            }
            menu.DropDown(position);
        }
    }
}
