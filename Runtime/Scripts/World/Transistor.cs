using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace WorldShaper
{
    public class Transistor : PersistentSingleton<Transistor>
    {
        [Header("Player")]
        public GameObject player;

        [Header("Passage Info")]
        public string startPoint;
        public string endPoint;
        public List<Connectable> connectables;

        [Header("Areas")]
        public AreaHandle currentArea;
        public AreaHandle[] areaHandles;

        [Header("Transitions")]
        public TransitionAnimation[] transitions;

        [Header("UI")]
        public Slider progressBar;
        public bool showProgressBar = false;

        public static Action OnTransitionStarted;
        public static Action OnTransitionCompleted;
        public static Action<string> OnEndPassageChanged;
        private AsyncOperation scene;

        protected override void Awake()
        {
            base.Awake();
            if (transitions == null)
            {
                transitions = gameObject.GetComponentsInChildren<TransitionAnimation>();
            }
        }

        private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

        private void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // If the scene is marked as ignored, return
            if (IgnoredScene(scene)) return;

            // Get the player if it is null
            if (player == null) GetPlayer();

            // Move the player to the spawn point
            if (CanMovePlayer())
            {
                // Get the end point from the current area connections
                if (endPoint == string.Empty) endPoint = FindConnectable(currentArea, startPoint);
                else if (endPoint != string.Empty) connectables = GetAllConnectables();
                player.transform.position = FindSpawnPointByValue(endPoint);
                GetConnectableByValue(endPoint).SetCanInteract(false);

                // Invoke the passage changed event
                OnEndPassageChanged?.Invoke(endPoint);
            }

            // Invoke the transition completed event
            OnTransitionCompleted?.Invoke();
        }

        private bool IgnoredScene(Scene scene)
        {
            // Check if the matching scene has no connections, if it does return true
            foreach (AreaHandle areaHandle in areaHandles)
            {
                // Check if the area handle scene path matches the loaded scene name
                if (areaHandle.currentScene.Path == scene.name)
                {
                    // Return true if the area handle has no connections
                    if (!areaHandle.HasConnections()) return true;
                    Debug.Log("Scene is ignored: " + scene.name);
                    break;
                }
            }

            // Return false if the scene is not ignored
            return false;
        }

        #region Change Area Methods

        public void ReloadCurrentArea()
        {
            // Clear the start passage
            startPoint = string.Empty;

            // Get the current scene name
            Scene scene = SceneManager.GetActiveScene();

            // Get the area name from the area handle
            string areaName = scene.name;

            // If the current area is null, get the current area from the scene name
            if (currentArea == null || currentArea.currentScene.Name != areaName)
            {
                // Intialize the area handle as null
                AreaHandle areaHandle = null;

                // Find the area handle by the scene with the matching name
                foreach (AreaHandle handle in areaHandles)
                {
                    if (handle.currentScene.Name == areaName)
                    {
                        areaHandle = handle;
                        break;
                    }
                }

                // Set the current area as the area handle
                currentArea = areaHandle;
            }

            // Get the end passage name from the current area connections as the first connection
            if (endPoint == string.Empty && currentArea.HasConnections())
            {
                startPoint = currentArea.connections[0].passage.value;
                endPoint = currentArea.connections[0].connectionName;
            }
            else if (endPoint != string.Empty && currentArea.HasConnections())
            {
                startPoint = currentArea.GetConnection(endPoint).passage.value;
            }

            // Change the scene with the current area name
            StartCoroutine(ExecuteAreaTransition(currentArea.currentScene.Name, "CrossFade"));
        }

        public void ChangeArea(AreaHandle areaHandle, string areaName, string passageName, string transitionName)
        {
            ConfigurePassageData(areaHandle, passageName);
            StartCoroutine(ExecuteAreaTransition(areaName, transitionName));
        }

        public void ChangeArea(string areaName, int endPointIndex = 0, string transitionName = "CrossFade")
        {
            // Intialize the area handle as null
            AreaHandle areaHandle = null;

            // Find the area handle by the scene with the matching name
            foreach (AreaHandle handle in areaHandles)
            {
                if (handle.currentScene.Name == areaName)
                {
                    areaHandle = handle;
                    break;
                }
            }

            // Get the start and end point names from the area handle connections based on the end point index
            if (areaHandle.HasConnections())
            {
                string startPointName = areaHandle.connections[endPointIndex].passage.value;
                string endPointName = areaHandle.connections[endPointIndex].connectionName;
                ConfigurePassageData(areaHandle, startPointName, endPointName);
            }

            // Change the scene
            StartCoroutine(ExecuteAreaTransition(areaName, transitionName));
        }

        public void ChangeArea(string areaName, string endPointName = null, string transitionName = "CrossFade")
        {
            // Intialize the area handle as null
            AreaHandle areaHandle = null;

            // Find the area handle by the scene with the matching name
            foreach (AreaHandle handle in areaHandles)
            {
                if (handle.currentScene.Name == areaName)
                {
                    areaHandle = handle;
                    break;
                }
            }

            // If the end point name is null, use the first connection from the area handle to set the start and end points, otherwise use the end point name to get the start point name
            if (areaHandle.HasConnections())
            {
                // Intialize the start point name as an empty string
                string startPointName = string.Empty;
                if (endPointName == null)
                {
                    startPointName = areaHandle.connections[0].passage.value;
                    endPointName = areaHandle.connections[0].connectionName;
                }
                else if (endPointName != null && areaHandle.HasConnections())
                {
                    Connection connection = areaHandle.GetConnection(endPointName);
                    startPointName = connection.passage.value;
                    endPointName = connection.connectionName;
                }

                // Set the passage data
                ConfigurePassageData(areaHandle, startPointName, endPointName);
            }

            // Change the scene
            StartCoroutine(ExecuteAreaTransition(areaName, transitionName));
        }

        public void ChangeArea(int areaIndex, int endPointIndex = 0, string transitionName = "CrossFade")
        {
            // Get the area handle from the areas array by index
            AreaHandle areaHandle = areaHandles[areaIndex];

            // Get the area name from the area handle
            string areaName = areaHandle.currentScene.Name;

            // Get the start and end point names from the area handle connections based on the end point index
            if (areaHandle.HasConnections())
            {
                string startPointName = areaHandle.connections[endPointIndex].passage.value;
                string endPointName = areaHandle.connections[endPointIndex].connectionName;
                ConfigurePassageData(areaHandle, startPointName, endPointName);
            }

            // Change the scene
            StartCoroutine(ExecuteAreaTransition(areaName, transitionName));
        }

        public void ChangeArea(int areaIndex, string endPointName = null, string transitionName = "CrossFade")
        {
            // Get the area handle from the areas array by index
            AreaHandle areaHandle = areaHandles[areaIndex];

            // Get the area name from the area handle
            string areaName = areaHandle.currentScene.Name;

            // If the end point name is null, use the first connection from the area handle to set the start and end points, otherwise use the end point name to get the start point name
            if (areaHandle.HasConnections())
            {
                // Intialize the start point name as an empty string
                string startPointName = string.Empty;

                if (endPointName == null)
                {
                    startPointName = areaHandle.connections[0].passage.value;
                    endPointName = areaHandle.connections[0].connectionName;
                }
                else if (endPointName != null && areaHandle.HasConnections())
                {
                    Connection connection = areaHandle.GetConnection(endPointName);
                    startPointName = connection.passage.value;
                    endPointName = connection.connectionName;
                }

                // Set the passage data
                ConfigurePassageData(areaHandle, startPointName, endPointName);
            }

            // Change the scene
            StartCoroutine(ExecuteAreaTransition(areaName, transitionName));
        }

        private IEnumerator ExecuteAreaTransition(string areaName, string transitionName)
        {
            // If the scene is already loading, return
            if (scene != null) yield break;

            // Invoke the transition started event
            OnTransitionStarted?.Invoke();

            // Get the transition animation from the transitions array by name
            TransitionAnimation transition = transitions.First(t => t.name == transitionName);

            // Delay the rest of the method until the transition has finished animating in
            yield return transition.AnimateTransitionIn();

            // Get the scene async operation and set it to not allow scene activation
            scene = SceneManager.LoadSceneAsync(areaName);
            scene.allowSceneActivation = false;

            // Show the progress bar if it is enabled
            if (showProgressBar) progressBar.gameObject.SetActive(true);

            // While the scene progress is less than 0.9, update the progress bar
            do
            {
                if (showProgressBar)
                {
                    progressBar.value = scene.progress;
                }
                yield return null;
            }
            while (scene.progress < 0.9f);

            // Wait for a second just to make sure the progress bar is full
            yield return new WaitForSeconds(1f);

            // Allow the scene to be activated
            scene.allowSceneActivation = true;

            // Hide the progress bar if it is enabled
            if (showProgressBar) progressBar.gameObject.SetActive(false);

            // Set the async operation to null
            scene = null;

            // Animate the transition out and wait for it to finish
            yield return transition.AnimateTransitionOut();
        }

        #endregion

        #region Passage Methods

        private void ConfigurePassageData(AreaHandle areaHandle, string startPassageName, string endPassageName = null)
        {
            currentArea = areaHandle;
            startPoint = startPassageName;
            if (endPassageName == null) endPoint = string.Empty;
            else endPoint = endPassageName;
        }

        private string FindConnectable(AreaHandle areaHandle, string passageName)
        {
            // Create the linked connectable string
            string linkedConnectable = string.Empty;

            // Get the connection with the matching passage name from the area handle
            if (areaHandle.ConnectionExists(passageName))
            {
                // Get the connection with the matching passage name from the area handle
                Connection connection = areaHandle.GetConnection(passageName);
                string endPointName = connection.passage.value;

                // Get the matching connectable from the area handle
                connectables = GetAllConnectables();
                if (connectables.Count > 1)
                {
                    // If there are multiple connectables, find the linked connectable with the matching value
                    foreach (Connectable connectable in connectables)
                    {
                        if (endPointName == connectable.GetValue())
                        {
                            linkedConnectable = connectable.GetValue();
                        }
                    }
                }
                else
                {
                    // If there is only one connectable, set the linked connectable as the first connectable value
                    linkedConnectable = connectables[0].GetValue();
                }
            }

            // Return the linked connectable
            return linkedConnectable;
        }

        private Vector3 FindSpawnPointByValue(string value)
        {
            // Initialize the spawn point
            Transform spawnPoint = null;

            // Get the spawn point from the connectable with the matching value
            Connectable passage = GetConnectableByValue(value);
            spawnPoint = passage.gameObject.transform;

            // Get the spawn location
            Vector3 spawnLocation = player.transform.position;
            if (spawnPoint != null)
            {
                spawnLocation = spawnPoint.position;
                Collider2D collider = player.GetComponent<Collider2D>();
                spawnLocation.y -= collider.offset.y;
            }

            // Return the spawn location
            return spawnLocation;
        }

        private Connectable GetConnectableByValue(string value)
        {
            // Get all the connectables in the scene, then find the connectable with the matching value
            foreach (Connectable connectable in connectables)
            {
                if (connectable.GetValue() == value)
                {
                    return connectable;
                }
            }

            return null;
        }

        private List<Connectable> GetAllConnectables()
        {
            List<Connectable> connectables = new List<Connectable>();
            foreach (Connectable connectable in FindObjectsOfType<Connectable>())
            {
                connectables.Add(connectable);
            }
            return connectables;
        }

        private List<string> GetAllConnectableValues()
        {
            // Create the list of connectable values
            List<string> values = new List<string>();

            // Get all the connectables in the scene
            foreach (Connectable connectable in connectables)
            {
                values.Add(connectable.GetValue());
            }

            // Return the list of connectable values
            return values;
        }

        #endregion

        #region Connection Methods

        public string GetFirstConnection(string areaHandleName)
        {
            // Find the area handle by name
            AreaHandle areaHandle = areaHandles.First(a => a.name == areaHandleName);

            // Get the first connection name from the area handle
            if (areaHandle.HasConnections())
            {
                return areaHandle.connections[0].connectionName;
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetFirstConnection(int areaIndex)
        {
            // Get the area handle by index
            AreaHandle areaHandle = areaHandles[areaIndex];

            // Get the first connection name from the area handle
            if (areaHandle.HasConnections())
            {
                return areaHandle.connections[0].connectionName;
            }
            else
            {
                return string.Empty;
            }
        }

        #endregion

        #region Player Methods

        private GameObject GetPlayer()
        {
            GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
            if (foundPlayer != null)
            {
                player = foundPlayer;
            }
            else
            {
                player = GameObject.Find("Player");
                if (player == null)
                {
                    Debug.LogWarning("No player found in the scene. Please check that they are is tagged or named Player");
                }
            }

            return player;
        }

        private bool CanMovePlayer()
        {
            return player != null && startPoint != string.Empty && currentArea != null;
        }

        #endregion
    }
}