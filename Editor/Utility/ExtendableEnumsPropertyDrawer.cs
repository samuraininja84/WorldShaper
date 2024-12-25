using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WorldShaper.Editor
{
    [CustomPropertyDrawer(typeof(ExtendableEnum))]
    public class ExtendableEnumPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Get single line height
            float singleLineHeight = EditorGUIUtility.singleLineHeight * 1f;
            return singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin change check
            EditorGUI.BeginChangeCheck();

            var atb = new ListToPopUpAttribute(typeof(ExtendableEnum), "list");
            var value = property.FindPropertyRelative("value");
            var strings = property.FindPropertyRelative("list");
            var showLabel = property.FindPropertyRelative("hideLabel");
            List<string> stringList = new List<string>();

            // Get the label from the property
            string propertyLabel = null;
            if (!showLabel.boolValue)
            {
                propertyLabel = property.displayName;
            }
            else if (showLabel.boolValue)
            {
                propertyLabel = string.Empty;
            }

            // Get the list of strings from the ExtendableEnum list
            if (strings != null)
            {
                stringList = new List<string>();
                for (int i = 0; i < strings.arraySize; i++)
                {
                    stringList.Add(strings.GetArrayElementAtIndex(i).stringValue);
                }
            }

            // If the list is empty, disable the popup gui before the popup is drawn
            if (stringList.Count <= 1 && stringList[0] == "None") GUI.enabled = false;

            // If the list is not empty, create a popup with the list
            if (stringList != null && stringList.Count > 0)
            {
                int selectedIndex = Mathf.Max(stringList.IndexOf(value.stringValue), 0);
                selectedIndex = EditorGUI.Popup(position, propertyLabel, selectedIndex, stringList.ToArray());
                string currentValue = stringList[selectedIndex];
                value.stringValue = currentValue;
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }

            // If the list is empty, enable the popup gui after the popup is drawn
            if (stringList.Count <= 1 && stringList[0] == "None") GUI.enabled = true;

            // Check if changes were made
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }

    [CustomPropertyDrawer(typeof(ListToPopUpAttribute))]
    public class ListToPopUpPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            // Get single line height
            float singleLineHeight = EditorGUIUtility.singleLineHeight * 1f;
            return singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin change check
            EditorGUI.BeginChangeCheck();

            ListToPopUpAttribute atb = attribute as ListToPopUpAttribute;
            List<string> stringList = new List<string>();

            if (atb.myType.GetField(atb.propertyName) != null)
            {
                stringList = atb.myType.GetField(atb.propertyName).GetValue(atb.myType) as List<string>;
            }

            if (stringList.Count <= 1) GUI.enabled = false;

            if (stringList != null && stringList.Count > 0)
            {
                int selectedIndex = Mathf.Max(stringList.IndexOf(property.stringValue), 0);
                selectedIndex = EditorGUI.Popup(position, property.displayName, selectedIndex, stringList.ToArray());
                property.stringValue = stringList[selectedIndex];
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }

            if (stringList.Count <= 1) GUI.enabled = true;

            // Check if changes were made
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
