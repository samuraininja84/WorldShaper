using System;
using UnityEngine;

namespace WorldShaper
{
    /// <summary>
    /// Represents a progress state that can be used to track the progress of an operation.
    /// </summary>
    [Serializable]
    public struct Progress : IProgress<float>
    {
        /// <summary>
        /// Represents the progress value, which is a float between 0 and 1.
        /// </summary>
        [ProgressBar("Progress", EmptyColor = "red", FullColor = "green")]
        public float value;

        /// <summary>
        /// Event that is invoked when the progress is updated.
        /// </summary>
        public event Action<float> OnProgressUpdated;

        /// <summary>
        /// Event that is invoked when the progress operation is completed.
        /// </summary>
        public event Action<bool> OnCompletion;

        /// <summary>
        /// The ratio of the progress value to the maximum value.
        /// </summary>
        private const float ratio = 1f;

        /// <summary>
        /// Indicates whether the progress is finished (i.e., has reached the maximum value).
        /// </summary>
        public bool Finished => value == ratio;

        /// <summary>
        /// Gets an instance of <see cref="Progress"/> representing no progress.
        /// </summary>
        public static Progress Empty => new Progress(0f);

        /// <summary>
        /// Constructor for the progress state.
        /// </summary>
        /// <param name="progress"></param>
        public Progress(float progress)
        {
            // Clamp the progress value between 0 and 1
            progress = Mathf.Clamp01(progress);

            // Calculate the progress value based on the ratio
            value = progress;

            // Set the Progress event to null
            OnProgressUpdated = null;

            // Set the Completion event to null
            OnCompletion = null;
        }

        /// <summary>
        /// Update the progress state.
        /// </summary>
        /// <param name="progress"></param>
        public void Report(float progress)
        {
            // Clamp the progress value between 0 and 1
            progress = Mathf.Clamp01(progress);

            // Calculate the progress value based on the ratio
            value = progress;

            // Invoke the Progress event with the current progress value
            OnProgressUpdated?.Invoke(progress / ratio);

            // If the progress has reached the maximum value, invoke the Completion event
            if (Finished) OnCompletion?.Invoke(true);
        }

        /// <summary>
        /// Get the progress value.
        /// </summary>
        /// <returns>
        /// A float representing the progress value, which is a value between 0 and 1.
        /// </returns>
        public float GetProgress() => value;

        /// <summary>
        /// Clear the progress value, resetting it to 0.
        /// </summary>
        public void Clear() => value = 0f;
    }
}