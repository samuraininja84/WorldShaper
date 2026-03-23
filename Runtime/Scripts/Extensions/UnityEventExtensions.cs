using System.Threading.Tasks;
using UnityEngine.Events;

namespace WorldShaper
{
    /// <summary>
    /// Provides extension methods for converting <see cref="UnityEvent"/> and <see cref="UnityEvent{T}"/> into <see cref="Task"/> and <see cref="Task{TResult}"/> respectively, enabling asynchronous programming with Unity events.
    /// </summary>
    /// <remarks>
    /// These extension methods allow developers to integrate Unity's event system with asynchronouscode by converting Unity events into tasks. 
    /// The tasks complete when the corresponding Unity events are triggered, and the event listeners are automatically removed to prevent memory leaks.
    /// </remarks>
    public static class UnityEventExtensions
    {
        /// <summary>
        /// Converts a <see cref="UnityEvent"/> into a <see cref="Task"/> that completes when the event is invoked.
        /// </summary>
        /// <remarks>This method allows asynchronous code to wait for a <see cref="UnityEvent"/> to be invoked. 
        /// The returned <see cref="Task"/> completes when the event is triggered, and the listener is automatically removed to prevent memory leaks.
        /// </remarks>
        /// <param name="unityEvent">The <see cref="UnityEvent"/> to be monitored for invocation.</param>
        /// <returns>A <see cref="Task"/> that completes when the <paramref name="unityEvent"/> is triggered.</returns>
        public static Task AsTask(this UnityEvent unityEvent)
        {
            // Check if the unityEvent is null and throw an exception if it is.
            if (unityEvent == null) throw new System.ArgumentNullException(nameof(unityEvent));

            // Create a TaskCompletionSource to represent the completion of the UnityEvent.
            var tcs = new TaskCompletionSource<bool>();

            // Declare the UnityAction outside to allow removal inside the action.
            UnityAction action = null;

            // Define the action to be invoked when the UnityEvent is triggered.
            action = () =>
            {
                // Remove the listener to avoid memory leaks.
                unityEvent.RemoveListener(action);

                // Set the result of the TaskCompletionSource to signal that the event has been invoked.
                tcs.SetResult(true);
            };

            // Add the listener to the UnityEvent.
            unityEvent.AddListener(action);

            // Return the task that will complete when the event is invoked.
            return tcs.Task;
        }

        /// <summary>
        /// Converts a <see cref="UnityEvent{T}"/> into a <see cref="Task{TResult}"/> that completes when the event is invoked.
        /// </summary>
        /// <remarks>
        /// This method allows asynchronous code to await the invocation of a UnityEvent. 
        /// The task completes when the event is triggered, and the event listener is automatically removed to prevent memory leaks.
        /// </remarks>
        /// <typeparam name="T">The type of the argument passed by the <see cref="UnityEvent{T}"/>.</typeparam>
        /// <param name="unityEvent">The <see cref="UnityEvent{T}"/> to convert into a task.</param>
        /// <returns>A <see cref="Task{TResult}"/> that completes with the argument of the <see cref="UnityEvent{T}"/> when the event is triggered.</returns>
        public static Task<T> AsTask<T>(this UnityEvent<T> unityEvent)
        {
            // Check if the unityEvent is null and throw an exception if it is.
            if (unityEvent == null) throw new System.ArgumentNullException(nameof(unityEvent));

            // Create a TaskCompletionSource to represent the completion of the UnityEvent.
            var tcs = new TaskCompletionSource<T>();

            // Declare the UnityAction outside to allow removal inside the action.
            UnityAction<T> action = null;

            // Define the action to be invoked when the UnityEvent is triggered.
            action = (arg) =>
            {
                // Remove the listener to avoid memory leaks.
                unityEvent.RemoveListener(action);

                // Set the result of the TaskCompletionSource to signal that the event has been invoked.
                tcs.SetResult(arg);
            };

            // Add the listener to the UnityEvent.
            unityEvent.AddListener(action);
            // Return the task that will complete when the event is invoked.
            return tcs.Task;
        }
    }
}