using System;
using System.Threading.Tasks;
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
        [Header("Dynamic References")]
        public static ITransitionController controller;

        // To Do: Remove the following properties and methods in a future update, as they will be replaced by method chains and a more flexible transition system.

        [Header("Transition Settings")]
        public static float TransitionDelay => WorldMap.transitionDelay;
        public static bool RealtimeTransitions => WorldMap.realtimeTransitions;
        public static bool ReloadActiveScene => WorldMap.reloadActiveScene;
        public static bool ReloadAdditiveScenes => WorldMap.reloadAdditiveScenes;
        public static bool UnloadUnusedAssets => WorldMap.unloadUnusedAssets;

        public static WorldMap WorldMap => WorldMap.Instance;

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

        /// <summary>
        /// Switches to the endpoint area defined by the specified connection.
        /// </summary>
        /// <remarks>This method uses the provided <paramref name="connection"/> to determine the destination area and configures the necessary passage data before initiating the transition.</remarks>
        /// <param name="connection">The connection object that defines the destination area and associated metadata.</param>
        public static async void SwitchToDestination(Connection connection, bool useTransition = true) => await HandleDestinationSwitch(connection, useTransition);

        /// <summary>
        /// Switches to the endpoint area of the specified area handle and connection name.
        /// </summary>
        /// <remarks>This method retrieves the connection associated with the specified area name from the provided <paramref name="areaHandle"/>, configures the passage data for the transition, and initiates the transition.</remarks>
        /// <param name="areaHandle">The handle representing the current area context. Must not be null.</param>
        /// <param name="connectionName">The name of the target area to switch to. Must correspond to a valid connection within the <paramref name="areaHandle"/>.</param>
        public static async void SwitchToDestination(AreaHandle areaHandle, string connectionName, bool useTransition = true) => await HandleDestinationSwitch(areaHandle.GetConnection(connectionName), useTransition);

        /// <summary>
        /// Switches the endpoint area of the specified area name and connection index.
        /// </summary>
        /// <remarks>This method initiates a scene transition to the specified area. Ensure that the <paramref name="areaName"/> corresponds to a valid scene and that the connection index is within the valid range for the area's connections.</remarks>
        /// <param name="areaName">The name of the area to switch to. This must match the name of an existing scene.</param>
        /// <param name="connectionIndex">The index of the connection point to use when transitioning to the new area. Defaults to <see langword="0"/>.</param>
        public static async void SwitchToDestination(string areaName, int connectionIndex = 0, bool useTransition = true) => await HandleDestinationSwitch(WorldMap.GetArea(areaName).GetConnection(connectionIndex), useTransition);

        /// <summary>
        /// Switches to the endpoint area of the specified area index and connection index.
        /// </summary>
        /// <remarks>
        /// This method initiates a scene transition to the specified area using the provided parameters. Ensure that <paramref name="areaIndex"/> corresponds to a valid area handle; otherwise, the behavior is undefined.</remarks>
        /// <param name="areaIndex">The index of the area to switch to. Must correspond to a valid area handle.</param>
        /// <param name="connectionIndex">The index of the connection point within the area. Defaults to <see langword="0"/>. Determines the starting and ending passage points for the transition.</param>
        public static async void SwitchToDestination(int areaIndex, int connectionIndex = 0, bool useTransition = true) => await HandleDestinationSwitch(WorldMap.GetArea(areaIndex).GetConnection(connectionIndex), useTransition);

        /// <summary>
        /// Centralized method to handle destination switching logic based around a given connection.
        /// </summary>
        /// <param name="connection">The connection object that defines the destination area and associated metadata.</param>
        /// <param name="useTransition">True to use transition animations; otherwise, false.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private static async Task HandleDestinationSwitch(Connection connection, bool useTransition = true)
        {
            // Invoke the OnTransitionStarted action to signal the start of the transition
            WorldMap.OnTransitionStarted.Invoke();

            // Get the destination connection, which is the endpoint of the provided connection if it exists; otherwise, it defaults to the original connection
            Connection destination = connection.GetEndpoint() ?? connection;

            // Perform the transition in animation before switching areas, if specified
            if (useTransition)
            {
                // Get the transition in animation from the connection
                if (destination != connection) SetTransitionIn(connection.transitionOut);
                else SetTransitionIn(connection.transitionIn);

                // Wait for the transition in animation to complete
                await TransitionIn(RealtimeTransitions);
            }

            // Get the target area handle by switching to the destination defined by the connection
            AreaHandle targetArea = WorldMap.SwitchToDestination(connection);

            // Switch to the new area using the specified connection and transition settings
            await WorldMap.ExecuteTransition(targetArea, ReloadActiveScene, ReloadAdditiveScenes, UnloadUnusedAssets);

            // Wait for the specified delay before loading the new area
            if (TransitionDelay > 0f) await Task.Delay(TimeSpan.FromSeconds(TransitionDelay));

            // Perform the transition out animation after switching areas, if specified
            if (useTransition)
            {
                // If the endpoint connection exists, use its transition in animation for the transition out phase; otherwise, use the original connection's transition out animation
                if (destination != connection) SetTransitionOut(destination.transitionIn);
                else SetTransitionOut(connection.transitionOut);

                // Wait for the transition out animation to complete
                await TransitionOut(RealtimeTransitions);
            }

            // Await the OnEnter event of the world map after the transition is complete
            await WorldMap.OnEnter(targetArea);

            // Invoke the OnTransitionCompleted action to signal the completion of the transition
            WorldMap.OnTransitionCompleted.Invoke();
        }

        /// <summary>
        /// Switches the current scene to the specified area using the provided connection.
        /// </summary>
        /// <param name="connection">The connection object that defines the destination area and associated metadata.</param>
        /// <param name="useTransition">True to use transition animations; otherwise, false.</param>
        public static async void SwitchToArea(Connection connection, bool useTransition = true) => await HandleAreaSwitch(connection, useTransition);

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
            WorldMap.OnTransitionStarted.Invoke();

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
            AreaHandle targetArea = WorldMap.SwitchToArea(connection);

            // Switch to the new area using the connection and transition settings
            await WorldMap.ExecuteTransition(targetArea, ReloadActiveScene, ReloadAdditiveScenes, UnloadUnusedAssets);

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
            await WorldMap.OnEnter(targetArea);

            // Invoke the OnTransitionCompleted action to signal the completion of the transition
            WorldMap.OnTransitionCompleted.Invoke();
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
            WorldMap.OnTransitionStarted.Invoke();

            // Perform the transition in animation before switching areas, if specified
            await TransitionIn(RealtimeTransitions);

            // Switch to the new area using the connection and transition settings
            await WorldMap.ExecuteTransition(area, ReloadActiveScene, ReloadAdditiveScenes, UnloadUnusedAssets);

            // Wait for the specified delay before loading the new area
            if (TransitionDelay > 0f) await Task.Delay(TimeSpan.FromSeconds(TransitionDelay));

            // Perform the transition out animation after switching areas, if specified
            await TransitionOut(RealtimeTransitions);

            // Await the OnEnter event of the world map after the transition is complete
            await WorldMap.OnEnter(area);

            // Invoke the OnTransitionCompleted action to signal the completion of the transition
            WorldMap.OnTransitionCompleted.Invoke();
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
        public static async void ReloadArea(bool useTransition = true)
        {
            // Invoke the OnTransitionStarted action to signal the start of the transition
            WorldMap.OnTransitionStarted.Invoke();

            // Perform the transition in animation before switching areas, if specified
            if (useTransition) await TransitionIn(RealtimeTransitions);

            // Get the target area handle by switching to the destination defined by the connection
            AreaHandle currentArea = WorldMap.currentArea;

            // Reload the current area
            await WorldMap.ReloadArea();

            // Wait for the specified delay before loading the new area
            if (TransitionDelay > 0f) await Task.Delay(TimeSpan.FromSeconds(TransitionDelay));

            // Perform the transition out animation after switching areas, if specified
            if (useTransition) await TransitionOut(RealtimeTransitions);

            // Await the OnEnter event of the world map after the transition is complete
            await WorldMap.OnEnter(currentArea);

            // Invoke the OnTransitionCompleted action to signal the completion of the transition
            WorldMap.OnTransitionCompleted.Invoke();
        }
    }
}