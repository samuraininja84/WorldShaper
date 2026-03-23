using UnityEngine;
using UnityEditor;

namespace WorldShaper.Editor
{
    /// <summary>
    /// Provides a custom property drawer for the <see cref="ObjectLocator"/> type in the Unity Editor.
    /// </summary>
    /// <remarks>
    /// This drawer allows users to select a target object and specify a tag within the Unity Editor.
    /// It includes a button to automatically find and assign the first GameObject with the specified tag in the scene.
    /// </remarks>
    [CustomPropertyDrawer(typeof(ObjectLocator))]
    public class ObjectLocatorPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin change check
            EditorGUI.BeginChangeCheck();

            // Get the current ExtendableEnum from the property
            ObjectLocator current = (ObjectLocator)fieldInfo.GetValue(property.serializedObject.targetObject);

            // Get the properties for value, list, showLabel, and showSelection
            var targetProperty = property.FindPropertyRelative("target");
            var tagProperty = property.FindPropertyRelative("tag");

            // Begin property
            EditorGUI.BeginProperty(position, label, property);

            // Draw label and get position
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            // Store the current indent level and set it to zero
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate the width and offset for the target and tag fields
            float spacing = 3f;
            float buttonWidth = 20f;
            float width = (position.width - spacing - buttonWidth) / 2;

            // Get the rects for the target and tag fields
            Rect targetRect = new Rect(position.x, position.y, width, EditorGUIUtility.singleLineHeight);
            Rect tagRect = new Rect(position.x + width + spacing, position.y, width, EditorGUIUtility.singleLineHeight);

            // Create a rect for the find object button
            Rect findObjectRect = new Rect(position.x + width + spacing + width, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);

            // Draw the target field
            EditorGUI.PropertyField(targetRect, targetProperty, GUIContent.none);

            // Draw the tag field
            tagProperty.stringValue = EditorGUI.TagField(tagRect, GUIContent.none, tagProperty.stringValue);

            // Create a GUIStyle for the find object button
            GUIStyle findObjectStyle = new GUIStyle(EditorStyles.miniButton);
            findObjectStyle.alignment = TextAnchor.MiddleCenter;
            findObjectStyle.padding = new RectOffset(1, 0, 1, 0);
            findObjectStyle.fixedHeight = EditorGUIUtility.singleLineHeight;
            findObjectStyle.fixedWidth = buttonWidth;

            // Create a GUIContent for the button
            GUIContent findObjectContent = EditorGUIUtility.IconContent("Animation.FilterBySelection");
            findObjectContent.tooltip = "Get the first GameObject with this tag in the scene";

            // Draw the find object button
            if (GUI.Button(findObjectRect, findObjectContent, findObjectStyle))
            {
                // Find the GameObject in the scene
                GameObject foundObject = GameObject.FindWithTag(tagProperty.stringValue);
                targetProperty.objectReferenceValue = foundObject;
            }

            // Reset indent level
            EditorGUI.indentLevel = indent;

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
