using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace UnityWeld.Binding
{
    public class CollectionBinding : AbstractTemplateSelector
    {
        /// <summary>
        /// Collection that we have bound to.
        /// </summary>
        private IEnumerable viewModelCollectionValue;

        private new void Awake()
        {
            Assert.IsNotNull(templates, "CollectionBinding must be assigned a template.");

            // Templates should always be deactivated since they're only used to clone new instances.
            templates.SetActive(false);

            base.Awake();
        }

        public override void Connect()
        {
            Disconnect();

            CacheTemplates();

            string propertyName;
            object newViewModel;
            ParseViewModelEndPointReference(viewModelPropertyName, out propertyName, out newViewModel);

            viewModel = newViewModel;

            viewModelPropertyWatcher = new PropertyWatcher(newViewModel, propertyName, NotifyPropertyChanged_PropertyChanged);

            BindCollection();
        }

        public override void Disconnect()
        {
            UnbindCollection();

            if (viewModelPropertyWatcher != null)
            {
                viewModelPropertyWatcher.Dispose();
                viewModelPropertyWatcher = null;
            }

            viewModel = null;
        }

        private void NotifyPropertyChanged_PropertyChanged()
        {
            RebindCollection();
        }

        private void Collection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // Add items that were added to the bound collection.
                    if (e.NewItems != null)
                    {
                        foreach (var item in e.NewItems)
                        {
                            InstantiateTemplate(item);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    // TODO: respect item order
                    // Remove items that have been deleted.
                    if (e.OldItems != null)
                    {
                        foreach (var item in e.OldItems)
                        {
                            DestroyTemplate(item);
                        }
                    }

                    break;
                case NotifyCollectionChangedAction.Reset:
                    DestroyAllTemplates();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Bind to the view model collection so we can monitor it for changes.
        /// </summary>
        private void BindCollection()
        {
            // Bind view model.
            var viewModelType = viewModel.GetType();

            string propertyName;
            string viewModelName;
            ParseEndPointReference(viewModelPropertyName, out propertyName, out viewModelName);

            var viewModelCollectionProperty = viewModelType.GetProperty(propertyName);
            if (viewModelCollectionProperty == null)
            {
                throw new ApplicationException("Expected property " + viewModelPropertyName + ", but it wasn't found on type " + viewModelType + ".");
            }

            // Get value from view model.
            var viewModelValue = viewModelCollectionProperty.GetValue(viewModel, null);
            if (viewModelValue == null)
            {
                throw new ApplicationException("Cannot bind to null property in view: " + viewModelPropertyName);
            }

            if (!(viewModelValue is IEnumerable))
            {
                throw new ApplicationException("Property " + viewModelPropertyName + " is not a collection and cannot be used to bind collections.");
            }
            viewModelCollectionValue = (IEnumerable)viewModelValue;

            // Generate children
            foreach (var value in viewModelCollectionValue)
            {
                InstantiateTemplate(value);
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
            DestroyAllTemplates();

            // Unsubscribe from collection changed events.
            if (viewModelCollectionValue != null)
            {
                var collectionChanged = viewModelCollectionValue as INotifyCollectionChanged;
                if (collectionChanged != null)
                {
                    collectionChanged.CollectionChanged -= Collection_CollectionChanged;
                }

                viewModelCollectionValue = null;
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
