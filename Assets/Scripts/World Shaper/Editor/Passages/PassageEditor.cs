using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace WorldShaper.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Passage))]
    public class PassageEditor : UnityEditor.Editor
    {
        public Passage passage;
        public AreaHandle[] areaHandles;
        public SerializedProperty passageProperty;
        public SerializedProperty enumProperty;
        private string areaHandlePath = "Assets/Scripts/Tooling/World Shaper/Resources/Connections";

        private void OnEnable()
        {
            passage = target as Passage;
            passage.CreateConnectionList();
            passageProperty = serializedObject.FindProperty("m_Script");
            enumProperty = serializedObject.FindProperty("passage");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            // Update the serialized object
            serializedObject.Update();

            // Display the script field for the Passage component and disable the script field
            GUI.enabled = false;
            EditorGUILayout.PropertyField(passageProperty);
            GUI.enabled = true;

            // Remove a gap between the script field and the current scene field
            EditorGUILayout.Space(4);

            // Based on the handling type, draw the area handle and passage fields for the start and end passages
            DrawAreaHandle("area", enumProperty);

            // Draw the can interact field
            EditorGUILayout.PropertyField(serializedObject.FindProperty("canInteract"));

            // Check if the area handle is null
            if (passage.area == null)
            {
                // Display a warning message
                EditorGUILayout.HelpBox("Area Handle is null. Please assign an Area Handle to the Passage.", MessageType.Warning);
            }
            else
            {
                // Check if the Connection exists, if it does display the connection points, otherwise display a warning message
                if (ConnectionExists())
                {
                    // Display the connection points
                    EditorGUILayout.HelpBox("Start Point: " + GetStartPointFromPassage() + "\nEnd Point: " + GetEndPointFromPassage(), MessageType.Info);
                }
                else if (passage.passage.value == "None" || !ConnectionExists())
                {
                    // Display a warning message
                    EditorGUILayout.HelpBox("Passage is set to None. Please assign a Passage to the Passage.", MessageType.Warning);
                }
            }

            // Check if the inspector has changed
            if (EditorGUI.EndChangeCheck())
            {
                // Apply the changes to the serialized object
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawAreaHandle(string areaProperty, SerializedProperty enumProperty)
        {
            // Begin the encapsulating horizontal group
            EditorGUILayout.BeginHorizontal();

            // Draw the area handle field and a dropdown button to find the matching area handle
            EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(areaProperty));
            EditorGUILayout.EndHorizontal();

            // Draw the passage field with the hide label property set to true
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(enumProperty, GUIContent.none);
            SerializedProperty hideLabel = enumProperty.FindPropertyRelative("hideLabel");
            hideLabel.boolValue = true;
            EditorGUILayout.EndHorizontal();

            // Get the button style for the connection
            GUIStyle buttonStyle = EditorStyles.miniButton;
            buttonStyle.padding = new RectOffset(1, 1, 1, 1);

            // Draw the find area handle button in a square horizontal group
            GUIContent searchContent = EditorGUIUtility.IconContent("d_preAudioLoopOff");
            if (GUILayout.Button(searchContent, buttonStyle))
            {
                passage.SetAreaHandle(FindMatchingAreaHandle());
                if (passage.area != null) passage.CreateConnectionList();
            }

            // End the encapsulating horizontal group
            EditorGUILayout.EndHorizontal();

            // Remove a gap between the area handle and the passage field
            EditorGUILayout.Space(-1);
        }

        private AreaHandle FindMatchingAreaHandle()
        {
            // Create a new area handle
            AreaHandle area = null;

            // Get the currently loaded scene in the editor
            string scenePath = ScenePath();

            // Look for the area handle that matches the passage
            areaHandles = GetAllAreaHandlesFromPath(areaHandlePath);

            // Find the area handle with the matching the scene reference, prioritizing connections over scenes
            foreach (AreaHandle areaHandle in areaHandles)
            {
                if (areaHandle.currentScene.Path == scenePath)
                {
                    area = areaHandle;
                    break;
                }
            }

            // Return the area handle
            return area;
        }

        private AreaHandle[] GetAllAreaHandlesFromPath(string filePath)
        {
            // Get all area handles from the path
            return AssetDatabase.FindAssets("t:AreaHandle", new[] { filePath })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AreaHandle>)
                .ToArray();
        }

        private string GetStartPointFromPassage()
        {
            // Create the connected passage string
            string startPoint = string.Empty;

            // Get the matching passage from the area handle
            if (passage.area != null && passage.area.ConnectionExists(passage.passage.value))
            {
                // Get the connection with the matching passage name from the area handle
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(passage.area.currentScene.Path);
                string currentScene = sceneAsset.name;

                // Set the start point to the current scene and the connection passage value
                startPoint = currentScene + " - " + passage.passage.value;
            }

            // Return the passage name
            return startPoint;
        }

        private string GetEndPointFromPassage()
        {
            // Create the connected passage string
            string endPoint = string.Empty;

            // Get the matching passage from the area handle
            if (passage.area != null && passage.area.ConnectionExists(passage.passage.value))
            {
                // Get the connection with the matching passage name from the area handle
                Connection connection = passage.area.GetConnection(passage.passage.value);
                string currentScenePath = connection.connectedScene.currentScene.Path;

                // Get the scene asset from the path and get the scene name
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(currentScenePath);
                string endPointScene = sceneAsset.name;

                // Set the end point to the end point scene and the passage value
                endPoint = endPointScene + " - " + connection.passage.value;
            }

            // Return the passage name
            return endPoint;
        }

        private bool ConnectionExists()
        {
            return GetStartPointFromPassage() != null && GetEndPointFromPassage() != null;
        }

        private string ScenePath()
        {
            return EditorSceneManager.GetActiveScene().path;
        }
    }
}
