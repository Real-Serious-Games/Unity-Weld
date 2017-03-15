using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityWeld.Binding.Internal;

namespace UnityWeld.Binding
{
    public abstract class AbstractTemplateSelector : AbstractMemberBinding
    {
        /// <summary>
        /// The view-model, cached during connection.
        /// </summary>
        protected object viewModel;

        /// <summary>
        /// The name of the property we are binding to on the view model.
        /// </summary>
        public string viewModelPropertyName = string.Empty;

        /// <summary>
        /// Watches the view-model property for changes.
        /// </summary>
        protected PropertyWatcher viewModelPropertyWatcher;

        /// <summary>
        /// The gameobject in the scene that is the parent object for the tenplates.
        /// </summary>
        public GameObject templatesRoot;

        /// <summary>
        /// All available templates indexed by the view model the are for.
        /// </summary>
        private IDictionary<Type, Template> AvailableTemplates
        {
            get
            {
                if (availableTemplates == null)
                {
                    CacheTemplates();
                }

                return availableTemplates;
            }
        }

        private IDictionary<Type, Template> availableTemplates;

        /// <summary>
        /// All the child objects that have been created, indexed by the view they are connected to.
        /// </summary>
        private readonly IDictionary<object, GameObject> instantiatedTemplates = new Dictionary<object, GameObject>();

        // Cache available templates.
        private void CacheTemplates()
        {
            availableTemplates = new Dictionary<Type, Template>();

            var templatesInScene = templatesRoot.GetComponentsInChildren<Template>(true);
            foreach (var template in templatesInScene)
            {
                template.gameObject.SetActive(false);
                var typeName = template.GetViewModelTypeName();
                var type = TypeResolver.TypesWithBindingAttribute.FirstOrDefault(t => t.ToString() == typeName);
                if (type == null)
                {
                    Debug.LogError(string.Format("Template object {0} references type {1}, but no matching type with a [Binding] attribute could be found.", template.name, typeName), template);
                    continue;
                }
                
                availableTemplates.Add(type, template);
            }
        }

        /// <summary>
        /// Create a clone of the template object and bind it to the specified view model.
        /// </summary>
        protected void InstantiateTemplate(object templateViewModel)
        {
            Assert.IsNotNull(templateViewModel, "Cannot instantiate child with null view model");
            
            // Select template.
            var selectedTemplate = FindTemplateForType(templateViewModel.GetType());

            var newObject = Instantiate(selectedTemplate);
            newObject.transform.SetParent(transform, false);

            instantiatedTemplates.Add(templateViewModel, newObject.gameObject);

            // Set up child bindings before we activate the template object so that they will be configured properly before trying to connect.
            newObject.InitChildBindings(templateViewModel);

            newObject.gameObject.SetActive(true);
        }

        /// <summary>
        /// Returns the template that best matches the specified type.
        /// </summary>
        private Template FindTemplateForType(Type templateType)
        {
            var possibleMatches = new List<KeyValuePair<int, Type>>();

            // Recursively look in the type, interfaces it implements and types it inherits
            // from for a type that matches a template. Also store how many steps away from 
            // the specified template the found template was.
            Action<Type, int> findMatches = null;
            findMatches = (t, index) =>
            {
                var baseType = t.BaseType;
                if (baseType != null && !baseType.IsInterface)
                {
                    findMatches(baseType, index + 1);
                }

                foreach (var interfaceType in t.GetInterfaces())
                {
                    findMatches(interfaceType, index + 1);
                }

                if (AvailableTemplates.Keys.Contains(t))
                {
                    possibleMatches.Add(new KeyValuePair<int, Type>(index, t));
                }
            };

            // Start the recursive function, starting with an index of 0
            findMatches(templateType, 0);

            if (!possibleMatches.Any())
            {
                throw new ApplicationException("Could not find any template matching type " + templateType);
            }

            var sorted = possibleMatches.OrderBy(m => m.Key);
            var selectedType = sorted.First();

            if (sorted.Skip(1).Any(m => m.Key == selectedType.Key))
            {
                throw new ApplicationException("Multiple templates were found that match type " + templateType
                    + ". Either remove one or provide a more template that more specifically matches the type.");
            }

            return AvailableTemplates[selectedType.Value];
        }

        /// <summary>
        /// Destroys the instantiated template associated with the provided object.
        /// </summary>
        protected void DestroyTemplate(object viewModelToDestroy)
        {
            Destroy(instantiatedTemplates[viewModelToDestroy]);
            instantiatedTemplates.Remove(viewModelToDestroy);
        }

        /// <summary>
        /// Destroys all instantiated templates.
        /// </summary>
        protected void DestroyAllTemplates()
        {
            foreach (var generatedChild in instantiatedTemplates.Values)
            {
                Destroy(generatedChild);
            }

            instantiatedTemplates.Clear();
        }
    }
}
