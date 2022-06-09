using System;

namespace UnityWeld.Binding.Exceptions
{
    /// <summary>
    /// Thrown when an end-point reference was specified in an invalid way and could
    /// not be parsed to work out a class or interface name and a member name.
    /// </summary>
    public class InvalidEndPointException : Exception
    {
        public InvalidEndPointException(string message)
            : base(message)
        {
        }
    }
}
