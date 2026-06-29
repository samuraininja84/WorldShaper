using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Eflatun.SceneReference;

namespace WorldShaper
{
    /// <summary>
    /// Represents a map for managing <see cref="AreaHandle"/> instances in a project.
    /// </summary>
    /// <remarks>The <see cref="WorldMap"/> class provides functionality to register, deregister, and
    /// manage areas represented by <see cref="AreaHandle"/> objects. 
    /// </remarks>
    [CreateAssetMenu(fileName = "New World Map", menuName = "World Shaper/New World Map")]
    public class WorldMap : ScriptableObject
    {
        [Header("World Info")]
        public string worldName = "New World";

        [Header("Transition Settings")]
        public WorldTransitionConfiguration config;

        [Header("Areas")]
        public List<AreaHandle> persistentAreas = new();
        public List<AreaHandle> registeredAreas = new();

        /// <summary>
        /// A list of persistent scenes associated with the persistent areas.
        /// </summary>
        /// <remarks>Used so that we don't have to iterate through the persistent areas every time we need to get the persistent scenes.</remarks>
        private List<SceneReference> persistentScenes = new();

        /// <summary>
        /// Gets the list of persistent scenes associated with the persistent areas.
        /// </summary>
        public List<SceneReference> PersistentScenes
        {
            // Try to get the persistent scenes from the persistent areas
            get
            {
                // Get the persistent scenes from the persistent areas
                if (persistentAreas.Count != 0 && persistentScenes.Count == 0) persistentScenes = persistentAreas.Select(area => area.activeScene).ToList();

                // Return the persistent scenes
                return persistentScenes;
            }
        }

        /// <summary>
        /// The singleton instance of the <see cref="WorldMap"/> class.
        /// </summary>
        protected static WorldMap instance;

