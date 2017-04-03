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
        /// Show a popup menu with some items disabled and a label to its left.
        /// </summary>
        public static void DoPopup<T>(
            GUIContent content, 
            GUIContent label,
            Func<T, string> menuName, 
            Func<T, bool> menuEnabled,
            Func<T, bool> isSelected,
            Action<T> callback, 
            T[] items)
        {
            var currentEvent = Event.current;
            var eventType = currentEvent.type;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);

            var position = GUILayoutUtility.GetLastRect();
            position.x += position.width;

            if (GUILayout.Button(content, EditorStyles.popup))
            {
                ShowMenu(menuName, menuEnabled, isSelected, callback, items, position);
            }

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Show a menu with some items disabled. Has a callback that will be called when an item is selected with the index of the selected item.
        /// Takes a dictionary of options and whether or not they should be enabled.
        /// </summary>
        private static void ShowMenu<T>(Func<T, string> menuName, Func<T, bool> menuEnabled, Func<T, bool> isSelected, Action<T> callback, T[] items, Rect position)
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
