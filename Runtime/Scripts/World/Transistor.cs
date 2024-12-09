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
        public string startPassage;
        public string endPassage;
        public List<Passage> passages;

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
                if (endPassage == string.Empty) endPassage = FindPassage(currentArea, startPassage);
                else if (endPassage != string.Empty) passages = GetAllPassages();
                player.transform.position = FindSpawnPointByValue(endPassage);
                GetPassageByValue(endPassage).canInteract = false;
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
            startPassage = string.Empty;

            // If the current area is null, get the current area from the scene name
            if (currentArea == null)
            {
                // Get the current scene name
                Scene scene = SceneManager.GetActiveScene();

                // Get the area name from the area handle
                string areaName = scene.name + "_Handle";

                // Get the area handle from the areas array by name
                AreaHandle areaHandle = areaHandles.First(a => a.name == areaName);

                // Set the current area as the area handle
                currentArea = areaHandle;
            }

            // Get the end passage name from the current area connections as the first connection
            if (endPassage == string.Empty && currentArea.HasConnections())
            {
                startPassage = currentArea.connections[1].passage.value;
                endPassage = currentArea.connections[1].connectionName;
            }
            else if (endPassage != string.Empty && currentArea.HasConnections())
            {
                startPassage = currentArea.GetConnection(endPassage).passage.value;
            }

            // Change the scene with the current area name
            StartCoroutine(ChangeArea(currentArea.currentScene.Name, "CrossFade"));
        }

        public void ChangeArea(AreaHandle areaHandle, string areaName, string passageName, string transitionName)
        {
            SetPassageData(areaHandle, passageName);
            StartCoroutine(ChangeArea(areaName, transitionName));
        }

        public void ChangeArea(string areaHandleName, int passageIndex = 0, string transitionName = "CrossFade")
        {
            // Create the handle name
            string handleName = areaHandleName;

            // Add the _Handle to the area handle name if it does not contain it
            if (!areaHandleName.Contains("_Handle")) handleName = areaHandleName + "_Handle";

            // Get the area handle from the areas array by name         
            AreaHandle areaHandle = areaHandles.First(a => a.name == handleName);

            // Get the area name from the area handle
            string areaName = areaHandle.currentScene.Name;

            // Get the passage name from the area handle connections based on the passage index
            if (areaHandle.HasConnections())
            {
                string startPassageName = areaHandle.connections[passageIndex].passage.value;
                string endPassageName = areaHandle.connections[passageIndex].connectionName;
                SetPassageData(areaHandle, startPassageName, endPassageName);
            }

            // Change the scene
            StartCoroutine(ChangeArea(areaName, transitionName));
        }

        public void ChangeArea(string areaHandleName, string endPassageName = null, string transitionName = "CrossFade")
        {
            // Create the handle name
            string handleName = areaHandleName;

            // Add the _Handle to the area handle name if it does not contain it
            if (!areaHandleName.Contains("_Handle")) handleName = areaHandleName + "_Handle";

            // Get the area handle from the areas array by name
            AreaHandle areaHandle = areaHandles.First(a => a.name == handleName);

            // Get the area name from the area handle
            string areaName = areaHandle.currentScene.Name;

            // If the passage name is null, get the first connection from the area handle
            if (areaHandle.HasConnections())
            {
                // Set the start passage name
                string startPassageName = string.Empty;

                if (endPassageName == null)
                {
                    startPassageName = areaHandle.connections[0].passage.value;
                    endPassageName = areaHandle.connections[0].connectionName;
                }
                else if (endPassageName != null && areaHandle.HasConnections())
                {
                    Connection connection = areaHandle.GetConnection(endPassageName);
                    startPassageName = connection.passage.value;
                    endPassageName = connection.connectionName;
                }

                // Set the passage data
                SetPassageData(areaHandle, startPassageName, endPassageName);
            }

            // Change the scene
            StartCoroutine(ChangeArea(areaName, transitionName));
        }

        public void ChangeArea(int areaIndex, string endPassageName = null, string transitionName = "CrossFade")
        {
            // Get the area handle from the areas array by index
            AreaHandle areaHandle = areaHandles[areaIndex];

            // Get the area name from the area handle
            string areaName = areaHandle.currentScene.Name;

            // If the passage name is null, get the first connection from the area handle
            if (areaHandle.HasConnections())
            {
                // Set the start passage name
                string startPassageName = string.Empty;

                if (endPassageName == null)
                {
                    startPassageName = areaHandle.connections[0].passage.value;
                    endPassageName = areaHandle.connections[0].connectionName;
                }
                else if (endPassageName != null && areaHandle.HasConnections())
                {
                    Connection connection = areaHandle.GetConnection(endPassageName);
                    startPassageName = connection.passage.value;
                    endPassageName = connection.connectionName;
                }

                // Set the passage data
                SetPassageData(areaHandle, startPassageName, endPassageName);
            }

            // Change the scene
            StartCoroutine(ChangeArea(areaName, transitionName));
        }

        public void ChangeArea(int areaIndex, int passageIndex = 0, string transitionName = "CrossFade")
        {
            // Get the area handle from the areas array by index
            AreaHandle areaHandle = areaHandles[areaIndex];

            // Get the area name from the area handle
            string areaName = areaHandle.currentScene.Name;

            // Get the passage name from the area handle connections based on the passage index
            if (areaHandle.HasConnections())
            {
                string startPassageName = areaHandle.connections[passageIndex].passage.value;
                string endPassageName = areaHandle.connections[passageIndex].connectionName;
                SetPassageData(areaHandle, startPassageName, endPassageName);
            }

            // Change the scene
            StartCoroutine(ChangeArea(areaName, transitionName));
        }

        private IEnumerator ChangeArea(string areaName, string transitionName)
        {
            if (scene != null) yield break;

            // Invoke the transition started event
            OnTransitionStarted?.Invoke();

            // Get the transition animation from the transitions array by name
            TransitionAnimation transition = transitions.First(t => t.name == transitionName);

            // Get the scene async operation and set it to not allow scene activation
            scene = SceneManager.LoadSceneAsync(areaName);
            scene.allowSceneActivation = false;

            // Animate the transition in
            yield return transition.AnimateTransitionIn();

            // Show the progress bar if it is enabled
            if (showProgressBar)
            {
                progressBar.gameObject.SetActive(true);
            }

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
            if (showProgressBar)
            {
                progressBar.gameObject.SetActive(false);
            }

            // Set the async operation to null
            scene = null;

            // Animate the transition out
            yield return transition.AnimateTransitionOut();
        }

        #endregion

        #region Passage Methods

        private void SetPassageData(AreaHandle areaHandle, string startPassageName, string endPassageName = null)
        {
            currentArea = areaHandle;
            startPassage = startPassageName;
            if (endPassageName == null) endPassage = string.Empty;
            else endPassage = endPassageName;
        }

        private string FindPassage(AreaHandle areaHandle, string passageName)
        {
            // Create the linked passage string
            string linkedPassageName = string.Empty;

            // Get the connection with the matching passage name from the area handle
            if (areaHandle.ConnectionExists(passageName))
            {
                // Get the connection with the matching passage name from the area handle
                Connection connection = areaHandle.GetConnection(passageName);
                string linkedPassageValue = connection.passage.value;

                // Get the matching passage from the area handle
                passages = GetAllPassages();
                if (passages.Count > 1)
                {
                    // If there are multiple passages, find the connected passage with the matching name
                    foreach (Passage passage in passages)
                    {
                        if (linkedPassageValue == passage.Value)
                        {
                            linkedPassageName = passage.Value;
                        }
                    }
                }
                else
                {
                    // If there is only one passage, set the connected passage name as the first passage name
                    linkedPassageName = passages[0].Value;
                }
            }

            // Invoke the passage changed event
            OnEndPassageChanged?.Invoke(linkedPassageName);

            // Return the passage name
            return linkedPassageName;
        }

        private Vector3 FindSpawnPointByValue(string value)
        {
            // Initialize the spawn point
            Transform spawnPoint = null;

            // Get the spawn point from the passage name
            Passage passage = GetPassageByValue(value);
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

        private Passage GetPassageByValue(string value)
        {
            // Get all the passages in the scene, then find the passage with the matching value
            foreach (Passage passage in passages)
            {
                if (passage.Value == value)
                {
                    return passage;
                }
            }

            return null;
        }

        private List<Passage> GetAllPassages()
        {
            List<Passage> passages = new List<Passage>();
            foreach (Passage passage in FindObjectsOfType<Passage>())
            {
                passages.Add(passage);
            }

            return passages;
        }

        private List<string> GetAllPassageValues()
        {
            // Create the list of passage values
            List<string> values = new List<string>();

            // Get all the passages in the scene
            foreach (Passage passage in passages)
            {
                values.Add(passage.Value);
            }

            // Return the list of passage values
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
            return player != null && startPassage != string.Empty && currentArea != null;
        }

        #endregion
    }
}