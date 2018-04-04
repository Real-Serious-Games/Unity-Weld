using System;
using System.Linq;
using System.Reflection;
using UnityWeld.Ioc;

namespace UnityWeld.Binding.Internal
{
    /// <summary>
    /// Helper class for creating adapters
    /// 
    /// Will resolve with a class marked by WeldContainerAttribute, if there is none it will fall back to using DefaultWeldContainer
    /// If there are multiple classes marked with WeldContainerAttribute, its initial setup will throw an InvalidOperationException
    /// </summary>
    public static class AdapterResolver
    {
        private static IWeldContainerIoC container;

        public static IAdapter CreateAdapter(Type adapterType)
        {
            if (container == null)
            {
                container = SetupWeldContainer();
            }
            return container.Resolve<IAdapter>(adapterType);
        }

        private static IWeldContainerIoC SetupWeldContainer()
        {
            var containerTypes = TypeResolver.TypesWithWeldContainerAttribute.Where(t => typeof(IWeldContainerIoC).IsAssignableFrom(t)).ToList();
            var containerTypesCount = containerTypes.Count;
            if (containerTypesCount > 0)
            {
                if (containerTypesCount > 1)
                {
                    throw new InvalidOperationException("You have multiple classes marked with the WeldContainer Attribute, only one can be used!");
                }

                var containerType = containerTypes[0];

                var useableMethods = containerType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(mi => mi.GetCustomAttributes(typeof(WeldContainerAttribute), false).Any() && typeof(IWeldContainerIoC).IsAssignableFrom(mi.ReturnType));

                var methodInfo = useableMethods.FirstOrDefault();

                if (methodInfo != null)
                {
                    return methodInfo.Invoke(null, null) as IWeldContainerIoC;
                }
                else
                {
                    return Activator.CreateInstance(containerType) as IWeldContainerIoC;
                }
            }
            else
            {
                return new DefaultWeldContainer();
            }
        }
    }
}
