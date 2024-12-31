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
        public SerializedProperty passageDataProperty;
        public SerializedProperty passageTypeProperty;
        public SerializedProperty enterInteractionProperty;
        public SerializedProperty exitInteractionProperty;
        public SerializedProperty canInteractProperty;

        private void OnEnable()
        {
            passage = target as Passage;
            passageProperty = serializedObject.FindProperty("m_Script");
            passageDataProperty = serializedObject.FindProperty("passage");
            passageTypeProperty = serializedObject.FindProperty("type");
            enterInteractionProperty = serializedObject.FindProperty("enterInteraction");
            exitInteractionProperty = serializedObject.FindProperty("exitInteraction");
            canInteractProperty = serializedObject.FindProperty("canInteract");
        }

        public override void OnInspectorGUI()
        {
            // Start checking if the inspector has been changed
            EditorGUI.BeginChangeCheck();

            // Update the serialized object
            serializedObject.Update();

            // Display the script field for the passage component and disable the script field
            GUI.enabled = false;
            EditorGUILayout.PropertyField(passageProperty);
            GUI.enabled = true;

            // Remove a gap between the script field and the current scene field
            EditorGUILayout.Space(4);

            // Draw the passage field
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(passageDataProperty);

            // Get the button style for the connection
            GUIStyle buttonStyle = EditorStyles.miniButton;
            buttonStyle.padding = new RectOffset(1, 1, 1, 1);

            // Draw the find area handle button in a square horizontal group
            EditorGUILayout.BeginHorizontal(GUILayout.Width(5));
            GUIContent searchContent = EditorGUIUtility.IconContent("d_preAudioLoopOff");
            if (GUILayout.Button(searchContent, buttonStyle))
            {
                passage.SetPassageData(FindMatchingAreaHandle());
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            // Draw the passage type field 
            EditorGUILayout.PropertyField(passageTypeProperty);

            // Draw the enter interaction field
            EditorGUILayout.PropertyField(enterInteractionProperty);

            // Draw the exit interaction field if the passage type is open
            if (passage.type == PassageType.Open)
            {
                EditorGUILayout.PropertyField(exitInteractionProperty);
            }

            // Draw the can interact field
            EditorGUILayout.PropertyField(canInteractProperty);

            // Check if the area handle is null
            if (passage.Area == null)
            {
                // Display a warning message
                EditorGUILayout.HelpBox("Area Handle is null. Please assign an Area Handle to the Passage.", MessageType.Warning);
            }
            else if (passage.Area != null)
            {
                // Check if the Connection exists, if it does display the connection points, otherwise display a warning message
                if (ConnectionExists())
                {
                    // Display the connection points
                    EditorGUILayout.HelpBox("Start Point: " + StartPoint() + "\nEnd Point: " + EndPoint(), MessageType.Info);
                }
                else if (passage.GetValue() == "None" || !ConnectionExists())
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

        private AreaHandle FindMatchingAreaHandle()
        {
            // Create a new area handle
            AreaHandle area = null;

            // Get the currently loaded scene in the editor
            string sceneName = ActiveSceneName();

            // Look for the area handle that matches the passage
            areaHandles = GetAllAreaHandles();

            // Find the area handle with the matching the scene reference, prioritizing connections over scenes
            foreach (AreaHandle areaHandle in areaHandles)
            {
                if (areaHandle.currentScene.Name == sceneName)
                {
                    area = areaHandle;
                    break;
                }
            }

            // Return the area handle
            return area;
        }

        private AreaHandle[] GetAllAreaHandles()
        {
            // Get all area handles from the path
            return AssetDatabase.FindAssets("t:AreaHandle", null)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AreaHandle>)
                .ToArray();
        }

        private string StartPoint()
        {
            // Create the connected passage string
            string startPoint = string.Empty;

            // Get the matching passage from the area handle
            if (passage.Area != null && passage.Area.ConnectionExists(passage.GetValue()))
            {
                // Get the connection with the matching passage name from the area handle
                string currentScene = passage.Area.currentScene.Name;

                // Set the start point to the current scene and the connection passage value
                startPoint = currentScene + " - " + passage.GetValue();
            }

            // Return the passage name
            return startPoint;
        }

        private string EndPoint()
        {
            // Create the connected passage string
            string endPoint = string.Empty;

            // Get the matching passage from the area handle
            if (passage.Area != null && passage.Area.ConnectionExists(passage.GetValue()))
            {
                // Get the connection with the matching passage name from the area handle
                Connection connection = passage.Area.GetConnection(passage.GetValue());

                // Get the scene asset from the path and get the scene name
                string endPointScene = connection.connectedScene.currentScene.Name;

                // Set the end point to the end point scene and the passage value
                endPoint = endPointScene + " - " + connection.passage.value;
            }

            // Return the passage name
            return endPoint;
        }

        private bool ConnectionExists()
        {
            return StartPoint() != null && EndPoint() != null;
        }

        private string ActiveSceneName()
        {
            return EditorSceneManager.GetActiveScene().name;
        }
    }
}
