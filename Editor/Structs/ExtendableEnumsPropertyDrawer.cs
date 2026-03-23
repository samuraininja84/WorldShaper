using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace WorldShaper.Editor
{
    /// <summary>
    /// Provides a custom property drawer for fields of type <see cref="ExtendableEnum"/> in the Unity Editor.
    /// </summary>
    /// <remarks>
    /// This property drawer renders a field in the Unity Inspector that allows users to select or define values for an <see cref="ExtendableEnum"/>. 
    /// </remarks>
    [CustomPropertyDrawer(typeof(ExtendableEnum))]
    public class ExtendableEnumPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => ExtendableEnumUtility.GetHeight(property);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) => ExtendableEnumUtility.DrawExtendableEnum(position, property, label, fieldInfo);
    }

    /// <summary>
    /// Provides utility methods for working with extendable enums in the Unity Editor.
    /// </summary>
    /// <remarks>
    /// This static class includes methods for rendering custom editor fields, managing selection controls, and performing operations on extendable enums.
    /// </remarks>
    public static class ExtendableEnumUtility
    {
        /// <summary>
        /// Draws a custom editor field for an extendable enum property in the Unity Inspector.
        /// </summary>
        /// <remarks>This method provides a custom UI for editing properties of type <c>ExtendableEnum</c>
        /// in the Unity Editor. It supports displaying a dropdown list of available enum values, along with additional
        /// controls for managing selection and locking behavior. The method handles cases where the enum list is empty
        /// or locked, ensuring appropriate UI behavior.</remarks>
        /// <param name="position">The position on the screen where the field should be drawn.</param>
        /// <param name="property">The serialized property representing the extendable enum.</param>
        /// <param name="label">The label to display next to the field in the Inspector.</param>
        public static void DrawExtendableEnum(Rect position, SerializedProperty property, GUIContent label, FieldInfo fieldInfo)
        {
            // Begin change check
            EditorGUI.BeginChangeCheck();

            // Get the list of extendable enums from the property
            var atb = new ListToPopUpAttribute(typeof(ExtendableEnum), "list");

            // Get the properties for value, list, showLabel, and showSelection
            var value = property.FindPropertyRelative("value");
            var strings = property.FindPropertyRelative("list");
            var selectionValue = property.FindPropertyRelative("selectionValue");
            var showSelection = property.FindPropertyRelative("showSelection");
            var locked = property.FindPropertyRelative("locked");

            // Begin property
            EditorGUI.BeginProperty(position, label, property);

            // If the locked property is true, disable the selection
            if (locked.boolValue) showSelection.boolValue = false;

            // Width for the buttons
            float buttonWidth = 25;

            // Get the label from the property
            string propertyLabel = property.displayName;

            // Initialize a string list to hold the values
            List<string> stringList = new List<string>();

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

            // Create a rect for the popup minus a button width
            Rect enumRect = new Rect(position.x, position.y, position.width - buttonWidth, EditorGUIUtility.singleLineHeight);

            // If the list is not empty, create a popup with the list
            if (stringList != null && stringList.Count > 0)
            {
                int selectedIndex = Mathf.Max(stringList.IndexOf(value.stringValue), 0);
                selectedIndex = EditorGUI.Popup(enumRect, propertyLabel, selectedIndex, stringList.ToArray());
                string currentValue = stringList[selectedIndex];
                value.stringValue = currentValue;
            }
            else
            {
                EditorGUI.PropertyField(enumRect, property, label);
            }

            // If the list is empty, enable the popup gui after the popup is drawn
            if (stringList.Count <= 1 && stringList[0] == "None") GUI.enabled = true;

            // Disable the GUI for the toggle button for Worldshaper, for the sake of clarity
            if (locked.boolValue) GUI.enabled = false;

            // Draw the selection toggle button
            DrawSelectionToggle(property, enumRect, buttonWidth);

            // Re-enable the GUI after the toggle button
            if (locked.boolValue) GUI.enabled = true;

            // Draw the selection controls if showingSelection is true
            DrawSelectionControls(property, fieldInfo, position, buttonWidth);

            // Check if changes were made
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            // End property
            EditorGUI.EndProperty();
        }

        /// <summary>
        /// Draws a toggle button that allows the user to show or hide a selection field in the editor.
        /// </summary>
        /// <remarks>The toggle button is displayed next to the provided <paramref name="enumRect"/> and
        /// updates both the  <paramref name="showSelection"/> property and an internal state when clicked. The button's
        /// icon changes  to reflect the current visibility state.</remarks>
        /// <param name="showSelection">A <see cref="SerializedProperty"/> representing the current state of the selection visibility.  This value
        /// is updated when the toggle button is clicked.</param>
        /// <param name="enumRect">The <see cref="Rect"/> defining the position and size of the associated enum field in the editor layout.</param>
        /// <param name="buttonWidth">The width of the toggle button, in pixels.</param>
        private static void DrawSelectionToggle(SerializedProperty property, Rect enumRect, float buttonWidth)
        {
            // Get the properties for showSelection and locked
            var showSelection = property.FindPropertyRelative("showSelection");
            var locked = property.FindPropertyRelative("locked");

            // Gui Content Image for the toggle button
            GUIContent toggleOn = EditorGUIUtility.IconContent("animationvisibilitytoggleon");
            GUIContent toggleOff = EditorGUIUtility.IconContent("animationvisibilitytoggleoff");
            GUIContent toggleContent = showSelection.boolValue ? toggleOff : toggleOn;

            // Create a GUIContent for the status icon based on the locked state
            GUIContent statusContent = locked.boolValue ? EditorGUIUtility.IconContent("LockIcon-On") : toggleContent;

            // Add a button to toggle the selection field
            float enumX = enumRect.x + enumRect.width + EditorGUIUtility.standardVerticalSpacing;
            Rect toggleButtonRect = new Rect(enumX, enumRect.y, buttonWidth, enumRect.height);
            if (GUI.Button(toggleButtonRect, statusContent))
            {
                showSelection.boolValue = !showSelection.boolValue;
            }
        }

        /// <summary>
        /// Draws the selection controls for managing an extendable enum, including a text field and associated action buttons.
        /// </summary>
        /// <remarks>This method displays a text field for the selected value and a set of buttons for
        /// adding, removing, renaming,  and performing additional actions on the extendable enum. The buttons are
        /// displayed only when the selection controls  are visible, as determined by the <c>showingSelection</c>
        /// flag.</remarks>
        /// <param name="position">The position and size of the control to be drawn.</param>
        /// <param name="property">The serialized property representing the extendable enum.</param>
        /// <param name="current">The current instance of the extendable enum being modified.</param>
        /// <param name="selectionValue">The serialized property representing the selected value as a string.</param>
        /// <param name="buttonWidth">The width of each button in the selection controls.</param>
        private static void DrawSelectionControls(SerializedProperty property, FieldInfo fieldInfo, Rect position, float buttonWidth)
        {
            // If showing selection, draw the selection field as a text field with an add and remove button next to it
            var currentValue = property.FindPropertyRelative("value");
            var showSelection = property.FindPropertyRelative("showSelection");
            var selectionValue = property.FindPropertyRelative("selectionValue");

            // If showing selection, draw the selection field as a text field with an add and remove button next to it
            if (!showSelection.boolValue) return;

            // Get the current ExtendableEnum from the property
            ExtendableEnum current = (ExtendableEnum)fieldInfo.GetValue(property.serializedObject.targetObject);

            // Button count
            int buttonCount = 4;

            // Calculate the width of the selection field minus the button widths
            float selectionFieldWidth = position.width - (buttonCount * buttonWidth) - EditorGUIUtility.standardVerticalSpacing * (buttonCount - 1);

            // Draw the selection field below the popup
            Rect selectionRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight, selectionFieldWidth, EditorGUIUtility.singleLineHeight);

            // Draw the selection field as a text field with selectionValue
            GUIContent selectionLabel = new GUIContent("Selected Value", "Allows you to add to and remove from, sort, and clear the enum's list");
            selectionValue.stringValue = EditorGUI.TextField(selectionRect, selectionLabel, selectionValue.stringValue);

            // Get the rect for the buttons
            Rect addButtonRect = new Rect(selectionRect.x + selectionRect.width + EditorGUIUtility.standardVerticalSpacing, selectionRect.y, buttonWidth, selectionRect.height);
            Rect removeButtonRect = new Rect(addButtonRect.x + buttonWidth + EditorGUIUtility.standardVerticalSpacing, selectionRect.y, buttonWidth, selectionRect.height);
            Rect renameButtonRect = new Rect(removeButtonRect.x + buttonWidth + EditorGUIUtility.standardVerticalSpacing, selectionRect.y, buttonWidth, selectionRect.height);
            Rect optionsButtonRect = new Rect(renameButtonRect.x + buttonWidth + EditorGUIUtility.standardVerticalSpacing, selectionRect.y, buttonWidth, selectionRect.height);

            // Create GUIContent for the add and remove buttons
            GUIContent plusContent = EditorGUIUtility.IconContent("d_Toolbar Plus");
            GUIContent minusContent = EditorGUIUtility.IconContent("d_Toolbar Minus");

            // Create GUIContent for the options buttons
            GUIContent optionsContent = EditorGUIUtility.IconContent("d_MoreOptions");

            // Create GUIContent for the rename, sort, and clear buttons
            GUIContent renameContent = EditorGUIUtility.IconContent("d_CustomTool");
            //GUIContent sortContent = EditorGUIUtility.IconContent("AlphabeticalSorting@2x");
            //GUIContent clearContent = EditorGUIUtility.IconContent("d_winbtn_win_close");

            // Draw the add button
            if (GUI.Button(addButtonRect, plusContent))
            {
                current.Add(selectionValue.stringValue);
            }

            // Draw the remove button 
            if (GUI.Button(removeButtonRect, minusContent))
            {
                SmartRemove(current, selectionValue.stringValue);
            }

            // Draw the rename button
            if (GUI.Button(renameButtonRect, renameContent))
            {
                current.Rename(currentValue.stringValue, selectionValue.stringValue);
            }

            // Draw the options button
            if (GUI.Button(optionsButtonRect, optionsContent))
            {
                ShowContextMenu(current, property, currentValue.stringValue);
            }
        }

        /// <summary>
        /// Removes an item from the specified <see cref="ExtendableEnum"/> instance based on the provided target value.
        /// </summary>
        /// <remarks>This method performs a smart removal operation based on the nature of the <paramref
        /// name="targetValue"/>: <list type="bullet"> <item>If <paramref name="targetValue"/> is a numeric string, it
        /// attempts to remove an item matching the numeric value. If no match is found, it removes the item at the
        /// numeric index, clamped to the valid range.</item> <item>If <paramref name="targetValue"/> is not numeric, it
        /// removes the item matching the string value directly.</item> </list></remarks>
        /// <param name="current">The <see cref="ExtendableEnum"/> instance from which the item will be removed.</param>
        /// <param name="targetValue">The value used to determine which item to remove. If the value represents a numeric index, the item at that
        /// index  will be removed (or the closest valid index if out of range). If the value matches an item
        /// numerically, the matching  item will be removed. Otherwise, the item matching the string value will be
        /// removed.</param>
        private static void SmartRemove(ExtendableEnum current, string targetValue)
        {
            // Check if the targetValue is a number and if so, remove the string at that index, otherwise remove the string directly
            if (int.TryParse(targetValue, out int numberValue))
            {
                // Check if there is a string in the list that matches the number value otherwise remove the string at the index
                string matchingString = current.list.Find(s => int.TryParse(s, out int parsedValue) && parsedValue == numberValue);
                if (matchingString != null)
                {
                    // If a matching string is found, remove it from the list
                    current.Remove(matchingString);
                }
                else
                {
                    // If no matching string, remove the string at the index
                    int indexToRemove = Mathf.Clamp(numberValue, 0, current.list.Count - 1);
                    if (indexToRemove < current.list.Count)
                    {
                        current.RemoveAt(indexToRemove);
                    }
                }
            }
            else
            {
                // If the targetValue is not a number, remove the string directly
                current.Remove(targetValue);
            }
        }

        /// <summary>
        /// Displays a context menu with options for managing the specified extendable enum.
        /// </summary>
        /// <remarks>The context menu includes options to sort or clear the list represented by the
        /// <paramref name="current"/> extendable enum. This method is typically used in editor extensions to provide
        /// user interaction for managing serialized data.</remarks>
        /// <param name="property">The serialized property associated with the extendable enum. This parameter is used for context but is not
        /// directly modified by the method.</param>
        /// <param name="current">The current instance of the extendable enum to be managed. Provides the functionality for sorting and
        /// clearing the list.</param>
        private static void ShowContextMenu(ExtendableEnum current, SerializedProperty property, string currentValue) 
        {
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Move To Start"), false, () => current.MoveToStart(currentValue));
            menu.AddItem(new GUIContent("Move To End"), false, () => current.MoveToEnd(currentValue));
            menu.AddItem(new GUIContent("Move Up"), false, () => current.MoveUp(currentValue));
            menu.AddItem(new GUIContent("Move Down"), false, () => current.MoveDown(currentValue));
            menu.AddItem(new GUIContent("Sort List"), false, () => current.Sort());
            menu.AddItem(new GUIContent("Clear List"), false, () => current.Clear());
            menu.ShowAsContext();
        }

        /// <summary>
        /// Calculates the height required to render the specified serialized property in the editor.
        /// </summary>
        /// <param name="property">The serialized property for which the height is being calculated.</param>
        /// <param name="showingSelection">A value indicating whether the property is currently showing a selection.  If <see langword="true"/>,
        /// additional height is allocated to accommodate the selection.</param>
        /// <returns>The height, in pixels, required to render the property.  This is equal to one or two times the standard
        /// single line height, depending on the value of <paramref name="showingSelection"/>.</returns>
        public static float GetHeight(SerializedProperty property)
        {
            bool showSelection = property.FindPropertyRelative("showSelection").boolValue;
            return EditorGUIUtility.singleLineHeight * (showSelection ? 2 : 1);
        }
    }
}
