using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Eflatun.SceneReference;

namespace WorldShaper
{
    [CustomPropertyDrawer(typeof(ConnectionReference))]
    public class ConnectionReferencePropertyDrawer : PropertyDrawer
    {
        private float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) => height;

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
            float spacing = 3f;
            float buttonWidth = 20;
            float width = ((position.width - (buttonWidth * 2)) / 2) - (spacing * 2);

            // Draw area, endPoint, and search button rects
            Rect areaRect = new Rect(position.x, position.y, width, EditorGUIUtility.singleLineHeight);
            Rect endPointRect = new Rect(position.x + width + spacing, position.y, width, EditorGUIUtility.singleLineHeight);
            Rect searchButtonRect = new Rect(position.x + (width * 2) + (spacing * 2), position.y, buttonWidth, EditorGUIUtility.singleLineHeight);
            Rect loadButtonRect = new Rect(position.x + (width * 2) + (spacing * 3) + buttonWidth, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);

            // Draw area property
            EditorGUI.PropertyField(areaRect, areaProperty, GUIContent.none);

            // Draw a dropdown for all the endPoints in the area
            int chosenEndPointIndexValue = endPointIndexProperty.intValue;
            AreaHandle area = areaProperty.objectReferenceValue as AreaHandle;
            if (area != null)
            {
                // Initialize the endPoint names with the area connections count as the array size
                string[] endPointNames = new string[area.connections.Count];

                // Check if the area has connections, otherwise set to "None"
                if (area.HasConnections()) endPointNames = area.GetAllConnectionNames().ToArray();
                else endPointNames = new string[] { "None" };

                // Draw the popup for endPoint selection
                chosenEndPointIndexValue = EditorGUI.Popup(endPointRect, chosenEndPointIndexValue, endPointNames);

                // Update the endPointProperty to the endPoint name at the chosen index
                endPointProperty.stringValue = endPointNames[chosenEndPointIndexValue];

                // Update the endPointIndexProperty to the chosen index
                endPointIndexProperty.intValue = chosenEndPointIndexValue;
            }
            else
            {
                // If no area is selected, set the endPoint to "None"
                EditorGUI.Popup(endPointRect, 0, new string[] { "None" });
            }

            // Get the button style and icon for the search button
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton);
            buttonStyle.padding = new RectOffset(1, 1, 0, 0);
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

            // Create the load button content
            GUIContent loadContent = new GUIContent(EditorGUIUtility.FindTexture("Assets/Plugins/World Shaper/Editor/EditorResources/Icons/LoadDestination.png"));
            loadContent.tooltip = "Load the destination area for this connection";

            // Get the area handle from the area property
            AreaHandle handle = areaProperty.objectReferenceValue as AreaHandle;

            // Check if handle is null, disable button if so
            if (handle == null) GUI.enabled = false;

            // Draw the load button
            if (GUI.Button(loadButtonRect, loadContent, buttonStyle))
            {
                // Get the connection using the selected endPoint name
                Connection connection = handle.GetConnection(endPointProperty.stringValue);

                // Load the destination area if the connection is found
                if (connection != null) LoadDestination(connection);

                // Log a warning if no connection is found
                else Debug.LogWarning($"No connection found with name '{endPointProperty.stringValue}' in area '{handle.name}'.");
            }

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

        private void LoadDestination(Connection connection)
        {
            // Check if play mode is active
            if (!Application.isPlaying)
            {
                // Check if the requested scene is already loaded
                bool isLoaded = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(connection.Destination.Path).isLoaded;

                // If not, open the scene first using the Editor SceneManager
                if (!isLoaded) EditorSceneManager.OpenScene(connection.Destination.Path);

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

        private AreaHandle FindMatchingAreaHandle()
        {
            // Create a new area handle
            AreaHandle area = null;

            // Get the currently loaded scene in the editor
            string sceneName = ActiveSceneName();

            // Find the area handle that matches the current scene name from the filtered areas
            area = FilteredAreas().FirstOrDefault(handle => MatchingScene(handle, sceneName));

            // If no area handle is found, log a warning
            if (area == null) Debug.LogWarning($"No AreaHandle found for the current scene: {sceneName}. Please ensure that handle matching area handle exists in the project.");

            // Return the area handle
            return area;
        }

        private List<AreaHandle> FilteredAreas()
        {
            // Get all area handles
            List<AreaHandle> areas = GetAllAreaHandles();

            // Make sure to remove areas that have an invalid scene reference
            areas.RemoveAll(handle => InvalidScene(handle));

            // Return the filtered list of area handles
            return areas;
        }

        private List<AreaHandle> GetAllAreaHandles()
        {
            // Get all area handles from the path
            return AssetDatabase.FindAssets("t:AreaHandle", null)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AreaHandle>)
                .ToList();
        }

        private string ActiveSceneName() => EditorSceneManager.GetActiveScene().name;

        private bool MatchingScene(AreaHandle handle, string name) => handle.activeScene != null && handle.activeScene.Name == name;

        private bool InvalidScene(AreaHandle handle)
        {
            // Check if the current scene is null or has an unsafe state
            bool invalidScene = handle.activeScene == null || handle.activeScene.State == SceneReferenceState.Unsafe;

            // If the scene is invalid, log a warning
            if (invalidScene) Debug.LogWarning($"AreaHandle: {handle.name} has an invalid scene. Removing from list.");

            // Return whether the scene is invalid
            return invalidScene;
        }
    }
}