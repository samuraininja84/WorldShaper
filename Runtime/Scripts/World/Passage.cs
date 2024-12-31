using UnityEngine;
using Puppeteer;
using Eflatun.SceneReference;

namespace WorldShaper
{
    public class Passage : Connectable
    {
        public PassageData passage;
        public PassageType type;
        public ThreadBase enterInteraction;
        public ThreadBase exitInteraction;
        public bool canInteract = true;

        public AreaHandle Area => passage.Area;

        private void OnValidate()
        {
            if (type == PassageType.Closed) exitInteraction = null;
        }

        private void LoadArea()
        {
            // Get the scene reference and check if it is null
            SceneReference scene = GetConnectedScene();
            if (scene == null)
            {
                Debug.LogError("Scene is null");
                return;
            }
            else
            {
                // If the scene is not null, get the scene name from the reference
                string areaName = scene.Name;

                // Set the passage name
                string passageName = GetValue();

                // Load the scene
                Transistor.Instance.ChangeArea(Area, areaName, passageName, "CrossFade");
            }
        }

        private bool CanUsePassage()
        {
            bool canUsePassage = canInteract;
            switch (type)
            {
                case PassageType.Open:
                    canUsePassage = true;
                    break;
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

        private SceneReference GetConnectedScene()
        {
            if (Area == null)
            {
                return null;
            }
            else
            {
                foreach (var connection in Area.connections)
                {
                    if (connection.connectionName == GetValue())
                    {
                        return connection.connectedScene.currentScene;
                    }
                }
                return null;
            }
        }

        private ThreadBase GetInteraction()
        {
            // Create the interaction and set it to null
            ThreadBase interaction = null;

            // Check if the player can interact before setting the appropriate interaction
            if (!canInteract && enterInteraction != null)
            {
                interaction = enterInteraction;
            }
            else if (canInteract && exitInteraction != null)
            {
                interaction = exitInteraction;
            }

            // Return the interaction
            return interaction;
        }

        public override void SetCanInteract(bool status)
        {
            canInteract = status;
        }

        public override void SetPassageData(AreaHandle handle)
        {
            passage = new PassageData(handle);
        }

        public override AreaHandle GetArea()
        {
            return passage.Area;
        }

        public override string GetValue()
        {
            return passage.Value;
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.CompareTag("Player") && !ThreadActive())
            {
                // Add the appropriate listener, if it is not null
                if (CanUsePassage())
                {
                    // Add the listener
                    GetInteraction().AddListener();

                    // Load the area
                    if (canInteract) LoadArea();
                }
                else if (!CanUsePassage())
                {
                    GetInteraction().AddListener();
                }
            }
        }

        private void OnTriggerExit(Collider collision)
        {
            if (collision.CompareTag("Player") && ThreadActive())
            {
                // Remove the active listener, if it is not null
                GetInteraction().RemoveListener();

                // Set the player to be able to interact if it is not already
                if (!canInteract) canInteract = true;
            }
        }

        private void OnDrawGizmos()
        {
            // Check if the enter interaction is null or if the move direction is zero
            if (enterInteraction != null && enterInteraction.GetMoveDirection() != Vector2.zero)
            {
                // Get the position of the passage and the direction of the enter interaction
                Vector3 pos = transform.position;
                Vector3 direction = enterInteraction.GetMoveDirection();

                // Draw the arrow
                DrawArrow.ForGizmo(pos, direction, Color.blue);
            }

            // Check if the exit interaction is null or if the move direction is zero
            if (exitInteraction != null && exitInteraction.GetMoveDirection() != Vector2.zero)
            {
                // Get the position of the passage and the direction of the exit interaction
                Vector3 pos = transform.position;
                Vector3 direction = exitInteraction.GetMoveDirection();

                // Draw the arrow
                DrawArrow.ForGizmo(pos, direction, Color.green);
            }
        }
    }

    [System.Serializable]
    public struct PassageData
    {
        public AreaHandle Area;
        public string Value;
        public int Index;

        public PassageData(AreaHandle area)
        {
            Area = area;
            Value = "";
            Index = 0;
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
