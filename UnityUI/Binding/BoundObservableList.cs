using System;
using System.Collections.Generic;
using System.Linq;


namespace UnityWeld.Binding
{
    /// <summary>
    /// An observable list that is bound to source list.
    /// </summary>
    public class BoundObservableList<DestT, SourceT> : ObservableList<DestT>
    {
        /// <summary>
        /// The source list.
        /// </summary>
        ObservableList<SourceT> source;

        /// <summary>
        /// Function that maps source items to dest items.
        /// </summary>
        private Func<SourceT, DestT> itemMap;

        /// <summary>
        /// Callback when new items are added.
        /// </summary>
        private Action<DestT> added;

        /// <summary>
        /// Callback when items are removed.
        /// </summary>
        private Action<DestT> removed;

        /// <summary>
        /// Callback invoked when the collection has changed.
        /// </summary>
        private Action changed;

        /// <summary>
        /// Cache that mimics the contents of the bound list.
        /// This is so we know the items that were cleared when the list is reset.
        /// </summary>
        private List<DestT> cache;

        public BoundObservableList(ObservableList<SourceT> source, Func<SourceT, DestT> itemMap) :
            base(source.Select(itemMap))
        {
            this.itemMap = itemMap;
            this.source = source;

            source.CollectionChanged += source_CollectionChanged;
        }

        public BoundObservableList(ObservableList<SourceT> source, Func<SourceT, DestT> itemMap, Action<DestT> added, Action<DestT> removed) :
            base(source.Select(itemMap))
        {
            if (added == null)
            {
                throw new ArgumentNullException("added", "added must not be null.");
            }
            if (removed == null)
            {
                throw new ArgumentNullException("removed", "removed must not be null.");
            }

            this.itemMap = itemMap;
            this.source = source;
            this.added = added;
            this.removed = removed;

            foreach (var item in this)
            {
                added(item);
            }

            source.CollectionChanged += source_CollectionChanged;
            this.CollectionChanged += BoundObservableList_CollectionChanged;
            cache = new List<DestT>(this);
        }

        public BoundObservableList(ObservableList<SourceT> source, Func<SourceT, DestT> itemMap, Action<DestT> added, Action<DestT> removed, Action changed) :
            base(source.Select(itemMap))
        {
            if (added == null)
            {
                throw new ArgumentNullException("added", "added must not be null.");
            }
            if (removed == null)
            {
                throw new ArgumentNullException("removed", "removed must not be null.");
            }

            this.itemMap = itemMap;
            this.source = source;
            this.added = added;
            this.removed = removed;
            this.changed = changed;

            foreach (var item in this)
            {
                added(item);
            }

            source.CollectionChanged += source_CollectionChanged;
            this.CollectionChanged += BoundObservableList_CollectionChanged;
            cache = new List<DestT>(this);
        }

        /// <summary>
        /// Event raised when the source collection has changed.
        /// </summary>
        private void source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var insertAt = e.NewStartingIndex;

                foreach (var item in e.NewItems)
                {
                    var generatedItem = itemMap((SourceT)item);

                    Insert(insertAt, generatedItem);
                    ++insertAt;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var removeAt = e.OldStartingIndex;

                for (var i = 0; i < e.OldItems.Count; i++)
                {
                    RemoveAt(removeAt);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Clear();
            }
        }

        /// <summary>
        /// Event raised when items are added to the bound list.
        /// </summary>
        private void BoundObservableList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add &&
                added != null)
            {
                var insertIndex = e.NewStartingIndex;

                foreach (var item in e.NewItems)
                {
                    var typedItem = (DestT)item;

                    added(typedItem);

                    cache.Insert(insertIndex, typedItem); // Keep the cache updated as new items come in.
                    ++insertIndex;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var typedItem = (DestT)item;

                    removed(typedItem);

                    cache.RemoveAt(e.OldStartingIndex); // Keep the cache updated as items are removed.
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                foreach (var item in cache)
                {
                    removed(item);
                }
                cache.Clear();
            }

            if (changed != null)
            {
                changed.Invoke();
            }
        }

    }
}
