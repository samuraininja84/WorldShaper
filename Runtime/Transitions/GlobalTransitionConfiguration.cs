using UnityEngine;

namespace WorldShaper
{
    [CreateAssetMenu(fileName = "Global Transition Configuration ", menuName = "World Shaper/New Global Transition Configuration ")]
    public class GlobalTransitionConfiguration : ScriptableObject
    {
        protected TransitionInfo.Settings DefaultSettings = TransitionInfo.Settings.Default;
        protected Vector2 DefaultDelays = Vector2.zero;

        public TransitionInfo.Settings GetDefaultSettings() => DefaultSettings;

        public Vector2 GetDefaultDelays() => DefaultDelays;
    }
}