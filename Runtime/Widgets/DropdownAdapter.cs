using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityWeld.Binding.Exceptions;

namespace UnityWeld.Widgets
{
    /// <summary>
    /// Adapter to set up and bind to a Dropdown using strings instead of OptionData.
    /// </summary>
    [Obsolete("Use DropdownBinding instead")]
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
    public class DropdownAdapter : MonoBehaviour
    {
        private Dropdown dropdown;
        private Dropdown Dropdown
        {
            get
            {
                if (dropdown == null)
                {
                    dropdown = GetComponent<Dropdown>();
                    if (dropdown == null)
                    {
                        throw new ComponentNotFoundException("DropdownAdapter must be placed on an object with a Dropdown");
                    }

                    // Dropdown should start with empty list of options
                    dropdown.options.Clear();

                    // Bind event for dropdown value changed
                    dropdown.onValueChanged.AddListener(newItemIndex => 
                        OnValueChanged.Invoke(dropdown.options[newItemIndex].text));
                }
                return dropdown;
            }
        }

        [Serializable]
        public class DropdownStringEvent : UnityEvent<string> { }

        [SerializeField]
        private DropdownStringEvent onValueChanged = new DropdownStringEvent();
        public DropdownStringEvent OnValueChanged
        {
            get
            {
                return onValueChanged;
            }
            set
            {
                onValueChanged = value;
            }
        }

        /// <summary>
        /// Used to remember the selection if it gets set before the options list is set.
        /// </summary>
        private string cachedSelection;

        /// <summary>
        /// String of all the text options in the dropdown.
        /// </summary>
        public string[] Options
        {
            get
            {
                return Dropdown.options
                    .Select(option => option.text)
                    .ToArray();
            }
            set
            {
                Dropdown.options = value
                    .Select(optionString => new Dropdown.OptionData(optionString))
                    .ToList();

                // Initialise the selection if it was already set before the options list was populated.
                if (cachedSelection != String.Empty)
                {
                    SetSelection(cachedSelection);
                }
            }
        }

        /// <summary>
        /// String of the text of the currently selected option.
        /// </summary>
        public string SelectedOption
        {
            get
            {
                if (Dropdown.options.Count > 0)
                {
                    return Dropdown.options[Dropdown.value].text;
                }
                else
                {
                    return String.Empty;
                }
            }
            set
            {
                SetSelection(value);
            }
        }

        /// <summary>
        /// If the options list has been initialised, set the selection and clear any cached selection. 
        /// Otherwise store it for use later since the list of options must be initialised before we can select one.
        /// </summary>
        private void SetSelection(string value)
        {
            // If the options list hasn't been initialised yet, store the value for later.
            if (Dropdown.options.Count == 0)
            {
                cachedSelection = value;
            }
            else
            {
                Dropdown.value = Dropdown.options
                    .Select(option => option.text)
                    .ToList()
                    .IndexOf(value);
                cachedSelection = String.Empty;
            }
        }
    }
}
