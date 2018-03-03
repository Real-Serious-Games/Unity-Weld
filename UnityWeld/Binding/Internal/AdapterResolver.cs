using System;
using System.Linq;
using System.Reflection;
using UnityWeld.Ioc;

namespace UnityWeld.Binding.Internal
{
    /// <summary>
    /// Helper class for creating adapters
    /// </summary>
    public class AdapterResolver
    {
        private static IWeldContainerIoC container;

        public static IAdapter CreateAdapter(Type adapterType)
        {
            if(container == null)
            {
                SetupWeldContainer();
            }
            return container.Resolve<IAdapter>(adapterType);
        }

        private static void SetupWeldContainer()
        {
            var containerTypes = TypeResolver.TypesWithWeldContainerAttribute.Where(t => typeof(IWeldContainerIoC).IsAssignableFrom(t));
            var containerTypesCount = containerTypes.Count();
            if (containerTypesCount > 0)
            {
                if(containerTypesCount > 1)
                {
                    UnityEngine.Debug.LogWarningFormat("You have multiple classes marked with the WeldContainer Attribute, only one can be used!");
                }

                var containerType = containerTypes.First();

                var useableMethods = containerType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(mi => mi.GetCustomAttributes(typeof(WeldContainerAttribute), false).Any() && typeof(IWeldContainerIoC).IsAssignableFrom(mi.ReturnType));
                if (useableMethods.Any())
                {
                    var methodInfo = useableMethods.First();
                    if (methodInfo != null)
                    {
                        container = methodInfo.Invoke(null, null) as IWeldContainerIoC;
                    }
                    else
                    {
                        container = Activator.CreateInstance(containerType) as IWeldContainerIoC;
                    }
                }
                else
                {
                    container = Activator.CreateInstance(containerType) as IWeldContainerIoC;
                }
            }

            if (container == null)
            {
                container = new DefaultWeldContainer();
            }
        }
    }
}
