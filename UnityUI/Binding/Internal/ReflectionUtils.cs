using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityUI.Binding
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// Helper function for invoking a named method via reflection.
        /// </summary>
        internal static void InvokeMethod(object obj, string methodName, object[] arguments)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj", "Cannot invoke method on null object.");
            }

            if (string.IsNullOrEmpty(methodName))
            {
                throw new ArgumentNullException("methodName", "Cannot send event to object with no method name specified.");
            }

            var argTypes = arguments != null ? arguments.Select(a => a.GetType()).ToArray() : new Type[0];
            var method = obj.GetType().GetMethod(methodName, argTypes);
            if (method == null)
            {
                throw new ApplicationException("Invalid method: " + methodName + "(" + string.Join(", ", argTypes.Select(type => type.Name).ToArray()) 
                    + ")" + " does not exist on object " + obj.GetType().Name + ".");
            }

            method.Invoke(obj, arguments);
        }

        public static Type[] FindTypesMarkedByAttribute(Type attributeType)
        {
            if (!attributeType.IsAssignableFrom(typeof(Attribute)))
            {
                throw new ArgumentException("Specified attributeType is not an attribute.", "attributeType");
            }

            var typesFound = new List<Type>();

            foreach (var type in GetAllTypes())
            {
                try
                {
                    if (type.GetCustomAttributes(attributeType, false).Any())
                    {
                        typesFound.Add(type);
                    }
                }
                catch (Exception)
                {
                    continue; // Some tyupes throw an exception when we try to use reflection on them.
                }
            }

            return typesFound.ToArray();
        }

        /// <summary>
        /// Returns an enumerable of all known types.
        /// </summary>
        private static IEnumerable<Type> GetAllTypes()
        {
            // TODO Rory 09/09/16: Cache types for faster lookup rather than scanning assemblies each time.

            var assemblies =
                AppDomain.CurrentDomain.GetAssemblies()
                    // Automatically exclude the Unity assemblies, which throw exceptions when we try to access them.
                    .Where(a =>
                        !a.FullName.StartsWith("UnityEngine") &&
                        !a.FullName.StartsWith("UnityEditor"));

            foreach (var assembly in assemblies)
            {
                Type[] types;

                try
                {
                    types = assembly.GetTypes();
                }
                catch (Exception)
                {
                    // Ignore assemblies that can't be loaded.
                    continue;
                }

                foreach (var type in types)
                {
                    yield return type;
                }
            }
        }
    }
}
