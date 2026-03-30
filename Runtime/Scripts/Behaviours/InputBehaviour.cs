using System;
using System.Threading.Tasks;
using UnityEngine;
using Puppeteer;

namespace WorldShaper
{
    public class InputBehaviour : MonoBehaviour, IBehaviour
    {
        [Header("Target")]
        public ObjectLocator target = ObjectLocator.Default;

        [Header("Entry")]
        public ThreadBase enterInteraction;
        public Optional<float> entryDuration = Optional<float>.Some(0.5f);

        [Header("Exit")]
        public ThreadBase exitInteraction;
        public Optional<float> exitDuration = Optional<float>.Some(0.5f);

        [Header("State")]
        public bool hasExited = false;

        /// <summary>
        /// Waits for the entry trigger condition to be met.
        /// </summary>
        private bool WaitForEntryTrigger => !entryDuration.Enabled || entryDuration.Value <= 0f;

        /// <summary>
        /// Waits for the exit trigger condition to be met.
        /// </summary>
        private bool WaitForExitTrigger => !exitDuration.Enabled || exitDuration.Value <= 0f;

        public async virtual Task OnEnter()
        {
            // Check if there is an exit interaction
            if (enterInteraction != null)
            {
                // Trigger the enter interaction
                enterInteraction.AddListener();

                // Check if we need to wait for the entry trigger
                if (WaitForEntryTrigger)
                {
                    // Wait until the interaction is complete
                    while (!hasExited) await Task.Yield();
                }
                else
                {
                    // Wait for the duration
                    await Task.Delay(TimeSpan.FromSeconds(entryDuration.Value));
                }

                // Remove the listener
                enterInteraction.RemoveListener();
            }

            // Set hasExited to false for future interactions
            hasExited = false;

            // Return a completed task
            await Task.CompletedTask;
        }

        public async virtual Task OnExit()
        {
            // Check if there is an exit interaction
            if (exitInteraction != null)
            {
                // Trigger the exit interaction
                exitInteraction.AddListener();

                // Check if we need to wait for the exit trigger
                if (WaitForExitTrigger)
                {
                    // Wait until the interaction is complete
                    while (hasExited) await Task.Yield();
                }
                else
                {
                    // Wait for the duration
                    await Task.Delay(TimeSpan.FromSeconds(exitDuration.Value));
                }

                // Remove the listener
                exitInteraction.RemoveListener();
            }

            // Set hasExited to false for future interactions
            hasExited = false;

            // Return a completed task
            await Task.CompletedTask;
        }

        /// <summary>
        /// Calculates the arrow direction based on the specified input direction and camera type.
        /// </summary>
        /// <param name="direction">The input direction vector. For orthographic cameras, this is used directly. For perspective cameras, the x and y components are used to compute the direction.</param>
        /// <param name="orthographic">Indicates whether the camera is orthographic.  If <see langword="true"/>, the input direction is used as-is; otherwise, the direction is adjusted for a perspective camera.</param>
        /// <returns>A <see cref="Vector3"/> representing the calculated arrow direction.</returns>
        private Vector3 GetArrowDirection(Vector3 direction, bool orthographic = true)
        {
            // Initialize the arrow direction
            Vector3 arrowDirection = Vector3.zero;

            // Determine the arrow direction based on the camera type
            if (orthographic) arrowDirection = direction;
            else arrowDirection = new Vector3(direction.x, 0, direction.y);

            // Return the arrow direction
            return arrowDirection;
        }

        private void OnTriggerExit(Collider collision)
        {
            // Check if the target matches the exiting object
            if (target.Matching(collision.gameObject)) hasExited = true;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Check if the target matches the entering object
            if (target.Matching(collision.gameObject)) hasExited = false;
        }

        private void OnDrawGizmosSelected()
        {
            // Check if the enter interaction is null or if the move direction is zero
            if (enterInteraction != null && enterInteraction.GetMoveDirection() != Vector2.zero)
            {
                // Get the position of the passage and the direction of the enter interaction
                Vector3 pos = transform.position;
                Vector3 direction = GetArrowDirection(enterInteraction.GetMoveDirection(), Camera.main.orthographic);

                // Draw the arrow
                DrawArrow.ForGizmo(pos, direction, Color.green);
            }

            // Check if the exit interaction is null or if the move direction is zero
            if (exitInteraction != null && exitInteraction.GetMoveDirection() != Vector2.zero)
            {
                // Get the position of the passage and the direction of the exit interaction
                Vector3 pos = transform.position;
                Vector3 direction = GetArrowDirection(exitInteraction.GetMoveDirection(), Camera.main.orthographic);

                // Draw the arrow
                DrawArrow.ForGizmo(pos, direction, Color.blue);
            }
        }
    }
}
