using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WorldShaper
{
    public class LoadPersistent : MonoBehaviour
    {
        public WorldMap worldMap;

        private async void Awake() 
        {
            // Find the world map in the resources
            if (worldMap == null) worldMap = Resources.FindObjectsOfTypeAll<WorldMap>().FirstOrDefault();

            // Load all persistent scenes defined in the world map
            foreach (var scene in worldMap.PersistentScenes)
            {
                // Check if the persistent scene is already loaded, if so, continue
                if (SceneManager.GetSceneByName(scene.Name).IsValid()) continue;

                // Load the persistent scene asynchronously in single mode
                await SceneManager.LoadSceneAsync(scene.Path, LoadSceneMode.Additive);
            }
        }
    }
}
