using System;
using UnityEngine;

namespace WorldShaper
{
    /// <summary>
    /// Specifies that a field should display a popup menu populated with values from a specified list.
    /// </summary>
    /// <remarks>
    /// This attribute is used to associate a field with a list of values defined in a specified type and property. 
    /// The list is dynamically retrieved at runtime based on the provided type and property name.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ListToPopUpAttribute : PropertyAttribute
    {
        /// <summary>
        /// Represents the type of the object or value associated with this instance.
        /// </summary>
        public Type myType;

        /// <summary>
        /// the name of the property in the specified type that contains the list of values.
        /// </summary>
        public string propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListToPopUpAttribute"/> class, specifying the type and property
        /// name to be used for populating a list.
        /// </summary>
        /// <param name="_myType">The type that contains the property to be used for populating the list. Cannot be <see langword="null"/>.</param>
        /// <param name="_propertyName">The name of the property within the specified type that will be used for populating the list. Cannot be <see
        /// langword="null"/> or empty.</param>
        public ListToPopUpAttribute(Type _myType, string _propertyName)
        {
            myType = _myType;
            propertyName = _propertyName;
        }
    }
}
