using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityUI.Binding
{
    /// <summary>
    /// Syncrhonises the value between two properties using reflection.
    /// </summary>
    public class PropertySync
    {
        /// <summary>
        /// The property that is the source of the synchronziation.
        /// </summary>
        private PropertyEndPoint source;

        /// <summary>
        /// The property that is the destination of the synchronziation.
        /// </summary>
        private PropertyEndPoint dest;

        /// <summary>
        /// The property that is set for any exception that occurs during type conversion and validation.
        /// </summary>
        private PropertyEndPoint exception;

        /// <summary>
        /// The Unity context for error logging.
        /// </summary>
        private Component context;

        public PropertySync(PropertyEndPoint source, PropertyEndPoint dest, PropertyEndPoint exception, Component context)
        {
            this.source = source;
            this.dest = dest;
            this.exception = exception;
            this.context = context;
        }

        /// <summary>
        /// Syncrhonise the value from the source to the destination.
        /// </summary>
        public void SyncFromSource()
        {
            try
            {
                dest.SetValue(source.GetValue());

                if (exception != null)
                {
                    exception.SetValue(null);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to convert value from " + source.ToString() + " to " + dest.ToString(), context);
                Debug.LogException(ex);
            }
        }

        /// <summary>
        /// Syncrhonise the value from the destination to the source.
        /// </summary>
        public void SyncFromDest()
        {

            try
            {
                source.SetValue(dest.GetValue());

                if (exception != null)
                {
                    exception.SetValue(null);
                }
            }
            catch (Exception ex)
            {
                if (exception != null)
                {
                    exception.SetValue(ex);
                }
                else
                {
                    Debug.LogError("Failed to convert value from " + source.ToString() + " to " + dest.ToString(), context);
                    Debug.LogException(ex);
                }
            }
        }
    }
}
