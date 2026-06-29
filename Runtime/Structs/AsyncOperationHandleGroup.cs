using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace WorldShaper
{
    /// <summary>
    /// A struct to group multiple AsyncOperationHandles together.
    /// </summary>
    /// <remarks>
    /// Used to track the progress and completion status of multiple async operations that return a SceneInstance.
    /// </remarks>
    public readonly struct AsyncOperationHandleGroup
    {
        /// <summary>
        /// The collection of handles representing asynchronous operations for loading or managing scenes.
        /// </summary>
        /// <remarks>
        /// Each handle in the collection corresponds to an asynchronous operation for a scene. 
        /// These handles can be used to monitor the status, retrieve results, or manage the lifecycle of the associated operations.
        /// </remarks>
        public readonly List<AsyncOperationHandle<SceneInstance>> Handles;

        /// <summary>
        /// Gets the overall progress as a percentage, calculated as the average completion percentage of all handles.
        /// </summary>
        public float Progress => Handles.Count == 0 ? 0 : Handles.Average(h => h.PercentComplete);

        /// <summary>
        /// Gets a value indicating whether all operations represented by the handles are complete.
        /// </summary>
        public bool IsDone => Handles.Count == 0 || Handles.All(o => o.IsDone);

        /// <summary>
        /// Gets a value indicating whether the collection is empty.
        /// </summary>
        public bool Empty => Handles.Count == 0;

        /// <summary>
        /// Gets the <see cref="AsyncOperationHandle{SceneInstance}"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the handle to retrieve.</param>
        /// <returns>The <see cref="AsyncOperationHandle{SceneInstance}"/> at the specified index.</returns>
        public AsyncOperationHandle<SceneInstance> this[int index] => Handles[index];

        /// <summary>
        /// Represents a group of asynchronous operation handles for managing scene instances.
        /// </summary>
        /// <remarks>This class initializes a collection of asynchronous operation handles with the
        /// specified capacity. It is typically used to manage multiple scene-loading operations in a structured
        /// way.</remarks>
        /// <param name="initialCapacity">The initial number of elements that the group can contain. Must be a non-negative value.</param>
        public AsyncOperationHandleGroup(int initialCapacity) => Handles = new List<AsyncOperationHandle<SceneInstance>>(initialCapacity);

        /// <summary>
        /// Creates a new <see cref="AsyncOperationHandleGroup"/> with the specified size.
        /// </summary>
        /// <param name="size">The number of operations the group can manage. Must be a non-negative integer.</param>
        /// <returns>An <see cref="AsyncOperationHandleGroup"/> instance configured to manage the specified number of operations.</returns>
        public static AsyncOperationHandleGroup OfSize(int size) => new AsyncOperationHandleGroup(Mathf.Max(0, size));

        /// <summary>
        /// Adds the specified asynchronous operation handle to the group.
        /// </summary>
        /// <param name="handle">The asynchronous operation handle to add, representing a scene instance.</param>
        /// <returns>The current <see cref="AsyncOperationHandleGroup"/> instance, allowing for method chaining.</returns>
        public AsyncOperationHandleGroup Add(AsyncOperationHandle<SceneInstance> handle)
        {
            // Add the handle to the list
            Handles.Add(handle);

            // Return the current instance for method chaining
            return this;
        }

        /// <summary>
        /// Adds a collection of asynchronous operation handles to the group.
        /// </summary>
        /// <param name="handles">The collection of <see cref="AsyncOperationHandle{SceneInstance}"/> objects to add to the group. Cannot be
        /// null.</param>
        /// <returns>The current <see cref="AsyncOperationHandleGroup"/> instance, allowing for method chaining.</returns>
        public AsyncOperationHandleGroup AddRange(IEnumerable<AsyncOperationHandle<SceneInstance>> handles)
        {
            // Add the handles to the list
            Handles.AddRange(handles);

            // Return the current instance for method chaining
            return this;
        }

        /// <summary>
        /// Returns a string representation of the current <see cref="AsyncOperationHandleGroup"/> instance, including the count of handles, progress, and completion status.
        /// </summary>
        /// <returns>A string that represents the current <see cref="AsyncOperationHandleGroup"/> instance. The string includes the number of handles, the progress as a percentage, and whether the operation is complete.</returns>
        public override string ToString() => $"AsyncOperationHandleGroup(Count: {Handles.Count}, Progress: {Progress:P2}, IsDone: {IsDone})";
    }
}
