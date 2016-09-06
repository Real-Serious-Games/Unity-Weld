using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityUI.Binding
{
    /// <summary>
    /// Interface for classes that generate collection changed events.
    /// </summary>
    public interface INotifyCollectionChanged
    {
        /// <summary>
        /// Event raised when the collection has been changed.
        /// </summary>
        event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;
    }
}
