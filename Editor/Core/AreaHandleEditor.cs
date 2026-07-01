using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace WorldShaper.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AreaHandle))]
    public class AreaHandleEditor : UnityEditor.Editor
    {
        private AreaHandle areaHandle;
        private SerializedProperty script;
        private SerializedProperty areaHandleTypeProperty;
        private SerializedProperty activeSceneProperty;
        private SerializedProperty additiveScenesProperty;
        private SerializedProperty connectionsProperty;
        private ReorderableList additiveScenesList;
        private ReorderableList connectionsList;
        private bool showConnections = true;

        private void OnEnable()
        {
            // Get the target AreaHandle object
            areaHandle = target as AreaHandle;

            // Validate the connections in the area handle to ensure they are consistent and valid
            areaHandle.ValidateConnections();

            // Find the serialized property for the script field
            script = serializedObject.FindProperty("m_Script");

            // Find the serialized properties for areaHandleType, activeScene, and additiveScenes
            areaHandleTypeProperty = serializedObject.FindProperty(nameof(AreaHandle.areaHandleType));
            activeSceneProperty = serializedObject.FindProperty(nameof(AreaHandle.activeScene));
            additiveScenesProperty = serializedObject.FindProperty(nameof(AreaHandle.additiveScenes));
            connectionsProperty = serializedObject.FindProperty(nameof(AreaHandle.connections));

            // Create a ReorderableList for the additiveScenes property
            CreateAdditiveScenesList();

            // Create a ReorderableList for the connections property
            CreateConnectionsList();
        }

        private void CreateAdditiveScenesList()
        {
            // Create a ReorderableList for the additiveScenes property
            additiveScenesList = new ReorderableList(serializedObject, additiveScenesProperty, true, true, true, true)
            {
                // Define how the header of the list should be drawn
                drawHeaderCallback = rect => EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Additive Scenes"),

                // Define how each element in the list should be drawn
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    // Get the element at the current index
                    var element = additiveScenesList.serializedProperty.GetArrayElementAtIndex(index);

                    // Adjust the rect for better spacing
                    rect.y += 2;

                    // Draw the properties
                    var entryRect = new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight);

                    // Draw the value type and key name fields
                    EditorGUI.PropertyField(entryRect, element, GUIContent.none);
                }
            };
        }

        private void CreateConnectionsList()
        {
            // Create a ReorderableList for the connections property
            connectionsList = new ReorderableList(serializedObject, connectionsProperty, true, true, true, true)
            {
                // Define how the header of the list should be drawn
                drawHeaderCallback = rect => EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), "Connections"),

                // Define what happens when the add button is clicked
                onAddCallback = (ReorderableList l) =>
                {
                    // Create a new Connection ScriptableObject
                    areaHandle.CreateConnection();

                    // Apply the modified properties to the serialized object
                    serializedObject.ApplyModifiedProperties();
                },

                // Define what happens when the remove button is clicked
                onRemoveCallback = (ReorderableList l) =>
                {
                    // Record the removal of the selected connection for undo functionality
                    Undo.RecordObject(target, "Removed Connection At Index " + l.index);

                    // Get the connection to delete based on the index of the removed element
                    var connectionToDelete = areaHandle.connections[l.index];

                    // Delete the connection ScriptableObject from the project
                    if (connectionToDelete != null) connectionToDelete.Delete();

                    // Remove the selected connection from the area handle's connections list
                    ReorderableList.defaultBehaviours.DoRemoveButton(l);

                    // Mark the serialized object as dirty to ensure changes are saved
                    serializedObject.SetIsDifferentCacheDirty();

                    // Apply the modified properties to the serialized object
                    serializedObject.ApplyModifiedProperties();
                },

                // Define what happens when the list is reordered
                onReorderCallback = (ReorderableList l) =>
                {
                    // Record the reordering of the connections list for undo functionality
                    Undo.RecordObject(target, "Reordered Connections");

                    // Mark the serialized object as dirty to ensure changes are saved
                    serializedObject.SetIsDifferentCacheDirty();

                    // Apply the modified properties to the serialized object
                    serializedObject.ApplyModifiedProperties();
                },

                // Dynamically calculate the height of each element based on its properties
                elementHeightCallback = (int index) =>
                {
                    // Get the connection at the current index
                    var connection = areaHandle.connections[index];

                    // Check if the connection is null, if so, skip to the next connection
                    if (connection == null) return EditorGUIUtility.singleLineHeight;

                    // Check if the connection is closed
                    bool closedConnection = connection.Closed();

                    // Check if the connection has a destination or an endpoint
                    bool hasDestination = connection.HasDestination();
                    bool hasEndpoint = connection.HasEndpoint();

                    // Determine if the connection has either a destination or an endpoint
                    bool hasEither = hasDestination || hasEndpoint;

                    // If the connection is closed and has no destination or endpoint, draw a smaller height
                    return EditorGUIUtility.singleLineHeight * (closedConnection ? hasEither ? 7 : 4 : 5) + 10;
                },

                // Define how each element in the list should be drawn
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    // Get the connection at the current index
                    var connection = areaHandle.connections[index];

                    // Check if the connection is null, if so, skip to the next connection
                    if (connection == null) return;

                    // Get the element at the current index
                    var element = new SerializedObject(connection);

                    // Adjust the rect for better spacing
                    rect.y += 2;

                    // Adjust this value for more or less spacing
                    float spacing = 2f;

                    // Adjust this value for the width of the buttons
                    float buttonWidth = 20f;

                    // Calculate the width for the property fields, leaving space for buttons
                    float width = rect.width - buttonWidth - spacing;

                    #region Connection Properties

                    // Get the properties of the Connection object
                    var name = element.FindProperty(nameof(Connection.connectionName));
                    var type = element.FindProperty(nameof(Connection.connectionType));
                    var destination = element.FindProperty(nameof(Connection.destination));
                    var transitionIn = element.FindProperty(nameof(Connection.transitionIn));
                    var transitionOut = element.FindProperty(nameof(Connection.transitionOut));

                    // Check if the connection is closed
                    bool connectionClosed = connection.Closed();

                    // Get the rects for the name, type, destination, transitionIn, and transitionOut properties, positioned vertically with spacing
                    var nameRect = new Rect(rect.x, rect.y, width, EditorGUIUtility.singleLineHeight);
                    var typeRect = nameRect.AddY(EditorGUIUtility.singleLineHeight + spacing);
                    var destinationRect = typeRect.AddY(EditorGUIUtility.singleLineHeight + spacing);
                    var transitionInRect = destinationRect.AddY(connectionClosed ? 0 : EditorGUIUtility.singleLineHeight + spacing);
                    var transitionOutRect = transitionInRect.AddY(EditorGUIUtility.singleLineHeight + spacing);

                    // Draw the properties
                    EditorGUI.PropertyField(nameRect, name);
                    EditorGUI.PropertyField(typeRect, type);
                    if (!connectionClosed) EditorGUI.PropertyField(destinationRect, destination);
                    EditorGUI.PropertyField(transitionInRect, transitionIn);
                    EditorGUI.PropertyField(transitionOutRect, transitionOut);

                    #endregion

                    #region Connection Action Buttons

                    // Get the mini button style for the connection action buttons
                    var miniButtonStyle = GetMiniButtonStyle();

                    // Create rects for the buttons, positioned to the right of the property fields
                    var buttonRect = new Rect(rect.x + width + spacing, rect.y, buttonWidth, EditorGUIUtility.singleLineHeight);

                    // Display buttons to set a two-way connection
                    var refreshContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("Sync"))) {
                        tooltip = "Set Two-Way Connection"
                    };

                    // Draw a button to set the connection to a two-way connection, which will sync the endpoint link
                    if (GUI.Button(buttonRect, refreshContent, miniButtonStyle))
                    {
                        // Sync the endpoint link to set the connection to a two-way connection
                        connection.SyncEndpointLink();

                        // Apply the modified properties to the serialized object
                        element.ApplyModifiedProperties();
                    }

                    // Move the buttonRect down for the next button
                    buttonRect.y += EditorGUIUtility.singleLineHeight + spacing;

                    // Get the GUIContent for the close button and set its tooltip
                    var closeSingleContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("OneWay"))) {
                        tooltip = "Set Endpoint to Closed"
                    };

                    // Draw a button to set the endpoint to closed, which will set the connection type to closed 
                    if (GUI.Button(buttonRect, closeSingleContent, miniButtonStyle))
                    {
                        // Get the endpoint connection for the current connection
                        var endpointConnection = connection.GetEndpoint();

                        // Set the connection type to closed, which will prevent the connection from being used
                        endpointConnection.SetConnectionType(ConnectionType.Closed);

                        // Apply the modified properties to the serialized object for the endpoint connection
                        var endpointElement = new SerializedObject(endpointConnection);

                        // Apply the modified properties to the serialized object
                        endpointElement.ApplyModifiedProperties();
                    }

                    // Move the buttonRect down for the next button
                    buttonRect.y += EditorGUIUtility.singleLineHeight + spacing;

                    // Get the GUIContent for the refresh button and set its tooltip
                    var refreshSingleContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("Refresh"))) {
                        tooltip = "Refresh Connection" };

                    // Draw a button to refresh the connection, which will update the destination and endpoint properties
                    if (GUI.Button(buttonRect, refreshSingleContent, miniButtonStyle))
                    {
                        // Refresh the connection to update the destination and endpoint properties
                        connection.Refresh();

                        // Apply the modified properties to the serialized object
                        element.ApplyModifiedProperties();
                    }

                    // If the connection is closed but still has a destination or endpoint, display a warning message
                    if (connectionClosed && (connection.HasDestination() || connection.HasEndpoint()))
                    {
                        // Create strings to determine if the connection has a destination or endpoint for the warning message
                        string hasDestination = connection.HasDestination() ? "has a destination" : string.Empty;
                        string hasEndpoint = connection.HasEndpoint() ? "has a endpoint" : string.Empty;
                        string and = (connection.HasDestination() && connection.HasEndpoint()) ? " and " : string.Empty;
                        string isPlural = (connection.HasDestination() && connection.HasEndpoint()) ? "them" : "it";

                        // Create a rect for the warning message, positioned below the property fields and buttons
                        var warningRect = new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight + spacing) * 4, rect.width, EditorGUIUtility.singleLineHeight * 2);

                        // Display a warning message if the connection is closed
                        EditorGUI.HelpBox(warningRect, $"This connection is closed but {hasDestination}{and}{hasEndpoint}.\nPlease clear {isPlural} to avoid issues with unwanted loading.", MessageType.Warning);

                        // Create a rect for the clear button, positioned below the warning message
                        var clearButtonRect = new Rect(rect.x, warningRect.y + warningRect.height + spacing, rect.width, EditorGUIUtility.singleLineHeight);

                        // Display a button to clear the destination
                        if (GUI.Button(clearButtonRect, "Clear Destination"))
                        {
                            // Clear the destination in the connection
                            connection.destination = ConnectionReference.None;

                            // Refresh the connection to update the destination and endpoint properties
                            connection.Refresh();

                            // Apply the modified properties to the serialized object
                            element.ApplyModifiedProperties();
                        }
                    }

                    #endregion

                    // Apply the modified properties to the serialized object
                    element.ApplyModifiedProperties();
                }
            };
        }

        public override void OnInspectorGUI()
        {
            // If the additiveScenesList or connectionsList is null, recreate them
            if (additiveScenesList == null) CreateAdditiveScenesList();
            if (connectionsList == null) CreateConnectionsList();

            // Start the change check
            EditorGUI.BeginChangeCheck();

            // Update the serialized object
            serializedObject.Update();

            // Display the script field for the AreaHandle component and disable the script field
            GUI.enabled = false;
            EditorGUILayout.PropertyField(script);
            GUI.enabled = true;

            // Remove a gap between the script field and the current scene field
            EditorGUILayout.Space(-10);

            // Display the active scene field
            EditorGUILayout.PropertyField(activeSceneProperty);

            // Display the area handle type field
            EditorGUILayout.PropertyField(areaHandleTypeProperty);

            // Add a space between the persistent scene field and the additive scenes field
            EditorGUILayout.Separator();

            // If it is a persistent scene do not allow additive scenes or connections
            if (areaHandle.Persistent() && (areaHandle.HasAdditiveScenes() || areaHandle.HasConnections()))
            {
                // Add a label for the buttons section
                EditorGUILayout.LabelField("Clean Up Actions", EditorStyles.boldLabel);

                // Draw a button to clear all additive scenes if there are any
                if (areaHandle.HasAdditiveScenes())
                {
                    // Draw a button to clear all additive scenes
                    if (GUILayout.Button("Clear All Additive Scenes"))
                    {
                        // Clear all additive scenes in the area handle
                        areaHandle.additiveScenes.Clear();

                        // Apply the modified properties to the serialized object
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                // Draw a button to clear all connections if there are any
                if (areaHandle.HasConnections())
                {
                    // Draw a button to clear all connections
                    if (GUILayout.Button("Clear All Connections"))
                    {
                        // Clear all connections in the area handle
                        areaHandle.ClearConnections();

                        // Apply the modified properties to the serialized object
                        serializedObject.ApplyModifiedProperties();
                    }
                }

                // Add a space between the info box and the buttons
                EditorGUILayout.Separator();

                // Draw a warning message if there are any additive scenes or connections
                DrawPersistentSceneWarning();
            }
            else if (areaHandle.Impassable())
            {
                // Display the additive scenes field
                additiveScenesList.DoLayoutList();

                // Draw a button to clear all connections if there are any
                if (areaHandle.HasConnections())
                {
                    // Add a space between the additive scenes field and the buttons
                    EditorGUILayout.Separator();

                    // Add a label for the buttons section
                    EditorGUILayout.LabelField("Clean Up Actions", EditorStyles.boldLabel);

                    // Draw a button to clear all connections
                    if (GUILayout.Button("Clear All Connections"))
                    {
                        // Clear all connections in the area handle
                        areaHandle.ClearConnections();

                        // Apply the modified properties to the serialized object
                        serializedObject.ApplyModifiedProperties();
                    }

                    // Draw a warning message if there are any connections
                    EditorGUILayout.HelpBox($"This Area Handle is marked as impassable but has {areaHandle.connections.Count} connections. Please clear them to avoid issues.", MessageType.Warning);
                }
            }
            else if (areaHandle.Normal()) 
            {
                // Display the additive scenes field
                additiveScenesList.DoLayoutList();

                // Add a space between the additive scenes field and the connections section
                EditorGUILayout.Separator();

                // Add a label for the connections section
                EditorGUILayout.LabelField("Connections", EditorStyles.boldLabel);

                // Display the connections field
                connectionsList.DoLayoutList();
            }

            // End the change check
            if (EditorGUI.EndChangeCheck())
            {
                // Apply the modified properties to the serialized object
                serializedObject.ApplyModifiedProperties();
            }
        }

        private static GUIStyle GetMiniButtonStyle()
        {
            // Get the button style for the connection
            return new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(1, 1, 1, 1),
                fixedHeight = EditorGUIUtility.singleLineHeight,
                fixedWidth = 20
            };
        }

        private void DrawPersistentSceneWarning()
        {
            // Construct the warning message parts
            string hasAdditiveScenes = areaHandle.HasAdditiveScenes() ? areaHandle.additiveScenes.Count.ToString() + " additive scenes" : string.Empty;
            string hasConnections = areaHandle.HasConnections() ? areaHandle.connections.Count.ToString() + " connections" : string.Empty;
            string hasAnd = (areaHandle.HasAdditiveScenes() && areaHandle.HasConnections()) ? " and " : string.Empty;

            // Display a warning message
            EditorGUILayout.HelpBox($"This Area Handle is marked as persistent but has {hasAdditiveScenes}{hasAnd}{hasConnections}. Please clear them to avoid issues.", MessageType.Warning);
        }

        private static string ToImagePath(string iconName) => IconPathExtensions.ToImagePath(iconName);
    }
}