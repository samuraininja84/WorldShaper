using System.Collections.Generic;
using UnityEngine;
using Eflatun.SceneReference;

namespace WorldShaper
{
    public class Passage : MonoBehaviour
    {
        public AreaHandle area;
        public ExtendableEnum passage;
        public bool canInteract = true;

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
                    if (connection.connectionName == passage.value)
                    {
                        return connection.connectedScene.currentScene;
                    }
                }
                return null;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                // Load the area
                if (canInteract) LoadArea();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
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
                string passageName = passage.value;

                // Load the scene
                Transistor.Instance.ChangeArea(area, areaName, passageName, "CrossFade");
            }
        }
    }
}
