using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityWeld.Binding;
using UnityWeld.Binding.Internal;

namespace UnityWeld_Editor
{
    /// <summary>
    /// A base editor for Unity-Weld bindings.
    /// </summary>
    public class BaseBindingEditor : Editor
    {
        /// <summary>
        /// Sets the specified value and sets dirty to true if it doesn't match the old value.
        /// </summary>
        protected void UpdateProperty<TValue>(Action<TValue> setter, TValue oldValue, TValue newValue, string undoActionName)
            where TValue : class
        {
            if (newValue == oldValue)
            {
                return;
            }

            Undo.RecordObject(target, undoActionName);

            setter(newValue);

            InspectorUtils.MarkSceneDirty(((Component)target).gameObject);
        }

        /// <summary>
        /// Display the adapters popup menu.
        /// </summary>
        protected static void ShowAdapterMenu(
            GUIContent label,
            string[] adapterTypeNames,
            string curValue,
            Action<string> valueUpdated
        )
        {
            var adapterMenu = new[] { "None" }
                .Concat(adapterTypeNames)
                .Select(typeName => new GUIContent(typeName))
                .ToArray();

            var curSelectionIndex = Array.IndexOf(adapterTypeNames, curValue) + 1; // +1 to account for 'None'.
            var newSelectionIndex = EditorGUILayout.Popup(
                    label,
                    curSelectionIndex,
                    adapterMenu
                );

            if (newSelectionIndex == curSelectionIndex)
            {
                return;
            }

            if (newSelectionIndex == 0)
            {
                valueUpdated(null); // No adapter selected.
            }
            else
            {
                valueUpdated(adapterTypeNames[newSelectionIndex - 1]); // -1 to account for 'None'.
            }
        }

        /// <summary>
        /// Display a popup menu for selecting a property from a view-model.
        /// </summary>
        protected void ShowViewModelPropertyMenu(
            GUIContent label,
            PropertyInfo[] bindableProperties,
            Action<string> propertyValueSetter,
            string curPropertyValue,
            Func<PropertyInfo, bool> menuEnabled
        )
        {
            InspectorUtils.DoPopup(
                new GUIContent(curPropertyValue),
                label,
                property => string.Concat(property.ReflectedType, "/", property.Name, " : ", property.PropertyType.Name),
                menuEnabled,
                property => MemberInfoToString(property) == curPropertyValue,
                property => UpdateProperty(
                    propertyValueSetter,
                    curPropertyValue,
                    MemberInfoToString(property),
                    "Set view-model property"
                ),
                bindableProperties
                    .OrderBy(property => property.ReflectedType.Name)
                    .ThenBy(property => property.Name)
                    .ToArray()
            );
        }

        /// <summary>
        /// Class used to wrap property infos
        /// </summary>
        private class OptionInfo
        {
            public OptionInfo(string menuName, PropertyInfo propertyInfo)
            {
                this.MenuName = menuName;
                this.PropertyInfo = propertyInfo;
            }

            public string MenuName { get; private set; }

            public PropertyInfo PropertyInfo { get; private set; }
        }

        /// <summary>
        /// The string used to show that no option is selected in the property menu.
        /// </summary>
        private static readonly string NoneOptionString = "None";

        /// <summary>
        /// Display a popup menu for selecting a property from a view-model.
        /// </summary>
        protected void ShowViewModelPropertyMenuWithNone(
            GUIContent label,
            PropertyInfo[] bindableProperties,
            Action<string> propertyValueSetter,
            string curPropertyValue,
            Func<PropertyInfo, bool> menuEnabled
        )
        {
            var options = bindableProperties
                .Select(property => new OptionInfo(string.Concat(property.ReflectedType, "/", property.Name, " : ", property.PropertyType.Name), property))
                .OrderBy(option => option.PropertyInfo.ReflectedType.Name)
                .ThenBy(option => option.PropertyInfo.Name);

            var noneOption = new OptionInfo(NoneOptionString, null);

            InspectorUtils.DoPopup(
                new GUIContent(string.IsNullOrEmpty(curPropertyValue) ? NoneOptionString : curPropertyValue),
                label,
                option => option.MenuName,
                option => option.MenuName == NoneOptionString ? true : menuEnabled(option.PropertyInfo),
                option =>
                {
                    if (option == noneOption)
                    {
                        return string.IsNullOrEmpty(curPropertyValue);
                    }
                    
                    return MemberInfoToString(option.PropertyInfo) == curPropertyValue;
                },
                option => UpdateProperty(
                    propertyValueSetter,
                    curPropertyValue,
                    option.PropertyInfo == null ? string.Empty : MemberInfoToString(option.PropertyInfo),
                    "Set view-model property"
                ),
                new[] { noneOption }
                    .Concat(options)
                    .ToArray()
            );
        }

