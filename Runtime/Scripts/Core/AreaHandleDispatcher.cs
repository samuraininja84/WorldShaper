using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Eflatun.SceneReference;

namespace WorldShaper
{
    /// <summary>
    /// Provides functionality for managing the loading and unloading of area handles, including associated scenes and
    /// resources.
    /// </summary>
    /// <remarks>
    /// The <see cref="AreaHandleDispatcher"/> class is designed to facilitate the asynchronous
    /// loading and unloading of scenes grouped under an <see cref="AreaHandle"/>. 
    /// It provides events to signal various stages of the process, such as when loading or unloading begins, when individual scenes are loaded or unloaded, and when the entire operation is complete.
    /// Key features include: 
    /// <list type="bullet">
    /// <item><description>Support for loading and unloading scenes asynchronously.</description></item>
    /// <item><description>Progress reporting for loading operations.</description></item>
    /// <item><description>Event-driven notifications for key milestones in the process.</description></item> </list>
    /// This class is particularly useful in scenarios where multiple scenes need to be managed as a cohesive unit, such as in large-scale game environments or modular applications.
    /// </remarks>
    public static class AreaHandleDispatcher
    {
        /// <summary>
        /// A reference to the WorldMap instance.
        /// </summary>
        /// <remarks>
        /// Must be assigned before using the AreaHandleDispatcher.
        /// </remarks>
        public static WorldMap WorldMapInstance;

        /// <summary>
        /// Event triggered when a scene is loaded.
        /// </summary>
        public static Action<string> OnSceneLoaded = delegate { };

        /// <summary>
        /// Event triggered when a scene is unloaded.
        /// </summary>
        public static Action<string> OnSceneUnloaded = delegate { };

        /// <summary>
        /// Event triggered when the active scene is changed.
        /// </summary>
        public static Action<string> OnActiveSceneChanged = delegate { };

        /// <summary>
        /// Event triggered when the area handle loading starts.
        /// </summary>
        public static Action OnAreaLoadingStarted = delegate { };

        /// <summary>
        /// Event triggered when the area handle is loaded.
        /// </summary>
        public static Action OnAreaLoaded = delegate { };

        /// <summary>
        /// Event triggered when the area handle unloading starts.
        /// </summary>
        public static Action OnAreaUnloadingStarted = delegate { };

        /// <summary>
        /// Event triggered when the area handle is unloaded.
        /// </summary>
        public static Action OnAreaUnloaded = delegate { };

        /// <summary>
        /// A group of handles to manage multiple async operations.
        /// </summary>
        public readonly static AsyncOperationHandleGroup handleGroup = AsyncOperationHandleGroup.OfSize(HandleCount);

        /// <summary>
        /// Represents the default number of handles to be used in operations.
        /// </summary>
        /// <remarks>
        /// This constant is used internally to define the maximum number of handles allowed. 
        /// <list type="bullet"> <item> <description>
        /// It is not intended for external use.
        /// </description> </item> </list>
        /// </remarks>
        private const int HandleCount = 10;

        /// <summary>
        /// The currently active area handle.
        /// </summary>
        private static AreaHandle ActiveAreaHandle;

