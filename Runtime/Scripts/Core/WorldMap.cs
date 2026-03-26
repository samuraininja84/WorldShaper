using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

        [Header("Connection Status")]
        public AreaHandle currentArea;
        public InterfaceReference<ILocationPointer> currentLocation;
        public ConnectionState connection = ConnectionState.Empty;
        public Progress transitionProgress = Progress.Empty;

        [Header("Areas")]
        public List<AreaHandle> persistentAreas = new();
        public List<AreaHandle> registeredAreas = new();
        private List<SceneReference> persistentScenes = new();

        [Header("Locations")]
        public List<InterfaceReference<ILocationPointer>> locations;

        /// <summary>
        /// Action invoked when a transition is started.
        /// </summary>
        public static Action OnTransitionStarted = delegate { };

        /// <summary>
        /// Action invoked when a transition is completed.
        /// </summary>
        public static Action OnTransitionCompleted = delegate { };

        /// <summary>
        /// Action invoked when the start point changes.
        /// </summary>
        public static Action<string> OnStartPointChanged = delegate { };

        /// <summary>
        /// Action invoked when the start point changes.
        /// </summary>
        public static Action<string> OnEndPointChanged = delegate { };

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
        /// Gets the name of the world, which is either the specified world name or the name of the scriptable object if the world name is not set.
        /// </summary>
        public string Name => worldName != string.Empty ? worldName : name;

        /// <summary>
        /// The name of the start point for the current connection.
        /// </summary>
        public string StartPoint { get => connection.startPoint; set => connection.SetStart(value); }

        /// <summary>
        /// The name of the end point for the current connection.
        /// </summary>
        public string EndPoint { get => connection.endPoint; set => connection.SetEnd(value); }

        #region Area Methods

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
        /// Retrieves all registered areas.
        /// </summary>
        /// <returns>
        /// A list of all registered AreaHandle instances.
        /// </returns>
        public List<AreaHandle> RetrieveAll() => registeredAreas;

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

        #region Connectable Methods

        /// <summary>
        /// Registers an <see cref="ILocationPointer"/> instance for tracking and management.
        /// </summary>
        /// <remarks>If the specified <paramref name="location"/> is already registered, it will not be added again.</remarks>
        /// <param name="location">The <see cref="ILocationPointer"/> instance to register. Cannot be null.</param>
        public void Register(ILocationPointer location) => locations.Add(new InterfaceReference<ILocationPointer>(location));

        /// <summary>
        /// Removes the specified location object from the collection of registered objects.
        /// </summary>
        /// <remarks>This method removes all references to the specified object from the collection. If the object is not found, no action is taken.</remarks>
        /// <param name="location">The location object to deregister. Cannot be <see langword="null"/>.</param>
        public void Unregister(ILocationPointer location) => locations.RemoveAll(reference => reference.Value == location);

        /// <summary>
        /// Tries to retrieve a location object by its endpoint.
        /// </summary>
        /// <param name="endPoint">The endpoint string to search for.</param>
        /// <param name="location">The output parameter that will hold the found location object if successful; otherwise, null.</param>
        /// <returns>A boolean value indicating whether a matching location object was found.</returns>
        private bool TryGetLocation(string endPoint, out ILocationPointer location)
        {
            // Try to get the location from the collection, if successful set the out parameter and return true
            if (locations.TryGetLocation(endPoint, out ILocationPointer reference))
            {
                // Get the location from the reference
                location = reference;

                // Return true if a matching location is found
                return true;
            }

            // Set the out parameter to null if no matching location is found
            location = default;

            // Returns false if no matching location is found
            return false;
        }

        /// <summary>
        /// Retrieves all location objects in the scene and returns them as a list of interface references.
        /// </summary>
        /// <returns>A list of <see cref="InterfaceReference{IConnectable}"/> objects representing all location objects found
        /// in the scene. The list will be empty if no location objects are present.</returns>
        private List<InterfaceReference<ILocationPointer>> GetAllConnectables() => ILocationPointerExtensions.GetLocationPointers();

        #endregion

        #region Transition Methods

        /// <summary>
        /// Configures passage data by setting the current area, start point, and optional end point.
        /// </summary>
        /// <remarks>This method updates the current area and passage points based on the provided
        /// parameters. The <paramref name="endPoint"/> parameter is optional; if not provided, it defaults to an empty
        /// string.</remarks>
        /// <param name="areaHandle">The handle representing the area to be set as the current area.</param>
        /// <param name="startPoint">The name of the start point for the passage. Cannot be null.</param>
        /// <param name="endPoint">The name of the end point for the passage. If null, the end point will be set to an empty string.</param>
        private void ConfigurePassageData(AreaHandle areaHandle, string startPoint, string endPoint)
        {
            // Set the current area to the area handle
            currentArea = areaHandle;

            // Set the start point name to the passage name
            StartPoint = startPoint;

            // If the end passage name is null, set the end point to an empty string
            EndPoint = endPoint ?? string.Empty;

            // Invoke the OnStartPointChanged action to signal the start point has changed
            OnStartPointChanged.Invoke(StartPoint);

            // Invoke the OnEndPointChanged action to signal the end point has changed
            OnEndPointChanged.Invoke(EndPoint);
        }

        /// <summary>
        /// Switches the current context to the specified area based on the provided connection.
        /// </summary>
        /// <remarks>This method retrieves the destination area from the provided connection, configures the passage data using the connection details, and returns the corresponding area handle.</remarks>
        /// <param name="connection">The connection object containing the destination area, connection name, and endpoint information.</param>
        /// <returns>An <see cref="AreaHandle"/> representing the destination area specified in the connection.</returns>
        public AreaHandle SwitchToDestination(Connection connection) => SwitchToDestination(connection.destinationArea, connection.connectionName, connection.Endpoint);

        /// <summary>
        /// Switches the current context to the specified area and configures the passage data using the provided start and end points.
        /// </summary>
        /// <remarks>This method configures the passage data for the specified area using the provided start and end points. The caller is responsible for ensuring that the area handle and passage points are valid.</remarks>
        /// <param name="areaHandle">The handle representing the area to switch to.</param>
        /// <param name="startPoint">The starting point of the passage within the area. Cannot be null or empty.</param>
        /// <param name="endPoint">The ending point of the passage within the area. Cannot be null or empty.</param>
        /// <returns>The <see cref="AreaHandle"/> representing the area that was switched to.</returns>
        public AreaHandle SwitchToDestination(AreaHandle areaHandle, string startPoint, string endPoint)
        {
            // Configure the passage data with the area handle and passage name
            ConfigurePassageData(areaHandle, startPoint, endPoint);

            // Return the area handle
            return areaHandle;
        }

        /// <summary>
        /// Switches to the specified area and retrieves the associated connection by name.
        /// </summary>
        /// <remarks>This method retrieves the connection associated with the specified <paramref name="connectionName"/>  from the provided <paramref name="areaHandle"/>. The returned <see cref="AreaHandle"/> remains unchanged.</remarks>
        /// <param name="areaHandle">The handle representing the area to switch to.</param>
        /// <param name="connectionName">The name of the connection to retrieve within the specified area. Cannot be null or empty.</param>
        /// <returns>The same <see cref="AreaHandle"/> instance provided as input.</returns>
        public AreaHandle SwitchToDestination(AreaHandle areaHandle, string connectionName)
        {
            // Get the connection from the area handle by name
            Connection connection = areaHandle.GetConnection(connectionName);

            // Return the area handle
            return SwitchToDestination(areaHandle, connection.connectionName, connection.Endpoint);
        }

        /// <summary>
        /// Switches to the specified area based on the provided connection.
        /// </summary>
        /// <param name="connection">The connection object containing the destination area and connection name.</param>
        /// <returns>An <see cref="AreaHandle"/> representing the area that was switched to.</returns>
        public AreaHandle SwitchToArea(Connection connection) => SwitchToArea(GetArea(connection), connection.connectionName);

        /// <summary>
        /// Switches to the specified area and prepares the transition using the given connection index.
        /// </summary>
        /// <remarks>This method initializes the transition to the specified area by configuring the passage data based on the selected connection. The caller is responsible for ensuring that the area name and connection index are valid.</remarks>
        /// <param name="areaHandle">The handle of the area to switch to. This must match the handle of an existing area.</param>
        /// <param name="connectionIndex">The index of the connection within the area to use for the transition. Must be a valid index within the area's connections.</param>
        /// <returns>An <see cref="AreaHandle"/> representing the area that was switched to.</returns>
        public AreaHandle SwitchToArea(AreaHandle areaHandle, int connectionIndex) => SwitchToArea(areaHandle, areaHandle.GetConnection(connectionIndex).connectionName);

        /// <summary>
        /// Switches to the specified area and prepares the passage data for the transition.
        /// </summary>
        /// <remarks>This method retrieves the specified area and connection, configures the passage data
        /// for the transition, and returns the handle to the area. Ensure that the provided indices are valid to avoid unexpected behavior.</remarks>
        /// <param name="areaHandle">The handle of the area to switch to. Must correspond to a valid area.</param>
        /// <param name="connectionName">The name of the connection within the specified area. Must correspond to a valid connection in the area.</param>
        /// <returns>An <see cref="AreaHandle"/> representing the area that was switched to.</returns>
        public AreaHandle SwitchToArea(AreaHandle areaHandle, string connectionName)
        {
            // Set the passage data to prepare for the transition
            ConfigurePassageData(areaHandle, string.Empty, connectionName);

            // Return the area handle
            return areaHandle;
        }

        /// <summary>
        /// Reloads the current area, optionally applying transitions and reloading duplicate objects.
        /// </summary>
        /// <remarks>
        /// This method determines the current area based on the active scene and reloads it. 
        /// If the area has connections, it sets the start and end points for the transition based on the area's connections. 
        /// The method supports asynchronous execution and can apply optional transitions during the reload process.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task ReloadArea()
        {
            // Get the area name from the area handle
            string areaName = SceneManager.GetActiveScene().name;

            // If the current area is null, get the current area from the scene name and set it to the current area
            if (currentArea == null || currentArea.activeScene.Name != areaName) currentArea = GetArea(areaName);

            // Get the end passage name from the current area connections as the first connection
            if (EndPoint == string.Empty && currentArea.HasConnections())
            {
                // Set the start passage name to the first connection endpoint
                StartPoint = currentArea.First.Endpoint;

                // Set the end passage name to the first connection name
                EndPoint = currentArea.First.connectionName;

                // Invoke the OnEndPointChanged action to signal the end point has changed
                OnEndPointChanged.Invoke(EndPoint);
            }
            else if (EndPoint != string.Empty && currentArea.ConnectionExists(EndPoint))
            {
                // Get the start passage name from the current area connections based on the end passage name
                StartPoint = currentArea.GetConnection(EndPoint).Endpoint;
            }

            // Invoke the OnStartPointChanged action to signal the start point has changed
            OnStartPointChanged.Invoke(StartPoint);

            // Start the area transition with the area name and default transition name
            await ExecuteTransition(currentArea);
        }

        #endregion

        #region Execution Methods

        /// <summary>
        /// Executes a transition between areas, including optional animations and a delay.
        /// </summary>
        /// <remarks>
        /// This method performs the following steps in sequence: 
        /// <param name="handle">The <see cref="AreaHandle"/> representing the area to be loaded.</param>
        /// <paramref name="reloadActiveScene"/>>Indicates whether to reload the active scene during the transition. Defaults to <see langword="false"/>.</paramref>
        /// <param name="reloadAdditiveScenes">A value indicating whether to reload additive scenes during the transition. Defaults to <see langword="false"/>.</param>
        /// <param name="unloadUnusedAssets">A value indicating whether to unload unused assets after the transition. Defaults to <see langword="false"/>.</param>
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task ExecuteTransition(AreaHandle handle, bool reloadActiveScene = false, bool reloadAdditiveScenes = false, bool unloadUnusedAssets = false)
        {
            // Invoke the OnTransitionStarted action to signal the start of the transition
            OnTransitionStarted.Invoke();

            // Set the WorldMap instance in the AreaHandleDispatcher
            AreaHandleDispatcher.WorldMapInstance = this;

            // Load the new area using the AreaHandleDispatcher
            await AreaHandleDispatcher.LoadAreas(handle, transitionProgress, reloadActiveScene, reloadAdditiveScenes, unloadUnusedAssets);

            // Initialize locations for the loaded area
            await Intialize(handle);

            // Invoke the OnActivate method to handle activation logic
            await OnActivate(handle);

            // Invoke the OnTransitionCompleted action to signal the completion of the transition
            OnTransitionCompleted.Invoke();
        }

        /// <summary>
        /// Initializes all location objects in the specified area and prepares them for interaction.
        /// </summary>
        /// <remarks>This method retrieves all location objects in the scene associated with the specified area handle and initializes them asynchronously. 
        /// If the area handle has no connections, the method logs a debug message and exits without performing any initialization. 
        /// The connection matching the end point is disabled for interaction during this process.
        /// </remarks>
        /// <param name="handle">The <see cref="AreaHandle"/> representing the area to initialize. Must contain valid connections.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task Intialize(AreaHandle handle)
        {
            // If the area handle for the loaded scene has no connections, log a debug message and return, ignoring the scene
            if (!handle.HasConnections())
            {
                // Log a debug message if debugging is enabled
                Debug.Log("Area Handle has no connections, ignoring scene for player relocation.");

                // Return early if the scene is ignored
                return;
            }

            // Set the current area to the loaded handle
            currentArea = handle;

            // Clear the existing locations list
            locations.Clear();

            // Get all locations in the scene if not already set
            locations = GetAllConnectables();

            // Convert the locations to a list of initialization tasks
            var operations = locations.Select(connectable => connectable.Value.Initialize());

            // Await the completion of all initialization tasks
            await operations.Combine();
        }

        /// <summary>
        /// Activates the connection associated with the specified endpoint, disabling interaction during the activation
        /// process.
        /// </summary>
        /// <remarks>This method retrieves the location object corresponding to the endpoint, disables its interaction, and then performs the activation asynchronously.</remarks>
        /// <param name="handle">The <see cref="AreaHandle"/> representing the area being entered. Must contain valid connections.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task OnActivate(AreaHandle handle)
        {
            // If the area handle for the loaded scene has no connections ignore the scene for player relocation and return
            if (!handle.HasConnections()) return;

            // Find the connection in the area handle that matches the end point and disable interaction
            if (TryGetLocation(EndPoint, out ILocationPointer connectable)) currentLocation.Set(connectable);

            // Check if the location is not null
            if (currentLocation.HasValue)
            {
                // Disable the location to prevent interaction
                connectable.SetActive(false);

                // Add the activation task for the matching location
                await connectable.Activate();
            }
            else
            {
                // Return a completed task if no matching location is found
                await Task.CompletedTask;
            }
        }

        /// <summary>
        /// Handles the logic for entering a connection point in the current area.
        /// </summary>
        /// <remarks>
        /// This method locates the location object associated with the specified endpoint and triggers its entry logic asynchronously. 
        /// If no matching location is found, the method completes without performing any action.
        /// </remarks>
        /// <param name="handle">The <see cref="AreaHandle"/> representing the area being entered. Must contain valid connections.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public async Task OnEnter(AreaHandle handle)
        {
            // If the area handle for the loaded scene has no connections ignore the scene for player relocation and return
            if (!handle.HasConnections()) return;

            // Find the connection in the area handle that matches the end point and disable interaction
            if (TryGetLocation(EndPoint, out ILocationPointer connectable)) currentLocation.Set(connectable);

            // Check if the location is not null
            if (currentLocation.HasValue)
            {
                // Await the OnEntry task for the matching location
                await connectable.Enter();
            }
            else
            {
                // Return a completed task if no matching location is found
                await Task.CompletedTask;
            }
        }

        #endregion
    }
}