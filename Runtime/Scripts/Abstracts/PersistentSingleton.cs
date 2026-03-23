using UnityEngine;

namespace WorldShaper
{
    /// <summary>
    /// Provides a base class for creating persistent singleton components in Unity.
    /// </summary>
    /// <remarks>
    /// This class ensures that only one instance of the specified type <typeparamref name="T"/> exists in the scene. 
    /// <para>The instance persists across scene loads and can be accessed globally via the <see cref="Current"/> property.</para>
    /// <para>If no instance exists, one will be automatically created.</para>
    /// <para>If multiple instances are found, duplicates will be destroyed.</para>
    /// </remarks>
    public abstract class PersistentSingleton<T> : MonoBehaviour where T : PersistentSingleton<T>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the singleton should automatically detach from its parent in the hierarchy when the object awakens.
        /// </summary>
        /// <remarks>
        /// This property is typically used to ensure that the singleton remains independent in the scene hierarchy. 
        /// If set to <see langword="true"/>, the object will detach from its parent during the <c>Awake</c> lifecycle event.
        /// </remarks>
        [Header("Persistent Singleton")]
        [Tooltip("If this is true, this singleton will auto detach if it finds itself parented on awake")]
        public bool UnparentOnAwake = true;

        /// <summary>
        /// Represents the singleton instance of the type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// This static field is intended to store the single instance of the type <typeparamref name="T"/> in a singleton pattern. 
        /// Access to this field should be managed carefully to ensure thread safety and proper initialization.
        /// </remarks>
        protected static T instance = null;

        /// <summary>
        /// Gets the current instance of the singleton type <typeparamref name="T"/>.
        /// </summary>
        public static T Current => HasInstance ? instance : Instance;

        /// <summary>
        /// Gets a value indicating whether an instance of the object exists.
        /// </summary>
        public static bool HasInstance => instance != null;

        /// <summary>
        /// Checks or sets whether the singleton instance is loaded.
        /// </summary>
        public static bool IsLoaded
        {
            get => HasInstance;
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
        /// Gets the singleton instance of the type <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>The instance is ensured to persist across scene loads by marking its associated GameObject as non-destructible.</remarks>
        public static T Instance
        {
            get
            {
                // If the instance is null, ensure it exists
                if (instance == null) Load();

                // Ensure the instance is not destroyed on load
                DontDestroyOnLoad(instance.transform.gameObject);

                // Return the instance
                return instance;
            }
        }

        protected virtual void Awake() => Initialize();

        /// <summary>
        /// Initializes the singleton instance of the class, ensuring only one instance exists and persists across
        /// scenes.
        /// </summary>
        /// <remarks>
        /// This method should be called during the object's initialization phase. 
        /// If the singleton instance is already set, any additional instances will be destroyed. 
        /// If the singleton instance is not set, this instance will be assigned as the singleton and marked to persist across scene loads.
        /// Additionally, if <see cref="UnparentOnAwake"/> is <see langword="true"/>, the object will be unparented during initialization.
        /// </remarks>
        protected virtual void Initialize()
        {
            // If not playing, do not initialize
            if (!Application.isPlaying) return;

            // If the singleton is parented, unparent it
            if (UnparentOnAwake) transform.SetParent(null);

            // If the instance is null, set this as the instance and dont destroy it on load, otherwise destroy this game object
            if (instance == null)
            {
                // Set this instance as the singleton instance
                instance = this as T;

                // Ensure the instance is not destroyed on load
                DontDestroyOnLoad(transform.gameObject);

                // Enable the component
                enabled = true;
            }
            else
            {
                // If this instance is not the current instance, destroy this game object
                if (this != instance) Destroy(this.gameObject);
            }
        }

        /// <summary>
        /// Finds the first existing instance of the specified type in the scene or creates a new one if none exists.
        /// </summary>
        /// <remarks>
        /// If an instance of the specified type is not found in the scene, a new GameObject is created, and the component of the specified type is added to it. 
        /// The new GameObject is automatically named using the type name with the suffix " (Auto Created)".
        /// </remarks>
        protected static void Load()
        {
            // Try to find an existing instance in the scene
            instance = FindFirstObjectByType<T>();

            // If no instance was found, create a new one
            if (instance == null)
            {
                // Create a new GameObject
                GameObject obj = new GameObject();

                // Name the GameObject based on the type name and indicate it was auto-created with a suffix
                obj.name = $"{typeof(T).Name} (Auto Created)";

                // Add the component of type T to the new GameObject and assign it to the instance
                instance = obj.AddComponent<T>();
            }
        }

        /// <summary>
        /// Unloads the singleton instance of the class.
        /// </summary>
        protected static void Unload()
        {
            // If an instance exists, teardown and clear it
            if (IsLoaded)
            {
                // Call the teardown hook
                instance.OnTeardown();

                // Clear the instance reference
                instance = null;
            }
        }

        protected virtual void OnInit() { }

        protected virtual void OnTeardown() { }

        protected virtual void OnDestroy()
        {
            // Check if the instance being destroyed is the current singleton instance before unloading
            if (instance != this) return;

            // Unload the singleton instance when this object is destroyed
            Unload();
        }
    }
}