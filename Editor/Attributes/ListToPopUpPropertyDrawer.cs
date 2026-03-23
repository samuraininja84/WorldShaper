using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WorldShaper.Editor
{
    /// <summary>
    /// A custom property drawer that displays a string property as a popup menu populated with values from a specified list.
    /// </summary>
    /// <remarks>
    /// This property drawer is used in conjunction with the <see cref="ListToPopUpAttribute"/> to
    /// dynamically populate a popup menu with string values retrieved from a static list field in a specified type. 
    /// The selected value from the popup is assigned to the string property being drawn.  
    /// If the specified list is null or empty, the property will fall back to a default text field.
    /// </remarks>
    [CustomPropertyDrawer(typeof(ListToPopUpAttribute))]
    public class ListToPopUpPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin change check
            EditorGUI.BeginChangeCheck();

            // Get the ListToPopUpAttribute and retrieve the list of strings
            ListToPopUpAttribute atb = attribute as ListToPopUpAttribute;
            List<string> stringList = new List<string>();

            // Begin property
            EditorGUI.BeginProperty(position, label, property);

            // Check if the field exists in the specified type and retrieve its value
            if (atb.myType.GetField(atb.propertyName) != null)
            {
                stringList = atb.myType.GetField(atb.propertyName).GetValue(atb.myType) as List<string>;
            }

            // If the string list is not null and has items, create a popup
            if (stringList != null && stringList.Count > 0)
            {
                // Make sure the index cannot be negative
                int selectedIndex = Mathf.Max(0, stringList.IndexOf(property.stringValue));

                // Draw the popup with the list of strings and the current index
                selectedIndex = EditorGUI.Popup(position, property.displayName, selectedIndex, stringList.ToArray());

                // Update the property value with the selected string
                property.stringValue = stringList[selectedIndex];
            }
            else
            {
                // If the list is empty or null, fall back to the default property field
                EditorGUI.PropertyField(position, property, label);
            }

            // Check if changes were made
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            // End property
            EditorGUI.EndProperty();
        }
    }
}
