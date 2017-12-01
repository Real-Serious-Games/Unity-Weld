using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityWeld.Binding.Internal;

namespace UnityWeld.Binding
{
    /// <summary>
    /// Bind a property in the view model to a parameter in an Animator, subscribing to OnPropertyChanged 
    /// and updating the Animator parameter accordingly (note that this does not update the view model when
    /// the parameter changes).
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
    public class AnimatorParameterBinding : AbstractMemberBinding
    {
        /// <summary>
        /// Type of the adapter we're using to adapt between the view model property and UI property.
        /// </summary>
        public string ViewAdapterTypeName
        {
            get { return viewAdapterTypeName; }
            set { viewAdapterTypeName = value; }
        }

        [SerializeField]
        private string viewAdapterTypeName;

        /// <summary>
        /// Options for adapting from the view model to the UI property.
        /// </summary>
        public AdapterOptions ViewAdapterOptions
        {
            get { return viewAdapterOptions; }
            set { viewAdapterOptions = value; }
        }

        [SerializeField]
        private AdapterOptions viewAdapterOptions;

        /// <summary>
        /// Name of the property in the view model to bind.
        /// </summary>
        public string ViewModelPropertyName
        {
            get { return viewModelPropertyName; }
            set { viewModelPropertyName = value; }
        }

        [SerializeField]
        private string viewModelPropertyName;

        /// <summary>
        /// Parameter name on the Animator
        /// </summary>
        public string AnimatorParameterName
        {
            get { return animatorParameterName; }
            set { animatorParameterName = value; }
        }
        [SerializeField]
        private string animatorParameterName;

        /// <summary>
        /// The parameter type that we are binding to
        /// </summary>
        public AnimatorControllerParameterType AnimatorParameterType
        {
            get { return animatorParameterType; }
            set { animatorParameterType = value; }
        }
        [SerializeField]
        private AnimatorControllerParameterType animatorParameterType;

        /// <summary>
        /// Watches the view-model for changes that must be propagated to the view.
        /// </summary>
        private PropertyWatcher viewModelWatcher;

        /// <summary>
        /// Animator to use
        /// </summary>
        private Animator _animator;

        //Properties to bind to
        public float FloatParameter
        {
            set
            {
                if(_animator != null)
                {
                    _animator.SetFloat(AnimatorParameterName, value);
                }
            }
        }

        public int IntParameter
        {
            set
            {
                if(_animator != null)
                {
                    _animator.SetInteger(AnimatorParameterName, value);
                }
            }
        }

        public bool BoolParameter
        {
            set
            {
                if(_animator != null)
                {
                    _animator.SetBool(AnimatorParameterName, value);
                }
            }
        }

        public bool TriggerParameter
        {
            set
            {
                if (_animator != null)
                {
                    if (value)
                    {
                        _animator.SetTrigger(AnimatorParameterName);
                    }
                    else
                    {
                        _animator.ResetTrigger(AnimatorParameterName);
                    }
                }
            }
        }

        public override void Connect()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            Assert.IsTrue(
                _animator != null,
                "Animator is null!"
            );

            Assert.IsTrue(
                !string.IsNullOrEmpty(AnimatorParameterName),
                "AnimatorParameter is not set"
            );

            string propertyName;
            switch (AnimatorParameterType)
            {
                case AnimatorControllerParameterType.Float:
                    {
                        propertyName = "FloatParameter";
                        break;
                    }
                case AnimatorControllerParameterType.Int:
                    {
                        propertyName = "IntParameter";
                        break;
                    }
                case AnimatorControllerParameterType.Bool:
                    {
                        propertyName = "BoolParameter";
                        break;
                    }
                case AnimatorControllerParameterType.Trigger:
                    {
                        propertyName = "TriggerParameter";
                        break;
                    }
                default:
                    {
                        propertyName = "";
                        break;
                    }
            }

            var viewModelEndPoint = MakeViewModelEndPoint(viewModelPropertyName, null, null);

            var propertySync = new PropertySync(
                // Source
                viewModelEndPoint,

                // Dest
                new PropertyEndPoint(
                    this,
                    propertyName,
                    CreateAdapter(viewAdapterTypeName),
                    viewAdapterOptions,
                    "Animator",
                    this
                ),

                // Errors, exceptions and validation.
                null, // Validation not needed

                this
            );

            viewModelWatcher = viewModelEndPoint.Watch(
                () => propertySync.SyncFromSource()
            );

            // Copy the initial value over from the view-model.
            propertySync.SyncFromSource();
        }

        public override void Disconnect()
        {
            if (viewModelWatcher != null)
            {
                viewModelWatcher.Dispose();
                viewModelWatcher = null;
            }
        }
    }
}
