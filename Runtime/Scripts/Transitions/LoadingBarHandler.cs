using UnityEngine;
using UnityEngine.UI;

namespace WorldShaper
{
    public class LoadingBarHandler : MonoBehaviour
    {
        [Header("UI")]
        public Slider progressBar;
        public WorldMap worldMap;

        private bool HasProgressBar => progressBar != null;

        private void OnEnable()
        {
            worldMap.transitionProgress.OnProgressUpdated += OnProgressed;
            worldMap.transitionProgress.OnCompletion += OnCompletion;
        }

        private void OnDisable()
        {
            worldMap.transitionProgress.OnProgressUpdated -= OnProgressed;
            worldMap.transitionProgress.OnCompletion -= OnCompletion;
        }

        /// <summary>
        /// Updates the progress bar value based on the current transition progress.
        /// </summary>
        /// <param name="progress"></param>
        private void OnProgressed(float progress)
        {
            // Show the progress bar if it is set to show on start
            if (HasProgressBar)
            {
                progressBar.gameObject.SetActive(true);
                progressBar.value = progress;
            }
        }

        /// <summary>
        /// Hides the loading bar when the transition is complete.
        /// </summary>
        /// <param name="status"></param>
        private void OnCompletion(bool status)
        {
            // Set the status of the loading bar
            if (HasProgressBar) progressBar.gameObject.SetActive(!status);
        }
    }
}
