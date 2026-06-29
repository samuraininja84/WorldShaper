using UnityEngine;

namespace WorldShaper
{
    public class TransitionBuilder : ITransitionBuilder, IDestinationStep, ITransitionStep, IDelayStep, ISettingsStep, IBuildStep
    {
        public AreaHandle Area;
        public Connection Connection;
        public TransitionIdentifier TransitionIn;
        public TransitionIdentifier TransitionOut;
        public TransitionInfo.Settings Settings;
        public Vector2 Delays;

        public static ITransitionBuilder Create() => new TransitionBuilder();

        internal static TransitionInfo TestChain()
        {
            return Create()
                .WithArea(ScriptableObject.CreateInstance<AreaHandle>())
                .WithDestination("TestConnection")
                .WithSettings(TransitionInfo.Settings.Default)
                .WithDelayedTransitions(ScriptableObject.CreateInstance<TransitionIdentifier>())
                .WithDelay(0.5f, 0.5f).Build();
        }

        #region Area Step

        public IDestinationStep WithArea(AreaHandle area)
        {
            Area = area;
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

        public TransitionInfo Build() => TransitionInfo.Create
        (
            Area,
            Connection,
            TransitionIn,
            TransitionOut,
            Settings,
            Delays
        );
    }

    #region Builder Interfaces

    /// <summary>
    /// A builder interface for constructing a <see cref="TransitionInfo"/> object in a fluent manner.
    /// </summary>
    public interface ITransitionBuilder
    {
        IDestinationStep WithArea(AreaHandle area);
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