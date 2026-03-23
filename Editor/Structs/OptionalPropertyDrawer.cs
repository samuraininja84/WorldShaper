using UnityEngine;
using UnityEditor;

namespace WorldShaper.Editor
{
    /// <summary>
    /// Provides a custom property drawer for the <see cref="Optional{T}"/> type in the Unity Editor.
    /// </summary>
    /// <remarks>
    /// This property drawer renders the value of the <see cref="Optional{T}"/> type alongside a toggle that determines whether the value is enabled. 
    /// <para>When the toggle is unchecked, the value field is disabled in the inspector.</para>
    /// <para>The drawer ensures that changes to the value or toggle are applied to the serialized object.</para>
    /// </remarks>
    [CustomPropertyDrawer(typeof(Optional<>))]
    public class OptionalPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property.FindPropertyRelative("value"));

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin change check
            EditorGUI.BeginChangeCheck();

            // Get the properties for value and enabled state
            var valueProperty = property.FindPropertyRelative("value");
            var enabledProperty = property.FindPropertyRelative("enabled");

            // Begin the property
            EditorGUI.BeginProperty(position, label, property);

            // Adjust the position for value property
            position.width -= 24;

            // Draw the value property and disable it if the enabled property is false
            EditorGUI.BeginDisabledGroup(!enabledProperty.boolValue);
            EditorGUI.PropertyField(position, valueProperty, label, true);
            EditorGUI.EndDisabledGroup();

            // Store the current indent level
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Adjust position for the enabled toggle
            position.x += position.width + 24;
            position.width = position.height = EditorGUI.GetPropertyHeight(enabledProperty);
            position.x -= position.width;

            // Draw the enabled property as a toggle
            EditorGUI.PropertyField(position, enabledProperty, GUIContent.none);

            // Restore the indent level
            EditorGUI.indentLevel = indent;

            // End the property
            EditorGUI.EndProperty();

            // End change check
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}

