using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WorldShaper
{
    [CustomPropertyDrawer(typeof(PassageData))]
    public class PassageDataPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin change check
            EditorGUI.BeginChangeCheck();

            // Get value properties
            var areaProperty = property.FindPropertyRelative("Area");
            var endPointProperty = property.FindPropertyRelative("Value");
            var endPointIndexProperty = property.FindPropertyRelative("Index");

            // Begin property
            EditorGUI.BeginProperty(position, label, property);

            // Draw label and get position
            Rect labelPosition = position;
            position = EditorGUI.PrefixLabel(labelPosition, GUIUtility.GetControlID(FocusType.Passive), label);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate the width and offset for the area and value properties
            float width = position.width / 2f;
            float gap = 3f;

            // Draw endPoint and area properties side by side
            Rect areaRect = new Rect(position.x, position.y, width, position.height);
            Rect endPointRect = new Rect(position.x + width + gap, position.y, width, position.height);

            // Draw a dropdown for all the endPoints in the area
            int chosenEndPointIndexValue = endPointIndexProperty.intValue;
            AreaHandle area = areaProperty.objectReferenceValue as AreaHandle;
            if (area != null)
            {
                string[] endPointNames = new string[area.connections.Count];
                if (area.HasConnections()) endPointNames = area.GetAllConnectionNames().ToArray();
                else endPointNames = new string[] { "None" };

                chosenEndPointIndexValue = EditorGUI.Popup(endPointRect, chosenEndPointIndexValue, endPointNames);
                endPointProperty.stringValue = endPointNames[chosenEndPointIndexValue];
                endPointIndexProperty.intValue = chosenEndPointIndexValue;
            }
            else
            {
                EditorGUI.Popup(endPointRect, 0, new string[] { "None" });
            }

            // Draw area property
            EditorGUI.PropertyField(areaRect, areaProperty, GUIContent.none);

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

        private string ScenePath()
        {
            return EditorSceneManager.GetActiveScene().path;
        }
    }
}
