using UnityEngine;
using UnityEditor;

namespace WorldShaper.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AreaHandle))]
    public class AreaHandleEditor : UnityEditor.Editor
    {
        public AreaHandle areaHandle;
        public SerializedProperty script;
        public SerializedProperty areaHandleTypeProperty;
        public SerializedProperty activeSceneProperty;
        public SerializedProperty additiveScenesProperty;

        private bool showConnections = true;

        private void OnEnable()
        {
            areaHandle = target as AreaHandle;
            areaHandle.ValidateConnections();
            script = serializedObject.FindProperty("m_Script");
            areaHandleTypeProperty = serializedObject.FindProperty("areaHandleType");
            activeSceneProperty = serializedObject.FindProperty("activeScene");
            additiveScenesProperty = serializedObject.FindProperty("additiveScenes");
        }

        public override void OnInspectorGUI()
        {
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
                EditorGUILayout.PropertyField(additiveScenesProperty);

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
                EditorGUILayout.PropertyField(additiveScenesProperty);

                // Add a space between the additive scenes field and the connections section
                EditorGUILayout.Separator();

                // Add a label for the connections section
                EditorGUILayout.LabelField("Connections", EditorStyles.boldLabel);

                // Draw the connections if there are any
                DrawConnections();

                // Add a space between the connections and the buttons
                EditorGUILayout.Separator();

                // Add a label for the buttons section
                EditorGUILayout.LabelField("Connection Actions", EditorStyles.boldLabel);

                // Draw the connection action buttons
                DrawActions();
            }

            // End the change check
            if (EditorGUI.EndChangeCheck())
            {
                // Apply the modified properties to the serialized object
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawConnections()
        {
            // Check if there are any connections in the area handle, then display the connections
            if (areaHandle.HasConnections())
            {
                // Display a foldout header group for the connections
                showConnections = EditorGUILayout.BeginFoldoutHeaderGroup(showConnections, "Connections: " + areaHandle.connections.Count.ToString());

                // If the foldout is expanded, display each connection
                if (showConnections)
                {
                    // Get the area handle
                    for (int i = 0; i < areaHandle.connections.Count; i++)
                    {
                        // Draw each connection in the list
                        DrawConnection(areaHandle.connections[i]);
                    }
                }

                // End the foldout header group
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        private void DrawConnection(Connection connection)
        {
            // Create a serialized object for the connection
            SerializedObject connectionProperty = new SerializedObject(connection);

            // Update the connection
            connectionProperty.Update();

            // Get the connection name, connected scene, and endpoint properties
            SerializedProperty connectionName = connectionProperty.FindProperty("connectionName");
            SerializedProperty connectionType = connectionProperty.FindProperty("connectionType");
            SerializedProperty connectedScene = connectionProperty.FindProperty("destinationArea");
            SerializedProperty transitionIn = connectionProperty.FindProperty("transitionIn");
            SerializedProperty transitionOut = connectionProperty.FindProperty("transitionOut");
            SerializedProperty endpoint = connectionProperty.FindProperty("endpoint");

            // Check if the connection is closed
            bool closedConnection = connection.Closed();

            // Start a horizontal group to encapsulate the connection fields and their buttons
            EditorGUILayout.BeginHorizontal();

            // Start a vertical group to encapsulate the connection fields
            EditorGUILayout.BeginVertical();

            // Display the connection fields
            EditorGUILayout.PropertyField(connectionName);
            EditorGUILayout.PropertyField(connectionType);
            if (!closedConnection) EditorGUILayout.PropertyField(connectedScene);
            EditorGUILayout.PropertyField(transitionIn);
            EditorGUILayout.PropertyField(transitionOut);
            if (!closedConnection) EditorGUILayout.PropertyField(endpoint);

            // If the connection is closed but still has a destination or endpoint, display a warning message
            if (closedConnection && (connection.HasDestination() || connection.HasEndpoint()))
            {
                // Create strings to determine if the connection has a destination or endpoint for the warning message
                string hasDestination = connection.HasDestination() ? "has a destination" : string.Empty;
                string hasEndpoint = connection.HasEndpoint() ? "has an endpoint" : string.Empty;
                string and = (connection.HasDestination() && connection.HasEndpoint()) ? " and " : string.Empty;
                string isPlural = (connection.HasDestination() && connection.HasEndpoint()) ? "them" : "it";

                // Display a warning message if the connection is closed
                EditorGUILayout.HelpBox($"This connection is closed but {hasDestination}{and}{hasEndpoint}. Please clear {isPlural} to avoid issues with unwanted loading.", MessageType.Warning);

                // Display a button to clear the destination and endpoint if the connection has either
                if (connection.HasDestination() || connection.HasEndpoint())
                {
                    // Display a button to clear the destination
                    if (GUILayout.Button("Clear Destination & Endpoint"))
                    {
                        // Clear the destination in the connection
                        connection.destinationArea = null;

                        // Clear the endpoint in the connection
                        connection.endpoint.Set("None");

                        // Refresh the connection to update the destination and endpoint properties
                        connection.Refresh();

                        // Apply the modified properties to the serialized object
                        serializedObject.ApplyModifiedProperties();
                    }
                }
            }

            // End the vertical group for the connection fields
            EditorGUILayout.EndVertical();

            // Get the button style for the connection
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton);
            buttonStyle.padding = new RectOffset(1, 1, 1, 1);
            buttonStyle.fixedHeight = EditorGUIUtility.singleLineHeight;
            buttonStyle.fixedWidth = 20;

            // Create a grid layout to display the buttons
            EditorGUILayout.BeginVertical(GUILayout.Width(1));

            // Display buttons to set a two-way connection
            EditorGUILayout.BeginHorizontal(GUILayout.Width(1));
            GUIContent refreshContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("Sync")));
            refreshContent.tooltip = "Set Two-Way Connection";
            if (GUILayout.Button(refreshContent, buttonStyle))
            {
                connection.SyncEndpointLink();
                serializedObject.ApplyModifiedProperties();
            }

            // Display a button to set the endpoint to closed
            GUIContent closeSingleContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("OneWay")));
            closeSingleContent.tooltip = "Set Endpoint to Closed";
            if (GUILayout.Button(closeSingleContent, buttonStyle))
            {
                connection.SetConnectionType(ConnectionType.Closed);
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();

            // Display a button to refresh the connection
            EditorGUILayout.BeginHorizontal(GUILayout.Width(1));
            GUIContent refreshSingleContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("Refresh")));
            refreshSingleContent.tooltip = "Refresh Connection";
            if (GUILayout.Button(refreshSingleContent, buttonStyle))
            {
                connection.Refresh();
                serializedObject.ApplyModifiedProperties();
            }

            // Display a button to remove the connection
            GUIContent closeContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("Delete")));
            closeContent.tooltip = "Remove Connection";
            if (GUILayout.Button(closeContent, buttonStyle))
            {
                connection.Remove();
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();

            // Display a button to move the connection to the top of the list
            EditorGUILayout.BeginHorizontal(GUILayout.Width(1));
            GUIContent topContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("MoveTop")));
            topContent.tooltip = "Move Connection To Top";
            if (GUILayout.Button(topContent, buttonStyle))
            {
                areaHandle.MoveToTop(connection);
                serializedObject.ApplyModifiedProperties();
            }

            // Display a button to move the connection up in the list
            GUIContent upContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("MoveUp")));
            upContent.tooltip = "Move Connection Up";
            if (GUILayout.Button(upContent, buttonStyle))
            {
                areaHandle.MoveConnectionUp(connection);
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();

            // Display a button to move the connection to the bottom of the list
            EditorGUILayout.BeginHorizontal(GUILayout.Width(1));
            GUIContent bottomContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("MoveBottom")));
            bottomContent.tooltip = "Move Connection To Bottom";
            if (GUILayout.Button(bottomContent, buttonStyle))
            {
                areaHandle.MoveToBottom(connection);
                serializedObject.ApplyModifiedProperties();
            }

            // Display a button to move the connection down in the list
            GUIContent downContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("MoveDown")));
            downContent.tooltip = "Move Connection Down";
            if (GUILayout.Button(downContent, buttonStyle))
            {
                areaHandle.MoveConnectionDown(connection);
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();

            // End the encapsulating vertical group
            EditorGUILayout.EndVertical();

            // End the encapsulating horizontal group
            EditorGUILayout.EndHorizontal();

            // Apply changes to the serialized object
            connectionProperty.ApplyModifiedProperties();

            // Add a space between connections
            EditorGUILayout.Space(5);
        }

        private void DrawActions()
        {
            // Get the button style for the connection
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton);
            buttonStyle.padding = new RectOffset(1, 1, 1, 1);
            buttonStyle.fixedHeight = EditorGUIUtility.singleLineHeight;

            // Total width available for buttons, accounting for padding
            float totalWidth = EditorGUIUtility.currentViewWidth - 25;

            // If the area handle is normal and has connections, let the buttons take the full width, otherwise split it in half
            buttonStyle.fixedWidth = !areaHandle.HasConnections() ? totalWidth : (totalWidth) / 2;

            // Begin a horizontal layout for the action buttons
            EditorGUILayout.BeginHorizontal();

            // Display buttons to create a new connection
            GUIContent plusContent = EditorGUIUtility.IconContent("d_Toolbar Plus");
            plusContent.tooltip = "Create New Connection";
            if (GUILayout.Button(plusContent, buttonStyle))
            {
                // Create a new connection at the end of the list
                areaHandle.CreateConnection();

                // Apply the modified properties to the serialized object
                serializedObject.ApplyModifiedProperties();
            }

            // Check if there are any connections in the area handle, then display a button to clear all connections
            if (areaHandle.HasConnections())
            {
                // Get the button content for the clear all connections button
                GUIContent minusContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("ClearAll")));
                minusContent.tooltip = "Clear All Connections";

                // Draw the button to clear all connections
                if (GUILayout.Button(minusContent, buttonStyle))
                {
                    // Clear all connections in the area handle
                    areaHandle.ClearConnections();

                    // Apply the modified properties to the serialized object
                    serializedObject.ApplyModifiedProperties();
                }
            }

            // End the horizontal layout for the action buttons
            EditorGUILayout.EndHorizontal();
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