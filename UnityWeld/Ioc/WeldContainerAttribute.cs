using System;

namespace UnityWeld.Ioc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class WeldContainerAttribute : Attribute
    {
    }
}
