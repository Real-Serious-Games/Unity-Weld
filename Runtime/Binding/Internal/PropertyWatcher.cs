using System;
using System.ComponentModel;

namespace UnityWeld.Binding.Internal
{
    /// <summary>
    /// Watches an object for property changes and invokes an action when the property has changed.
    /// </summary>
    public class PropertyWatcher : IDisposable
    {
        /// <summary>
        /// The object that owns the property that is being watched.
        /// </summary>
        private object propertyOwner;

        /// <summary>
        /// The name of the property that is being watched.
        /// </summary>
        private readonly string propertyName;

        /// <summary>
        /// The action to invoke when the property has changed.
        /// </summary>
        private readonly Action action;

        private bool disposed;

        public PropertyWatcher(object propertyOwner, string propertyName, Action action)
        {
            this.propertyOwner = propertyOwner;
            this.propertyName = propertyName;
            this.action = action;

            var notifyPropertyChanged = propertyOwner as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += propertyOwner_PropertyChanged;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing && propertyOwner != null)
            {
                var notifyPropertyChanged = propertyOwner as INotifyPropertyChanged;
                if (notifyPropertyChanged != null)
                {
                    notifyPropertyChanged.PropertyChanged -= propertyOwner_PropertyChanged;
                }

                propertyOwner = null;
            }

            disposed = true;
        }

        private void propertyOwner_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == propertyName)
            {
                action();
            }
        }
    }
}
