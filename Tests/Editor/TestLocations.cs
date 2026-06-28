using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using BackgroundProgress = UnityEditor.Progress;
using UnsafeReason = Eflatun.SceneReference.SceneReferenceUnsafeReason;

namespace WorldShaper.Tests
{
    public class TestLocations
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="WorldMap"/> class.
        /// </summary>
        public static WorldMap WorldMap => WorldMap.Instance;

        [Test]
        public async Task TestPersistentScenes()
        {
            // Load all persistent scenes defined in the world map
            for (int i = 0; i < WorldMap.PersistentScenes.Count; i++)
            {
                // Get the persistent scene at the current index
                var scene = WorldMap.PersistentScenes[i];

                // If the scene is not valid, throw an exception to indicate that the test cannot proceed
                if (scene == null || scene.UnsafeReason != UnsafeReason.None) throw new System.Exception($"Persistent scene at index {i} is not valid. Cannot proceed with the test.");

                // Open the first scene in single mode to ensure that it is the only scene loaded
                if (i == 0) EditorSceneManager.OpenScene(scene.Path, OpenSceneMode.Single);
                else EditorSceneManager.OpenScene(scene.Path, OpenSceneMode.Additive);
            }

            // Log that all persistent scenes have been loaded
            Debug.Log("All persistent scenes contained in the world map have been loaded successfully.");
        }

        [Test]
        public async Task TestRegisteredAreas()
        {
            for (int i = 0; i < WorldMap.registeredAreas.Count; i++)
            {
                // Get the registered area at the current index
                var area = WorldMap.registeredAreas[i];

                // If the area is not valid, throw an exception to indicate that the test cannot proceed
                if (area == null || !area.IsValid) throw new System.Exception($"Registered area at index {i} is not valid. Cannot proceed with the test.");

                // Load the area using the EditorLoad method, which will load all scenes associated with the area handle
                await EditorLoad(area, loadPersistentScenes: false, unloadUnusedAssets: true);
            }

            // Log that all areas have been loaded
            Debug.Log("All areas contained in the world map have been loaded successfully.");
        }

        [Test]
        public async Task TestAllAreas()
        {
            for (int i = 0; i < WorldMap.registeredAreas.Count; i++)
            {
                // Get the registered area at the current index
                var area = WorldMap.registeredAreas[i];

                // If the area is not valid, throw an exception to indicate that the test cannot proceed
                if (area == null || !area.IsValid) throw new System.Exception($"Registered area: {area.Name} at index {i} is not valid. Cannot proceed with the test.");

                // Load the area using the EditorLoad method, which will load all scenes associated with the area handle
                await EditorLoad(area, loadPersistentScenes: true, unloadUnusedAssets: true);
            }

            // Log that all areas have been loaded
            Debug.Log("All areas contained in the world map have been loaded successfully.");
        }

        [Test]
        public async Task TestAllLocations()
        {
            for (int i = 0; i < WorldMap.registeredAreas.Count; i++)
            {
                // Get the registered area at the current index
                var area = WorldMap.registeredAreas[i];

                // If the area is not valid, throw an exception to indicate that the test cannot proceed
                if (area == null || !area.IsValid) throw new System.Exception($"Registered area: {area.Name} at index {i} is not valid. Cannot proceed with the test.");

                // Load the area using the EditorLoad method, which will load all scenes associated with the area handle
                await EditorLoad(area, loadPersistentScenes: true, unloadUnusedAssets: true);

                // Iterate through each connection in the area and test if it is valid and can be found in the loaded scenes
                for (int j = 0; j < area.connections.Count; j++)
                {
                    // Get the connection at the current index
                    var connection = area.connections[j];

                    // If the connection is not valid, throw an exception to indicate that the test cannot proceed
                    if (connection == null || !connection.IsValid) throw new System.Exception($"Connection: {connection.Endpoint} in area: {area.Name} at index {j} is not valid. Cannot proceed with the test.");

                    // Log the connection information for debugging purposes
                    Debug.Log($"Testing connection: {connection.Endpoint} in area: {area.Name}");

                    // Get all location pointers in the loaded scenes to test connections against
                    ILocationPointer[] pointers = ILocationPointerExtensions.GetAllLocations().ToArray();

                    // Find the location with the connection name
                    ILocationPointer target = pointers.FirstOrDefault(c => c.GetEndpoint() == connection.Endpoint);

                    // Move the camera to the target pointer if found
                    if (target != null)
                    {
                        // Set the camera position to the target location's position
                        SceneView.lastActiveSceneView.pivot = target.GetPosition();
                        SceneView.lastActiveSceneView.Repaint();
                    }
                    else
                    {
                        // Log a warning if no location is found with the specified connection name
                        Debug.LogWarning($"No connectable found with name '{connection.Endpoint}' in {connection.Destination.Name}.");
                    }
                }
            }

            // Log that all areas have been loaded
            Debug.Log("All areas contained in the world map have been loaded successfully.");
        }