        /// <summary>
        /// Asynchronously loads all scenes associated with the specified area handle, optionally reloading duplicate
        /// scenes.
        /// </summary>
        /// <remarks>
        /// This method performs the following operations: 
        /// <list type="bullet"> <item>Unloads any scenes that are not part of the specified area handle.</item> 
        /// <item>Loads all scenes defined in the area handle, either as regular or addressable scenes.</item> 
        /// <item>Reports progress through the <paramref name="progress"/> parameter, if provided.</item>
        /// <item>Sets the active scene to the one specified in the area handle after loading is complete.</item> 
        /// </list> 
        /// Events are invoked at various stages of the process:
        /// <list type="bullet"> 
        /// <item><see cref="OnAreaLoadingStarted"/> is invoked at the start of the loading process.</item> 
        /// <item><see cref="OnSceneLoaded"/> is invoked for each scene as it is loaded.</item>
        /// <item><see cref="OnActiveSceneChanged"/> is invoked when the active scene is set after loading.</item>
        /// <item><see cref="OnAreaLoaded"/> is invoked after all scenes have been successfully loaded.</item>
        /// </list>
        /// </remarks>
        /// <param name="handle">The <see cref="AreaHandle"/> representing the group of scenes to load.</param>
        /// <param name="progress">An optional progress reporter that receives updates on the loading progress, as a value between 0 and 1.</param>
        /// <param name="reloadActiveScene">A boolean value indicating whether to reload the active scene if it is already loaded as part of the active area handle. The default value is <see langword="false"/>.</param>
        /// <param name="reloadAdditiveScenes">A boolean value indicating whether to reload additive scenes that are already loaded as part of the active area handle. The default value is <see langword="false"/>.</param>
        /// <param name="unloadUnusedAssets">A boolean value indicating whether to unload unused assets after unloading scenes. The default value is <see langword="false"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task completes when all scenes are loaded.</returns>
        public static async Task LoadAreas(AreaHandle handle, IProgress<float> progress, bool reloadActiveScene = false, bool reloadAdditiveScenes = false, bool unloadUnusedAssets = false)
        {
            // Invoke the OnAreaLoadingStarted event
            OnAreaLoadingStarted.Invoke();

            // Set the active area handle to the provided group
            ActiveAreaHandle = handle;

            // Unload all scenes that are not part of the active area handle
            await UnloadAreas(reloadActiveScene, reloadAdditiveScenes, unloadUnusedAssets);

            // Get the count of currently loaded scenes
            int sceneCount = SceneManager.sceneCount;

            // Get the count of scenes to load from the active area handle
            var totalScenesToLoad = ActiveAreaHandle.GetAllScenes().Count;

            // Initialize the Operation Group with the total number of scenes to load
            var operationGroup = AsyncOperationGroup.OfSize(totalScenesToLoad);

            // Iterate through each scene in the active area handle
            for (var i = 0; i < totalScenesToLoad; i++)
            {
                // Get the scene data from scene in the current iteration
                var sceneReference = handle.GetAllScenes()[i];

                // Check if the scene is already loaded and if we should reload it, if not, skip it
                if (!reloadAdditiveScenes && sceneReference.LoadedScene.isLoaded) continue;

                // Check what type of reference state the scene has and load it accordingly
                if (sceneReference.State == SceneReferenceState.Regular)
                {
                    // Start loading the scene and add the operation to the operation group
                    var operation = SceneManager.LoadSceneAsync(sceneReference.Path, LoadSceneMode.Additive);

                    // Add the operation to the operation group
                    operationGroup.Operations.Add(operation);
                }
                else if (sceneReference.State == SceneReferenceState.Addressable)
                {
                    // Start loading the addressable scene
                    var sceneHandle = Addressables.LoadSceneAsync(sceneReference.Path, LoadSceneMode.Additive);

                    // Add the scene handle to the handle group
                    handleGroup.Handles.Add(sceneHandle);
                }

                // Invoke the OnSceneLoaded event with the scene name
                OnSceneLoaded.Invoke(sceneReference.Name);
            }

            // Wait until all AsyncOperations in the group are done
            while (!operationGroup.IsDone || !handleGroup.IsDone)
            {
                // Report the progress of the operation group and handle group
                progress?.Report((operationGroup.Progress + handleGroup.Progress) / 2);

                // Delay to avoid tight loop
                await Task.Delay(100);
            }

            // Get the active scene from the area handle by its type
            Scene activeScene = SceneManager.GetSceneByName(ActiveAreaHandle.GetActiveScene().Name);

            // Check if the active scene is valid, if so, set it as the active scene
            if (activeScene.IsValid())
            {
                // Set the active scene to the one specified in the area handle
                SceneManager.SetActiveScene(activeScene);

                // Invoke the OnActiveSceneChanged event with the active scene name
                OnActiveSceneChanged.Invoke(activeScene.name);
            }

            // Invoke the OnSceneGroupLoaded event
            OnAreaLoaded.Invoke();
        }

