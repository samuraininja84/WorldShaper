using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper
{
    /// <summary>
    /// This static class provides extension methods for the <see cref="ILocationPointer"/> interface.
    /// </summary>
    /// <remarks>
    /// These methods facilitate operations such as retrieving spawn locations, finding endpoints, and managing <see cref="ILocationPointer"/> objects.
    /// </remarks>
    public static class ILocationPointerExtensions
    {
        /// <summary>
        /// Finds and returns the endpoint associated with the specified connection within the given area.
        /// </summary>
        /// <remarks>This method attempts to locate a connection within the specified area that matches the given name and retrieves its endpoint.</remarks>
        /// <param name="handle">The handle to the area containing connections and locationPointer.</param>
        /// <param name="name">The value used to locate the associated connection.</param>
        /// <returns>A string representing the value of the <see cref="ILocationPointer"/> linked to the specified passage name. 
        /// Returns an empty string if no matching connection or <see cref="ILocationPointer"/> is found.</returns>
        public static string FindEndpoint(this AreaHandle handle, string name) => handle.ConnectionExists(name) ? handle.GetConnection(name).Endpoint : string.Empty;

        /// <summary>
        /// Retrieves the first <see cref="ILocationPointer"/> object whose value matches the specified string.
        /// </summary>
        /// <remarks>This method searches through the collection of <see cref="ILocationPointer"/> objects and returns the first one whose value matches the specified string.</remarks>
        /// <param name="locationPointer"> The list of <see cref="ILocationPointer"/> objects to search through.</param>
        /// <param name="endpoint">The endpoint value to match against the <see cref="ILocationPointer"/> objects.</param>
        /// <returns>An <see cref="InterfaceReference{T}"/> of type <see cref="ILocationPointer"/> representing the first matching <see cref="ILocationPointer"/> object,  or <see langword="null"/> if no match is found.</returns>
        public static InterfaceReference<ILocationPointer> GetConnectable(this List<InterfaceReference<ILocationPointer>> locationPointer, string endpoint) => locationPointer.Find(c => c.Value.GetEndpoint() == endpoint);

        /// <summary>
        /// Attempts to retrieve a <see cref="ILocationPointer"/> object whose value matches the specified string.
        /// </summary>
        /// <param name="connectables"> The list of <see cref="ILocationPointer"/> objects to search through.</param>
        /// <param name="endpoint">The endpoint value to match against the <see cref="ILocationPointer"/> objects.</param>
        /// <param name="<see cref="ILocationPointer"/>">The output parameter that will hold the found <see cref="ILocationPointer"/> object if successful; otherwise, it will be <see langword="default"/>.</param>
        /// <returns>A boolean value indicating whether a matching <see cref="ILocationPointer"/> object was found.</returns>
        public static bool TryGetConnectable(this List<InterfaceReference<ILocationPointer>> connectables, string endpoint, out ILocationPointer location)
        {
            // Attempt to get the location
            if (connectables.Exists(c => c.Value.GetEndpoint() == endpoint))
            {
                // Set the output parameter to the found location
                location = connectables.GetConnectable(endpoint).Value;

                // Return true to indicate the location was found
                return true;
            }

            // Set the output parameter to default if not found
            location = default;

            // Return true if the location was found, false otherwise
            return false;
        }

        /// <summary>
        /// Determines the <see cref="ILocationPointer"/> of a spawn point based on the specified value.
        /// </summary>
        /// <remarks>The method attempts to find a spawn point associated with the given value by querying
        /// <see cref="ILocationPointer"/> objects. If a matching <see cref="ILocationPointer"/> is found, their position is returned. 
        /// If no <see cref="ILocationPointer"/> is found, a warning is logged, and a zero vector is returned.</remarks>
        /// <param name="value">A string representing the identifier used to locate the spawn point.</param>
        /// <returns>A <see cref="Vector3"/> representing the calculated spawn <see cref="ILocationPointer"/>. If no matching spawn point is found, 
        /// the player's current position is returned as a fallback.</returns>
        public static bool TryGetLocation(this List<InterfaceReference<ILocationPointer>> connectables, string value, out Vector3 location)
        {
            // Get the location with the matching value
            InterfaceReference<ILocationPointer> connectable = connectables.GetConnectable(value);

            // Set the spawn location to the location position if it exists
            if (connectable.HasValue)
            {
                // Set the spawn location to the location position
                location = connectable.Value.GetPosition();

                // Return true to indicate a location was found
                return true;
            }

            // If no location is found, return false and log a warning
            Debug.LogWarning($"No connectable found for value: {value}, setting spawn location to Vector3.zero as a fallback.");

            // Set the spawn location to Vector3.zero
            location = Vector3.zero;

            // Return false to indicate no location was found
            return false;
        }

        /// <summary>
        /// Retrieves all <see cref="ILocationPointer"/> objects in the scene and returns them as a list of interface references.
        /// </summary>
        /// <returns>A list of <see cref="InterfaceReference{IConnectable}"/> objects representing all <see cref="ILocationPointer"/> objects found
        /// in the scene. The list will be empty if no location objects are present.</returns>
        public static List<InterfaceReference<ILocationPointer>> GetConnectableReferences()
        {
            // Create the list of locationPointer
            List<InterfaceReference<ILocationPointer>> connectables = new();

            // Get all the locationPointer in the scene
            foreach (ILocationPointer connectable in GetAllConnectables())
            {
                // Add the location to the list
                connectables.Add(new InterfaceReference<ILocationPointer>(connectable));
            }

            // Return the list of locationPointer
            return connectables;
        }

        /// <summary>
        /// Retrieves all objects in the scene that implement the <see cref="ILocationPointer"/> interface.
        /// </summary>
        /// <remarks>This method searches the scene for all <see cref="MonoBehaviour"/> instances and
        /// filters them to include only those that implement the <see cref="ILocationPointer"/> interface.</remarks>
        /// <returns>An enumerable collection of objects that implement the <see cref="ILocationPointer"/> interface. If no such objects are found, the collection will be empty.</returns>
        public static IEnumerable<ILocationPointer> GetAllConnectables() => GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ILocationPointer>();
    }
}