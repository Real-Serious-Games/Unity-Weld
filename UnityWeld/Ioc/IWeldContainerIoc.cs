using System;
using UnityWeld.Binding;

namespace UnityWeld.Ioc
{
    public interface IWeldContainerIoC
    {
        T Resolve<T>() where T : IAdapter;
        T Resolve<T>(Type type) where T : IAdapter;
    }
}