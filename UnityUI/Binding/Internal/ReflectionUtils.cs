using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityUI.Binding
{
    internal class ReflectionUtils
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
    }
}