        /// <summary>
        /// Shows a dropdown for selecting a property in the UI to bind to.
        /// </summary>
        protected void ShowViewPropertyMenu(
            GUIContent label, 
            PropertyInfo[] properties, 
            Action<string> propertyValueSetter,
            string curPropertyValue,
            out Type selectedPropertyType
        )
        {
            var propertyNames = properties
                .Select(MemberInfoToString)
                .ToArray();
            var selectedIndex = Array.IndexOf(propertyNames, curPropertyValue);
            var content = properties.Select(prop => new GUIContent(string.Concat(
                    prop.ReflectedType.Name, 
                    "/",
                    prop.Name, 
                    " : ",
                    prop.PropertyType.Name
                )))
                .ToArray();

            var newSelectedIndex = EditorGUILayout.Popup(label, selectedIndex, content);
            if (newSelectedIndex != selectedIndex)
            {
                var newSelectedProperty = properties[newSelectedIndex];

                UpdateProperty(
                    propertyValueSetter,
                    curPropertyValue,
                    MemberInfoToString(newSelectedProperty),
                    "Set view property"
                );

                selectedPropertyType = newSelectedProperty.PropertyType;
            }
            else
            {
                if (selectedIndex < 0)
                {
                    selectedPropertyType = null;
                    return;
                }

                selectedPropertyType = properties[selectedIndex].PropertyType;
            }
        }

        /// <summary>
        /// Show dropdown for selecting a UnityEvent to bind to.
        /// </summary>
        protected void ShowEventMenu(
            BindableEvent[] events,
            Action<string> propertyValueSetter,
            string curPropertyValue
        )
        {
            var eventNames = events
                .Select(BindableEventToString)
                .ToArray();
            var selectedIndex = Array.IndexOf(eventNames, curPropertyValue);
            var content = events
                .Select(evt => new GUIContent(evt.ComponentType.Name + "." + evt.Name))
                .ToArray();

            var newSelectedIndex = EditorGUILayout.Popup(
                new GUIContent("View event", "Event on the view to bind to."),
                selectedIndex,
                content
            );

            if (newSelectedIndex == selectedIndex)
            {
                return;
            }

            var selectedEvent = events[newSelectedIndex];
            UpdateProperty(
                propertyValueSetter,
                curPropertyValue,
                BindableEventToString(selectedEvent),
                "Set bound event"
            );
        }

        /// <summary>
        /// Returns whether or not we should show an adapter options selector for the specified 
        /// adapter type and finds the type for the specified type name.
        /// </summary>
        protected bool ShouldShowAdapterOptions(string adapterTypeName, out Type adapterType)
        {
            // Don't show selector until an adapter has been selected.
            if (string.IsNullOrEmpty(adapterTypeName))
            {
                adapterType = null;
                return false;
            }

            var adapterAttribute = FindAdapterAttribute(adapterTypeName);
            if (adapterAttribute == null)
            {
                adapterType = null;
                return false;
            }

            adapterType = adapterAttribute.OptionsType;

            // Don't show selector unless the current adapter has its own overridden
            // adapter options type.
            return adapterType != typeof(AdapterOptions);
        }

        /// <summary>
        /// Show a field for selecting an AdapterOptions object matching the specified type of adapter.
        /// </summary>
        protected void ShowAdapterOptionsMenu(
            string label, 
            Type adapterOptionsType, 
            Action<AdapterOptions> propertyValueSetter, 
            AdapterOptions currentPropertyValue,
            float fadeAmount
        )
        {
            if (EditorGUILayout.BeginFadeGroup(fadeAmount))
            {
                EditorGUI.indentLevel++;

                var newAdapterOptions = (AdapterOptions)EditorGUILayout.ObjectField(
                    label, 
                    currentPropertyValue, 
                    adapterOptionsType, 
                    false
                );

                EditorGUI.indentLevel--;

                UpdateProperty(
                    propertyValueSetter, 
                    currentPropertyValue, 
                    newAdapterOptions,
                    "Set adapter options"
                );
            }
            EditorGUILayout.EndFadeGroup();
        }

        /// <summary>
        /// Find the adapter attribute for a named adapter type.
        /// </summary>
        protected AdapterAttribute FindAdapterAttribute(string adapterName)
        {
            if (!string.IsNullOrEmpty(adapterName))
            {
                var adapterType = TypeResolver.FindAdapterType(adapterName);
                if (adapterType != null)
                {
                    return TypeResolver.FindAdapterAttribute(adapterType);
                }
            }

            return null;
        }

        /// <summary>
        /// Pass a type through an adapter and get the result.
        /// </summary>
        protected Type AdaptTypeBackward(Type inputType, string adapterName)
        {
            var adapterAttribute = FindAdapterAttribute(adapterName);

            return adapterAttribute != null ? adapterAttribute.InputType : inputType;
        }

        /// <summary>
        /// Pass a type through an adapter and get the result.
        /// </summary>
        protected Type AdaptTypeForward(Type inputType, string adapterName)
        {
            var adapterAttribute = FindAdapterAttribute(adapterName);

            return adapterAttribute != null ? adapterAttribute.OutputType : inputType;
        }

        /// <summary>
        /// Convert a MemberInfo to a uniquely identifiable string.
        /// </summary>
        protected static string MemberInfoToString(MemberInfo member)
        {
            return string.Concat(member.ReflectedType.ToString(), ".", member.Name);
        }

        /// <summary>
        /// Convert a BindableEvent to a uniquely identifiable string.
        /// </summary>
        private static string BindableEventToString(BindableEvent evt)
        {
            return string.Concat(evt.ComponentType.ToString(), ".", evt.Name);
        }

        /// <summary>
        /// Returns an array of all the names of adapter types that match the 
        /// provided prediate function.
        /// </summary>
        protected static string[] GetAdapterTypeNames(Func<Type, bool> adapterSelectionPredicate)
        {
            return TypeResolver.TypesWithAdapterAttribute
                .Where(adapterSelectionPredicate)
                .Select(type => type.ToString())
                .ToArray();
        }
    }
}
