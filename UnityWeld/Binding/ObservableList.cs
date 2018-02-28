using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityWeld.Binding
{
    public class ObservableList<T> : IList<T>, INotifyCollectionChanged, ITypedList
    {
        /// <summary>
        /// Inner (non-obsevable) list.
        /// </summary>
        private readonly List<T> innerList = new List<T>();

        /// <summary>
        /// Event raised when the collection has been changed.
        /// </summary>
        public event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;

        /// <summary>
        /// Default constructor
        /// </summary>
        public ObservableList() { }

        /// <summary>
        /// Create from existing items.
        /// </summary>
        public ObservableList(IEnumerable<T> items)
        {
            innerList.AddRange(items);
        }

        public int IndexOf(T item)
        {
            return innerList.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            innerList.Insert(index, item);

            if (CollectionChanged != null)
            {
                CollectionChanged(this, NotifyCollectionChangedEventArgs.ItemAdded(item, index));
            }
        }

        public void RemoveAt(int index)
        {
            var item = innerList[index];

            innerList.RemoveAt(index);

            if (CollectionChanged != null)
            {
                CollectionChanged(this, NotifyCollectionChangedEventArgs.ItemRemoved(item, index));
            }
        }

        public T this[int index]
        {
            get
            {
                return innerList[index];
            }
            set
            {
                innerList[index] = value;
            }
        }

        public void Add(T item)
        {
            var newIndex = innerList.Count;

            innerList.Add(item);

            if (CollectionChanged != null)
            {
                CollectionChanged(this, NotifyCollectionChangedEventArgs.ItemAdded(item, newIndex));
            }
        }

        public void Clear()
        {
            var oldItems = innerList.Cast<object>().ToArray();

            innerList.Clear();

            if (CollectionChanged != null)
            {
                CollectionChanged(this, NotifyCollectionChangedEventArgs.Reset(oldItems));
            }
        }

        public bool Contains(T item)
        {
            return innerList.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return innerList.Count;
            }
        }

        /// <summary>
        /// Specifies the type of items in the list.
        /// </summary>
        public Type ItemType
        {
            get
            {
                return typeof(T);
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return true;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        object IList.this[int index]
        {
            get
            {
                return innerList[index];
            }
            set
            {
                innerList[index] = (T)value;
            }
        }

        public bool Remove(T item)
        {
            var index = innerList.IndexOf(item);
            var result = innerList.Remove(item);

            if (result && CollectionChanged != null)
            {
                CollectionChanged(this, NotifyCollectionChangedEventArgs.ItemRemoved(item, index));
            }

            return result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return innerList.GetEnumerator();
        }

        public int Add(object item)
        {
            var newIndex = innerList.Count;

            innerList.Add((T)item);

            if (CollectionChanged != null)
            {
                CollectionChanged(this, NotifyCollectionChangedEventArgs.ItemAdded(item, newIndex));
            }

            return innerList.Count - 1;
        }

        public bool Contains(object item)
        {
            return innerList.Contains((T)item);
        }

        public int IndexOf(object item)
        {
            return innerList.IndexOf((T)item);
        }

        public void Insert(int index, object item)
        {
            innerList.Insert(index, (T)item);

            if (CollectionChanged != null)
            {
                CollectionChanged(this, NotifyCollectionChangedEventArgs.ItemAdded(item, index));
            }
        }

        public void Remove(object item)
        {
            var index = innerList.IndexOf((T)item);
            var result = innerList.Remove((T)item);

            if (result && CollectionChanged != null)
            {
                CollectionChanged(this, NotifyCollectionChangedEventArgs.ItemRemoved(item, index));
            }
        }

        public void CopyTo(Array array, int index)
        {
            innerList.CopyTo((T[])array, index);
        }
    }

    public static class LinqExts
    {
        /// <summary>
        /// Convert an IEnumerable into an observable list.
        /// </summary>
        public static ObservableList<T> ToObservableList<T>(this IEnumerable<T> source)
        {
            return new ObservableList<T>(source);
        }

        /// <summary>
        /// Convert a variable length argument list of items to an ObservableList.
        /// </summary>
        public static ObservableList<T> ObservableListFromItems<T>(params T[] items)
        {
            return new ObservableList<T>(items);
        }
    }
}
