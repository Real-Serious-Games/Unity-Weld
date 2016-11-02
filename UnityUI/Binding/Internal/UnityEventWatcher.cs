using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace UnityUI.Binding
{
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
    }

    /// <summary>
    /// Watches an component for a Unity event and triggers an action when the event is raised.
    /// </summary>
    public class UnityEventWatcher : IDisposable
    {
        /// <summary>
        /// The component that owns the event to listen to.
        /// </summary>
        private Component component;

        /// <summary>
        /// The event to listen to.
        /// </summary>
        private string eventName;

        /// <summary>
        /// The action to invoke when the event is raised.
        /// </summary>
        private Action action;

        /// <summary>
        /// Helper object that links to the Unity event.
        /// </summary>
        private UnityEventBinderBase unityEventBinder;

        public UnityEventWatcher(Component component, string eventName, Action action)
        {
            this.component = component;
            this.eventName = eventName;
            this.action = action;
            this.unityEventBinder = new UnityEventBinderFactory().Create(GetBoundEvent(eventName, component).UnityEvent, action);
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
        /// Get all bindable Unity events from a particular component.
        /// </summary>
        private static IEnumerable<BindableEvent> GetBindableEvents(Component component)
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
        }

        /// <summary>
        /// Get a particular Unity event from a component.
        /// </summary>
        private static BindableEvent GetBoundEvent(string boundEventName, Component component)
        {
            var componentType = component.GetType();
            var boundEvent = GetBindableEvents(component)
                .FirstOrDefault();

            if (boundEvent == null)
            {
                throw new ApplicationException("Could not bind to event \"" + boundEventName + "\" on component " + componentType.Name);
            }

            return boundEvent;
        }

        /// <summary>
        /// Get all bindable Unity events on a particular game object.
        /// </summary>
        public static BindableEvent[] GetBindableEvents(GameObject gameObject) //todo: Consider moving this to TypeResolver.
        {
            return gameObject.GetComponents(typeof(Component))
                .SelectMany(component => GetBindableEvents(component))
                .ToArray();
        }
    }
}