        /// <summary>
        /// Asynchronously unloads all non-persistent scenes and addressable scenes associated with the current area.
        /// </summary>
        /// <remarks>
        /// This method unloads all scenes except the active scene and any persistent scenes. 
        /// It also clears the handle group and unloads addressable scenes. 
        /// Events are invoked at various stages of the unloading process to signal progress: 
        /// <list type="bullet"> 
        /// <item><description><see cref="OnAreaUnloadingStarted"/> is invoked at the start of the unloading process.</description></item>
        /// <item><description><see cref="OnSceneUnloaded"/> is invoked for each scene that is successfully unloaded.</description></item> 
        /// <item><description><see cref="OnAreaUnloaded"/> is invoked after all scenes and resources have been unloaded.</description></item> 
        /// </list> 
        /// The method waits for all asynchronous operations to complete before signaling that the unloading process is finished.
        /// </remarks>
        /// <param name="reloadActiveScene">A boolean value indicating whether to reload the active scene after unloading. The default value is <see langword="false"/>.</param>
        /// <param name="reloadAdditiveScenes">A boolean value indicating whether to reload additive scenes after unloading. The default value is <see langword="false"/>.</param>
        /// <param name="unloadUnusedAssets">A boolean value indicating whether to unload unused assets after unloading scenes. The default value is <see langword="false"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task UnloadAreas(bool reloadActiveScene = false, bool reloadAdditiveScenes = false, bool unloadUnusedAssets = false)
        {
            // Invoke the OnAreaUnloadingStarted event
            OnAreaUnloadingStarted.Invoke();

            // Initialize a list to keep track of scenes to unload
            var scenes = new List<string>();

            // Get the active scene from the scene manager
            var activeScene = SceneManager.GetActiveScene().name;

            // Get the count of currently loaded scenes
            int sceneCount = SceneManager.sceneCount;

            // Loop through all loaded scenes
            for (var i = 0; i < sceneCount; i++)
            {
                // Get the scene at the current index
                var sceneAt = SceneManager.GetSceneAt(i);

                // Check if the scene is loaded
                if (!sceneAt.isLoaded) continue;

                // Get the name of the scene
                var sceneName = sceneAt.name;

                // Check if the scene is a persistent scene, if so, skip it
                if (IsPersistent(sceneName)) continue;

                // Check if the scene is part of the active area handle, if so, skip it
                if (handleGroup.Handles.Any(h => h.IsValid() && h.Result.Scene.name == sceneName)) continue;

                // If we should not reload duplicate scenes and the scene is part of the active area handle, skip it
                if (!reloadAdditiveScenes && ActiveAreaHandle.activeScene.Name == sceneName) continue;

                // Add the scene name to the list of scenes to unload
                scenes.Add(sceneName);
            }

            // If we should not reload duplicate scenes, remove the additive scenes from the list of scenes to unload
            if (!reloadAdditiveScenes) scenes.RemoveAll(s => ActiveAreaHandle.GetAllScenes().Any(h => h.Name == s));

            // Intialize the Operation Group with the count of scenes to unload
            var operationGroup = AsyncOperationGroup.OfSize(scenes.Count);

            // Loop through each scene in the scenes list
            foreach (var scene in scenes)
            {
                // Start unloading the scene and add the operation to the operation group
                var operation = SceneManager.UnloadSceneAsync(scene);

                // Check if the operation is not null, if so, continue
                if (operation == null) continue;

                // Add the operation to the operation group
                operationGroup.Operations.Add(operation);

                // Invoke the OnSceneUnloaded event with the scene name
                OnSceneUnloaded.Invoke(scene);
            }

            // Unload all addressable scenes in the handle group
            foreach (var handle in handleGroup.Handles)
            {
                // Check if the handle is valid, if so, unload the scene
                if (handle.IsValid()) Addressables.UnloadSceneAsync(handle);
            }

            // Clear the handles from the handle group
            handleGroup.Handles.Clear();

            // Wait until all AsyncOperations in the group are done, if not, delay
            while (!operationGroup.IsDone) await Task.Delay(100);

            // Invoke the OnAreaUnloaded event
            OnAreaUnloaded.Invoke();

            // Optionally unload unused assets to free up memory
            if (unloadUnusedAssets) await Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// Reloads the currently active area by unloading and then reloading it asynchronously.
        /// </summary>
        /// <remarks>If there is no active area handle, the method completes without performing any
        /// operation.</remarks>
        /// <param name="unloadUnusedAssets">Indicates whether unused assets should be unloaded during the reload process. Set to <see langword="true"/> to release unused assets; otherwise, <see langword="false"/>.</param>
        /// <returns>A task that represents the asynchronous reload operation.</returns>
        public static async Task ReloadAreas(bool unloadUnusedAssets = false)
        {
            // Check if there is an active area handle, if not, return
            if (ActiveAreaHandle == null) return;

            // Unload the current area handle
            await UnloadAreas(true, true, unloadUnusedAssets);

            // Load the active area handle again
            await LoadAreas(ActiveAreaHandle, null, true, true, unloadUnusedAssets);
        }

        /// <summary>
        /// Determines whether any area or scene is currently in the process of loading.
        /// </summary>
        /// <remarks>This method can be used to check if loading operations are still in progress before performing actions that require all areas or scenes to be fully loaded.</remarks>
        /// <returns>true if at least one area or scene is still loading; otherwise, false.</returns>
        public static bool IsAreaLoading() => handleGroup.Handles.Any(h => h.IsValid() && !h.IsDone) || SceneManager.sceneCount > 0 && SceneManager.GetSceneAt(0).isLoaded == false;

        /// <summary>
        /// Determines whether all scenes associated with the specified area handle are currently loaded.
        /// </summary>
        /// <remarks>
        /// This method checks the currently loaded scenes in the application and verifies whether they match the scenes defined in the provided <paramref name="handle"/>. 
        /// A scene is considered loaded if it is both present in the scene manager and marked as loaded.
        /// </remarks>
        /// <param name="handle">The <see cref="AreaHandle"/> representing the area whose scenes are to be checked.</param>
        /// <returns><see langword="true"/> if all scenes associated with the specified <paramref name="handle"/> are loaded; otherwise, <see langword="false"/>.</returns>
        public static bool IsAreaLoaded(AreaHandle handle)
        {
            // Get the count of currently loaded scenes
            int sceneCount = SceneManager.sceneCount;

            // Loop through all loaded scenes
            for (var i = 0; i < sceneCount; i++)
            {
                // Get the scene at the current index
                var sceneAt = SceneManager.GetSceneAt(i);

                // Check if the scene is loaded
                if (!sceneAt.isLoaded) continue;

                // Get the name of the scene
                var sceneName = sceneAt.name;

                // Check if the scene is part of the area handle, if not, return false
                if (!handle.GetAllScenes().Any(s => s.Name == sceneName)) return false;
            }

            // If all scenes are part of the area handle, return true
            return true;
        }

        /// <summary>
        /// Determines whether the specified scene name corresponds to the persistent scene.
        /// </summary>
        /// <param name="name">The name of the scene to check.</param>
        /// <returns><see langword="true"/> if the specified scene name matches the persistent scene name; otherwise, <see langword="false"/>.</returns>
        public static bool IsPersistent(string name) => WorldMapInstance.IsPersistentScene(name);
    }
}