using System.Threading.Tasks;
using UnityEngine;
using Puppeteer;

namespace WorldShaper
{
    public class Passage : BaseConnectable
    {
        public ConnectionReference passage;
        public PassageType type;

        [Header("Intialization")]
        public ObjectLocator target = ObjectLocator.Default;
        public Vector3 positionOffset;

        //[Header("Behaviours")]
        //public InterfaceReference<IBehaviour>[] onInitializeMethods;
        //public InterfaceReference<IBehaviour>[] onActivateMethods;
        //public InterfaceReference<IBehaviour>[] onEnterMethods;
        //public InterfaceReference<IBehaviour>[] onExitMethods;

        [Header("Interactions")]
        public ThreadBase enterInteraction;
        public ThreadBase exitInteraction;
        public bool canInteract = true;

        public AreaHandle Area => passage.Area;

        private void OnValidate()
        {
            // Ensure exit interaction is null if passage is closed
            if (type == PassageType.Closed) exitInteraction = null;
        }

        private void LoadArea() => passage.LoadDestination();

        private bool CanUsePassage()
        {
            bool canUsePassage = canInteract;
            switch (type)
            {
                //case PassageType.Open:
                //    canUsePassage = true;
                //    break;
                case PassageType.Closed:
                    canUsePassage = false;
                break;
            }
            return canUsePassage;
        }

        private bool ThreadActive()
        {
            if (GetInteraction() != null) return GetInteraction().ThreadActive();
            return false;
        }

        private ThreadBase GetInteraction()
        {
            // Create the interaction and set it to null
            ThreadBase interaction = null;

            // Check if the player can interact before setting the appropriate interaction
            if (!canInteract && enterInteraction != null) interaction = enterInteraction;
            else if (canInteract && exitInteraction != null) interaction = exitInteraction;

            // Return the interaction
            return interaction;
        }
        public override Task Activate()
        {
            // Ensure the target reference is valid
            target.FindIfNull();

            // Set the target position
            target.Set(GetPosition());

            // Return a completed task
            return Task.CompletedTask;
        }

        #region Placeholders

        //public async override Task Initialize()
        //{
        //    // Check if there are no methods to initialize
        //    if (onInitializeMethods.Length == 0)
        //    {
        //        // Return a completed task
        //        await Task.CompletedTask;

        //        // No methods to initialize, return early
        //        return;
        //    }

        //    // Await each method's OnInitialize if it has a value
        //    foreach (var method in onInitializeMethods)
        //    {
        //        // Await each method's OnInitialize if it has a value
        //        if (method.HasValue) await method.Value.OnInitialize();
        //    }
        //}

        //public async override Task Activate()
        //{
        //    // Check if there are no methods to activate
        //    if (onActivateMethods.Length == 0)
        //    {
        //        // Return a completed task
        //        await Task.CompletedTask;

        //        // No methods to activate, return early
        //        return;
        //    }

        //    // Await each method's OnActivate if it has a value
        //    foreach (var method in onActivateMethods)
        //    {
        //        // Await each method's OnActivate if it has a value
        //        if (method.HasValue) await method.Value.OnActivate();
        //    }
        //}

        //public async override Task Enter()
        //{
        //    // Check if there are no methods to enter
        //    if (onEnterMethods.Length == 0)
        //    {
        //        // Return a completed task
        //        await Task.CompletedTask;

        //        // No methods to enter, return early
        //        return;
        //    }

        //    // Await each method's OnEnter if it has a value
        //    foreach (var method in onEnterMethods)
        //    {
        //        // Await each method's OnEnter if it has a value
        //        if (method.HasValue) await method.Value.OnEnter();
        //    }
        //}

        //public async override Task Exit()
        //{
        //    // Check if there are no methods to exit
        //    if (onExitMethods.Length == 0)
        //    {
        //        // Return a completed task
        //        await Task.CompletedTask;
        //        // No methods to exit, return early
        //        return;
        //    }

        //    // Await each method's OnExit if it has a value
        //    foreach (var method in onExitMethods)
        //    {
        //        // Await each method's OnExit if it has a value
        //        if (method.HasValue) await method.Value.OnExit();
        //    }
        //}

        #endregion

        public override void SetActive(bool status) => canInteract = status;

        public override void AssignConnectedArea(AreaHandle handle) => passage = new ConnectionReference(handle);

        public override void AssignConnection(ConnectionReference data) => passage = data;

        public override AreaHandle GetDestinationArea() => passage.Area;

        public override string GetEndpoint() => passage.Value;

        public override Vector3 GetPosition() => transform.position + positionOffset;

        private void OnDrawGizmos()
        {
            // Set the gizmo color
            Gizmos.color = Color.cyan;

            // Draw a wire sphere at the passage position
            Gizmos.DrawWireSphere(GetPosition(), 0.25f);
        }

        private void OnTriggerEnter(Collider collision)
        {
            // Check if the target matches and the thread is not active
            if (target.Matching(collision.gameObject))
            {
                // Get the appropriate interaction
                ThreadBase interaction = GetInteraction();

                // Add the listener, if it is not null
                if (interaction != null && !ThreadActive()) interaction.AddListener();

                // Initiate area loading
                if (CanUsePassage()) LoadArea();
            }
        }

        private void OnTriggerExit(Collider collision)
        {
            // Check if the target matches and the thread is active
            if (target.Matching(collision.gameObject))
            {
                // Get the appropriate interaction
                ThreadBase interaction = GetInteraction();

                // Add the listener, if it is not null
                if (interaction != null && ThreadActive()) interaction.RemoveListener();

                // Set the player to be able to interact if it is not already
                canInteract = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Check if the target matches and the thread is not active
            if (target.Matching(collision.gameObject))
            {
                // Get the appropriate interaction
                ThreadBase interaction = GetInteraction();

                // Add the listener, if it is not null
                if (interaction != null && ThreadActive()) interaction.AddListener();

                // Initiate area loading
                if (CanUsePassage()) LoadArea();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // Check if the target matches and the thread is active
            if (target.Matching(collision.gameObject))
            {
                // Get the appropriate interaction
                ThreadBase interaction = GetInteraction();

                // Add the listener, if it is not null
                if (interaction != null && ThreadActive()) interaction.RemoveListener();

                // Set the player to be able to interact if it is not already
                canInteract = true;
            }
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
