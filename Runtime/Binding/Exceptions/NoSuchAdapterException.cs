using System;

namespace UnityWeld.Binding.Exceptions
{
    /// <summary>
    /// The specified adapter does not exist.
    /// </summary>
    public class NoSuchAdapterException : Exception
    {
        public NoSuchAdapterException(string adapterTypeName)
            : base(string.Format("Could not find adapter type '{0}'.", adapterTypeName))
        {
        }
    }
}
