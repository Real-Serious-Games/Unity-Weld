using System;
using UnityWeld.Binding;

namespace UnityWeld.Ioc
{
    /// <summary>
    /// Implementation of IWeldContainerIoC to be used by defaults if no other type has been specified by WeldContainerAttribute
    /// This implementation only allows for IAdapter types with a constructor with no arguments
    /// </summary>
    public class DefaultWeldContainer : IWeldContainerIoC
    {
        public T Resolve<T>() where T : IAdapter
        {
            return Activator.CreateInstance<T>();
        }

        public T Resolve<T>(Type type) where T : IAdapter
        {
            return (T)Activator.CreateInstance(type);
        }
    }
}
