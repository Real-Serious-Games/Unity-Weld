using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityUI.Binding
{
    /// <summary>
    /// Component that can be placed at the root of a tree of objects that we want to bind to one or more view models.
    /// </summary>
    public class BindingRoot : MonoBehaviour
    {
        void Start()
        {
            // Set up all bindings
            foreach (var binding in GetComponentsInChildren<IMemberBinding>())
            {
                binding.Init();
            }
        }
    }
}
