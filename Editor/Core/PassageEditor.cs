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

        private void OnEnable() => passage = target as Passage;

        public override void OnInspectorGUI()
        {
            // Start checking if the inspector has been changed
            EditorGUI.BeginChangeCheck();

            // Draw the base inspector
            DrawDefaultInspector();

            // Check if the area handle is null
            if (passage.Area == null)
            {
                // Display a warning message
                EditorGUILayout.HelpBox("Area Handle is null. Please assign an Area Handle to the Endpoint.", MessageType.Warning);
            }
            else if (passage.Area != null)
            {
                // Check if the Connection exists, if it does display the connection points, otherwise display a warning message
                if (ConnectionExists())
                {
                    // Display the connection points
                    EditorGUILayout.HelpBox("Start Point: " + StartPoint() + "\nEnd Point: " + EndPoint(), MessageType.Info);
                }
                else if (passage.GetEndpoint() == "None" || !ConnectionExists())
                {
                    // Display a warning message
                    EditorGUILayout.HelpBox("Endpoint is set to None. Please assign an Endpoint.", MessageType.Warning);
                }
            }

            // Check if the inspector has changed
            if (EditorGUI.EndChangeCheck())
            {
                // Apply the changes to the serialized object
                serializedObject.ApplyModifiedProperties();
            }
        }

        private string StartPoint()
        {
            // Create the connected passage string
            string startPoint = string.Empty;

            // Get the matching passage from the area handle
            if (passage.Area != null && passage.Area.ConnectionExists(passage.GetEndpoint()))
            {
                // Get the connection with the matching passage name from the area handle
                string currentScene = passage.Area.activeScene.Name;

                // Set the start point to the current scene and the connection passage value
                startPoint = currentScene + " - " + passage.GetEndpoint();
            }

            // Return the passage name
            return startPoint;
        }

        private string EndPoint()
        {
            // Create the connected passage string
            string endPoint = string.Empty;

            // Get the matching passage from the area handle
            if (passage.Area != null && passage.Area.ConnectionExists(passage.GetEndpoint()))
            {
                // Get the connection with the matching passage name from the area handle
                Connection connection = passage.Area.GetConnection(passage.GetEndpoint());

                // Get the scene asset from the path and get the scene name
                string endPointScene = connection.Destination.Name;

                // Set the end point to the end point scene and the passage value
                endPoint = endPointScene + " - " + connection.endpoint.value;
            }

            // Return the passage name
            return endPoint;
        }

        private bool ConnectionExists() => StartPoint() != null && EndPoint() != null;
    }
}
