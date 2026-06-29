using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper
{
    /// <summary>
    /// The Transistor class is a singleton that manages area transitions and connections in the World Shaper system.
    /// </summary>
    /// <remarks>
    /// This class handles the loading and unloading of scenes, manages player relocation, and maintains connections.
    /// Runs before other scripts to ensure proper initialization.
    /// </remarks>
    public static class Transistor
    {
        public static AreaHandle currentArea;
        public static InterfaceReference<ILocationPointer> currentLocation;
        public static List<InterfaceReference<ILocationPointer>> locations;
        public static ConnectionState connection = ConnectionState.Empty;
        public static Progress transitionProgress = Progress.Empty;

        /// <summary>
        /// The transition controller responsible for managing transition animations and effects.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>This controller handles the visual and functional aspects of transitions between areas, ensuring a smooth experience for the player.</item>
        /// <item>Left ambiguous to allow for user-defined transition controllers, enabling customization of transition behavior and appearance.</item>
        /// </list>
        /// </remarks>
        public static ITransitionController controller;

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
        public static Action<string> OnEndPointChanged = delegate { };

        /// <summary>
        /// Gets the singleton instance of the WorldMap class.
        /// </summary>
        public static WorldMap WorldMap => WorldMap.Instance;

        /// <summary>
        /// The name of the end point for the current connection.
        /// </summary>
        public static string EndPoint { get => connection.endPoint; set => connection.SetEnd(value); }

        // Default transition settings, to be overridden by the user if desired.
        // These settings control the behavior of transitions between areas, including delay and whether to reload scenes or unload unused assets.

        public const float TransitionDelay = 2f;
        public const bool RealtimeTransitions = false;
        public const bool ReloadActiveScene = false;
        public const bool ReloadAdditiveScenes = false;
        public const bool UnloadUnusedAssets = true;

        #region Transition Animation Methods

        /// <summary>
        /// Initiates the "transition in" animation for the current state.
        /// </summary>
        /// <param name="realTime">A value indicating whether the animation should be performed in real-time.  If <see langword="true"/>, the animation will respect real-time constraints; otherwise, it may use scaled time.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task completes when the transition animation finishes.</returns>
        public static Task TransitionIn(bool realTime = false) => controller.AnimateTransitionIn(realTime);

        /// <summary>
        /// Initiates the transition-out animation.
        /// </summary>
        /// <param name="realTime">A value indicating whether the transition should be performed in real-time. If <see langword="true"/>, the transition respects real-time constraints; otherwise, it may use scaled time.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation of the transition-out animation.</returns>
        public static Task TransitionOut(bool realTime = false) => controller.AnimateTransitionOut(realTime);

        /// <summary>
        /// Sets the transition-in animation to be used during area transitions.
        /// </summary>
        /// <param name="identifier">The identifier of the transition animation to set for the transition-in phase.</param>
        public static void SetTransitionIn(TransitionIdentifier identifier) => controller.SetInTransition(identifier);

        /// <summary>
        /// Sets the transition-out animation to be used during area transitions.
        /// </summary>
        /// <param name="identifier">The identifier of the transition animation to set for the transition-out phase.</param>
        public static void SetTransitionOut(TransitionIdentifier identifier) => controller.SetOutTransition(identifier);

        #endregion

        // To Do: Refactor the following methods to use a more flexible transition system, allowing for custom transition animations and effects.
        // Will also cut down on the number of methods in this class, as many of them are redundant or too lengthy.

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
        private static void ConfigurePassageData(AreaHandle areaHandle, string endPoint)
        {
            // Set the current area to the area handle
            currentArea = areaHandle;

            // If the end passage name is null, set the end point to an empty string
            EndPoint = endPoint ?? string.Empty;

            // Invoke the OnEndPointChanged action to signal the end point has changed
            OnEndPointChanged.Invoke(EndPoint);
        }

        /// <summary>
        /// Switches the current context to the specified area based on the provided connection.
        /// </summary>
        /// <remarks>This method retrieves the destination area from the provided connection, configures the passage data using the connection details, and returns the corresponding area handle.</remarks>
        /// <param name="connection">The connection object containing the destination area, connection name, and endpoint information.</param>
        /// <returns>An <see cref="AreaHandle"/> representing the destination area specified in the connection.</returns>
        public static AreaHandle SwitchToDestination(Connection connection)
        {
            // Configure the passage data with the area handle and passage name
            ConfigurePassageData(connection.destinationArea, connection.Endpoint);

            // Return the area handle
            return connection.destinationArea;
        }

        /// <summary>
        /// Switches to the specified area based on the provided connection.
        /// </summary>
        /// <param name="connection">The connection object containing the destination area and connection name.</param>
        /// <returns>An <see cref="AreaHandle"/> representing the area that was switched to.</returns>
        public static AreaHandle SwitchToArea(Connection connection)
        {
            // Get the area handle from the world map using the connection's destination area
            var areaHandle = WorldMap.GetArea(connection);

            // Set the passage data to prepare for the transition
            ConfigurePassageData(areaHandle, connection.connectionName);

            // Return the area handle
            return areaHandle;
        }

        #endregion

        #region Scene Switching Methods

        /// <summary>
        /// Switches the current scene to the specified area at the given connection index.
        /// </summary>
        /// <remarks>This method initiates a scene transition to the specified area. Ensure that the <paramref name="areaName"/> corresponds to a valid scene and that the connection index is within the valid range for the area's connections.</remarks>
        /// <param name="areaName">The name of the area to switch to. This must match the name of an existing scene.</param>
        /// <param name="connectionIndex">The index of the connection point to use when transitioning to the new area. Defaults to <see langword="0"/>.</param>
        public static async Task SwitchToArea(string areaName, int connectionIndex = 0) => await HandleAreaSwitch(WorldMap.GetArea(areaName).GetConnection(connectionIndex));

        /// <summary>
        /// Switches the current scene to the specified area
        /// </summary>
        /// <param name="area">The handle representing the target area to switch to. Cannot be null.</param>
        public static async void SwitchToArea(AreaHandle area) => await HandleAreaSwitch(area);

        /// <summary>
        /// Centralized method to handle area switching logic based around a given connection.
        /// </summary>
        /// <param name="connection">The connection object that defines the destination area and associated metadata.</param>
        /// <param name="useTransition">True to use transition animations; otherwise, false.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task HandleAreaSwitch(Connection connection, bool useTransition = true)
        {
            // Invoke the OnTransitionStarted action to signal the start of the transition
            OnTransitionStarted.Invoke();

            // Get the transition in animation from the connection's endpoint
            SetTransitionIn(connection.transitionIn);

            // Perform the transition in animation before switching areas, if specified
            if (useTransition)
            {
                // Get the transition in animation from the connection's endpoint
                SetTransitionIn(connection.transitionIn);

                // Wait for the transition in animation to complete
                await TransitionIn(RealtimeTransitions);
            }

            // Get the target area handle by switching to the destination defined by the connection
            AreaHandle targetArea = SwitchToArea(connection);

            // Switch to the new area using the connection and transition settings
            await Execute(targetArea, ReloadActiveScene, ReloadAdditiveScenes, UnloadUnusedAssets);

            // Wait for the specified delay before loading the new area
            if (TransitionDelay > 0f) await Task.Delay(TimeSpan.FromSeconds(TransitionDelay));

            // Perform the transition out animation after switching areas, if specified
            if (useTransition)
            {
                // Get the transition out animation from the connection
                SetTransitionOut(connection.transitionOut);

                // Wait for the transition out animation to complete
                await TransitionOut(RealtimeTransitions);
            }

            // Await the OnEnter event of the world map after the transition is complete
            await OnEnter(targetArea);

            // Invoke the OnTransitionCompleted action to signal the completion of the transition
            OnTransitionCompleted.Invoke();
        }

        /// <summary>
        /// Performs an asynchronous transition to the specified area, including transition animations and event signaling.
        /// </summary>
        /// <remarks>
        /// This method triggers transition animations and signals events before and after the area switch. 
        /// It also supports optional delays and asset management during the transition. 
        /// Await the returned task to ensure the transition is fully complete before proceeding.
        /// </remarks>
        /// <param name="area">The handle representing the target area to switch to. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task completes when the area transition and all related events are finished.</returns>
        public static async Task HandleAreaSwitch(AreaHandle area)
        {
            // Invoke the OnTransitionStarted action to signal the start of the transition
            OnTransitionStarted.Invoke();

            // Perform the transition in animation before switching areas, if specified
            await TransitionIn(RealtimeTransitions);

            // Switch to the new area using the connection and transition settings
            await Execute(area, ReloadActiveScene, ReloadAdditiveScenes, UnloadUnusedAssets);

            // Wait for the specified delay before loading the new area
            if (TransitionDelay > 0f) await Task.Delay(TimeSpan.FromSeconds(TransitionDelay));

            // Perform the transition out animation after switching areas, if specified
            await TransitionOut(RealtimeTransitions);

            // Await the OnEnter event of the world map after the transition is complete
            await OnEnter(area);

            // Invoke the OnTransitionCompleted action to signal the completion of the transition
            OnTransitionCompleted.Invoke();
        }

        /// <summary>
        /// Reloads the current area, optionally using transition animations.
        /// </summary>
        /// <remarks>
        /// This method reloads the current area in the world map. 
        /// If <paramref name="useTransition"/> is set to <see langword="true"/>, transition animations are performed before and after the reload process. 
        /// A delay may occur during the reload process, depending on the configuration.
        /// </remarks>
        /// <param name="useTransition">A value indicating whether to perform transition animations before and after reloading the area. <see langword="true"/> to use transitions; otherwise, <see langword="false"/>.</param>
        [Obsolete("This method is deprecated. Use SwitchToArea or SwitchToDestination methods for area transitions instead.")]
        public static async void ReloadArea(bool useTransition = true)
        {
            //// Invoke the OnTransitionStarted action to signal the start of the transition
            //OnTransitionStarted.Invoke();

            //// Perform the transition in animation before switching areas, if specified
            //if (useTransition) await TransitionIn(RealtimeTransitions);

            //// Get the area name from the area handle
            //string areaName = SceneManager.GetActiveScene().name;

            //// If the current area is null, get the current area from the scene name and set it to the current area
            //if (currentArea == null || currentArea.activeScene.Name != areaName) currentArea = WorldMap.GetArea(areaName);

            //// Get the end passage name from the current area connections as the first connection
            //if (EndPoint == string.Empty && currentArea.HasConnections())
            //{
            //    // Set the start passage name to the first connection endpoint
            //    StartPoint = currentArea.First.Endpoint;

            //    // Set the end passage name to the first connection name
            //    EndPoint = currentArea.First.connectionName;

            //    // Invoke the OnEndPointChanged action to signal the end point has changed
            //    OnEndPointChanged.Invoke(EndPoint);
            //}
            //else if (EndPoint != string.Empty && currentArea.ConnectionExists(EndPoint))
            //{
            //    // Get the start passage name from the current area connections based on the end passage name
            //    StartPoint = currentArea.GetConnection(EndPoint).Endpoint;
            //}

            //// Invoke the OnStartPointChanged action to signal the start point has changed
            //OnStartPointChanged.Invoke(StartPoint);

            //// Start the area transition with the area name and default transition name
            //await ExecuteTransition(currentArea);

            //// Wait for the specified delay before loading the new area
            //if (TransitionDelay > 0f) await Task.Delay(TimeSpan.FromSeconds(TransitionDelay));

            //// Perform the transition out animation after switching areas, if specified
            //if (useTransition) await TransitionOut(RealtimeTransitions);

            //// Await the OnEnter event of the world map after the transition is complete
            //await OnEnter(currentArea);

            //// Invoke the OnTransitionCompleted action to signal the completion of the transition
            //OnTransitionCompleted.Invoke();
        }

        #endregion

        // To Do: Cleanup the handling for the ILocationPointer Methods, as they are currently too long and redundant.
        // Will also need to refactor the handling for the ILocationPointer methods to use a more flexible system, allowing for custom location management and interaction.

        #region Execution Methods

        /// <summary>
        /// Registers an <see cref="ILocationPointer"/> instance for tracking and management.
        /// </summary>
        /// <remarks>If the specified <paramref name="location"/> is already registered, it will not be added again.</remarks>
        /// <param name="location">The <see cref="ILocationPointer"/> instance to register. Cannot be null.</param>
        public static void Register(ILocationPointer location) => locations.Add(InterfaceReference<ILocationPointer>.FromValue(location));

        /// <summary>
        /// Removes the specified location object from the collection of registered objects.
        /// </summary>
        /// <remarks>This method removes all references to the specified object from the collection. If the object is not found, no action is taken.</remarks>
        /// <param name="location">The location object to deregister. Cannot be <see langword="null"/>.</param>
        public static void Unregister(ILocationPointer location) => locations.RemoveAll(reference => reference.Value == location);

        /// <summary>
        /// Tries to retrieve a location object by its endpoint.
        /// </summary>
        /// <param name="endPoint">The endpoint string to search for.</param>
        /// <param name="location">The output parameter that will hold the found location object if successful; otherwise, null.</param>
        /// <returns>A boolean value indicating whether a matching location object was found.</returns>
        private static bool TryGetLocation(string endPoint, out ILocationPointer location)
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
        /// Initializes all location objects in the specified area and prepares them for interaction.
        /// </summary>
        /// <remarks>This method retrieves all location objects in the scene associated with the specified area handle and initializes them asynchronously. 
        /// If the area handle has no connections, the method logs a debug message and exits without performing any initialization. 
        /// The connection matching the end point is disabled for interaction during this process.
        /// </remarks>
        /// <param name="handle">The <see cref="AreaHandle"/> representing the area to initialize. Must contain valid connections.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task Intialize(AreaHandle handle)
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
            locations = ILocationPointerExtensions.GetLocationPointers();

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
        public static async Task OnActivate(AreaHandle handle)
        {
            // If the area handle for the loaded scene has no connections ignore the scene for player relocation and return
            if (!handle.HasConnections()) return;

            // Find the connection in the area handle that matches the end point and disable interaction
            if (TryGetLocation(EndPoint, out ILocationPointer connectable)) currentLocation.SetValue(connectable);

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
        public static async Task OnEnter(AreaHandle handle)
        {
            // If the area handle for the loaded scene has no connections ignore the scene for player relocation and return
            if (!handle.HasConnections()) return;

            // Find the connection in the area handle that matches the end point and disable interaction
            if (TryGetLocation(EndPoint, out ILocationPointer connectable)) currentLocation.SetValue(connectable);

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

        /// <summary>
        /// Executes a transition between areas, including optional animations and a delay.
        /// </summary>
        /// <remarks>
        /// This method performs the following steps in sequence:
        /// <list type="number">
        ///     <item>Invokes the <see cref="OnTransitionStarted"/> action to signal the start of the transition.</item>
        ///     <item>Loads the new area using the <see cref="AreaHandleDispatcher"/>.</item>
        ///     <item>Initializes locations for the loaded area.</item>
        ///     <item>Invokes the <see cref="OnActivate"/> method to handle activation logic.</item>
        ///     <item>Invokes the <see cref="OnTransitionCompleted"/> action to signal the completion of the transition.</item>
        /// </list>
        /// </remarks>
        /// <param name="handle">The <see cref="AreaHandle"/> representing the area to be loaded.</param>
        /// <param name="reloadActiveScene">Indicates whether to reload the active scene during the transition. Defaults to <see langword="false"/>.</param>
        /// <param name="reloadAdditiveScenes">Indicates whether to reload additive scenes during the transition. Defaults to <see langword="false"/>.</param>
        /// <param name="unloadUnusedAssets">Indicates whether to unload unused assets after the transition. Defaults to <see langword="false"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task Execute(AreaHandle handle, bool reloadActiveScene = false, bool reloadAdditiveScenes = false, bool unloadUnusedAssets = false)
        {
            // Invoke the OnTransitionStarted action to signal the start of the transition
            OnTransitionStarted.Invoke();

            // Load the new area using the AreaHandleDispatcher
            await AreaHandleDispatcher.LoadAreas(handle, transitionProgress, reloadActiveScene, reloadAdditiveScenes, unloadUnusedAssets);

            // Initialize locations for the loaded area
            await Intialize(handle);

            // Invoke the OnActivate method to handle activation logic
            await OnActivate(handle);

            // Invoke the OnTransitionCompleted action to signal the completion of the transition
            OnTransitionCompleted.Invoke();
        }

        #endregion
    }
}