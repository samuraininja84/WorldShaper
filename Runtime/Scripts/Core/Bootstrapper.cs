using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorldShaper
{
    /// <summary>
    /// Provides functionality to bootstrap the application by initializing and loading the persistent scene.
    /// </summary>
    /// <remarks>The <see cref="Bootstrapper"/> class is responsible for ensuring that the core persistent scene, identified by <see cref="WorldMap.PersistentSceneName"/>, is loaded before any other scenes. 
    /// This scene remains loaded throughout the application's lifecycle to serve as the foundation for the application.
    /// </remarks>
    public static class Bootstrapper
    {
        /// <summary>
        /// Represents the current instance of the world map used in the application.
        /// </summary>
        /// <remarks>
        /// This static field provides access to the global world map. 
        /// It can be used to retrieve or modify the state of the map shared across the application.
        /// Ensure thread safety when accessing this field in a multithreaded environment.
        /// </remarks>
        private static WorldMap worldMap;

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
        /// Initializes the application by loading all persistent scenes defined in the world map.
        /// </summary>
        /// <remarks>
        /// This method is executed automatically before the first scene is loaded, as specified by the <see cref="RuntimeInitializeOnLoadMethodAttribute"/>. 
        /// It ensures that all persistent scenes are loaded asynchronously in additive mode, unless they are already loaded.
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static async Task Initialize()
        {
            // Define a flag to track if all scenes are already loaded
            bool allScenesLoaded = true;

            // Load all persistent scenes defined in the world map
            foreach (var scene in Instance.PersistentScenes)
            {
                // Check if the persistent scene is already loaded, if so, continue
                if (SceneManager.GetSceneByName(scene.Name).IsValid()) continue;

                // Load the persistent scene asynchronously in single mode
                await SceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Additive);

                // Since we had to load a scene, set allScenesLoaded to false
                allScenesLoaded = false;
            }

            // If all scenes were already loaded, log a message
            if (allScenesLoaded) Debug.Log("Bootstrapper: All persistent scenes are already loaded.");
            else Debug.Log("Bootstrapper: Persistent scenes loaded successfully.");
        }
    }
}
