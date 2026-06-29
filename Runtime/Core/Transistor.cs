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
        public static TransitionInfo currentTransition;
        public static InterfaceReference<ILocationPointer> currentLocation;
        public static List<InterfaceReference<ILocationPointer>> locations;
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

        // To Do: Cleanup the handling for the ILocationPointer Methods, as they are currently too long and redundant.
        // Will also need to refactor the handling for the ILocationPointer methods to use a more flexible system, allowing for custom location management and interaction.

        #region Location Management & Execution Methods

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
        public static async Task OnActivate(AreaHandle handle, string endPoint)
        {
            // If the area handle for the loaded scene has no connections ignore the scene for player relocation and return
            if (!handle.HasConnections()) return;

            // Find the connection in the area handle that matches the end point and disable interaction
            if (TryGetLocation(endPoint, out ILocationPointer connectable))
            {
                // Set the current location to the matching location
                currentLocation.SetValue(connectable);

                // Disable the location to prevent interaction
                connectable.SetActive(false);

                // Add the activation task for the matching location
                await connectable.Activate();
            }

            // Return a completed task if no matching location is found
            await Task.CompletedTask;
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
        public static async Task OnEnter(AreaHandle handle, string endPoint)
        {
            // If the area handle for the loaded scene has no connections ignore the scene for player relocation and return
            if (!handle.HasConnections()) return;

            // Find the connection in the area handle that matches the end point and disable interaction
            if (TryGetLocation(endPoint, out ILocationPointer connectable))
            {
                // Set the current location to the matching location
                currentLocation.SetValue(connectable);

                // Await the OnEntry task for the matching location
                await connectable.Enter();
            }

            // Return a completed task if no matching location is found
            await Task.CompletedTask;
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
        /// <param name="transition">The <see cref="TransitionInfo"/> containing the area handle and transition settings.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task Execute(TransitionInfo transition)
        {
            // Store the area handle and transition settings from the TransitionInfo object
            var handle = transition.Area;
            var settings = transition.Options;

            // Get the transition settings from the TransitionInfo object
            bool reloadActiveScene = settings.HasFlag(TransitionInfo.Settings.ReloadActiveScene);
            bool reloadAdditiveScenes = settings.HasFlag(TransitionInfo.Settings.ReloadAdditiveScenes);
            bool unloadUnusedAssets = settings.HasFlag(TransitionInfo.Settings.UnloadUnusedAssets);

            // Load the new area using the AreaHandleDispatcher
            await AreaHandleDispatcher.LoadAreas(handle, transitionProgress, reloadActiveScene, reloadAdditiveScenes, unloadUnusedAssets);

            // Initialize locations for the loaded area
            await Intialize(handle);

            // Invoke the OnActivate method to handle activation logic
            await OnActivate(handle, transition.Connection.Name);
        }

        #endregion

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

        #region Transition Methods

        /// <summary>
        /// Switches to the endpoint area defined by the specified connection.
        /// </summary>
        /// <remarks>This method uses the provided <paramref name="connection"/> to determine the destination area and configures the necessary passage data before initiating the transition.</remarks>
        /// <param name="connection">The connection object that defines the destination area and associated metadata.</param>
        public static async Task SwitchToDestination(Connection connection) => await TransitionTo(TransitionInfo.Builder.FromDestination(connection));

        /// <summary>
        /// Switches the current scene to the specified area at the given connection index.
        /// </summary>
        /// <remarks>This method initiates a scene transition to the specified area. Ensure that the <paramref name="areaName"/> corresponds to a valid scene and that the connection index is within the valid range for the area's connections.</remarks>
        /// <param name="areaName">The name of the area to switch to. This must match the name of an existing scene.</param>
        /// <param name="connectionIndex">The index of the connection point to use when transitioning to the new area. Defaults to <see langword="0"/>.</param>
        public static async Task SwitchToArea(string areaName, int connectionIndex = 0) => await TransitionTo(TransitionInfo.Builder.FromArea(WorldMap.GetArea(areaName).GetConnection(connectionIndex)));

        /// <summary>
        /// Centralized method to handle area switching logic based around a given connection.
        /// </summary>
        /// <param name="connection">The connection object that defines the destination area and associated metadata.</param>
        /// <param name="useTransition">True to use transition animations; otherwise, false.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task SwitchToArea(Connection connection) => await TransitionTo(TransitionInfo.Builder.FromArea(connection));

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
        public static async Task SwitchToArea(AreaHandle area) => await TransitionTo(TransitionInfo.Builder.FromArea(area));

        /// <summary>
        /// Reloads the current area using the current transition information.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task completes when the area reload and all related events are finished.</returns>
        public static async Task ReloadArea() => await TransitionTo(currentTransition);

        /// <summary>
        /// Transitions to the specified area using the provided transition information.
        /// </summary>
        /// <param name="transition">The transition information to use for the area transition.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public static async Task TransitionTo(TransitionInfo transition) 
        { 
            // Invoke the OnTransitionStarted action to signal the start of the transition
            OnTransitionStarted.Invoke();

            // Wait for the specified delay before loading the new area
            if (transition.Delays.x > 0f) await Task.Delay(TimeSpan.FromSeconds(transition.Delays.x));

            // If there isn't a transition in animation specified, skip the transition in step
            if (transition.TransitionIn != null)
            {
                // Set the transition in animation in the controller
                SetTransitionIn(transition.TransitionIn);

                // Perform the transition in animation with the specified real-time option
                await TransitionIn(transition.Options.HasFlag(TransitionInfo.Settings.Realtime));
            }

            // Switch to the new area using the connection and transition settings
            await Execute(transition);

            // Wait for the specified delay before loading the new area
            if (transition.Delays.y > 0f) await Task.Delay(TimeSpan.FromSeconds(transition.Delays.y));

            // If there isn't a transition out animation specified, skip the transition out step
            if (transition.TransitionOut != null)
            {
                // Set the transition out animation in the controller
                SetTransitionOut(transition.TransitionOut);

                // Perform the transition out animation with the specified real-time option
                await TransitionOut(transition.Options.HasFlag(TransitionInfo.Settings.Realtime));
            }   

            // Await the OnEnter event of the world map after the transition is complete
            await OnEnter(transition.Area, transition.Connection.Name);

            // Invoke the OnTransitionCompleted action to signal the completion of the transition
            OnTransitionCompleted.Invoke();
        }

        #endregion
    }
}