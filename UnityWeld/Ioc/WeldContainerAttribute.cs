using System;

namespace UnityWeld.Ioc
{
    /// <summary>
    /// Class use: Marks the class to be used as WeldContainer, and it will be responsible for creating the adapter instances
    /// 
    /// Method use: If the class also has the attribute, and this method returns IWeldContainerIoC or an inherting type, the returned value will be used as the
    /// instance used by AdapterResolver
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
    public class WeldContainerAttribute : Attribute
    {
    }
}
