using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace WorldShaper
{
    /// <summary>
    /// Represents a sequence of asynchronous operations and callback actions that can be executed in order.
    /// </summary>
    /// <remarks>The <see cref="OperationSequence"/> struct provides a mechanism to define and manage a series of asynchronous tasks and callback actions that are executed sequentially. 
    /// It supports adding individual operations or callbacks, as well as collections of them, and allows for method chaining to build complex sequences fluently. 
    /// This type is particularly useful for scenarios where a series of dependent operations need to be executed in a specific order, such as loading resources, processing data, or executing workflows.
    /// </remarks>
    public struct OperationSequence
    {
        public Dictionary<int, Task> Operations;
        public Dictionary<int, Action> Callbacks;

        public int Count;
        public bool running;

        public OperationSequence(Task operation = null, Action callback = null)
        {
            // Initialize the operations and callbacks dictionaries
            Operations = new Dictionary<int, Task>();
            Callbacks = new Dictionary<int, Action>();

            // Initialize the count and running status
            Count = 0;
            running = false;

            // If an operation is provided, add it to the sequence
            if (operation != null) AddOperation(operation);

            // If a callback is provided, add it to the sequence
            if (callback != null) AddCallback(callback);
        }

        public OperationSequence(IEnumerable<Task> operations = null, IEnumerable<Action> callbacks = null)
        {
            // Initialize the operations and callbacks dictionaries
            Operations = new Dictionary<int, Task>();
            Callbacks = new Dictionary<int, Action>();

            // Initialize the count and running status
            Count = 0;
            running = false;

            // If operations are provided, add them to the sequence
            if (operations != null) AddOperations(operations);

            // If callbacks are provided, add them to the sequence
            if (callbacks != null) AddCallbacks(callbacks);
        }

        /// <summary>
        /// Gets an empty <see cref="OperationSequence"/> instance.
        /// </summary>
        public static OperationSequence Empty => new OperationSequence();

        /// <summary>
        /// Creates a new <see cref="OperationSequence"/> from the specified asynchronous operation.
        /// </summary>
        /// <param name="operation">The asynchronous operation to include in the sequence. Cannot be <see langword="null"/>.</param>
        /// <returns>An <see cref="OperationSequence"/> that encapsulates the provided operation.</returns>
        public static OperationSequence FromOperation(Task operation) => new OperationSequence(operation);

        /// <summary>
        /// Creates an <see cref="OperationSequence"/> that executes the specified callback action.
        /// </summary>
        /// <param name="callback">The action to be executed as part of the operation sequence. This parameter cannot be <see langword="null"/>.</param>
        /// <returns>An <see cref="OperationSequence"/> configured to execute the provided callback.</returns>
        public static OperationSequence FromCallback(Action callback) => new OperationSequence(null, callback);

        /// <summary>
        /// Creates a new <see cref="OperationSequence"/> from the specified collection of asynchronous operations.
        /// </summary>
        /// <param name="operations">A collection of tasks representing the asynchronous operations to include in the sequence. Each task in the collection must not be null.</param>
        /// <returns>An <see cref="OperationSequence"/> that encapsulates the provided operations.</returns>
        public static OperationSequence FromOperations(IEnumerable<Task> operations) => new OperationSequence(operations);

        /// <summary>
        /// Creates an <see cref="OperationSequence"/> from a collection of callback actions.
        /// </summary>
        /// <remarks>The returned <see cref="OperationSequence"/> will execute the provided actions sequentially when invoked. 
        /// Ensure that the actions in the collection are not <see langword="null"/> to avoid runtime errors.
        /// </remarks>
        /// <param name="callbacks">A collection of <see cref="Action"/> delegates representing the operations to be executed in sequence. Each action in the collection will be invoked in the order it appears.</param>
        /// <returns>An <see cref="OperationSequence"/> that encapsulates the provided callback actions.</returns>
        public static OperationSequence FromCallbacks(IEnumerable<Action> callbacks) => new OperationSequence(null, callbacks);

        /// <summary>
        /// Adds a scene loading operation to the current load plan.
        /// </summary>
        /// <param name="operation">The task representing the scene loading operation to be added. Cannot be <see langword="null"/>.</param>
        /// <returns>The current <see cref="OperationSequence"/> instance, allowing for method chaining.</returns>
        public OperationSequence AddOperation(Task operation)
        {
            // Update the operation index and initialize update the count
            int index = Count++;

            // Initialize the operations dictionary if it is null
            if (Operations == null) Operations = new Dictionary<int, Task>();

            // Add the operation to the list
            Operations.Add(index, operation);

            // Return the current instance for method chaining
            return this;
        }

        /// <summary>
        /// Adds a collection of tasks to the current scene load plan.
        /// </summary>
        /// <param name="ops">The collection of tasks to add. Each task represents an operation to be included in the scene load plan.</param>
        /// <returns>The current <see cref="OperationSequence"/> instance, allowing for method chaining.</returns>
        public OperationSequence AddOperations(IEnumerable<Task> ops)
        {
            // Initialize the operations dictionary if it is null
            if (Operations == null) Operations = new Dictionary<int, Task>();

            // Iterate through each operation in the provided collection
            for (int i = 0; i < ops.Count(); i++)
            {
                // Update the operation index and initialize update the count
                int index = Count++;

                // Add the operation to the list
                Operations.Add(index, ops.ElementAt(i));
            }

            // Return the current instance for method chaining
            return this;
        }

        /// <summary>
        /// Adds a callback action to the operation sequence.
        /// </summary>
        /// <param name="callback">The action to be executed as part of the operation sequence.  This parameter cannot be <see langword="null"/>.</param>
        /// <returns>The current <see cref="OperationSequence"/> instance, allowing for method chaining.</returns>
        public OperationSequence AddCallback(Action callback)
        {
            // Update the operation index and initialize update the count
            int index = Count++;

            // Initialize the operations dictionary if it is null
            if (Callbacks == null) Callbacks = new Dictionary<int, Action>();

            // Add the operation to the list
            Callbacks.Add(index, callback);

            // Return the current instance for method chaining
            return this;
        }

        /// <summary>
        /// Adds a collection of callback actions to the operation sequence.
        /// </summary>
        /// <remarks>The method assigns a unique index to each callback in the provided collection and stores them internally.  Callbacks are added in the order they appear in the collection. </remarks>
        /// <param name="cbs">A collection of <see cref="Action"/> delegates to be added as callbacks. Each action will be assigned a
        /// unique index.</param>
        /// <returns>The current <see cref="OperationSequence"/> instance, allowing for method chaining.</returns>
        public OperationSequence AddCallbacks(IEnumerable<Action> cbs)
        {
            // Initialize the operations dictionary if it is null
            if (Callbacks == null) Callbacks = new Dictionary<int, Action>();

            // Iterate through each operation in the provided collection
            for (int i = 0; i < cbs.Count(); i++)
            {
                // Update the operation index and initialize update the count
                int index = Count++;

                // Add the operation to the list
                Callbacks.Add(index, cbs.ElementAt(i));
            }

            // Return the current instance for method chaining
            return this;
        }

        /// <summary>
        /// Clears all operations, callbacks, and resets the count to zero.
        /// </summary>
        /// <remarks>If the sequence is currently running, this method does nothing and returns the current instance. 
        /// Otherwise, it clears the operations and callbacks, resets the count, and returns the current instance to support method chaining.</remarks>
        /// <returns>The current <see cref="OperationSequence"/> instance, allowing for method chaining.</returns>
        public OperationSequence Clear()
        {
            // If is running, do not clear and return this early
            if (running) return this;

            // Clear the operations dictionary
            Operations?.Clear();

            // Clear the callbacks dictionary
            Callbacks?.Clear();

            // Reset the count
            Count = 0;

            // Return the current instance for method chaining
            return this;
        }

        /// <summary>
        /// Executes a series of asynchronous operations sequentially.
        /// </summary>
        /// <remarks>
        /// This method processes each operation in the <c>operations</c> collection in the order
        /// they appear, awaiting the completion of one operation before starting the next. 
        /// If an operation throws an exception, the method stops execution and propagates the exception to the caller.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execution of the operations. The task completes when all operations have been executed, or when an exception is thrown.</returns>
        public async Task Execute()
        {
            // Set the running flag to true
            running = true;

            // Execute each operation and callback in sequence
            for (int i = 0; i < Count; i++)
            {
                // If there is an operation at the current index, await its completion
                if (Operations != null && Operations.ContainsKey(i)) await Operations[i];

                // If there is a callback at the current index, invoke it
                if (Callbacks != null && Callbacks.ContainsKey(i)) Callbacks[i]?.Invoke();
            }

            // Set the running flag to false
            running = false;
        }
    }
}
