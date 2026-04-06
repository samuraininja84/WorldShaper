using System.Threading.Tasks;
using UnityEngine;

namespace WorldShaper
{
    public class Passage : BaseLocationPointer
    {
        public ConnectionReference passage;
        public PassageType type;
        public bool canInteract = true;

        [Header("Intialization")]
        public ObjectLocator target = ObjectLocator.Default;
        public Vector3 positionOffset;

        [Header("Behaviours")]
        public InterfaceReference<IBehaviour>[] onInitializeMethods = null;
        public InterfaceReference<IBehaviour>[] onActivateMethods = null;
        public InterfaceReference<IBehaviour>[] onEnterMethods = null;
        public InterfaceReference<IBehaviour>[] onExitMethods = null;

        public AreaHandle Area => passage.Area;

        private bool CanUsePassage => canInteract && type == PassageType.Open;

        public override void SetActive(bool status) => canInteract = status;

        public async override Task Initialize()
        {
            // Check if there are no methods to initialize
            if (onInitializeMethods == null || onInitializeMethods.Length == 0)
            {
                // Return a completed task
                await Task.CompletedTask;

                // No methods to initialize, return early
                return;
            }

            // Await each method's OnInitialize if it has a value
            foreach (var method in onInitializeMethods)
            {
                // Await each method's OnInitialize if it has a value
                if (method.HasValue) await method.Value.OnInitialize();
            }
        }

        public async override Task Activate()
        {
            // Ensure the target reference is valid
            target.FindIfNull();

            // Set the target position
            target.Set(GetPosition());

            // Check if there are no methods to activate
            if (onActivateMethods == null || onActivateMethods.Length == 0)
            {
                // Return a completed task
                await Task.CompletedTask;

                // No methods to activate, return early
                return;
            }

            // Await each method's OnActivate if it has a value
            foreach (var method in onActivateMethods)
            {
                // Await each method's OnActivate if it has a value
                if (method.HasValue) await method.Value.OnActivate();
            }
        }

        public async override Task Enter()
        {
            // Check if there are no methods to enter
            if (onEnterMethods == null || onEnterMethods.Length == 0)
            {
                // Return a completed task
                await Task.CompletedTask;

                // No methods to enter, return early
                return;
            }

            // Await each method's OnEnter if it has a value
            foreach (var method in onEnterMethods)
            {
                // Await each method's OnEnter if it has a value
                if (method.HasValue) await method.Value.OnEnter();
            }
        }

        public async override Task Exit()
        {
            // Check if there are no methods to exit
            if (onExitMethods == null || onExitMethods.Length == 0)
            {
                // Return a completed task
                await Task.CompletedTask;

                // No methods to exit, return early
                return;
            }

            // Await each method's OnExit if it has a value
            foreach (var method in onExitMethods)
            {
                // Await each method's OnExit if it has a value
                if (method.HasValue) await method.Value.OnExit();
            }
        }

        public override Vector3 GetPosition() => transform.position + positionOffset;

        public override string GetEndpoint() => passage.Value;

        private async void TriggerEnter(GameObject gameObject)
        {
            // Check if the target matches and the player can use the passage, then load the destination area
            if (target.Matching(gameObject) && CanUsePassage)
            {
                // Await the exit of the current area
                await Exit();

                // Load the destination area through the passage reference
                passage.LoadDestination();
            }
        }

        private void TriggerExit(GameObject gameObject)
        {
            // Check if the target matches, then set the player to be able to interact again
            if (target.Matching(gameObject)) canInteract = true;
        }

        private void OnTriggerEnter(Collider collision) => TriggerEnter(collision.gameObject);

        private void OnTriggerExit(Collider collision) => TriggerExit(collision.gameObject);

        private void OnTriggerEnter2D(Collider2D collision) => TriggerEnter(collision.gameObject);

        private void OnTriggerExit2D(Collider2D collision) => TriggerExit(collision.gameObject);

        private void OnDrawGizmos()
        {
            // Set the gizmo color
            Gizmos.color = Color.cyan;

            // Draw a wire sphere at the passage position
            Gizmos.DrawWireSphere(GetPosition(), 0.25f);
        }
    }

    public enum PassageType
    {
        [Tooltip("Passage Can Be Entered Into & Exited From")]
        Open,
        [Tooltip("Passage Can Be Exited From")]
        Closed
    }
}
