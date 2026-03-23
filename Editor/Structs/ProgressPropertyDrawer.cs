using UnityEngine;
using UnityEditor;

namespace WorldShaper
{
    [CustomPropertyDrawer(typeof(Progress))]
    public class ProgressPropertyDrawer : PropertyDrawer
    {
        private float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => height;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin change check
            EditorGUI.BeginChangeCheck();

            // Get the properties for progress
            var progressProperty = property.FindPropertyRelative("value");

            // Draw the property field 
            EditorGUI.BeginProperty(position, label, property);

            // Set the label for the property
            position = EditorGUI.PrefixLabel(position, label);

            // Calculate the rect for the progress bar
            Rect progressRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // Draw the progress bar
            EditorGUI.PropertyField(progressRect, progressProperty, GUIContent.none);

            // End change check
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            // End the property
            EditorGUI.EndProperty();
        }
    }
}
