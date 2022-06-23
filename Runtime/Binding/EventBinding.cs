using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityWeld.Binding.Internal;

namespace UnityWeld.Binding
{
    /// <summary>
    /// Class for binding Unity UI events to methods in a view model.
    /// </summary>
    [AddComponentMenu("Unity Weld/Event Binding")]
    [HelpURL("https://github.com/Real-Serious-Games/Unity-Weld")]
    public class EventBinding : AbstractMemberBinding
    {
        /// <summary>
        /// Name of the method in the view model to bind to.
        /// </summary>
        public string ViewModelMethodName
        {
            get { return viewModelMethodName; }
            set { viewModelMethodName = value; }
        }

        [SerializeField]
        private string viewModelMethodName;

        /// <summary>
        /// Name of the event in the view to bind to.
        /// </summary>
        public string ViewEventName
        {
            get { return viewEventName; }
            set { viewEventName = value; }
        }

        [SerializeField, FormerlySerializedAs("uiEventName")]
        private string viewEventName;

        /// <summary>
        /// Watches a Unity event for updates.
        /// </summary>
        private UnityEventWatcher eventWatcher;

        public override void Connect()
        {
            string methodName;
            object viewModel;
            ParseViewModelEndPointReference(
                viewModelMethodName, 
                out methodName, 
                out viewModel
            );
            var viewModelMethod = viewModel.GetType().GetMethod(methodName, new Type[0]);

            string eventName;
            Component view;
            ParseViewEndPointReference(viewEventName, out eventName, out view);

            eventWatcher = new UnityEventWatcher(view, eventName, 
                () =>
                {
                    if (viewModelMethod != null)
                    {
                        viewModelMethod.Invoke(viewModel, new object[0]);
                    }
                });
        }

        public override void Disconnect()
        {
            if (eventWatcher != null)
            {
                eventWatcher.Dispose();
                eventWatcher = null;
            }
        }
    }
}
