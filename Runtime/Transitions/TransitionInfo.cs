using UnityEngine;
using Identifier = WorldShaper.TransitionIdentifier;

namespace WorldShaper
{
    public struct TransitionInfo 
    {
        public AreaHandle Area;
        public Connection Connection;
        public Identifier TransitionIn;
        public Identifier TransitionOut;
        public Settings Options;
        public Vector2 Delays;

        private TransitionInfo(AreaHandle area, Connection connection, Identifier transitionIn, Identifier transitionOut, Settings settings,Vector2 delays)
        {
            Area = area;
            Connection = connection;
            Options = settings;
            TransitionIn = transitionIn;
            TransitionOut = transitionOut;
            Delays = delays;
        }

        private static TransitionInfo Create(AreaHandle area, Connection connection, Identifier @in, Identifier @out, Settings settings, Vector2 delays) => new(area, connection, @in, @out, settings, delays);

        /// <summary>
        /// Flags that define the settings for a transition, such as whether to use real-time, reload scenes, or unload unused assets.
        /// </summary>
        [System.Flags]
        public enum Settings
        {
            /// <summary>
            /// No special settings are applied during the transition.
            /// </summary>
            None = 0,

            /// <summary>
            /// Use real-time during the transition.
            /// </summary>
            /// <remarks>This is useful if you want the transition to be unaffected by time scaling, such as when pausing the game or slowing down time.</remarks>
            Realtime = 1 << 0,

            /// <summary>
            /// Reload the active scene during the transition.
            /// </summary>
            /// <remarks>This is useful if you want to ensure that the current scene is reloaded when transitioning, which can help prevent issues with stale data or references.</remarks>
            ReloadActiveScene = 1 << 1,

            /// <summary>
            /// Reload additive scenes during the transition.
            /// </summary>
            /// <remarks>This can help prevent issues with stale data or references in additive scenes, but may cause a slight delay if the scenes need to be reloaded.</remarks>
            ReloadAdditiveScenes = 1 << 2,

            /// <summary>
            /// Unload unused assets after the transition is complete. This can help reduce memory usage, but may cause a slight delay if assets need to be reloaded later.
            /// </summary>
            UnloadUnusedAssets = 1 << 3,

            /// <summary>
            /// The default settings for a transition will be to use real-time and unload unused assets, which is the most common case.
            /// </summary>
            Default = Realtime | UnloadUnusedAssets,

            /// <summary>
            /// Reloads all scenes, including the active scene and any additive scenes. This is a combination of ReloadActiveScene and ReloadAdditiveScenes.
            /// </summary>
            ReloadAllScenes = ReloadActiveScene | ReloadAdditiveScenes,

            /// <summary>
            /// Represents all possible settings for a transition.
            /// </summary>
            All = Realtime | ReloadActiveScene | ReloadAdditiveScenes | UnloadUnusedAssets
        }

        /// <summary>
        /// A builder struct for constructing a <see cref="TransitionInfo"/> object in a fluent manner.
        /// </summary>
        public struct Builder : ITransitionBuilder, IDestinationStep, ITransitionStep, IDelayStep, ISettingsStep, IBuildStep
        {
            public AreaHandle Area;
            public Connection Connection;
            public Identifier TransitionIn;
            public Identifier TransitionOut;
            public Settings Settings;
            public Vector2 Delays;

            public static ITransitionBuilder Create() => new Builder();

            #region Default TransitionInfo Creation

            /// <summary>
            /// Creates a <see cref="TransitionInfo"/> object from an area handle, using default settings and to the first connection in that area.
            /// </summary>
            /// <param name="area">The handle representing the target area to switch to. Cannot be null.</param>
            /// <returns>A <see cref="TransitionInfo"/> object configured for the specified area.</returns>
            public static TransitionInfo FromArea(AreaHandle area)
            {
                // Assumes that the area has at least one connection and retrieves the first connection in that area.
                var connection = area.GetConnection(0);

                // Return a TransitionInfo object with the specified area, the first connection in that area, default settings, and transitions set to the connection's transitionIn and transitionOut values.
                return Create()
                    .WithDestination(area, connection)
                    .WithSettings(Settings.Default)
                    .WithTransitions(connection.transitionIn, connection.transitionOut)
                    .Build();
            }

