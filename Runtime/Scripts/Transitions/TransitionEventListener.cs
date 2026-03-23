using UnityEngine;
using UnityEngine.Events;

namespace WorldShaper
{
    public class TransitionEventListener : MonoBehaviour
    {
        public UnityEvent OnTransitionStarted;
        public UnityEvent OnTransitionCompleted;

        private void OnEnable()
        {
            // Subscribe to transition events
            WorldMap.OnTransitionStarted += OnTransitionStarted.Invoke;
            WorldMap.OnTransitionCompleted += OnTransitionCompleted.Invoke;
        }

        private void OnDisable()
        {
            // Unsubscribe from transition
            WorldMap.OnTransitionStarted -= OnTransitionStarted.Invoke;
            WorldMap.OnTransitionCompleted -= OnTransitionCompleted.Invoke;
        }
    }
}