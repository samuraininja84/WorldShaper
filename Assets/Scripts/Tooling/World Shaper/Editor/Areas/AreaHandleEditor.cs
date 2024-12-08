using UnityEngine;
using UnityEditor;

namespace WorldShaper.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(AreaHandle))]
    public class AreaHandleEditor : UnityEditor.Editor
    {
        public AreaHandle areaHandle;
        public SerializedProperty areaHandleProperty;
        public SerializedProperty currentSceneProperty;

        bool showConnections = true;

        private void OnEnable()
        {
            areaHandle = target as AreaHandle;
            areaHandle.ValidateConnections();
            areaHandleProperty = serializedObject.FindProperty("m_Script");
            currentSceneProperty = serializedObject.FindProperty("currentScene");
        }

        public override void OnInspectorGUI()
        {
            // Start the change check
            EditorGUI.BeginChangeCheck();

            // Update the serialized object
            serializedObject.Update();

            // Display the script field for the AreaHandle component and disable the script field
            GUI.enabled = false;
            EditorGUILayout.PropertyField(areaHandleProperty);
            GUI.enabled = true;

            // Remove a gap between the script field and the current scene field
            EditorGUILayout.Space(-10);

            // Display the current scene field and accept changes
            EditorGUILayout.PropertyField(currentSceneProperty);

            // Check if there are any connections in the area handle, then display the connections
            if (GetConnections())
            {
                showConnections = EditorGUILayout.BeginFoldoutHeaderGroup(showConnections, "Connections: " + areaHandle.connections.Count.ToString());
                if (showConnections)
                {
                    foreach (Connection connection in areaHandle.connections)
                    {
                        DrawConnection(connection);
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }

            // Display buttons to create a new connection
            GUIContent plusContent = EditorGUIUtility.IconContent("d_Toolbar Plus");
            EditorGUILayout.LabelField("Create New Connection");
            if (GUILayout.Button(plusContent))
            {
                // Create a new connection at the end of the list
                AreaHandle areaHandle = (AreaHandle)target;
                areaHandle.CreateConnection();
                serializedObject.ApplyModifiedProperties();
            }

            // Display a button to clear all connections
            GUIContent minusContent = EditorGUIUtility.IconContent("d_Toolbar Minus");
            EditorGUILayout.LabelField("Clear Connections");
            if (GUILayout.Button(minusContent))
            {
                // Clear all connections in the list
                AreaHandle areaHandle = (AreaHandle)target;
                areaHandle.ClearConnections();
                serializedObject.ApplyModifiedProperties();
            }

            // End the change check
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawConnection(Connection connection)
        {
            // Create a serialized object for the connection
            SerializedObject connectionProperty = new SerializedObject(connection);

            // Update the connection
            connectionProperty.Update();

            // Get the connection name, connected scene, and passage properties
            SerializedProperty connectionName = connectionProperty.FindProperty("connectionName");
            SerializedProperty connectedScene = connectionProperty.FindProperty("connectedScene");
            SerializedProperty passage = connectionProperty.FindProperty("passage");

            // Start a horizontal group to encapsulate the connection fields and their buttons
            EditorGUILayout.BeginHorizontal();

            // Display the connection name, connected scene, and passage fields as a vertical group
            EditorGUILayout.BeginVertical();
            EditorGUILayout.PropertyField(connectionName);
            EditorGUILayout.PropertyField(connectedScene);
            EditorGUILayout.PropertyField(passage);
            EditorGUILayout.EndVertical();

            // Get the button style for the connection
            GUIStyle buttonStyle = EditorStyles.miniButton;
            buttonStyle.padding = new RectOffset(1, 1, 1, 1);

            // Create a grid layout to display the buttons
            EditorGUILayout.BeginVertical(GUILayout.Width(1));

            // Display buttons to set the passage link
            EditorGUILayout.BeginHorizontal(GUILayout.Width(1));
            GUIContent refreshContent = EditorGUIUtility.IconContent("d_preAudioLoopOff");
            if (GUILayout.Button(refreshContent, buttonStyle))
            {
                connection.SetPassageLink();
                serializedObject.ApplyModifiedProperties();
            }

            // Display a button to remove the connection
            GUIContent closeContent = EditorGUIUtility.IconContent("d_winbtn_win_close");
            if (GUILayout.Button(closeContent, buttonStyle))
            {
                connection.Remove();
                serializedObject.ApplyModifiedProperties();
            }
            EditorGUILayout.EndHorizontal();

            // Display a button to move the connection up in the list
            EditorGUILayout.BeginHorizontal(GUILayout.Width(1));
            GUIContent upContent = EditorGUIUtility.IconContent("Toolbar Plus");
            if (GUILayout.Button(upContent, buttonStyle))
            {
                areaHandle.MoveConnectionUp(connection);
                serializedObject.ApplyModifiedProperties();
            }

            // Display a button to move the connection down in the list
            GUIContent downContent = EditorGUIUtility.IconContent("Toolbar Minus");
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

        private bool GetConnections()
        {
            if (areaHandle.connections.Count > 0)
            {
                return true;
            }
            return false;
        }
    }
}