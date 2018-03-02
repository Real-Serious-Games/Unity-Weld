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
        private static AdapterResolver _instance;
        private static AdapterResolver Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AdapterResolver();
                }
                return _instance;
            }
        }

        public static IAdapter CreateAdapter(Type adapterType)
        {
            return Instance._container.Resolve<IAdapter>(adapterType);
        }

        private IWeldContainerIoC _container;
        public AdapterResolver()
        {
            var containerTypes = TypeResolver.TypesWithWeldContainerAttribute.Where(t => typeof(IWeldContainerIoC).IsAssignableFrom(t));
            if (containerTypes.Any())
            {
                var containerType = containerTypes.First();

                var useableMethods = containerType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(mi => mi.GetCustomAttributes(typeof(WeldContainerAttribute), false).Any() && typeof(IWeldContainerIoC).IsAssignableFrom(mi.ReturnType));
                if (useableMethods.Any())
                {
                    var methodInfo = useableMethods.First();
                    if (methodInfo != null)
                    {
                        _container = methodInfo.Invoke(null, null) as IWeldContainerIoC;
                    }
                    else
                    {
                        _container = Activator.CreateInstance(containerType) as IWeldContainerIoC;
                    }
                }
            }

            if (_container == null)
            {
                _container = new DefaultWeldContainer();
            }
        }
    }
}
