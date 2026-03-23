using UnityEngine;
using UnityEditor;

namespace WorldShaper
{
    [CustomPropertyDrawer(typeof(ConnectionState))]
    public class ConnectionStatePropertyDrawer : PropertyDrawer
    {
        private float height = EditorGUIUtility.singleLineHeight;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => height;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin change check
            EditorGUI.BeginChangeCheck();

            // Get the properties for start and end points
            var startPointProperty = property.FindPropertyRelative("startPoint");
            var endPointProperty = property.FindPropertyRelative("endPoint");

            // Draw the property field 
            EditorGUI.BeginProperty(position, label, property);

            // Set the label for the property
            position = EditorGUI.PrefixLabel(position, label);

            // Calculate the rects for the start and end points
            float gap = 25f;
            float width = (position.width - gap) / 2f;

            // Calculate the rects for the start and end points side by side
            Rect startPointRect = new Rect(position.x, position.y, width, EditorGUIUtility.singleLineHeight);
            Rect endPointRect = new Rect(position.x + width + gap, position.y, width, EditorGUIUtility.singleLineHeight);

            // Calculate the rects for an arrow between the start and end points
            float size = 25f;

            // Center the arrow vertically between the two fields
            Rect arrowRect = new Rect(startPointRect.xMax, startPointRect.y + (startPointRect.height - size) / 2f, gap, size);

            // Draw the start and end point properties
            EditorGUI.PropertyField(startPointRect, startPointProperty, GUIContent.none);
            EditorGUI.PropertyField(endPointRect, endPointProperty, GUIContent.none);

            // Create the arrow icon content
            GUIContent arrowContent = EditorGUIUtility.IconContent("d_Animation.Play");
            arrowContent.tooltip = "From - To";

            // Create the style for the arrow icon
            GUIStyle arrowStyle = new GUIStyle(EditorStyles.label);
            arrowStyle.alignment = TextAnchor.MiddleCenter;
            arrowStyle.normal.textColor = Color.yellowNice;
            arrowStyle.fixedHeight = size;
            arrowStyle.fixedWidth = gap;

            // Draw the arrow icon
            EditorGUI.LabelField(arrowRect, arrowContent, arrowStyle);

            // End change check
            if (EditorGUI.EndChangeCheck())
            {
                // Apply the modified properties to ensure changes are saved
                property.serializedObject.ApplyModifiedProperties();
            }

            // End the property
            EditorGUI.EndProperty();
        }
    }
}
