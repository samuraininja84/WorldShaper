using UnityEngine;
using UnityEditor;

namespace WorldShaper.Editor
{
	[CustomPropertyDrawer(typeof(ProgressBarAttribute))]
	public class ProgressBarAttributeDrawer : PropertyDrawer
	{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin change check
            EditorGUI.BeginChangeCheck();

            // Get the ProgressBarAttribute
            ProgressBarAttribute progressBarAttribute = (ProgressBarAttribute)attribute;

            // Get the property type
            bool floatValue = property.propertyType == SerializedPropertyType.Float;

            // Draw the label if it is not null
            if (label != null) position = EditorGUI.PrefixLabel(position, label);

            // Make sure the property is a float or an int
            if (floatValue)
			{
                // Draw an invisible slider to allow for value changes and text input
                GUI.color = Color.clear;
                GUI.backgroundColor = Color.clear;

                // Draw the slider
                float sliderValue = GUI.HorizontalSlider(position, property.floatValue, progressBarAttribute.min, progressBarAttribute.max);

                // Reset the position and color
                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;

                // If the value has changed, update the property
                property.floatValue = sliderValue;

                // Get the value from the property
                float value = property.floatValue;

                // Clamp the value between min and max
                value = Mathf.Clamp(value, progressBarAttribute.min, progressBarAttribute.max);

                // Normalize the value between 0 and 1
                value = Mathf.InverseLerp(progressBarAttribute.min, progressBarAttribute.max, value);

                // Get the color from the attribute if it is not null
                GUI.color = progressBarAttribute.GetProgressColor(value, progressBarAttribute.max);
                GUI.backgroundColor = progressBarAttribute.Background;

                // Draw the progress bar
                EditorGUI.ProgressBar(position, value, string.Format("{0}: {1:0.00}%", progressBarAttribute.Name, value * 100f));
            }
            else
			{
				EditorGUI.HelpBox(position, "ProgressBarAttribute can only be applied to a Float or an Int!", MessageType.Warning);
			}

            // Reset the color to default
            GUI.color = Color.white;
            GUI.backgroundColor = Color.white;

            // End change check
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
		}
	}
}