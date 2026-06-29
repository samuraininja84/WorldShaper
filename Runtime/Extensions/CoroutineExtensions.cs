using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorldShaper
{
    public static class CoroutineExtensions
    {
        /// <summary>
        /// Converts an IEnumerator to a Task.
        /// </summary>
        /// <param name="enumerator">The IEnumerator to convert.</param>
        /// <returns>A Task that completes when the IEnumerator finishes.</returns>
        public static Task ToTask(this IEnumerator enumerator) => enumerator.AsCompletedTask();

        /// <summary>
        /// Combines multiple enumerators into a single enumerator that iterates through all elements sequentially.
        /// </summary>
        /// <remarks>
        /// This method processes each enumerator in the input collection sequentially. 
        /// It advances through all elements of the first enumerator before moving to the next, and so on.
        /// </remarks>
        /// <param name="enumerators">A collection of enumerators to combine.</param> 
        /// <returns>An enumerator that iterates through the elements of each enumerator in the order they appear in the input collection.</returns>
        public static IEnumerator Combine(this IEnumerable<IEnumerator> enumerators)
        {
            // Iterate through each enumerator in the collection.
            foreach (var enumerator in enumerators)
            {
                // Move to the next element in the current enumerator when the previous one is done.
                while (enumerator.MoveNext())
                {
                    // Wait for the current element to finish before moving to the next one.
                    yield return enumerator.Current;
                }
            }
        }
    }
}