using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityUI.Binding
{
    /// <summary>
    /// Mark a class, interface, method or property as bindable. Bindable methods and properties must 
    /// reside within classes or interfaces that have also been marked as bindable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Interface, 
        AllowMultiple = false, 
        Inherited = false)]
    public class BindingAttribute : Attribute
    {
    }
}
