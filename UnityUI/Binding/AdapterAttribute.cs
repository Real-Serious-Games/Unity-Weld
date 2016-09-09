using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityUI.Binding
{
    /// <summary>
    /// Attribute that defines what types an adapter can convert from and to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AdapterAttribute : Attribute
    {
        public AdapterAttribute(Type inputType, Type outputType)
        {
            InputType = inputType;
            OutputType = outputType;
        }

        public Type InputType { get; private set; }

        public Type OutputType { get; private set; }
    }
}
