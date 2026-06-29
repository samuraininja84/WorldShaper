using UnityEngine;

namespace WorldShaper
{
    [CreateAssetMenu(fileName = "WorldTransitionConfiguration ", menuName = "World Shaper/New World Transition Configuration ")]
    public class WorldTransitionConfiguration : ScriptableObject
    {
        [SerializeField] protected TransitionInfo.Settings DefaultSettings = TransitionInfo.Settings.Default;
        [SerializeField] protected Vector2 DefaultDelays = Vector2.zero;

        public TransitionInfo.Settings GetDefaultSettings() => DefaultSettings;

        public Vector2 GetDefaultDelays() => DefaultDelays;
    }
}