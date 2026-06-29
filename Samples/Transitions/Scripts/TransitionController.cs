using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper.Samples
{
    [AddComponentMenu("World Shaper/Transitions/Transition Controller")]
    public class TransitionController : PersistentSingleton<TransitionController>, ITransitionController
    {
        [Header("Transition Camera")]
        public Camera transitionCamera;

        [Header("Current Transitions")]
        public InterfaceReference<ITransition> inTransition;
        public InterfaceReference<ITransition> outTransition;

        [Header("Available Transitions")]
        public List<InterfaceReference<ITransition>> availableTransitions;

        public ITransition InTransition => inTransition.Value;

        public ITransition OutTransition => outTransition.Value;

        protected override void OnInit() => Transistor.controller = this;

        protected override void OnTeardown() => Transistor.controller = null;

        public void SetInTransition(TransitionIdentifier transitionId)
        {
            // Find the transition by its identifier
            if (TryGetTransition(transitionId, out var transition))
            {
                // Only set the transition if it was found
                inTransition.SetValue(transition);
            }
        }

        public void SetOutTransition(TransitionIdentifier transitionId)
        {
            // Find the transition by its identifier
            if (TryGetTransition(transitionId, out var transition))
            {
                // Only set the transition if it was found
                outTransition.SetValue(transition);
            }
        }

        public virtual async Task AnimateTransitionIn(bool realTime = false)
        {
            // Set the in transition state to true before starting the in transition
            InTransition.SetTransitionState(true);

            // Enable the transition camera if it exists
            if (transitionCamera != null)
            {
                // Enable the transition camera
                transitionCamera.gameObject.SetActive(true);

                // Try to get the main camera, and if it exists, copy its position and rotation to the transition camera
                var mainCamera = Camera.main;

                // Copy the main camera position and rotation to the transition camera
                if (mainCamera != null) CopyCameraSettings(Camera.main);
            }

            // Await the in transition animation
            await InTransition.AnimateTransitionIn(realTime);
        }

        public virtual async Task AnimateTransitionOut(bool realTime = false)
        {
            // Set the in transition state to false before starting the out transition
            InTransition.SetTransitionState(false);

            // Enable the transition camera if it exists
            if (transitionCamera != null)
            {
                // Enable the transition camera
                transitionCamera.gameObject.SetActive(true);

                // Try to get the main camera, and if it exists, copy its position and rotation to the transition camera
                var mainCamera = Camera.main;

                // Copy the main camera position and rotation to the transition camera
                if (mainCamera != null) CopyCameraSettings(Camera.main);
            }

            // Await the out transition animation
            await OutTransition.AnimateTransitionOut(realTime);

            // Disable the transition camera if it exists
            if (transitionCamera != null) transitionCamera.enabled = false;
        }

        public virtual float GetDuration() => Mathf.Max(InTransition.GetDuration(), OutTransition.GetDuration());

        private bool TryGetTransition(TransitionIdentifier transitionId, out ITransition transition)
        {
            // Find the transition by its identifier
            transition = availableTransitions.Find(t => t.Value.GetIdentifier() == transitionId).Value;

            // Return whether the transition was found
            return transition != null;
        }

        private void CopyCameraSettings(Camera camera)
        {
            transitionCamera.transform.SetPositionAndRotation(camera.transform.position, camera.transform.rotation);
            transitionCamera.fieldOfView = camera.fieldOfView;
        }

        [ContextMenu("Get Transitions")]
        private void GetTransitions()
        {
            // Get all transitions in under the TransitionController in the scene
            var transitions = GetComponentsInChildren<ITransition>();

            // Clear the available transitions list
            availableTransitions.Clear();

            // Add all transitions to the available transitions list
            foreach (var transition in transitions) availableTransitions.Add(InterfaceReference<ITransition>.FromValue(transition));
        }
    }
}
