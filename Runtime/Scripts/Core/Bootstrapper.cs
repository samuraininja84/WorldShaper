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
        /// Gets the singleton instance of the <see cref="WorldMap"/> class.
        /// </summary>
        public static WorldMap Instance => WorldMap.Instance;

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