        /// <summary>
        /// The singleton instance of the <see cref="WorldMap"/>. 
        /// </summary>
        /// <remarks>
        /// This property provides access to the single instance of the <see cref="WorldMap"/> class, ensuring that only one instance exists throughout the application. 
        /// If an instance does not already exist, it attempts to find and load one from the project's assets. 
        /// If no instance is found, it will return null until an instance is created or assigned.
        /// </remarks>
        public static WorldMap Instance
        {
            get
            {
#if UNITY_EDITOR
                // Only look for an instance if there isn't one already assigned, to avoid unnecessary searches
                if (!HasInstance)
                {
                    // Search the project for assets of type WorldMap and get their GUIDs
                    var guids = UnityEditor.AssetDatabase.FindAssets("t:" + nameof(WorldMap));

                    // If at least one WorldMap asset is found, load the first one
                    if (guids.Length > 0) instance = (WorldMap)UnityEditor.AssetDatabase.LoadMainAssetAtPath(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
                }
#endif
                // Return the instance
                return instance;
            }
        }

        /// <summary>
        /// A boolean property that indicates whether an instance of the <see cref="WorldMap"/> exists.
        /// </summary>
        public static bool HasInstance => instance != null;

        /// <summary>
        /// Gets the name of the world, which is either the specified world name or the name of the scriptable object if the world name is not set.
        /// </summary>
        public string Name => worldName != string.Empty ? worldName : name;

        /// <summary>
        /// Gets the number of registered areas.
        /// </summary>
        public int Count => registeredAreas.Count;

        /// <summary>
        /// True if the map is empty, false otherwise.
        /// </summary>
        public bool Empty => registeredAreas.Count == 0;

        /// <summary>
        /// Gets the <see cref="AreaHandle"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the <see cref="AreaHandle"/> to retrieve.</param>
        /// <returns>The <see cref="AreaHandle"/> at the specified index.</returns>
        public AreaHandle this[int index] => GetArea(index);

        private void OnEnable()
        {
            // If there is no existing instance, assign this instance to the static instance variable.
            if (!HasInstance) instance = this;
        }

        #region Connection Methods

        public Connection GetConnection(SerializableGuid id) => registeredAreas.Find(area => area.ConnectionExists(id)).GetConnection(id);

        public Connection GetConnection(string name) => registeredAreas.Find(area => area.ConnectionExists(name)).GetConnection(name);

        public bool TryGetConnection(SerializableGuid id, out Connection connection)
        {
            connection = GetConnection(id);
            return connection != null;
        }

        public bool TryGetConnection(string name, out Connection connection)
        {
            connection = GetConnection(name);
            return connection != null;
        }

        #endregion

        #region Area Methods

        /// <summary>
        /// Creates a lookup dictionary that maps each registered area to its list of connections.
        /// </summary>
        /// <returns>A dictionary where the keys are <see cref="AreaHandle"/> instances and the values are lists of <see cref="Connection"/> objects associated with each area.</returns>
        public Dictionary<AreaHandle, List<Connection>> CreateWorldLookup()
        {
            var worldLookup = new Dictionary<AreaHandle, List<Connection>>();
            foreach (var area in registeredAreas) worldLookup.Add(area,  area.connections);
            return worldLookup;
        }

        /// <summary>
        /// Retrieves all registered areas.
        /// </summary>
        /// <returns>
        /// A list of all registered AreaHandle instances.
        /// </returns>
        public List<AreaHandle> RetrieveAll() => registeredAreas;

        /// <summary>
        /// Attempts to retrieve an area associated with a specific connection. Returns true if found, false otherwise.
        /// </summary>
        /// <param name="connection">The connection to search for.</param>
        /// <param name="area">When this method returns, contains the AreaHandle associated with the specified connection, if found; otherwise, null.</param>
        /// <returns>True if the area is found; otherwise, false.</returns>
        public bool TryGetArea(Connection connection, out AreaHandle area)
        {
            area = GetArea(connection);
            return area != null;
        }

        /// <summary>
        /// Attempts to retrieve an area by its name. Returns true if found, false otherwise.
        /// </summary>
        /// <param name="name">The name of the area to retrieve.</param>
        /// <param name="area">When this method returns, contains the AreaHandle associated with the specified name, if found; otherwise, null.</param>
        /// <returns>True if the area is found; otherwise, false.</returns>
        public bool TryGetArea(string name, out AreaHandle area)
        {
            area = GetArea(name);
            return area != null;
        }

        /// <summary>
        /// Attempts to retrieve an area by its index. Returns true if found, false otherwise.
        /// </summary>
        /// <param name="index">The index of the area to retrieve.</param>
        /// <param name="area">When this method returns, contains the AreaHandle associated with the specified index, if found; otherwise, null.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range of the registered areas list.</exception>"
        /// <returns>True if the area is found; otherwise, false.</returns>
        public bool TryGetArea(int index, out AreaHandle area)
        {
            if (index < 0 || index >= registeredAreas.Count) throw new ArgumentOutOfRangeException($"The index {index} is out of range of the registered areas list.");
            area = GetArea(index);
            return area != null;
        }

        /// <summary>
        /// Gets the area associated with a specific connection.
        /// </summary>
        /// <param name="connection"> The connection to search for.</param>
        /// <returns>An AreaHandle which contains the connection, or null if not found.</returns>
        public AreaHandle GetArea(Connection connection) => registeredAreas.Find(area => area.connections.Contains(connection));

        /// <summary>
        /// Retrieves an area by the name of its current scene.
        /// </summary>
        /// <param name="name"></param>
        /// <returns> An AreaHandle instance if found, otherwise null. </returns>
        public AreaHandle GetArea(string name) => registeredAreas.Find(area => area.IsValid && area.activeScene.Name == name);

        /// <summary>
        /// Retrieves an area by its index in the map.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// An AreaHandle instance if found, otherwise null.
        /// </returns>
        public AreaHandle GetArea(int index) => registeredAreas[index];

        /// <summary>
        /// Checks if the map contains a specific area.
        /// </summary>
        /// <param name="area">The AreaHandle to check for.</param>
        /// <returns>True if the area is registered, otherwise false.</returns>
        public bool Contains(AreaHandle area) => registeredAreas.Contains(area);

        /// <summary>
        /// Checks if the map contains an area with a specific scene name.
        /// </summary>
        /// <param name="sceneName">The name of the scene to check for.</param>
        /// <returns>True if the area is registered, otherwise false.</returns>
        public bool Contains(string sceneName) => registeredAreas.Exists(area => area.IsValid && area.activeScene.Name == sceneName);

        /// <summary>
        /// Determines whether the specified scene is a persistent scene.
        /// </summary>
        /// <param name="sceneName">The name of the scene to check.</param>
        /// <returns><see langword="true"/> if the specified scene is a persistent scene; otherwise, <see langword="false"/>.</returns>
        public bool IsPersistentScene(string sceneName) => PersistentScenes.Exists(scene => scene.Name == sceneName);

        #endregion

        #region Registration Methods

        /// <summary>
        /// Searches the project for all AreaHandle instances and registers them.
        /// </summary>
        [ContextMenu("Register All")]
        public void SearchProjectForHandles()
        {
            // Clear the current map to avoid duplicates.
            Clear();

            // Find all AreaHandle instances in the project.
            List<AreaHandle> handles = Resources.FindObjectsOfTypeAll<AreaHandle>().ToList();

            // If no handles are found, log a message and return.
            if (handles.Count == 0)
            {
                // Log a warning message.
                Debug.LogWarning("No Area Handles found in the project's resources.");

                // Return early if no handles are found.
                return;
            }

            // Register each found handle.
            foreach (AreaHandle handle in handles)
            {
                // If the area is already registered, do not add it again.
                if (registeredAreas.Contains(handle) || persistentAreas.Contains(handle))
                {
                    // Log a warning message.
                    Debug.LogWarning($"Area '{handle.name}' is already registered in the World Map. Skipping duplicate registration.");

                    // Return early without registering the area.
                    return;
                }

                // If the area is null, log an error and do not register.
                if (handle.activeScene == null)
                {
                    // Log an error message.
                    Debug.LogError($"Area '{handle.name}' has a null active scene. Cannot register.");

                    // Return early without registering the area.
                    return;
                }

                // If the area is invalid, log an error and do not register.
                if (!handle.IsValid)
                {
                    // Log an error message.
                    Debug.LogError($"Area '{handle.name}' is invalid. Cannot register.");

                    // Skip registering the invalid area.
                    continue;
                }

                // If the area is a persistent area, add it to the persistent areas list.
                if (handle.Persistent() && !persistentAreas.Contains(handle))
                {
                    // Add the persistent area to the persistent areas list.
                    persistentAreas.Add(handle);
                }
                else if ((handle.Normal() || handle.Impassable()) && !registeredAreas.Contains(handle))
                {
                    // Add the registered area list.
                    registeredAreas.Add(handle);
                }
            }

            // Sort the registered areas after adding new handles.
            Sort();

            // Log the number of handles found and registered.
            Debug.Log($"Found and registered: {handles.Count} Area Handles from the project's resources.");
        }

        /// <summary>
        /// Sorts the registered areas by their current scene names.
        /// </summary>
        [ContextMenu("Sort Areas")]
        public void Sort()
        {
            // Sort the persistent areas by their scene names.
            persistentAreas.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));

            // Sort the registered areas by their scene names.
            registeredAreas.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Clears the map of all registered areas.
        /// </summary>
        [ContextMenu("Clear Areas")]
        public void Clear()
        {
            // Clear the persistent areas list.
            persistentAreas.Clear();

            // Clear the persistent scenes list.
            persistentScenes.Clear();

            // Clear the registered areas list.
            registeredAreas.Clear();
        }

        #endregion
    }
}