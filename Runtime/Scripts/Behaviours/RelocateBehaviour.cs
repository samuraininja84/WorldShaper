using System.Threading.Tasks;
using UnityEngine;

namespace WorldShaper
{
    public class RelocateBehaviour : MonoBehaviour, IBehaviour
    {
        [Header("Intialization")]
        public ObjectLocator target = ObjectLocator.Default;
        public Vector3 positionOffset;

        public Vector3 Position => transform.position + positionOffset;

        public virtual Task OnActivate()
        {
            // Ensure the target reference is valid
            target.FindIfNull();

            // Set the target position
            target.Set(Position);

            // Return a completed task
            return Task.CompletedTask;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw the position 
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(Position, 0.1f);
        }
    }
}
