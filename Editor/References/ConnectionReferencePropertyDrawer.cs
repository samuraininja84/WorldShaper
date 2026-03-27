using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Eflatun.SceneReference;

namespace WorldShaper.Editor
{
    [CustomPropertyDrawer(typeof(ConnectionReference))]
    public class ConnectionReferencePropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Begin change check
            EditorGUI.BeginChangeCheck();

            // Get value properties
            var areaProperty = property.FindPropertyRelative(nameof(ConnectionReference.Area));
            var connectionIdProperty = property.FindPropertyRelative(nameof(ConnectionReference.ID));
            var connectionNameProperty = property.FindPropertyRelative(nameof(ConnectionReference.Value));
            var connectionIndexProperty = property.FindPropertyRelative(nameof(ConnectionReference.Index));

            // Begin property
            EditorGUI.BeginProperty(position, label, property);

            // Draw label and get position
            Rect labelPosition = position;
            position = EditorGUI.PrefixLabel(labelPosition, GUIUtility.GetControlID(FocusType.Passive), label);

            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            // Calculate the width and offset for the area and value properties
            float buttonWidth = 20;
            float width = position.width - (buttonWidth * 3);

            // Draw search, connection, and load button rects
            Rect searchButtonRect = new Rect(position.x, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);
            Rect connectionRect = new Rect(position.x + buttonWidth, position.y, width, EditorGUIUtility.singleLineHeight);
            Rect loadAreaRect = new Rect(position.x + buttonWidth + width, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);
            Rect loadDestinationRect = new Rect(position.x + buttonWidth + width + buttonWidth, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);

