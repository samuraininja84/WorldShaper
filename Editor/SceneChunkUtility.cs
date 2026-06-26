using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;

namespace WorldShaper.Editor
{
    public static class SceneChunkUtility
    {
        // String constants for scene chunk creation
        static string basePath = "Assets/Scenes/";
        static string title = "Create New Scene Chunk";
        static string defaultName = "New Scene Chunk";
        static string fileExtension = "unity";
        static string message = "Save A New Scene Chunk To: ";

        [Shortcut("World Shaper/Scene Chunking/Create Scene Chunk", KeyCode.Q, ShortcutModifiers.Action)]
        [MenuItem("GameObject/New Scene Chunk", priority = 0)]
        public static void ChunkScene()
        {
            // Check if there are any selected GameObjects to chunk, if not, show a warning
            if (!CanChunkScene())
            {
                Debug.LogWarning("No GameObjects selected to chunk into a new scene.");
                return;
            }

            // Get all the selected GameObjects in the scene
            List<GameObject> selectedObjects = Selection.gameObjects.ToList();

            // Get the path for the new scene chunk
            string proposedSceneName = $"{EditorSceneManager.GetActiveScene().name} - {defaultName}";
            string scenePath = PromptForSceneChunkPath(proposedSceneName);

            // Check if the scene path is valid
            if (ValidPath(scenePath))
            {
                // Ensure the scene path is valid
                Scene newScene = CreateSceneChunk(scenePath);

                // Move the selected GameObjects to a new scene chunk
                MoveSelectedObjectsToScene(selectedObjects, newScene);

                // Optionally save the current scene to preserve changes
                SaveCurrentSceneIfNeeded();
            }
        }

        [MenuItem("GameObject/New Scene Chunk", true)]
        public static bool CanChunkScene() => Selection.gameObjects.Length > 0;

        private static bool ValidPath(string scenePath) => !string.IsNullOrEmpty(scenePath) && scenePath.EndsWith($".{fileExtension}");

        private static Scene CreateSceneChunk(string scenePath)
        {
            // Create a new scene
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);

            // Save the new scene to the specified path
            EditorSceneManager.SaveScene(newScene, scenePath);

            // Log the creation of the new scene
            Debug.Log($"New scene chunk created: {newScene.name} at {scenePath}");

            // Return the newly created scene
            return newScene;
        }

        private static string PromptForSceneChunkPath(string name) => EditorUtility.SaveFilePanelInProject(title, name, fileExtension, message, basePath);

        private static void MoveSelectedObjectsToScene(List<GameObject> selectedObjects, Scene newScene)
        {
            // Set the new scene as the active scene
            SceneManager.SetActiveScene(newScene);

            // Move the selected GameObjects to the new scene
            foreach (GameObject obj in selectedObjects) MoveGameObjectToScene(obj, newScene);

            // Log the completion of the operation
            Debug.Log($"Moved {selectedObjects.Count} GameObjects to the new scene chunk: {newScene.name}");
        }

        private static void MoveGameObjectToScene(GameObject obj, Scene newScene)
        {
            // Check if the GameObject is not null
            if (obj != null)
            {
                // Move the GameObject to the new scene
                SceneManager.MoveGameObjectToScene(obj, newScene);
            }
            else
            {
                // Log a warning if the GameObject is null
                Debug.LogWarning("Selected GameObject is null, skipping.");
            }
        }

        private static void SaveCurrentSceneIfNeeded()
        {
            // Optionally, you can save the current scene to ensure changes are not lost
            if (EditorSceneManager.GetActiveScene().isDirty)
            {
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
                Debug.Log("Current scene saved to preserve changes.");
            }
            else
            {
                Debug.Log("No changes to save in the current scene.");
            }
        }
    }
}
