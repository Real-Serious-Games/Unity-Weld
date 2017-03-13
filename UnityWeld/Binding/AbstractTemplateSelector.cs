using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

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
        public GameObject templates;

        /// <summary>
        /// All available templates indexed by the view model the are for.
        /// </summary>
        private readonly IDictionary<string, Template> availableTemplates = new Dictionary<string, Template>();

        /// <summary>
        /// All the child objects that have been created, indexed by the view they are connected to.
        /// </summary>
        private readonly IDictionary<object, GameObject> instantiatedTemplates = new Dictionary<object, GameObject>();

        protected new void Awake()
        {
            Assert.IsNotNull(templates, "No templates have been assigned.");

            CacheTemplates();

            base.Awake();
        }

        // Cache available templates.
        protected void CacheTemplates()
        {
            availableTemplates.Clear();

            var templatesInScene = templates.GetComponentsInChildren<Template>(true);
            foreach (var template in templatesInScene)
            {
                template.gameObject.SetActive(false);
                availableTemplates.Add(template.GetViewModelTypeName(), template);
            }
        }

        /// <summary>
        /// Create a clone of the template object and bind it to the specified view model.
        /// </summary>
        protected void InstantiateTemplate(object viewModel)
        {
            Assert.IsNotNull(viewModel, "Cannot instantiate child with null view model");
            
            // Select template.
            var viewModelTypeString = viewModel.GetType().ToString();
            Template selectedTemplate = null;
            if (!availableTemplates.TryGetValue(viewModelTypeString, out selectedTemplate))
            {
                throw new ApplicationException("Cannot find matching template for: " + viewModelTypeString);
            }

            var newObject = Instantiate(selectedTemplate);
            newObject.transform.SetParent(transform, false);

            instantiatedTemplates.Add(viewModel, newObject.gameObject);

            // Set up child bindings before we activate the template object so that they will be configured properly before trying to connect.
            newObject.InitChildBindings(viewModel);

            newObject.gameObject.SetActive(true);
        }

        /// <summary>
        /// Destroys the instantiated template associated with the provided object.
        /// </summary>
        protected void DestroyTemplate(object viewModel)
        {
            Destroy(instantiatedTemplates[viewModel]);
            instantiatedTemplates.Remove(viewModel);
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
