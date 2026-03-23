using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper
{
    public class TransitionController : TransitionAnimation
    {
        [Header("Transition Camera")]
        public Camera transitionCamera;

        [Header("Current Transitions")]
        public TransitionAnimation inTransition;
        public TransitionAnimation outTransition;

        [Header("Available Transitions")]
        public List<TransitionAnimation> availableTransitions;

        public override async Task AnimateTransitionIn(bool realTime = false)
        {
            // Set the in transition state to true before starting the in transition
            inTransition.SetTransitionState(true);

            // Enable the transition camera if it exists
            if (transitionCamera != null)
            {
                // Enable the transition camera
                transitionCamera.gameObject.SetActive(true);

                // Copy the main camera position and rotation to the transition camera
                CopyCameraSettings(Camera.main);
            }

            // Await the in transition animation
            await inTransition.AnimateTransitionIn(realTime);
        }

        public override async Task AnimateTransitionOut(bool realTime = false)
        {
            // Set the in transition state to false before starting the out transition
            inTransition.SetTransitionState(false);

            // Enable the transition camera if it exists
            if (transitionCamera != null)
            {
                // Enable the transition camera
                transitionCamera.gameObject.SetActive(true);

                // Copy the main camera position and rotation to the transition camera
                CopyCameraSettings(Camera.main);
            }

            // Await the out transition animation
            await outTransition.AnimateTransitionOut(realTime);

            // Disable the transition camera if it exists
            if (transitionCamera != null) transitionCamera.enabled = false;
        }

        public override void SetTransitionState(bool status) { }

        public override float GetDuration() => Mathf.Max(inTransition.GetDuration(), outTransition.GetDuration());

        public void SetInTransition(TransitionAnimation transition) => inTransition = transition;

        public void SetOutTransition(TransitionAnimation transition) => outTransition = transition;

        public void SetInTransition(TransitionIdentifier transitionId)
        {
            // Find the transition by its identifier
            if (TryGetTransition(transitionId, out TransitionAnimation transition))
            {
                // Only set the transition if it was found
                inTransition = transition;
            }
        }

        public void SetOutTransition(TransitionIdentifier transitionId)
        {
            // Find the transition by its identifier
            if (TryGetTransition(transitionId, out TransitionAnimation transition))
            {
                // Only set the transition if it was found
                outTransition = transition;
            }
        }

        private bool TryGetTransition(TransitionIdentifier transitionId, out TransitionAnimation transition)
        {
            // Find the transition by its identifier
            transition = availableTransitions.Find(t => t.GetIdentifier() == transitionId);

            // Return whether the transition was found
            return transition != null;
        }

        public TransitionAnimation GetTransition(string name) => availableTransitions.Find(t => t.name.Equals(name));

        public TransitionAnimation GetTransition(int index) => availableTransitions[index];

        private void CopyCameraSettings(Camera camera)
        {
            transitionCamera.transform.position = camera.transform.position;
            transitionCamera.transform.rotation = camera.transform.rotation;
            transitionCamera.fieldOfView = camera.fieldOfView;
        }
    }
}
