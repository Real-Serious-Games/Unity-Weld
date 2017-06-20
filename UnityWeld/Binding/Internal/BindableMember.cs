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
    public class BindableMember<MemberType> where MemberType : MemberInfo
    {
        /// <summary>
        /// The bindable member info (usually a PropertyInfo or MethodInfo)
        /// </summary>
        public readonly MemberType Member;

        /// <summary>
        /// View model that the property or method belongs to.
        /// </summary>
        public readonly Type ViewModelType;

        public BindableMember(MemberType member, Type viewModelType)
        {
            Member = member;
            ViewModelType = viewModelType;
        }

        public override string ToString()
        {
            return string.Concat(ViewModelType.ToString(), ".", Member.Name);
        }
    }
}
