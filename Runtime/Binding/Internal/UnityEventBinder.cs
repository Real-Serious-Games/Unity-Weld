using System;
using System.Linq;
using UnityEngine.Events;
using UnityWeld.Binding.Exceptions;

namespace UnityWeld.Binding.Internal
{
    /// <summary>
    /// Factory for adding the correct type of listener to a generic UnityEvent, given a view model and 
    /// the name of a method in that view model to bind the UnityEvent to.
    /// </summary>
    internal static class UnityEventBinderFactory
    {
        /// <summary>
        /// Set up and bind a given UnityEvent with a list of types matching its generic type arguments.
        /// </summary>
        public static UnityEventBinderBase Create(UnityEventBase unityEvent, Action action)
        {
            // Note that to find the paramaters of events on the UI, we need to see what 
            // generic arguments were passed to the UnityEvent they inherit from.
            var baseType = unityEvent.GetType().BaseType;
            var eventArgumentTypes = baseType != null
                ? baseType.GetGenericArguments()
                : null;

            if (eventArgumentTypes == null || !eventArgumentTypes.Any())
            {
                return new UnityEventBinder(unityEvent, action);
            }

            try
            {
                var genericType = typeof(UnityEventBinder<>).MakeGenericType(eventArgumentTypes);
                return (UnityEventBinderBase)Activator.CreateInstance(
                    genericType, 
                    unityEvent, 
                    action
                );
            }
            catch (ArgumentException ex)
            {
                // There are only UnityEvents and UnityActions that support up to 5 arguments. 
                // MakeGenericType will throw an ArgumentException if it is used to try and create a type with arguments that don't match any generic type.
                throw new InvalidEventException("Cannot bind event with more than 5 arguments", ex);
            }
            
        }
    }

    /// <summary>
    /// Abstract class for generic event binders to inherit from.
    /// </summary>
    internal abstract class UnityEventBinderBase : IDisposable
    {
        public abstract void Dispose();
    }

    internal class UnityEventBinder : UnityEventBinderBase
    {
        private UnityEvent unityEvent;
        private readonly Action action;

        public UnityEventBinder(UnityEventBase unityEvent, Action action)
        {
            this.unityEvent = (UnityEvent)unityEvent;
            this.action = action;
            this.unityEvent.AddListener(EventHandler);
        }

        public override void Dispose()
        {
            if (unityEvent == null)
            {
                return;
            }

            unityEvent.RemoveListener(EventHandler);
            unityEvent = null;
        }

        private void EventHandler()
        {
            action();
        }
    }

    internal class UnityEventBinder<T0> : UnityEventBinderBase
    {
        private UnityEvent<T0> unityEvent;
        private readonly Action action;

        public UnityEventBinder(UnityEventBase unityEvent, Action action)
        {
            this.unityEvent = (UnityEvent<T0>)unityEvent;
            this.action = action;
            this.unityEvent.AddListener(EventHandler);
        }

        public override void Dispose()
        {
            if (unityEvent == null)
            {
                return;
            }

            unityEvent.RemoveListener(EventHandler);
            unityEvent = null;
        }

        private void EventHandler(T0 arg0)
        {
            action();
        }
    }

    internal class UnityEventBinder<T0, T1> : UnityEventBinderBase
    {
        private UnityEvent<T0, T1> unityEvent;
        private readonly Action action;

        public UnityEventBinder(UnityEventBase unityEvent, Action action)
        {
            this.unityEvent = (UnityEvent<T0, T1>)unityEvent;
            this.action = action;
            this.unityEvent.AddListener(EventHandler);
        }

        public override void Dispose()
        {
            if (unityEvent == null)
            {
                return;
            }

            unityEvent.RemoveListener(EventHandler);
            unityEvent = null;
        }

        private void EventHandler(T0 arg0, T1 arg1)
        {
            action();
        }
    }
}
