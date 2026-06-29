using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorldShaper
{
    /// <summary>
    /// Provides extension methods for working with <see cref="Task"/> objects, enabling additional functionality such as converting tasks to Unity coroutines, combining multiple tasks, waiting for conditions, and handling task exceptions.
    /// </summary>
    /// <remarks>
    /// This static class includes utility methods to simplify common asynchronous programming scenarios. 
    /// It is designed to enhance the usability of <see cref="Task"/> objects in various contexts, including Unity development and general .NET applications. 
    /// The methods in this class are thread-safe unless otherwise specified.
    /// </remarks>
    public static class TaskExtensions
    {
        /// <summary>
        /// Converts the Task into an IEnumerator for Unity coroutine usage.
        /// </summary>
        /// <remarks>
        /// When used on a faulted Task, GetResult() will propagate the original exception. see: https://devblogs.microsoft.com/pfxteam/task-exception-handling-in-net-4-5/
        /// </remarks>
        /// <param name="task">The Task to convert.</param>
        /// <returns>An IEnumerator representation of the Task.</returns>
        public static IEnumerator AsCoroutine(this Task task)
        {
            // If the task is already completed, we can return immediately.
            while (!task.IsCompleted) yield return null;

            // This will block until the task is completed, and if it is faulted, it will throw the exception.
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Wraps the provided object into a completed Task.
        /// </summary>
        /// <param name="obj">The object to be wrapped in a Task.</param>
        /// <typeparam name="T">The type of the object.</typeparam>
        /// <returns>A completed Task containing the object.</returns>
        public static Task<T> AsCompletedTask<T>(this T obj) => Task.FromResult(obj);

        /// <summary>
        /// Waits for all the tasks in the provided collection to complete.
        /// </summary>
        /// <remarks>
        /// This method is useful for scenarios where you need to wait for multiple asynchronous operations to complete. 
        /// If any of the tasks in the collection fail, the returned task will also fail and propagate the first encountered exception.
        /// </remarks>
        /// <param name="tasks">A collection of tasks to wait for. The collection must not be null, and its elements must not be null.</param>
        /// <returns>
        /// A task that represents the completion of all the tasks in the collection. 
        /// If the collection is empty, the returned task is already completed.
        /// </returns>
        public static Task Combine(this IEnumerable<Task> tasks)
        {
            // If there are no tasks, return a completed task.
            if (tasks == null || !tasks.Any()) return Task.CompletedTask;

            // Return a task that completes when all provided tasks are completed.
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Waits asynchronously until the specified condition is met or the timeout period elapses.
        /// </summary>
        /// <remarks>This method repeatedly evaluates the <paramref name="condition"/> at the specified polling interval until it returns <see langword="true"/> or the timeout period elapses. 
        /// If no timeout is specified, the method waits indefinitely until the condition is met.</remarks>
        /// <param name="condition">A function that evaluates to <see langword="true"/> when the desired condition is met.</param>
        /// <param name="timeoutMs">The maximum time, in milliseconds, to wait for the condition to be met. Specify a value less than 0 to wait indefinitely. The default value is -1.</param>
        /// <param name="pollIntervalMs">The interval, in milliseconds, at which the condition is evaluated. Must be greater than 0. The default value is 33.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the condition is met within the timeout period; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="condition"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="pollIntervalMs"/> is less than or equal to 0.</exception>
        public static async Task<bool> WaitUntil(this Func<bool> condition, int timeoutMs = -1, int pollIntervalMs = 33)
        {
            // Check if the condition is null and throw an exception if it is.
            if (condition == null) throw new ArgumentNullException(nameof(condition));

            // Throw an exception if the poll interval is less than or equal to zero.
            if (pollIntervalMs <= 0) throw new ArgumentOutOfRangeException(nameof(pollIntervalMs), "Poll interval must be greater than zero.");

            // Start the wait loop task that polls the condition.
            var waitTask = RunWaitLoop(condition, pollIntervalMs);

            // If a timeout is not specified, wait indefinitely.
            if (timeoutMs < 0)
            {
                // Await the wait task to complete.
                await waitTask.ConfigureAwait(false);

                // Condition met before timeout.
                return true;
            }

            // Create a timeout task that completes after the specified timeout.
            var timeoutTask = Task.Delay(timeoutMs);

            // Wait for either the wait task or the timeout task to complete.
            var finished = await Task.WhenAny(waitTask, timeoutTask);

            // Return true if the wait task completed first, false if the timeout occurred first.
            return finished == waitTask;
        }

        /// <summary>
        /// Repeatedly evaluates a condition at specified intervals until the condition returns <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// This method is asynchronous and does not block the calling thread. 
        /// The condition is evaluated immediately upon entering the loop, and then at intervals specified by <paramref name="pollIntervalMs"/>. 
        /// Use this method to implement polling logic in scenarios where the condition may become true over time.</remarks>
        /// <param name="condition">A delegate that represents the condition to evaluate. 
        /// The loop continues until this delegate returns <see langword="true"/>.</param>
        /// <param name="pollIntervalMs">The interval, in milliseconds, between successive evaluations of the condition. Must be a non-negative value.</param>
        /// <returns>A task that completes when the condition returns <see langword="true"/>.</returns>
        private static async Task RunWaitLoop(Func<bool> condition, int pollIntervalMs)
        {
            // Continue polling the condition at specified intervals until it returns true.
            while (!condition()) await Task.Delay(pollIntervalMs).ConfigureAwait(false);
        }

        /// <summary>
        /// Marks a task to be forgotten, meaning any exceptions thrown by the task will be caught and handled.
        /// </summary>
        /// <param name="task">The task to be forgotten.</param>
        /// <param name="onException">The optional action to execute when an exception is caught. If provided, the exception will not be rethrown.</param>
        public static async void Forget(this Task task, Action<Exception> onException = null)
        {
            // Try to run the task asynchronously, if it is already completed, this will not block.
            try
            {
                // Await the task to ensure it runs to completion.
                await task;
            }

            // Catch any exceptions that occur during the task execution.
            catch (Exception exception)
            {
                // If no exception handler is provided, rethrow the exception.
                if (onException == null) throw exception;

                // If an exception handler is provided, call it with the exception.
                onException(exception);
            }
        }
    }
}