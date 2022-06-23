using System;

namespace UnityWeld.Binding.Exceptions
{
    /// <summary>
    /// Specified type cannot be used as an adapter for the specified types or does not 
    /// implement IAdapter.
    /// </summary>
    public class InvalidAdapterException : Exception
    {
        public InvalidAdapterException(string message)
            : base(message)
        {
        }
    }
}
