using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityWeld_Editor
{
    /// <summary>
    /// Common utilities for custom inspectors.
    /// </summary>
    internal class InspectorUtils
    {
        /// <summary>
        /// Show a menu with some items disabled. Has a callback that will be called when an item is selected with the index of the selected item.
        /// Takes a dictionary of options and whether or not they should be enabled.
        /// </summary>
        public static void ShowMenu<T>(Func<T, string> menuName, Func<T, bool> menuEnabled, Func<T, bool> isSelected, Action<T> callback, T[] items, Rect position)
        {
            var menu = new GenericMenu();

            for (var i = 0; i < items.Length; i++)
            {
                // Need to cache index so that it doesn't get passed through to the callback by reference.
                int index = i;
                var item = items[index];

                var content = new GUIContent(menuName(item));

                if (menuEnabled(item))
                {
                    menu.AddItem(content, isSelected(item), () => callback(item));
                }
                else
                {
                    menu.AddDisabledItem(content);
                }
            }

            menu.DropDown(position);
        }

        /// <summary>
        /// Tell Unity that a change has been made to a specified object and we have to save the scene.
        /// </summary>
        public static void MarkSceneDirty(GameObject gameObject)
        {
            // TODO: Undo.RecordObject also marks the scene dirty, so this will no longer be necessary once undo support is added.
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
}
