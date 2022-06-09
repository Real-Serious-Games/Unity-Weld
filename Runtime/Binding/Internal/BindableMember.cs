using System;
using System.Reflection;

namespace UnityWeld.Binding.Internal
{
    /// <summary>
    /// Data structure combining a bindable property or method with the view model it belongs 
    /// to. This is needed because we can't always rely on MemberInfo.ReflectedType
    /// returning the type of the view model if the property or method was declared in an 
    /// interface that the view model inherits from.
    /// </summary>
    public class BindableMember<TMemberType> where TMemberType : MemberInfo
    {
        /// <summary>
        /// The bindable member info (usually a PropertyInfo or MethodInfo)
        /// </summary>
        public readonly TMemberType Member;

        /// <summary>
        /// View model that the property or method belongs to.
        /// </summary>
        public readonly Type ViewModelType;

        /// <summary>
        /// Name of the view model type.
        /// </summary>
        public string ViewModelTypeName
        {
            get
            {
                return ViewModelType.Name;
            }
        }

        /// <summary>
        /// Name of the member.
        /// </summary>
        public string MemberName
        {
            get
            {
                return Member.Name;
            }
        }

        public BindableMember(TMemberType member, Type viewModelType)
        {
            Member = member;
            ViewModelType = viewModelType;
        }

        public override string ToString()
        {
            return string.Concat(ViewModelType.ToString(), ".", MemberName);
        }
    }
}
