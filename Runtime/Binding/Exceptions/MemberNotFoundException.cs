using System;

namespace UnityWeld.Binding.Exceptions
{
    /// <summary>
    /// Thrown when a property or method could not be found on the specified class or interface.
    /// </summary>
    public class MemberNotFoundException : Exception
    {
        public MemberNotFoundException(string message)
            : base(message)
        {
        }
    }
}
