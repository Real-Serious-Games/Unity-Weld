using System;

namespace UnityWeld.Binding.Exceptions
{
    /// <summary>
    /// Thrown when an attempt is made to bind to a property that must not be null.
    /// </summary>
    public class PropertyNullException : Exception
    {
        public PropertyNullException(string message)
            : base (message)
        {
        }
    }
}
