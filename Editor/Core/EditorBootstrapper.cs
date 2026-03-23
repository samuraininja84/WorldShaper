using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace WorldShaper.Editor
{
    /// <summary>
    /// Provides functionality for managing the application's world map and persistent scenes.
    /// </summary>
    /// <remarks>
    /// The <see cref="EditorBootstrapper"/> class offers static methods and properties to interact with the global <see cref="WorldMap"/> instance, retrieve resources, and manage the loading state of persistent scenes. 
    /// It is primarily designed for use within the Unity Editor and includes menu integrations for scene management.
    /// </remarks>
    public static class EditorBootstrapper
    {
        /// <summary>
        /// Represents the current instance of the world map used in the application.
        /// </summary>
        /// <remarks>
        /// This static field provides access to the global world map. 
        /// It can be used to retrieve or modify the state of the map shared across the application.
        /// Ensure thread safety when accessing this field in a multithreaded environment.
        /// </remarks>
        public static WorldMap worldMap;

        /// <summary>
        /// Gets the singleton instance of the <see cref="WorldMap"/> class.
        /// </summary>
        /// <remarks>This property ensures that only one instance of the <see cref="WorldMap"/> class is created and shared across the application. The instance is lazily initialized when first accessed.</remarks>
        public static WorldMap Instance => worldMap != null ? worldMap : worldMap = GetWorldMap();

        /// <summary>
        /// Retrieves the first <see cref="WorldMap"/> instance found in the application's resources.
        /// </summary>
        /// <remarks>
        /// This method searches for all objects of type <see cref="WorldMap"/> in the application's resources and returns the first instance found. 
        /// If no <see cref="WorldMap"/> is present, a warning is logged, and the method returns <see langword="null"/>.</remarks>
        /// <returns>The first <see cref="WorldMap"/> instance found in the resources, or <see langword="null"/> if no instance is found.</returns>
        public static WorldMap GetWorldMap()
        {
            // Find the world map in the resources
            var worldMap = Resources.FindObjectsOfTypeAll<WorldMap>().FirstOrDefault();

            // If no world map is found, log a warning and return
            if (worldMap == null)
            {
                // Log a warning indicating that no WorldMap was found
                Debug.LogWarning("No WorldMap found in resources. Ensure a WorldMap asset is present.");

                // Return early since we cannot proceed without a WorldMap
                return null;
            }

            // Return the found world map
            return worldMap;
        }

        /// <summary>
        /// Toggles the loading state of all persistent scenes defined in the <see cref="WorldMap"/> asset.
        /// </summary>
        /// <remarks>
        /// This method is accessible via the Unity Editor menu under "Tools/World Shaper/Toggle Persistent Scene(s)" and can be triggered using the shortcut <c>&%l</c> (Shift + Alt + L on Windows, Shift + Option + L on macOS). 
        /// If all persistent scenes are currently loaded, this method unloads them.
        /// Otherwise, it loads all persistent  scenes defined in the <see cref="WorldMap"/> asset. 
        /// The method logs the operation's outcome to the Unity Console.
        /// </remarks>
        [MenuItem("Tools/World Shaper/Toggle Persistent Scene(s) #&l")]
        private static void TogglePersistentScenes()
        {
            // If all persistent scenes are loaded, unload them
            if (AllLoaded(Instance))
            {
                // Unload all persistent scenes defined in the world map
                foreach (var scene in Instance.PersistentScenes)
                {
                    // Check if the persistent scene is loaded, if not, continue
                    if (!EditorSceneManager.GetSceneByName(scene.Name).IsValid()) continue;

                    // Ask the user to save any unsaved changes before unloading
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        // Close the persistent scene
                        EditorSceneManager.CloseScene(scene.LoadedScene, true);

                        // Log that the changes were saved
                        Debug.Log($"Unsaved changes in scene '{scene.Name}' have been saved.");
                    }
                    else
                    {
                        // Log that the user canceled the save operation
                        Debug.Log($"Unloading of scene '{scene.Name}' was canceled by the user. Please finish any unsaved changes manually.");
                    }
                }

                // Log that all persistent scenes have been unloaded
                Debug.Log("All persistent scenes have been unloaded.");

                // Return early since we have unloaded all scenes
                return;
            }

            // Load all persistent scenes defined in the world map
            foreach (var scene in Instance.PersistentScenes)
            {
                // Check if the persistent scene is already loaded, if so, continue
                if (EditorSceneManager.GetSceneByName(scene.Name).IsValid()) continue;

                // Load the persistent scene asynchronously in single mode
                EditorSceneManager.OpenScene(scene.Path, OpenSceneMode.Additive);
            }

            // Log that all persistent scenes have been loaded
            Debug.Log("All persistent scenes have been loaded.");
        }

        /// <summary>
        /// Determines whether all persistent scenes are currently loaded and valid in the editor.
        /// </summary>
        /// <remarks>A scene is considered valid if it is loaded in the editor and its name matches one of the persistent scenes.</remarks>
        /// <returns><see langword="true"/> if all persistent scenes are loaded and valid; otherwise, <see langword="false"/>.</returns>
        private static bool AllLoaded(WorldMap worldMap) => worldMap.PersistentScenes.TrueForAll(scene => EditorSceneManager.GetSceneByName(scene.Name).IsValid());
    }
}