            // Get the button style and icon for the search button
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton);
            buttonStyle.padding = new RectOffset(2, 2, 1, 1);
            buttonStyle.fixedHeight = EditorGUIUtility.singleLineHeight;
            buttonStyle.fixedWidth = buttonWidth;

            // Create the search button content
            GUIContent searchContent = EditorGUIUtility.IconContent("Animation.FilterBySelection");
            searchContent.tooltip = "Get the first AreaHandle that contains this scene";

            // Draw the search button
            if (GUI.Button(searchButtonRect, searchContent, buttonStyle))
            {
                // Find the matching area handle and set it
                AreaHandle foundArea = FindMatchingAreaHandle();
                if (foundArea != null) areaProperty.objectReferenceValue = foundArea;
            }

            // Validate the GUID
            ValidateGUID(areaProperty, connectionIdProperty, connectionNameProperty);

            // Draw the connection selector
            DrawConnectionSelector(connectionRect, areaProperty, connectionIdProperty, connectionNameProperty, connectionIndexProperty);

            // Get the area handle from the area property
            AreaHandle handle = areaProperty.objectReferenceValue as AreaHandle;

            // Check if handle is null, disable button if so
            if (handle == null) GUI.enabled = false;

            // Create the load area button content
            GUIContent loadAreaContent = new GUIContent(EditorGUIUtility.FindTexture(IconPathExtensions.ToImagePath("LoadArea")), "Load the area for this connection");

            // Create the load destination button content
            GUIContent loadDestinationContent = new GUIContent(EditorGUIUtility.FindTexture(IconPathExtensions.ToImagePath("LoadDestination")), "Load the destination area for this connection");

            // Draw the load area button
            if (GUI.Button(loadAreaRect, loadAreaContent, buttonStyle)) LoadArea(handle, connectionNameProperty.stringValue);

            // Draw the load button
            if (GUI.Button(loadDestinationRect, loadDestinationContent, buttonStyle)) LoadDestination(handle, connectionNameProperty.stringValue);

            // Re-enable GUI if it was disabled
            GUI.enabled = true;

            // Reset indent level
            EditorGUI.indentLevel = indent;

            // Check if changes were made
            if (EditorGUI.EndChangeCheck())
            {
                // Apply modified properties
                property.serializedObject.ApplyModifiedProperties();
            }

            // End property
            EditorGUI.EndProperty();
        }

        private static void ValidateGUID(SerializedProperty areaProperty, SerializedProperty idProperty, SerializedProperty nameProperty)
        {
            // Check if the guid is valid (not null and not empty)
            bool validGuid = ValidGUID(idProperty.boxedValue as SerializableGuid?, out SerializableGuid value);

            // If the connection name is null, try to get the connection name using the connection ID, index, or name properties in that order of priority
            if (!string.IsNullOrEmpty(nameProperty.stringValue) && areaProperty.objectReferenceValue != null)
            {
                // Try to get the connection ID from the area using the connection name
                AreaHandle area = areaProperty.objectReferenceValue as AreaHandle;
                Connection connection = area.GetConnection(nameProperty.stringValue);

                // If a connection is found with the specified name, set the connection ID from the area using the connection name
                if (connection != null && (!validGuid || value != connection.connectionId))
                {
                    // If a connection is found with the specified name, set the connection ID from the area using the connection name
                    if (connection != null) idProperty.boxedValue = connection.connectionId;

                    // Apply modified properties
                    idProperty.serializedObject.ApplyModifiedProperties();
                }
            }
        }

        private static void DrawConnectionSelector(Rect position, SerializedProperty areaProperty, SerializedProperty idProperty, SerializedProperty nameProperty, SerializedProperty indexProperty)
        {
            // Get the label
            var label = new GUIContent();

            // Get the current connection reference value
            var area = areaProperty.objectReferenceValue as AreaHandle;

            // If the area is null or the area has no connections, set the label to None
            if (area == null || !area.HasConnections())
            {
                // Set the label to "None" if there are no connections in the area
                label.text = "None";
            }
            else
            {
                // Check if the guid is valid (not null and not empty)
                bool validGuid = ValidGUID(idProperty.boxedValue as SerializableGuid?, out SerializableGuid value);

                // Get the connection name at the current index to display in the label
                var connectionName = validGuid ? WorldMap.Instance.GetConnection(value)?.Name : null;

                // If the connection name is null, try to get the connection name using the connection ID or name properties
                if (string.IsNullOrEmpty(connectionName))
                {
                    // Try to get the connection name using the connection ID, index, or name properties in that order of priority
                    if (indexProperty.intValue != -1) connectionName = area.GetConnection(indexProperty.intValue)?.Name;
                    else if (nameProperty.stringValue != null) connectionName = nameProperty.stringValue;
                }

                // Set the label to the area name and the connection name at the current index
                label.text = FormatForLabel(area.Name, connectionName ?? "None");

                // Get the size for the label from the mini pull down style
                var size = EditorStyles.miniPullDown.CalcSize(label);

                // If the position width is less than the label size, set the tooltip to the full label text and shorten the label text to just the connection name for better display
                if (position.width < size.x)
                {
                    label.tooltip = label.text;
                    label.text = connectionName;
                }
            }

            // Draw the dropdown for the connection selection
            if (EditorGUI.DropdownButton(position, label, FocusType.Passive))
            {
                PopupWindow.Show
                (
                    position,
                    new DatabaseTreePopup(new DatabaseTreeView(null, selection => 
                    {
                        // Get the area from the selection and set the area property
                        area = WorldMap.Instance.GetArea(selection);
                        areaProperty.objectReferenceValue = area;

                        // Get the connection from the area using the selection and set the connection ID
                        idProperty.boxedValue = selection.connectionId;

                        // Get the connection name and index from the area using the selection
                        var (name, index) = area.GetConnectionInfo(selection.connectionId);

                        // Set the connection name from the area using the selection and set the connection name
                        nameProperty.stringValue = name;

                        // Get the connection index from the area using the selection and set the connection index
                        indexProperty.intValue = index;

                        // Apply modified properties
                        idProperty.serializedObject.ApplyModifiedProperties();
                    }))
                    {
                        Width = Mathf.Max(position.width, 300)
                    }
                );
            }
        }

        private async void LoadArea(AreaHandle handle, string connectionName)
        {
            // Get the handle name for logging purposes, replacing any underscores with spaces
            string handleName = handle.Name.Replace("_", " ");

            // If the handle is invalid, log an error to the console and return early
            if (!handle.IsValid)
            {
                // Log an error explainning why we can't load the scene
                Debug.LogError($"The {handleName} Area Handle is Invalid, please make sure that the scene is properly set up in the {handleName} Area Handle asset, returning early.");

                // Return early, since we can't load the invalid scene
                return;
            }

            // Get the connection using the selected endPoint name
            Connection connection = handle.GetConnection(connectionName);

            // Check if play mode is active
            if (!Application.isPlaying)
            {
                // Check if the requested scene is already loaded
                bool isLoaded = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(handle.activeScene.Path).isLoaded;

                // If not, open the scene first using the Editor SceneManager
                if (!isLoaded)
                {
                    // Ask the user if they want to save the current scene if there are unsaved changes before opening the new scene
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(handle.activeScene.Path);
                }

                // Find the location with the connection name
                ILocationPointer[] pointer = ILocationPointerExtensions.GetAllLocations().ToArray();
                ILocationPointer target = pointer.FirstOrDefault(c => c.GetEndpoint() == connectionName);

                // Move the camera to the target pointer if found
                if (target != null)
                {
                    // Set the camera position to the target location's position
                    SceneView.lastActiveSceneView.pivot = target.GetPosition();
                    SceneView.lastActiveSceneView.Repaint();
                }
                else
                {
                    // Log a warning if no location is found with the specified connection name
                    Debug.LogWarning($"No connectable found with name '{connectionName}' in the loaded area.");
                }
            }
            else
            {
                // Load the area using the connection
                connection.LoadArea();
            }
        }

        private void LoadDestination(AreaHandle handle, string connectionName)
        {
            // Get the connection using the selected endPoint name
            Connection connection = handle.GetConnection(connectionName);

            // Log a error and return if no connection is found
            if (connection == null)
            {
                // Log an error if no connection is found with the specified name in the area handle
                Debug.LogError($"No connection found with name '{connectionName}' in area '{handle.name}'.");

                // Return if no connection is found
                return;
            }

            // Check if play mode is active
            if (!Application.isPlaying)
            {
                // Check if the requested scene is already loaded
                bool isLoaded = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(connection.Destination.Path).isLoaded;

                // If not, open the scene first using the Editor SceneManager
                if (!isLoaded)
                {
                    // Ask the user if they want to save the current scene if there are unsaved changes before opening the new scene
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene(connection.Destination.Path);
                }

                // Find the location with the connection name
                ILocationPointer[] pointer = ILocationPointerExtensions.GetAllLocations().ToArray();
                ILocationPointer target = pointer.FirstOrDefault(c => c.GetEndpoint() == connection.Endpoint);

                // Move the camera to the target pointer if found
                if (target != null)
                {
                    // Set the camera position to the target pointer's position
                    SceneView.lastActiveSceneView.pivot = target.GetPosition();
                    SceneView.lastActiveSceneView.Repaint();
                }
                else
                {
                    // Log a warning if no pointer is found with the specified connection name
                    Debug.LogWarning($"No connectable found with name '{connection.Endpoint}' in {connection.Destination.Name}.");
                }
            }
            else
            {
                // Load the destination using the connection
                connection.LoadDestination();
            }
        }

        private List<AreaHandle> FilteredAreas()
        {
            // Get all area handles
            List<AreaHandle> areas = WorldMap.Instance.registeredAreas;

            // Make sure to remove areas that have an invalid scene reference
            areas.RemoveAll(handle => InvalidScene(handle));

            // Return the filtered list of area handles
            return areas;
        }

        private AreaHandle FindMatchingAreaHandle()
        {
            // Create a new area handle
            AreaHandle area = null;

            // Get the currently loaded scene in the editor
            string sceneName = EditorSceneManager.GetActiveScene().name;

            // Find the area handle that matches the current scene name from the filtered areas
            area = FilteredAreas().FirstOrDefault(handle => handle.activeScene != null && handle.activeScene.Name == sceneName);

            // If no area handle is found, log a warning
            if (area == null) Debug.LogWarning($"No AreaHandle found for the current scene: {sceneName}. Please ensure that handle matching area handle exists in the project.");

            // Return the area handle
            return area;
        }

        private bool InvalidScene(AreaHandle handle)
        {
            // Check if the current scene is null or has an unsafe state
            bool invalidScene = handle.activeScene == null || handle.activeScene.State == SceneReferenceState.Unsafe;

            // If the scene is invalid, log a warning
            if (invalidScene) Debug.LogWarning($"AreaHandle: {handle.name} has an invalid scene. Removing from list.");

            // Return whether the scene is invalid
            return invalidScene;
        }

        private static bool ValidGUID(SerializableGuid? guid, out SerializableGuid validGuid)
        {
            // Check if the guid is valid (not null and not empty)
            bool isValid = guid.HasValue && guid.Value != SerializableGuid.Empty;

            // If the guid is not valid, set the valid guid to empty
            validGuid = isValid ? guid.Value : SerializableGuid.Empty;

            // Return whether the guid is valid
            return isValid;
        }

        private static string FormatForLabel(string area, string connection)
        {
            // If the name is null or empty, return "None"
            if (string.IsNullOrEmpty(area) || string.IsNullOrEmpty(connection) || connection == "None") return "None";

            // Replace underscores with spaces for better readability
            return $"{area}/{connection}".Replace("_", " ").Replace("-", " ").Trim();
        }
    }
}