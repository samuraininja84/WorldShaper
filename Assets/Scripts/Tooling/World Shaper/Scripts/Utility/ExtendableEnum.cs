using System;
using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper
{
    [Serializable]
    public class ExtendableEnum
    {
        public List<string> list = new List<string> { "None" };
        public string value;
        public bool hideLabel = false;

        public ExtendableEnum(List<string> strings, int i = 1)
        {
            list = SetEnums(strings);
            if (i > 1 && i < list.Count - 1) value = list[i];
            else value = list[0];
        }

        public List<string> SetEnums(List<string> strings)
        {
            list = strings;
            return list;
        }

        public bool Equal(List<string> strings)
        {
            return list == strings;
        }

        public bool Equal(string value)
        {
            return this.value == value;
        }

        public bool Empty()
        {
            return list.Count <= 1;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class ListToPopUpAttribute : PropertyAttribute
    {
        public Type myType;
        public string propertyName;

        public ListToPopUpAttribute(Type _myType, string _propertyName)
        {
            myType = _myType;
            propertyName = _propertyName;
        }
    }
}
