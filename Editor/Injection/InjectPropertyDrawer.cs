using UnityEditor;
using UnityEngine;
using WorldShaper.Editor;

namespace WorldShaper.Injection.Editor
{
    [CustomPropertyDrawer(typeof(InjectAttribute))]
    public class InjectPropertyDrawer : PropertyDrawer 
    {
        Texture2D icon;

        private Texture2D LoadIcon() 
        {
            // If the icon is null, load it from the specified path
            if (icon == null) icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPathExtensions.ToImagePath("inject"));

            // Return the loaded icon
            return icon;
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
        {
            // Load the icon for the inject attribute
            icon = LoadIcon();

            // Define the rectangle for the icon, positioned to the left of the property field
            var iconRect = new Rect(position.x, position.y, 20, 20);

            // Adjust the position of the property field to make space for the icon
            position.xMin += 24;

            // Draw the icon if it is not null
            if (icon != null) 
            {
                // Save the current GUI color
                var savedColor = GUI.color;

                // Change the GUI color to green if the property is not null, otherwise keep it as the saved color
                GUI.color = property.objectReferenceValue == null ? savedColor : Color.green;

                // Draw the icon in the inspector
                GUI.DrawTexture(iconRect, icon);

                // Reset the GUI color to its original value
                GUI.color = savedColor;
            }

            // Draw the property field with the label
            EditorGUI.PropertyField(position, property, label);
        }
    }
}