using System;

namespace UnityWeld.Binding
{
    /// <summary>
    /// Mark a class, interface, method or property as bindable. Bindable methods and properties must 
    /// reside within classes or interfaces that have also been marked as bindable.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Interface, 
        Inherited = false)]
    public class BindingAttribute : Attribute
    {
    }
}
