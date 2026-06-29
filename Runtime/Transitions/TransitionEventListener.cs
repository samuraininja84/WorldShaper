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
            Transistor.OnTransitionStarted += OnTransitionStarted.Invoke;
            Transistor.OnTransitionCompleted += OnTransitionCompleted.Invoke;
        }

        private void OnDisable()
        {
            // Unsubscribe from transition
            Transistor.OnTransitionStarted -= OnTransitionStarted.Invoke;
            Transistor.OnTransitionCompleted -= OnTransitionCompleted.Invoke;
        }
    }
}