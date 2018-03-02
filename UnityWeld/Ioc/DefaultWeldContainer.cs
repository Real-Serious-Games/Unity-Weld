using System;
using UnityWeld.Binding;

namespace UnityWeld.Ioc
{
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
