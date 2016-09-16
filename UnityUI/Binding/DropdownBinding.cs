using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UnityUI.Binding
{
    [RequireComponent(typeof(Dropdown))]
    public class DropdownBinding : AbstractMemberBinding
    {
        /// <summary>
        /// Name of the property in the view model to bind for the current selection.
        /// </summary>
        public string viewModelSelectionPropertyName;

        /// <summary>
        /// Name of the property in the view model to bind for the list of options.
        /// </summary>
        public string viewModelOptionsPropertyName;

        private static readonly string uiSelectionPropertyName = "SelectedOption";
        private static readonly string uiOptionsPropertyName = "Options";
        private static readonly string uiEventName = "OnValueChanged";

        /// <summary>
        /// Type of the component we're binding to.
        /// Must be a string so because Types can't be serialised in the scene.
        /// </summary>
        public string boundComponentType;

        private PropertyBinder selectionPropertyBinder;

        private PropertyBinder optionsPropertyBinder;

        private EventBinder selectionChangedEventBinder;

        /// <summary>
        /// Type name of the adapter for converting a selection value in the 
        /// view model to what the UI expects (which should be a string).
        /// </summary>
        public string selectionViewModelToUIAdapter;

        /// <summary>
        /// Type name of the adapter for converting a selection value in the 
        /// UI back to the type needed by the view model.
        /// </summary>
        public string selectionUIToViewModelAdapter;

        /// <summary>
        /// Adapter for converting the options list in the view model 
        /// to the correct format to display in the UI.
        /// </summary>
        public string optionsAdapter;

        public override void Connect()
        {
            var viewModelBinding = GetViewModelBinding();

            selectionPropertyBinder = new PropertyBinder(this.gameObject,
                viewModelSelectionPropertyName,
                uiSelectionPropertyName,
                boundComponentType,
                CreateAdapter(selectionViewModelToUIAdapter),
                viewModelBinding.BoundViewModel);

            optionsPropertyBinder = new PropertyBinder(this.gameObject,
                viewModelOptionsPropertyName,
                uiOptionsPropertyName,
                boundComponentType,
                CreateAdapter(optionsAdapter),
                viewModelBinding.BoundViewModel);

            selectionChangedEventBinder = new EventBinder(this.gameObject,
                "set_" + viewModelSelectionPropertyName,
                uiEventName,
                boundComponentType,
                CreateAdapter(selectionUIToViewModelAdapter),
                viewModelBinding);
        }

        public override void Disconnect()
        {
            if (selectionPropertyBinder != null)
            {
                selectionPropertyBinder.Dispose();
                selectionPropertyBinder = null;
            }

            if (optionsPropertyBinder != null)
            {
                optionsPropertyBinder.Dispose();
                optionsPropertyBinder = null;
            }

            if (selectionChangedEventBinder != null)
            {
                selectionChangedEventBinder.Dispose();
                selectionChangedEventBinder = null;
            }
        }

        void OnDestroy()
        {
            Disconnect();
        }

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
                        throw new ApplicationException("DropdownAdapter must be placed on an object with a Dropdown");
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
