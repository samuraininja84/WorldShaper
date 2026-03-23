using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper
{
    /// <summary>
    /// Represents a group of asynchronous operations that can be tracked collectively for progress and completion
    /// status.
    /// </summary>
    /// <remarks>
    /// This struct provides a way to manage and monitor multiple asynchronous operations as a single unit.
    /// It allows querying the overall progress and completion status of the group.
    /// </remarks>
    public readonly struct AsyncOperationGroup
    {
        /// <summary>
        /// A list of asynchronous operations associated with the current context.
        /// </summary>
        /// <remarks>
        /// This list contains instances of <see cref="AsyncOperation"/> that represent ongoing or completed asynchronous tasks.
        /// </remarks>
        public readonly List<AsyncOperation> Operations;

        /// <summary>
        /// Gets the overall progress of all operations as a value between 0 and 1.
        /// </summary>
        public float Progress => Operations.Count == 0 ? 0 : Operations.Average(o => o.progress);

        /// <summary>
        /// Gets a value indicating whether all operations have been completed.
        /// </summary>
        public bool IsDone => Operations.All(o => o.isDone);

        /// <summary>
        /// Gets a value indicating whether the collection is empty.
        /// </summary>
        public bool Empty => Operations.Count == 0;

        /// <summary>
        /// Gets the <see cref="AsyncOperation"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the operation to retrieve.</param>
        /// <returns></returns>
        public AsyncOperation this[int index] => Operations[index];

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncOperationGroup"/> class with a specified initial capacity
        /// for the operations list.
        /// </summary>
        /// <remarks>The <paramref name="initialCapacity"/> parameter allows preallocating memory for the
        /// operations list, which can improve performance when the approximate number of operations is known in
        /// advance. If <paramref name="initialCapacity"/> is set to 0, the list will be initialized with the default
        /// capacity.</remarks>
        /// <param name="initialCapacity">The initial number of elements that the operations list can contain. Must be a non-negative integer.</param>
        public AsyncOperationGroup(int initialCapacity) => Operations = new List<AsyncOperation>(initialCapacity);

        /// <summary>
        /// Creates a new <see cref="AsyncOperationGroup"/> with the specified size.
        /// </summary>
        /// <param name="size">The number of operations that the group can manage concurrently. Must be greater than zero.</param>
        /// <returns>A new instance of <see cref="AsyncOperationGroup"/> configured with the specified size.</returns>
        public static AsyncOperationGroup OfSize(int size) => new AsyncOperationGroup(Mathf.Max(0, size));

        /// <summary>
        /// Adds the specified asynchronous operation to the group.
        /// </summary>
        /// <param name="operation">The asynchronous operation to add. Cannot be <see langword="null"/>.</param>
        /// <returns>The current <see cref="AsyncOperationGroup"/> instance, allowing for method chaining.</returns>
        public AsyncOperationGroup Add(AsyncOperation operation)
        {
            // Add the operation to the list
            Operations.Add(operation);

            // Return the current instance for method chaining
            return this;
        }

        /// <summary>
        /// Adds a collection of asynchronous operations to the group.
        /// </summary>
        /// <param name="operations">The collection of <see cref="AsyncOperation"/> instances to add. Cannot be <see langword="null"/>.</param>
        /// <returns>The current <see cref="AsyncOperationGroup"/> instance, allowing for method chaining.</returns>
        public AsyncOperationGroup AddRange(IEnumerable<AsyncOperation> operations)
        {
            // Add the operations to the list
            Operations.AddRange(operations);

            // Return the current instance for method chaining
            return this;
        }

        /// <summary>
        /// Returns a string representation of the current <see cref="AsyncOperationGroup"/> instance,  including the count of operations, progress, and completion status.
        /// </summary>
        /// <returns>A string that represents the current state of the <see cref="AsyncOperationGroup"/>, formatted as "AsyncOperationGroup(Count={Operations.Count}, Progress={Progress:F2}, IsDone={IsDone})".</returns>
        public override string ToString() => $"AsyncOperationGroup(Count={Operations.Count}, Progress={Progress:F2}, IsDone={IsDone})";
    }
}
