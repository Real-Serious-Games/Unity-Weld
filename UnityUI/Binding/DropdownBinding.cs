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

        /// <summary>
        /// The name of the property to assign an exception to when adapter/validation fails.
        /// </summary>
        public string exceptionPropertyName;

        /// <summary>
        /// Adapter to apply to any adapter/validation exception that is assigned to the view model.
        /// </summary>
        public string exceptionAdapterTypeName;

        /// <summary>
        /// Watches the selection property in the view-model for changes.
        /// </summary>
        private PropertyWatcher selectionPropertyWatcher;

        /// <summary>
        /// Watches for selection changed event to update the view-model.
        /// </summary>
        private UnityEventWatcher selectionEventWatcher;

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

        /// <summary>
        /// Cached drop down component.
        /// </summary>
        private Dropdown dropdown;

        public override void Connect()
        {
            dropdown = GetComponent<Dropdown>();

            var selectionPropertyEndPoint = MakeViewModelEndPoint(viewModelSelectionPropertyName, selectionUIToViewModelAdapter);

            var selectionPropertySync = new PropertySync(
                // Source
                selectionPropertyEndPoint,

                // Dest
                new PropertyEndPoint(
                    this,
                    "SelectedOption",
                    CreateAdapter(selectionViewModelToUIAdapter),
                    "view",
                    this
                ),

                // Errors, exceptions and validation.
                !string.IsNullOrEmpty(exceptionPropertyName)
                    ? MakeViewModelEndPoint(exceptionPropertyName, exceptionAdapterTypeName)
                    : null
                    ,

                this
            );

            selectionPropertyWatcher = selectionPropertyEndPoint
                .Watch(() => selectionPropertySync.SyncFromSource());

            selectionEventWatcher = new UnityEventWatcher(
                dropdown,
                "onValueChanged",
                () =>
                {
                    selectedOption = Options[dropdown.value]; // Copy value back from dropdown.
                    selectionPropertySync.SyncFromDest();
                }
            );

            var optionsPropertySync = new PropertySync(
                // Source
                MakeViewModelEndPoint(viewModelOptionsPropertyName, null),

                // Dest
                new PropertyEndPoint(
                    this,
                    "Options",
                    CreateAdapter(optionsAdapter),
                    "view",
                    this
                ),

                // Errors, exceptions and validation.
                null, // Validation not needed

                this
            );

            // Copy the initial value from view-model to view.
            selectionPropertySync.SyncFromSource();
			optionsPropertySync.SyncFromSource();
            UpdateOptions();
        }

        public override void Disconnect()
        {
            if (selectionPropertyWatcher != null)
            {
                selectionPropertyWatcher.Dispose();
                selectionPropertyWatcher = null;
            }

            if (selectionEventWatcher != null)
            {
                selectionEventWatcher.Dispose();
                selectionEventWatcher = null;
            }

            dropdown = null;
        }

        /// <summary>
        /// Used to remember the selection if it gets set before the options list is set.
        /// </summary>
        private string selectedOption = string.Empty;

        /// <summary>
        /// Cached options.
        /// </summary>
        private string[] options = new string[0];

        /// <summary>
        /// String of all the text options in the dropdown.
        /// </summary>
        public string[] Options
        {
            get
            {
                return options;
            }
            set
            {
                options = value;

                if (dropdown != null)
                {
                    UpdateOptions();
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
                return selectedOption;
            }
            set
            {
                if (selectedOption == value)
                {
                    return;
                }

                selectedOption = value;

                UpdateSelectedOption();
            }
        }

        /// <summary>
        /// Update the options.
        /// </summary>
        private void UpdateOptions()
        {
            dropdown.options = options
                .Select(option => new Dropdown.OptionData(option))
                .ToList();

            UpdateSelectedOption();
        }

        /// <summary>
        /// Update the selected option.
        /// </summary>
        private void UpdateSelectedOption()
        {
            if (dropdown == null)
            {
                return; // Not connected.
            }

            dropdown.value = Array.IndexOf(Options, selectedOption);
        }
    }
}
