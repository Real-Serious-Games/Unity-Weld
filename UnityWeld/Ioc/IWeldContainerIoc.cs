using System;
using UnityWeld.Binding;

namespace UnityWeld.Ioc
{
    /// <summary>
    /// Base type for weld containers, used by AdapterResolver to get the instance of adapters. Combine with the WeldContainerAttribute
    /// to use your implementation instead of DefaultWeldContainer
    /// </summary>
    public interface IWeldContainerIoC
    {
        T Resolve<T>() where T : class, IAdapter;
        T Resolve<T>(Type type) where T : class, IAdapter;
    }
}