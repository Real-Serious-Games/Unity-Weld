using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUI.Binding
{
    public class EventBinder : IDisposable
    {
        private GameObject gameObject;

        /// <summary>
        /// Type of the component we're binding to.
        /// </summary>
        string boundComponentType;

        /// <summary>
        /// Name of the event to bind to.
        /// </summary>
        string boundEventName;

        UnityEventBinderBase unityEventBinder;

        /// <summary>
        /// Create a new EventBinder
        /// </summary>
        public EventBinder(GameObject gameObject, string methodName, string eventName, string boundComponentType, IAdapter adapter, IViewModelBinding viewModel)
        {
            this.gameObject = gameObject;
            this.boundEventName = eventName;
            this.boundComponentType = boundComponentType;

            var boundEvent = GetBoundEvent();
            unityEventBinder = new UnityEventBinderFactory().Create(boundEvent.UnityEvent, viewModel, methodName);
        }

        /// <summary>
        /// Use reflection to find all components with unity events on an object
        /// </summary>
        public static IEnumerable<BindableEvent> GetBindableEvents(GameObject gameObject)
        {
            return gameObject.GetComponents<Component>()
                .SelectMany(component =>
                {
                    var type = component.GetType();

                    var bindableEventsFromProperties = type
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(propertyInfo => propertyInfo.PropertyType.IsSubclassOf(typeof(UnityEventBase)))
                        .Select(propertyInfo => new BindableEvent()
                        {
                            UnityEvent = (UnityEventBase)propertyInfo.GetValue(component, null),
                            Name = propertyInfo.Name,
                            DeclaringType = propertyInfo.DeclaringType,
                            ComponentType = component.GetType()
                        });

                    var bindableEventsFromFields = type
                        .GetFields(BindingFlags.Instance | BindingFlags.Public)
                        .Where(fieldInfo => fieldInfo.FieldType.IsSubclassOf(typeof(UnityEventBase)))
                        .Select(fieldInfo => new BindableEvent()
                        {
                            UnityEvent = (UnityEventBase)fieldInfo.GetValue(component),
                            Name = fieldInfo.Name,
                            DeclaringType = fieldInfo.DeclaringType,
                            ComponentType = type
                        });

                    return bindableEventsFromFields.Concat(bindableEventsFromProperties);
                });
        }

        /// <summary>
        /// Use reflection to find all components with Unity events on this object.
        /// </summary>
        public IEnumerable<BindableEvent> GetBindableEvents()
        {
            return GetBindableEvents(gameObject);
        }

        private BindableEvent GetBoundEvent()
        {
            var boundEvent = GetBindableEvents()
                .Where(evt => evt.Name == boundEventName && evt.ComponentType.Name == boundComponentType)
                .FirstOrDefault();

            if (boundEvent == null)
            {
                throw new ApplicationException("Could not bind to event \"" + boundEventName + "\" on component " + boundComponentType);
            }

            return boundEvent;
        }

        public void Dispose()
        {
            if (unityEventBinder != null)
            {
                unityEventBinder.Dispose();
                unityEventBinder = null;
            }
        }

        /// <summary>
        /// Information needed to bind to a UnityEvent on a component.
        /// </summary>
        public class BindableEvent
        {
            /// <summary>
            /// UnityEvent to bind to.
            /// </summary>
            public UnityEventBase UnityEvent { get; set; }

            /// <summary>
            /// The name of the property or field contaning this event.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// The type that the event belongs to.
            /// </summary>
            public Type DeclaringType { get; set; }

            /// <summary>
            /// Type of the component we're binding to. This doesn't account for multiple 
            /// components of the same type on the same GameObject, but there doesn't seem
            /// to be any other easy way of doing that since component instance IDs are
            /// re-generated frequently.
            /// </summary>
            public Type ComponentType { get; set; }

            /// <summary>
            /// Returns the types of the event
            /// </summary>
            public Type[] GetEventTypes()
            {
                return UnityEvent.GetType().BaseType.GetGenericArguments();
            }
        }
    }
}