        /// <summary>
        /// Asynchronously loads all scenes associated with the specified area handle, optionally reloading duplicate
        /// scenes.
        /// </summary>
        /// <remarks>
        /// This method performs the following operations: 
        /// <list type="bullet"> 
        ///     <item>Unloads any scenes that are not part of the specified area handle.</item> 
        ///     <item>Loads all scenes defined in the area handle, as editor scenes.</item> 
        ///     <item>Reports progress to background progress operations to provide feedback to the user during the loading process.</item>
        ///     <item>Sets the active scene to the one specified in the area handle after loading is complete.</item> 
        /// </list>
        /// </remarks>
        /// <param name="handle">The <see cref="AreaHandle"/> representing the group of scenes to load.</param>
        /// <param name="unloadUnusedAssets">A boolean value indicating whether to unload unused assets after unloading scenes. The default value is <see langword="false"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task completes when all scenes are loaded.</returns>
        public async Task EditorLoad(AreaHandle handle, bool loadPersistentScenes = false, bool unloadUnusedAssets = false)
        {
            // Start a background progress operation to provide feedback to the user during the loading process
            int progressId = BackgroundProgress.Start("Loading Area...");

            // Get the count of scenes to load from the area handle
            var handleScenesToLoad = handle.additiveScenes.Count;

            // Get the count of persistent scenes that will also be loaded
            var persistentScenesToLoad = loadPersistentScenes ? WorldMap.PersistentScenes.Count : 0;

            // Get the total number of scenes that will be loaded, including the scenes in the area handle and any persistent scenes, for progress reporting purposes
            int totalScenesToLoad = handleScenesToLoad + persistentScenesToLoad + 1;

            // Add a small progress value for the active scene since it is the first step in loading the area, and we want to give some feedback to the user that the loading has started
            float SceneProgress(int index) => (float)index / (float)totalScenesToLoad;

            // Get the active scene from the area handle by its type
            Scene activeScene = SceneManager.GetSceneByName(handle.GetActiveScene().Name);

            // Ask the user to save any modified scenes before loading the new area, since loading a new area will unload the current scenes and any unsaved changes will be lost
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // If the user cancels the save operation, finish the background progress operation and mark it as canceled, then return early to avoid loading the new area
                BackgroundProgress.Finish(progressId, BackgroundProgress.Status.Canceled);

                // Return early since the user canceled the save operation and we should not load the new area
                return;
            }

            // Open the active scene using the EditorSceneManager and set it as the active scene
            EditorSceneManager.OpenScene(handle.GetActiveScene().Path, OpenSceneMode.Single);

            // Check if the active scene is valid, if so, set it as the active scene, and report progress for loading the active scene
            if (activeScene.IsValid())
            {
                // Set the active scene to the one specified in the area handle
                SceneManager.SetActiveScene(activeScene);

                // Report progress for the active scene loading
                BackgroundProgress.Report(progressId, SceneProgress(0), $"Loading Active Scene: {activeScene.name}");
            }

            // Store the method to load a scene additively
            void LoadSceneAdditive(List<Eflatun.SceneReference.SceneReference> scenes, int i, string descriptionTag)
            {
                // Get the scene data from scene in the current iteration
                var reference = scenes[i];

                // Check if the scene is already loaded, if so, skip it to avoid loading it twice
                if (reference.LoadedScene.isLoaded) return;

                // Open the scene additively using the EditorSceneManager and add the operation to the operation group
                EditorSceneManager.OpenScene(reference.Path, OpenSceneMode.Additive);

                // Report the progress as the scene is being loaded, only if the scene reference is valid to avoid reporting progress for invalid scenes which can be confusing to the user
                BackgroundProgress.Report(progressId, SceneProgress(i), descriptionTag + " " + reference.Name);
            }

            // Iterate through each scene in the active area handle
            for (var i = 0; i < handleScenesToLoad; i++) LoadSceneAdditive(handle.additiveScenes, i, "Loading Additive Scene:");

            // Iterate through each persistent scene in the WorldMap and load it if it is not already loaded, since persistent scenes should always be loaded regardless of the area handle we are loading
            for (var i = 0; i < persistentScenesToLoad; i++) LoadSceneAdditive(WorldMap.PersistentScenes, i, "Loading Persistent Scene:");

            // Report progress as complete
            BackgroundProgress.Report(progressId, 1f, "Loading Complete");

            // Delay to avoid tight loop
            await Task.Delay(100);

            // Unload unused assets if specified
            if (unloadUnusedAssets) await Resources.UnloadUnusedAssets();

            // Finish the background progress operation and mark it as succeeded
            BackgroundProgress.Finish(progressId, BackgroundProgress.Status.Succeeded);
        }
    }
}
