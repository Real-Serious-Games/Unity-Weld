using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using UnityEngine;

namespace UnityUI.Binding
{
    public class CollectionBinding : AbstractMemberBinding
    {
        /// <summary>
        /// Name of the property in the view model to bind.
        /// </summary>
        public string viewModelPropertyName = string.Empty;

        /// <summary>
        /// All the child objects that have been created, indexed by the view they are connected to.
        /// </summary>
        IDictionary<object, GameObject> generatedChildren = new Dictionary<object, GameObject>();

        /// <summary>
        /// Collection that we have bound to.
        /// </summary>
        IEnumerable viewModelCollectionValue;

        /// <summary>
        /// Template to clone for instances of objects within the collection.
        /// </summary>
        [SerializeField]
        [Tooltip("Template to clone for each item in the collection")]
        public TemplateBinding template;

        /// <summary>
        /// Cached view model binding, set on connection.
        /// </summary>
        private IViewModelBinding viewModelBinding;

        public override void Connect()
        {
            this.viewModelBinding = GetViewModelBinding();

            var notifyPropertyChanged = viewModelBinding.BoundViewModel as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += NotifyPropertyChanged_PropertyChanged;
            }

            BindCollection();
        }

        private void NotifyPropertyChanged_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == viewModelPropertyName)
            {
                RebindCollection();
            }
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // Add items that were added to the bound collection.
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        AddAndInstantiateChild(item);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // TODO: respect item order
                // Remove items that have been deleted.
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        var itemToRemove = item;

                        Destroy(generatedChildren[itemToRemove]);
                        generatedChildren.Remove(itemToRemove);
                    }
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var generatedChild in generatedChildren.Values)
                {
                    Destroy(generatedChild);
                }

                generatedChildren.Clear();
            }
        }

        /// <summary>
        /// Create a clone of the template object and bind it to the specified view model.
        /// </summary>
        private void AddAndInstantiateChild(object viewModel)
        {
            var newObject = Instantiate(template);
            newObject.transform.SetParent(transform);
            newObject.gameObject.SetActive(true);

            generatedChildren.Add(viewModel, newObject.gameObject);

            var templateRectTransform = template.GetComponent<RectTransform>();
            var newRectTransform = newObject.GetComponent<RectTransform>();
            if (newRectTransform != null && templateRectTransform != null)
            {
                newRectTransform.localScale = templateRectTransform.localScale;
                newRectTransform.localPosition = templateRectTransform.localPosition;
            }

            // Set bound view.
            newObject.ConnectChildBindings(viewModel);
        }

        public override void Disconnect()
        {
            UnbindCollection();

            if (this.viewModelBinding != null)
            {
                var notifyPropertyChanged = viewModelBinding.BoundViewModel as INotifyPropertyChanged;
                if (notifyPropertyChanged != null)
                {
                    notifyPropertyChanged.PropertyChanged -= NotifyPropertyChanged_PropertyChanged;
                }

                viewModelBinding = null;
            }

        }

        /// <summary>
        /// Bind to the view model collection so we can monitor it for changes.
        /// </summary>
        private void BindCollection()
        {
            // Bind view model.
            var viewModelCollectionProperty = viewModelBinding
                .BoundViewModel.GetType()
                .GetProperty(viewModelPropertyName);
            if (viewModelCollectionProperty == null)
            {
                throw new ApplicationException("Expected property " + viewModelPropertyName + " not found on type " + viewModelName);
            }

            // Get value from view model.
            var viewModelValue = viewModelCollectionProperty.GetValue(viewModelBinding.BoundViewModel, null);
            if (viewModelValue == null)
            {
                throw new ApplicationException("Cannot bind to null property in view: " + viewModelName + "." + viewModelPropertyName);
            }

            if (!(viewModelValue is IEnumerable))
            {
                throw new ApplicationException("Property " + viewModelName + "." + viewModelPropertyName
                    + " is not a collection and cannot be used to bind collections.");
            }
            viewModelCollectionValue = (IEnumerable)viewModelValue;

            // Generate children
            foreach (var value in viewModelCollectionValue)
            {
                AddAndInstantiateChild(value);
            }

            // Subscribe to collection changed events.
            var collectionChanged = viewModelCollectionValue as INotifyCollectionChanged;
            if (collectionChanged != null)
            {
                collectionChanged.CollectionChanged += Collection_CollectionChanged;
            }
        }

        /// <summary>
        /// Unbind from the collection, stop monitoring it for changes.
        /// </summary>
        private void UnbindCollection()
        {
            // Delete all generated children
            foreach (var child in generatedChildren.Values)
            {
                Destroy(child);
            }

            generatedChildren.Clear();

            // Unsubscribe from collection changed events.
            if (viewModelCollectionValue != null)
            {
                var collectionChanged = viewModelCollectionValue as INotifyCollectionChanged;
                if (collectionChanged != null)
                {
                    collectionChanged.CollectionChanged -= Collection_CollectionChanged;
                }
            }
        }

        /// <summary>
        /// Rebind to the collection when it has changed on the view-model.
        /// </summary>
        private void RebindCollection()
        {
            UnbindCollection();
            BindCollection();
        }

    }
}
