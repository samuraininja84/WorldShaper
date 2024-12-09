using System.Collections.Generic;
using UnityEngine;
using Eflatun.SceneReference;
using InputReader;

namespace WorldShaper
{
    public class Passage : MonoBehaviour
    {
        public AreaHandle area;
        public ExtendableEnum passage;
        public InputMiddleware enterInteraction;
        public InputMiddleware exitInteraction;
        public bool canInteract = true;

        public string Value => passage.value;

        private void OnValidate()
        {
            CreateConnectionList();
        }

        public void SetAreaHandle(AreaHandle handle)
        {
            area = handle;
            CreateConnectionList(true);
        }

        public void CreateConnectionList(bool refresh = false)
        {
            if (!refresh)
            {
                passage.SetEnums(GetPassagesFromAreaHandle(area));
            }
            else
            {
                passage = new ExtendableEnum(GetPassagesFromAreaHandle(area));
            }
        }

        private List<string> GetPassagesFromAreaHandle(AreaHandle handle)
        {
            // Create the list of connections
            List<string> connections = new List<string> { "None" };

            // Check if the handle is null or if it has no connections
            if (handle == null || handle.connections.Count == 0)
            {
                return connections;
            }
            else
            {
                // Create a list of connection names
                connections = new List<string>();
                foreach (var connectionData in handle.connections)
                {
                    connections.Add(connectionData.connectionName);
                }
                return connections;
            }
        }

        public SceneReference GetSceneFromAreaHandle()
        {
            if (area == null)
            {
                return null;
            }
            else
            {
                foreach (var connection in area.connections)
                {
                    if (connection.connectionName == Value)
                    {
                        return connection.connectedScene.currentScene;
                    }
                }
                return null;
            }
        }

        public InputMiddleware GetInteraction()
        {
            // Create the interaction and set it to null
            InputMiddleware interaction = null;

            // Check if the player can interact before setting the appropriate interaction
            if (canInteract && enterInteraction != null)
            {
                if (exitInteraction != null) interaction = exitInteraction;
            }
            else if (enterInteraction != null)
            {
                interaction = enterInteraction;
            }

            // Return the interaction
            return interaction;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && !ListenerActive())
            {
                // Add the appropriate listener, if it is not null
                if (GetInteraction() != null)
                {
                    // Add the listener
                    GetInteraction().AddListener();

                    // Load the area
                    if (canInteract) LoadArea();
                }
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player") && ListenerActive())
            {
                // Remove the active listener, if it is not null
                if (ListenerActive()) GetInteraction().RemoveListener();

                // Set the player to be able to interact if it is not already
                if (!canInteract) canInteract = true;
            }
        }

        private void LoadArea()
        {
            // Get the scene reference and check if it is null
            SceneReference scene = GetSceneFromAreaHandle();
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
                string passageName = Value;

                // Load the scene
                Transistor.Instance.ChangeArea(area, areaName, passageName, "CrossFade");
            }
        }

        private bool ListenerActive()
        {
            if (GetInteraction() != null) return GetInteraction().ListenerActive();
            return false;
        }

        private void OnDrawGizmos()
        {
            // Check if the exit interaction is null or if the move direction is zero
            if (exitInteraction == null || exitInteraction.inputState.moveDirection == Vector2.zero) return;

            // Get the position of the passage and the direction of the passage
            Vector3 pos = transform.position;
            Vector3 direction = exitInteraction.inputState.moveDirection;

            // Draw the arrow
            DrawArrow.ForGizmo2D(pos, direction, Color.green);
        }
    }
}
