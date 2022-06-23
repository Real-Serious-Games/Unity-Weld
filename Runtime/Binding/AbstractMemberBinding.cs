using System.Linq;
using UnityEngine;
using UnityWeld.Binding.Exceptions;
using UnityWeld.Binding.Internal;

namespace UnityWeld.Binding
{
    /// <summary>
    /// Base class for binders to Unity MonoBehaviours.
    /// </summary>
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
    public abstract class AbstractMemberBinding : MonoBehaviour, IMemberBinding
    {
        private bool _isInitCalled;

        [SerializeField, Header("Automatically bind once on \"OnEnable()\"")]
        private bool _isAutoConnection;


        /// <summary>
        /// Initialise this binding. Used when we first start the scene.
        /// Detaches any attached view models, finds available view models afresh and then connects the binding.
        /// </summary>
        public virtual void Init()
        {
            if(_isAutoConnection && !gameObject.activeInHierarchy)
            {
                return; //wait for enabling
            }

            if (_isInitCalled)
            {
                return; //avoid double connect
            }

            _isInitCalled = true;

            Disconnect();
            Connect();
        }

        /// <summary>
        /// Scan up the hierarchy and find a view model that corresponds to the specified name.
        /// </summary>
        private object FindViewModel(string viewModelName)
        {
            var trans = transform;
            while(trans != null)
            {
                using(var cache = trans.gameObject.GetComponentsWithCache<MonoBehaviour>(false))
                {
                    var monoBehaviourViewModel = cache.Components
                                                      .FirstOrDefault(component => component.GetType().ToString() == viewModelName);
                    if(monoBehaviourViewModel != null)
                    {
                        return monoBehaviourViewModel;
                    }

                    var providedViewModel = cache.Components
                                                 .Select(component => component.GetViewModelData())
                                                 .Where(component => component != null)
                                                 .FirstOrDefault(viewModelData => viewModelData.TypeName == viewModelName);

                    if(providedViewModel != null)
                    {
                        return providedViewModel.Model;
                    }
                }

                trans = trans.parent;
            }

            throw new ViewModelNotFoundException(
                $"Tried to get view model {viewModelName} but it could not be found on " +
                $"object {gameObject.name}. Check that a ViewModelBinding for that view model exists further up in " +
                "the scene hierarchy. "
            );
        }

        /// <summary>
        /// Make a property end point for a property on the view model.
        /// </summary>
        protected PropertyEndPoint MakeViewModelEndPoint(string viewModelPropertyName, string adapterId,
                                                         AdapterOptions adapterOptions)
        {
            string propertyName;
            object viewModel;
            ParseViewModelEndPointReference(viewModelPropertyName, out propertyName, out viewModel);

            var adapter = TypeResolver.GetAdapter(adapterId);
            return new PropertyEndPoint(viewModel, propertyName, adapter, adapterOptions, "view-model", this);
        }

        /// <summary>
        /// Parse an end-point reference including a type name and member name separated by a period.
        /// </summary>
        protected static void ParseEndPointReference(string endPointReference, out string memberName,
                                                     out string typeName)
        {
            var lastPeriodIndex = endPointReference.LastIndexOf('.');
            if(lastPeriodIndex == -1)
            {
                throw new InvalidEndPointException(
                    "No period was found, expected end-point reference in the following format: <type-name>.<member-name>. " +
                    "Provided end-point reference: " + endPointReference
                );
            }

            typeName = endPointReference.Substring(0, lastPeriodIndex);
            memberName = endPointReference.Substring(lastPeriodIndex + 1);
            //Due to (undocumented) unity behaviour, some of their components do not work with the namespace when using GetComponent(""), and all of them work without the namespace
            //So to be safe, we remove all namespaces from any component that starts with UnityEngine
            if(typeName.StartsWith("UnityEngine."))
            {
                typeName = typeName.Substring(typeName.LastIndexOf('.') + 1);
            }

            if(typeName.Length == 0 || memberName.Length == 0)
            {
                throw new InvalidEndPointException(
                    "Bad format for end-point reference, expected the following format: <type-name>.<member-name>. " +
                    "Provided end-point reference: " + endPointReference
                );
            }
        }

        /// <summary>
        /// Parse an end-point reference and search up the hierarchy for the named view-model.
        /// </summary>
        protected void ParseViewModelEndPointReference(string endPointReference, out string memberName,
                                                       out object viewModel)
        {
            string viewModelName;
            ParseEndPointReference(endPointReference, out memberName, out viewModelName);

            viewModel = FindViewModel(viewModelName);
            if(viewModel == null)
            {
                throw new ViewModelNotFoundException("Failed to find view-model in hierarchy: " + viewModelName);
            }
        }

        /// <summary>
        /// Parse an end-point reference and get the component for the view.
        /// </summary>
        protected void ParseViewEndPointReference(string endPointReference, out string memberName, out Component view)
        {
            string boundComponentType;
            ParseEndPointReference(endPointReference, out memberName, out boundComponentType);

            view = GetComponent(boundComponentType);
            if(view == null)
            {
                throw new ComponentNotFoundException("Failed to find component on current game object: " +
                                                     boundComponentType);
            }
        }

        /// <summary>
        /// Connect to all the attached view models
        /// </summary>
        public abstract void Connect();

        /// <summary>
        /// Disconnect from all attached view models.
        /// </summary>
        public abstract void Disconnect();

        /// <summary>
        /// Standard MonoBehaviour awake message, do not call this explicitly.
        /// Initialises the binding.
        /// </summary>
        protected void OnEnable()
        {
            if (!_isAutoConnection || _isInitCalled)
            {
                return;
            }

            Init();
        }

        /// <summary>
        /// Clean up when the game object is destroyed.
        /// </summary>
        public virtual void OnDestroy()
        {
            Disconnect();
        }

        public void ResetBinding()
        {
            _isInitCalled = false;
            Disconnect();
        }
    }
}