using System;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WorldShaper
{
    /// <summary>
    /// Provides a base class for implementing the Singleton design pattern with <see cref="ScriptableObject"/> types.
    /// </summary>
    /// <remarks>
    /// This class simplifies the creation and management of singleton instances for <see cref="ScriptableObject"/> types. 
    /// <para>It ensures that only one instance of the type <typeparamref name="T"/> exists and provides mechanisms for creating, loading, and accessing the instance.</para> 
    /// <para>The singleton instance is automatically initialized when accessed via the <see cref="Instance"/> property.</para> 
    /// <para>In Unity Editor, if the singleton instance is not found in the resources folder, a new instance is created and saved as an asset.</para> 
    /// <para>This behavior is intended to streamline development workflows.</para> 
    /// <para> Use this class to manage shared state or functionality that needs to persist across scenes or application lifecycles.</para>
    /// </remarks>
    /// <typeparam name="T">The type of the singleton instance, which must inherit from <see cref="ScriptableObject"/>.</typeparam>
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableSingleton<T>
    {
        /// <summary>
        /// Holds a single instance of the type <typeparamref name="T"/> for use in singleton patterns or shared state scenarios.
        /// </summary>
        /// <remarks>This field is intended to store a single, static instance of the type <typeparamref name="T"/>.
        /// It is typically used in scenarios where a single shared instance is required, such as implementing the Singleton design pattern.
        /// </remarks>
        protected static Lazy<T> instance = null;

        /// <summary>
        /// Gets the current singleton instance of the type <typeparamref name="T"/>.
        /// </summary>
        public static T Current => instance.Value != null ? instance.Value : Instance.Value;

        /// <summary>
        /// Gets the singleton instance of the type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// This property ensures that a single instance of the type <typeparamref name="T"/> is maintained. 
        /// Accessing this property will initialize the instance if it has not already been created or loaded.
        /// </remarks>
        public static Lazy<T> Instance
        {
            get
            {
                // If the instance is null, try to load it from resources or create a new one.
                if (!Initialized) RefreshInstance();

                // Return the instance.
                return instance;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the singleton instance has been initialized.
        /// </summary>
        public static bool Initialized => instance.IsValueCreated;

        /// <summary>
        /// Checks or sets whether the singleton instance is loaded.
        /// </summary>
        public static bool IsLoaded
        {
            get => Initialized;
            set
            {
                if (value)
                {
                    // Load the instance if it is not already loaded
                    if (!IsLoaded) Load();
                }
                else
                {
                    // Unload the instance if it is loaded
                    if (IsLoaded) Unload();
                }
            }
        }

        /// <summary>
        /// Retrieves the asset name specified by the <see cref="AssetPathAttribute"/> applied to the type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// This method inspects the type <typeparamref name="T"/> for the presence of the <see cref="AssetPathAttribute"/>. 
        /// If the attribute is found, the asset name specified in the attribute is returned. 
        /// If the attribute is not found, an error is logged, and an empty string is returned.
        /// </remarks>
        /// <returns>
        /// The asset name defined in the <see cref="AssetPathAttribute"/> of type <typeparamref name="T"/>. 
        /// Returns an empty string if the attribute is not found.
        /// </returns>
        private static string GetAssetName()
        {
            // Check if the type T has the AssetPathAttribute.
            var attributes = typeof(T).GetCustomAttributes(true);

            // If the attribute is found, return the path specified in the AssetPathAttribute.
            foreach (object attribute in attributes)
            {
                // Check if the attribute is of type AssetPathAttribute and return the asset name if it is.
                if (attribute is AssetPathAttribute pathAttribute) return pathAttribute.Asset;
            }

            // If the attribute is not found, log an error and return an empty string.
            Debug.LogError($"{typeof(T)} does not have {nameof(AssetPathAttribute)}.");

            // Return the type name if no asset name is specified.
            return typeof(T).Name;
        }

        /// <summary>
        /// Retrieves the resource path specified by the <see cref="AssetPathAttribute"/> applied to the type <c>T</c>.
        /// </summary>
        /// <remarks>
        /// If the type <c>T</c> does not have the <see cref="AssetPathAttribute"/>, an error is logged, and an empty string is returned. 
        /// Ensure that the type <c>T</c> is decorated with the <see cref="AssetPathAttribute"/> to avoid this scenario. 
        /// </remarks>
        /// <returns>The resource path defined in the <see cref="AssetPathAttribute"/> of the type <c>T</c>, or an empty string if the attribute is not present.</returns>
        private static string GetResourcePath()
        {
            // Check if the type T has the AssetPathAttribute.
            var attributes = typeof(T).GetCustomAttributes(true);

            // If the attribute is found, return the path specified in the AssetPathAttribute.
            foreach (object attribute in attributes)
            {
                // Check if the attribute is of type AssetPathAttribute, and return the path if it is.
                if (attribute is AssetPathAttribute pathAttribute) return pathAttribute.Path;
            }

            // If the attribute is not found, log an error and return an empty string.
            Debug.LogWarning($"{typeof(T)} does not have {nameof(AssetPathAttribute)}.");

            // Return the default path "Assets/Resources" if no path is specified.
            return "Assets/Resources";
        }

        /// <summary>
        /// Clears the current instance of the singleton, setting it to <see langword="null"/>.
        /// </summary>
        /// <remarks>This method can be used to reset the singleton instance, allowing it to be
        /// reinitialized the next time it is accessed. Use with caution, as clearing the instance may affect other
        /// parts of the application relying on the singleton.</remarks>
        public static void RefreshInstance()
        {
            // If the instance has not been created yet, no action is needed.
            if (!Initialized)
            {
                // Load the instance.
                instance = Load();

                // Return early.
                return;
            }

            // Destroy the existing instance if it has been created.
            instance.Value.Destroy();

            // Get a new instance.
            instance = Load();
        }

        /// <summary>
        /// Creates or loads a lazy-initialized instance of the specified ScriptableObject type.
        /// </summary>
        /// <remarks>
        /// This method attempts to locate an existing instance of the ScriptableObject in the Unity Resources folder or the AssetDatabase (in the Unity Editor). 
        /// If no instance is found, a new instance may be created and saved as an asset, depending on the environment and configuration. 
        /// The method ensures that only one instance is returned, even if multiple instances are found, by selecting the first instance sorted by name.
        /// </remarks>
        /// <returns>A <see cref="Lazy{T}"/> containing the ScriptableObject instance. Returns <see langword="null"/> if no instance could be found or created.</returns>
        internal static Lazy<T> Load()
        {
            // Return a lazy-initialized instance of the ScriptableObject.
            return new Lazy<T>(() =>
            {
                // Get the type of the ScriptableObject.
                Type type = typeof(T);

                // Get the asset name for the instance using the custom attribute.
                string assetName = GetAssetName();

                // Get the resource path for the instance using the custom attribute.
                string path = GetResourcePath();

                // If the file path is not empty, construct the full path to the asset.
                string fullPath = $"{path}/{assetName}";

                // Try loading a named instance based on the asset name.
                T selected = Resources.Load<T>(fullPath);

                // If a named instance was found, return it.
                if (selected != null) return selected;
                else
                {
                    // Load all instances of the ScriptableObject type from the Resources folder.
                    T[] instances = Resources.LoadAll<T>("");

                    // Check if any instances were found.
                    if (instances == null || instances.Length < 1)
                    {
                        // No instances found, throw an exception.
                        throw new Exception($"Could not find any scriptable object<{assetName}> instances in the resources");
                    }
                    else if (instances.Length > 1)
                    {
                        // Warn about multiple instances found.
                        Debug.LogWarning($"Multiple Instances of the scriptable object<{assetName}> found in the resources. The first one by name will be used, others will be unloaded");

                        // Sort instances by name to ensure consistent selection.
                        Array.Sort(instances, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
                    }

                    // Return the first instance found.
                    selected = instances[0];

                    // Unload unused assets
                    for (int i = 1; i < instances.Length; i++) Resources.UnloadAsset(instances[i]);

                    // If a named instance was found, return it.
                    if (selected != null) return selected;
#if UNITY_EDITOR
                    // If no instances were found, create a new instance in the editor.
                    if (instances == null || instances.Length == 0)
                    {
                        // Create a new instance
                        T newInstance = CreateInstance<T>();

                        // Ensure the target directory exists
                        string directory = System.IO.Path.GetDirectoryName(fullPath);

                        // Create directory if it doesn't exist
                        if (!System.IO.Directory.Exists(directory)) System.IO.Directory.CreateDirectory(directory);

                        // Create the asset at the candidate path
                        AssetDatabase.CreateAsset(newInstance, fullPath);

                        // Refresh the AssetDatabase to recognize the new asset
                        AssetDatabase.Refresh();

                        // Save the AssetDatabase to persist changes
                        AssetDatabase.SaveAssets();

                        // Return the newly created instance
                        selected = newInstance;
                    }

#endif
                    // Return the first instance after sorting by name.
                    return selected;
                }
            });
        }

        /// <summary>
        /// Unloads the singleton instance from memory and clears the reference.
        /// </summary>
        public static void Unload()
        {
            // If the instance is loaded, unload it.
            if (instance.IsValueCreated)
            {
                // Unload the underlying asset from memory.
                Resources.UnloadAsset(instance.Value);

                // Clear the instance reference.
                instance.Value.Destroy();

                // Unload unused assets to free up memory.
                Resources.UnloadUnusedAssets();
            }
        }

        /// <summary>
        /// Saves the singleton asset in the Unity Editor and optionally unloads it from memory.
        /// </summary>
        /// <param name="shouldUnload">True to unload the asset after saving; false to keep it loaded. Default is true.</param>
        public static void SaveAssetInEditor(bool shouldUnload = true)
        {
#if UNITY_EDITOR
            if (IsLoaded)
            {
                EditorUtility.SetDirty(instance.Value);
                AssetDatabase.SaveAssets();

                // Optionally unload the asset after saving to free up memory.
                if (shouldUnload) Unload();
            }
#endif
        }
    }

    /// <summary>
    /// Convenience helpers for Unity object lifetimes.
    /// </summary>
    public static class LifetimeHelpers
    {
        /// <summary>
        /// Destroys a Unity object using SmartDestroy (DestroyImmediate in edit mode, Destroy in play mode).
        /// </summary>
        /// <param name="afterTime">Optional delay in seconds for runtime Destroy.</param>
        public static void Destroy<T>(this T source, float? afterTime = null) where T : Object => source.SmartDestroy(afterTime);

        /// <summary>
        /// Destroys an object using DestroyImmediate in editor edit mode, otherwise Destroy (optionally delayed).
        /// Avoids deleting assets on disk; unloads asset objects instead.
        /// </summary>
        public static void SmartDestroy(this Object obj, float? afterTime = null)
        {
            // If the object is null, throw an exception.
            if (obj == null) throw new ArgumentNullException(nameof(obj), "Object to destroy cannot be null.");

#if UNITY_EDITOR
            // Editor mode destroy.
            if (Application.isEditor && !Application.isPlaying)
            {
                // If this is an asset object, unload it so a fresh instance can be loaded next time.
                string assetPath = AssetDatabase.GetAssetPath(obj);

                // If the asset path is not empty, it is an asset object.
                if (!string.IsNullOrEmpty(assetPath))
                {
                    // Unload the asset instead of destroying it.
                    Resources.UnloadAsset(obj);

                    // Return early to avoid runtime Destroy.
                    return;
                }

                // Editor immediate destroy.
                Object.DestroyImmediate(obj);

                // Return early to avoid runtime Destroy.
                return;
            }
#endif

            // Runtime destroy based on delay.
            if (afterTime.HasValue)
            {
                // Runtime delayed destroy.
                Object.Destroy(obj, afterTime.Value);
            }
            else
            {
                // Runtime immediate destroy.
                Object.Destroy(obj);
            }
        }
    }
}