            /// <summary>
            /// Creates a <see cref="TransitionInfo"/> object from a connection that goes to the parent area of this connection, using default settings.
            /// </summary>
            /// <param name="connection">The connection object that is used to determine the parent area to switch to. Cannot be null.</param>
            /// <returns>A <see cref="TransitionInfo"/> object configured for the specified connection.</returns>
            public static TransitionInfo FromArea(Connection connection)
            {
                return Create()
                    .WithDestination(WorldMap.Instance.GetArea(connection), connection)
                    .WithSettings(Settings.Default)
                    .WithTransitions(connection.transitionIn, connection.transitionOut)
                    .Build();
            }

            /// <summary>
            /// Creates a <see cref="TransitionInfo"/> object from a connection that goes to the destination area of this connection, using default settings.
            /// </summary>
            /// <param name="connection">The connection object that is used to determine the destination area to switch to. Cannot be null.</param>
            /// <returns>A <see cref="TransitionInfo"/> object configured for the specified connection.</returns>
            public static TransitionInfo FromDestination(Connection connection)
            {
                // Assumes that the connection has a valid destination area and retrieves the endpoint connection in that area.
                var destination = connection.GetEndpoint();

                // If the destination connection is not found, fall back to using the connection's transitionOut value.
                var transitionOut = destination != null ? destination.transitionOut : connection.transitionOut;

                // Return a TransitionInfo object with the destination area, the endpoint connection in that area, default settings, and transitions set to the connection's transitionOut and the endpoint's transitionOut values.
                return Create()
                    .WithDestination(connection.destinationArea, destination)
                    .WithSettings(Settings.Default)
                    .WithTransitions(connection.transitionOut, transitionOut)
                    .Build();
            }

            #endregion

            #region Area Step

            public IDestinationStep WithArea(AreaHandle area)
            {
                Area = area;
                return this;
            }

            public ISettingsStep WithDestination(AreaHandle area, Connection connection)
            {
                Area = area;
                Connection = connection;
                return this;
            }

            public ISettingsStep WithReference(ConnectionReference reference)
            {
                Area = reference.Area;
                Connection = reference.GetCurrent();
                return this;
            }

            #endregion

            #region Destination Step

            /// <summary>
            /// Assumes that the area has been set before calling this method in the destination step.
            /// </summary>
            /// <remarks>Falls back to the world map if the connection is not found in the area.</remarks>
            /// <param name="guid">The GUID of the destination connection.</param>
            /// <returns>A transition step with the destination connection set.</returns>
            public ISettingsStep WithDestination(SerializableGuid guid)
            {
                // Assumes that the area has been set before calling this method in the destination step.
                Connection = Area.GetConnection(guid);

                // If the connection is not found in the area, try to get it from the world map.
                if (Connection == null) Connection = WorldMap.Instance.GetConnection(guid);

                // Return the transition step with the destination connection set.
                return this;
            }

            /// <summary>
            /// Assumes that the area has been set before calling this method in the destination step.
            /// </summary>
            /// <remarks>Falls back to the world map if the connection is not found in the area.</remarks>
            /// <param name="name">The name of the destination connection.</param>
            /// <returns>A transition step with the destination connection set.</returns>
            public ISettingsStep WithDestination(string name)
            {
                // Assumes that the area has been set before calling this method in the destination step.
                Connection = Area.GetConnection(name);

                // If the connection is not found in the area, try to get it from the world map.
                if (Connection == null) Connection = WorldMap.Instance.GetConnection(name);

                // Return the transition step with the destination connection set.
                return this;
            }

            /// <summary>
            /// Assumes that the area has been set before calling this method in the destination step.
            /// </summary>
            /// <param name="index">The index of the destination connection.</param>
            /// <returns>A transition step with the destination connection set.</returns>
            public ISettingsStep WithDestination(int index)
            {
                // Assumes that the area has been set before calling this method in the destination step.
                Connection = Area.GetConnection(index);

                // Return the transition step with the destination connection set.
                return this;
            }

            #endregion

            #region Settings Step

