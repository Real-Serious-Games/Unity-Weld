using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Events;

namespace UnityUI.Binding
{
    /// <summary>
    /// Factory for adding the correct type of listener to a generic UnityEvent, given a view model and 
    /// the name of a method in that view model to bind the UnityEvent to.
    /// </summary>
    internal class UnityEventBinderFactory
    {
        /// <summary>
        /// Set up and bind a given UnityEvent with a list of types matching its generic type arguments.
        /// </summary>
        public UnityEventBinderBase Create(UnityEventBase unityEvent, IViewModelBinding viewModel, string methodName)
        {
            // Note that to find the paramaters of events on the UI, we need to see what 
            // generic arguments were passed to the UnityEvent they inherit from.
            var eventArgumentTypes = unityEvent.GetType().BaseType.GetGenericArguments();

            if (!eventArgumentTypes.Any())
            {
                return new UnityEventBinder(unityEvent, viewModel, methodName);
            }

            try
            {
                var genericType = typeof(UnityEventBinder<>).MakeGenericType(eventArgumentTypes);
                return (UnityEventBinderBase)Activator.CreateInstance(genericType, unityEvent, viewModel, methodName);
            }
            catch (ArgumentException ex)
            {
                // There are only UnityEvents and UnityActions that support up to 5 arguments. 
                // MakeGenericType will throw an ArgumentException if it is used to try and create a type with arguments that don't match any generic type.
                throw new ApplicationException("Cannot bind event with more than 5 arguments", ex);
            }
            
        }

        /// <summary>
        /// Set up and bind a given UnityEvent with a list of types matching its generic type arguments
        /// and an adapter.
        /// </summary>
        public UnityEventBinderBase Create(UnityEventBase unityEvent,
            IViewModelBinding viewModel,
            string methodName,
            IAdapter adapter)
        {
            // TODO Rory (16/09/2016): Add argument checking

            // Note that to find the paramaters of events on the UI, we need to see what 
            // generic arguments were passed to the UnityEvent they inherit from.
            var eventArgumentTypes = unityEvent.GetType().BaseType.GetGenericArguments();
            if (eventArgumentTypes.Count() != 1)
            {
                throw new ApplicationException("Adapters can only be used on events with a single argument");
            }

            var adapterType = adapter.GetType().GetCustomAttributes(false)
                .Where(attribute => attribute.GetType() == typeof(AdapterAttribute))
                .Select(attribute => (AdapterAttribute)attribute)
                .Single()
                .OutputType;

            var genericType = typeof(UnityEventBinder<>).MakeGenericType(eventArgumentTypes);
            return (UnityEventBinderBase)Activator
                .CreateInstance(genericType, unityEvent, viewModel, methodName, adapter);
        }
    }

    /// <summary>
    /// Abstract class for generic event binders to inherit from.
    /// </summary>
    abstract internal class UnityEventBinderBase : IDisposable
    {
        public abstract void Dispose();
    }

    internal class UnityEventBinder : UnityEventBinderBase
    {
        private string methodName;
        private UnityEvent unityEvent;
        private IViewModelBinding viewModel;

        public UnityEventBinder(UnityEventBase unityEvent, IViewModelBinding viewModel, string methodName)
        {
            this.unityEvent = (UnityEvent)unityEvent;
            this.viewModel = viewModel;
            this.methodName = methodName;
            this.unityEvent.AddListener(EventHandler);
        }

        public override void Dispose()
        {
            if (unityEvent != null)
            {
                unityEvent.RemoveListener(EventHandler);
                unityEvent = null;
            }
        }

        private void EventHandler()
        {
            viewModel.SendEvent(methodName);
        }
    }

    internal class UnityEventBinder<T0> : UnityEventBinderBase
    {
        private string methodName;
        private UnityEvent<T0> unityEvent;
        private IViewModelBinding viewModel;
        private IAdapter adapter = null;

        public UnityEventBinder(UnityEventBase unityEvent, IViewModelBinding viewModel, string methodName)
        {
            this.unityEvent = (UnityEvent<T0>)unityEvent;
            this.viewModel = viewModel;
            this.methodName = methodName;
            this.unityEvent.AddListener(EventHandler);
        }

        public UnityEventBinder(UnityEventBase unityEvent, IViewModelBinding viewModel, string methodName, IAdapter adapter)
        {
            this.unityEvent = (UnityEvent<T0>)unityEvent;
            this.viewModel = viewModel;
            this.methodName = methodName;
            this.adapter = adapter;
            this.unityEvent.AddListener(EventHandlerWithAdapter);
        }

        public override void Dispose()
        {
            if (unityEvent != null)
            {
                unityEvent.RemoveListener(EventHandler);
                unityEvent = null;
            }
        }

        private void EventHandler(T0 arg0)
        {
            viewModel.SendEvent(methodName, arg0);
        }

        private void EventHandlerWithAdapter(T0 arg0)
        {
            var value = adapter.Convert(arg0);
            viewModel.SendEvent(methodName, value);
        }
    }

    internal class UnityEventBinder<T0, T1> : UnityEventBinderBase
    {
        private string methodName;
        private UnityEvent<T0, T1> unityEvent;
        private IViewModelBinding viewModel;

        public UnityEventBinder(UnityEventBase unityEvent, IViewModelBinding viewModel, string methodName)
        {
            this.unityEvent = (UnityEvent<T0, T1>)unityEvent;
            this.viewModel = viewModel;
            this.methodName = methodName;
            this.unityEvent.AddListener(EventHandler);
        }

        public override void Dispose()
        {
            if (unityEvent != null)
            {
                unityEvent.RemoveListener(EventHandler);
                unityEvent = null;
            }
        }

        private void EventHandler(T0 arg0, T1 arg1)
        {
            viewModel.SendEvent(methodName, arg0, arg1);
        }
    }
}