            public ITransitionStep WithSettings(TransitionInfo.Settings settings)
            {
                Settings = settings;
                return this;
            }

            #endregion

            #region Transition Step

            public IBuildStep WithTransitions(TransitionIdentifier transition)
            {
                TransitionIn = transition;
                TransitionOut = transition;
                return this;
            }

            public IBuildStep WithTransitions(TransitionIdentifier transitionIn, TransitionIdentifier transitionOut)
            {
                TransitionIn = transitionIn;
                TransitionOut = transitionOut;
                return this;
            }

            public IDelayStep WithDelayedTransitions(TransitionIdentifier transition)
            {
                TransitionIn = transition;
                TransitionOut = transition;
                return this;
            }

            public IDelayStep WithDelayedTransitions(TransitionIdentifier transitionIn, TransitionIdentifier transitionOut)
            {
                TransitionIn = transitionIn;
                TransitionOut = transitionOut;
                return this;
            }

            #endregion

            #region Delay Step

            public IBuildStep WithDelay(float startDelay, float endDelay)
            {
                Delays = new(startDelay, endDelay);
                return this;
            }

            public IBuildStep WithDelay(Vector2 delays)
            {
                Delays = delays;
                return this;
            }

            public IBuildStep WithNoDelay()
            {
                Delays = Vector2.zero;
                return this;
            }

            #endregion

            public readonly TransitionInfo Build() => TransitionInfo.Create
            (
                Area,
                Connection,
                TransitionIn,
                TransitionOut,
                Settings,
                Delays
            );
        }
    }

    #region Builder Interfaces

    /// <summary>
    /// A builder interface for constructing a <see cref="TransitionInfo"/> object in a fluent manner.
    /// </summary>
    public interface ITransitionBuilder
    {
        IDestinationStep WithArea(AreaHandle area);
        ISettingsStep WithDestination(AreaHandle area, Connection connection);
        ISettingsStep WithReference(ConnectionReference reference);
    }

    /// <summary>
    /// A step in the transition building process that allows specifying the destination connection for the transition.
    /// </summary>
    /// <remarks>
    /// Generally assumes that the area has been set before calling any of the <see cref="WithDestination"/> methods.
    /// This step can be skipped if the transition is being built from a <see cref="ConnectionReference"/>.
    /// </remarks>
    public interface IDestinationStep
    {
        ISettingsStep WithDestination(SerializableGuid guid);
        ISettingsStep WithDestination(string name);
        ISettingsStep WithDestination(int index);
    }

    /// <summary>
    /// A step in the transition building process that allows specifying the settings for the transition.
    /// </summary>
    public interface ISettingsStep
    {
        ITransitionStep WithSettings(TransitionInfo.Settings settings);
    }

    /// <summary>
    /// A step in the transition building process that allows specifying the transitions to be used for entering and exiting the destination connection.
    /// </summary>
    /// <remarks>
    /// Transitions can be specified for both entering and exiting the destination connection, or the same transition can be used for both.
    /// They are also optional and can be left null if no transition is desired, if no transition is specified, the last transition used will be applied to the next transition.
    /// </remarks>
    public interface ITransitionStep
    {
        IBuildStep WithTransitions(TransitionIdentifier transition);
        IBuildStep WithTransitions(TransitionIdentifier transitionIn, TransitionIdentifier transitionOut);
        IDelayStep WithDelayedTransitions(TransitionIdentifier transition);
        IDelayStep WithDelayedTransitions(TransitionIdentifier transitionIn, TransitionIdentifier transitionOut);
    }

    /// <summary>
    /// A step in the transition building process that allows specifying the delay before and after the transition.
    /// </summary>
    /// <remarks>
    /// This step can be skipped if no delay is desired, via the normal <see cref="ITransitionStep.WithTransitions"/> method.
    /// </remarks>
    public interface IDelayStep
    {
        IBuildStep WithDelay(float startDelay, float endDelay);
        IBuildStep WithDelay(Vector2 delays);
    }

    /// <summary>
    /// A step in the transition building process that allows finalizing the transition and building the <see cref="TransitionInfo"/> object.
    /// </summary>
    public interface IBuildStep
    {
        TransitionInfo Build();
    }

    #endregion
}