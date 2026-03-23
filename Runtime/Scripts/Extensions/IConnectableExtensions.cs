using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper
{
    /// <summary>
    /// This static class provides extension methods for the <see cref="IConnectable"/> interface.
    /// </summary>
    /// <remarks>
    /// These methods facilitate operations such as retrieving spawn locations, finding endpoints, and managing connectable objects.
    /// </remarks>
    public static class IConnectableExtensions
    {
        /// <summary>
        /// Finds and returns the endpoint associated with the specified connection within the given area.
        /// </summary>
        /// <remarks>This method attempts to locate a connection within the specified area that matches the given name and retrieves its endpoint.</remarks>
        /// <param name="handle">The handle to the area containing connections and connectables.</param>
        /// <param name="name">The value used to locate the associated connection.</param>
        /// <returns>A string representing the value of the connectable linked to the specified passage name. 
        /// Returns an empty string if no matching connection or connectable is found.</returns>
        public static string FindEndpoint(this AreaHandle handle, string name) => handle.ConnectionExists(name) ? handle.GetConnection(name).Endpoint : string.Empty;

        /// <summary>
        /// Retrieves the first connectable object whose value matches the specified string.
        /// </summary>
        /// <remarks>This method searches through the collection of connectable objects and returns the first one whose value matches the specified string.</remarks>
        /// <param name="connectables"> The list of connectable objects to search through.</param>
        /// <param name="endpoint">The endpoint value to match against the connectable objects.</param>
        /// <returns>An <see cref="InterfaceReference{T}"/> of type <see cref="IConnectable"/> representing the first matching connectable object,  or <see langword="null"/> if no match is found.</returns>
        public static InterfaceReference<IConnectable> GetConnectable(this List<InterfaceReference<IConnectable>> connectables, string endpoint) => connectables.Find(c => c.Value.GetEndpoint() == endpoint);

        /// <summary>
        /// Attempts to retrieve a connectable object whose value matches the specified string.
        /// </summary>
        /// <param name="connectables"> The list of connectable objects to search through.</param>
        /// <param name="endpoint">The endpoint value to match against the connectable objects.</param>
        /// <param name="connectable">The output parameter that will hold the found connectable object if successful; otherwise, it will be <see langword="default"/>.</param>
        /// <returns>A boolean value indicating whether a matching connectable object was found.</returns>
        public static bool TryGetConnectable(this List<InterfaceReference<IConnectable>> connectables, string endpoint, out IConnectable connectable)
        {
            // Attempt to get the connectable
            if (connectables.Exists(c => c.Value.GetEndpoint() == endpoint))
            {
                // Set the output parameter to the found connectable
                connectable = connectables.GetConnectable(endpoint).Value;

                // Return true to indicate the connectable was found
                return true;
            }

            // Set the output parameter to default if not found
            connectable = default;

            // Return true if the connectable was found, false otherwise
            return false;
        }

        /// <summary>
        /// Determines the location of a spawn point based on the specified value.
        /// </summary>
        /// <remarks>The method attempts to find a spawn point associated with the given value by querying
        /// connectable objects. If a matching connectable is found, their position is returned. 
        /// If no connectable is found, a warning is logged, and a zero vector is returned.</remarks>
        /// <param name="value">A string representing the identifier used to locate the spawn point.</param>
        /// <returns>A <see cref="Vector3"/> representing the calculated spawn location. If no matching spawn point is found, 
        /// the player's current position is returned as a fallback.</returns>
        public static bool TryGetLocation(this List<InterfaceReference<IConnectable>> connectables, string value, out Vector3 location)
        {
            // Get the connectable with the matching value
            InterfaceReference<IConnectable> connectable = connectables.GetConnectable(value);

            // Set the spawn location to the connectable position if it exists
            if (connectable.HasValue)
            {
                // Set the spawn location to the connectable position
                location = connectable.Value.GetPosition();

                // Return true to indicate a connectable was found
                return true;
            }

            // If no connectable is found, return false and log a warning
            Debug.LogWarning($"No connectable found for value: {value}, setting spawn location to Vector3.zero as a fallback.");

            // Set the spawn location to Vector3.zero
            location = Vector3.zero;

            // Return false to indicate no connectable was found
            return false;
        }

        /// <summary>
        /// Retrieves all connectable objects in the scene and returns them as a list of interface references.
        /// </summary>
        /// <returns>A list of <see cref="InterfaceReference{IConnectable}"/> objects representing all connectable objects found
        /// in the scene. The list will be empty if no connectable objects are present.</returns>
        public static List<InterfaceReference<IConnectable>> GetConnectableReferences()
        {
            // Create the list of connectables
            List<InterfaceReference<IConnectable>> connectables = new();

            // Get all the connectables in the scene
            foreach (IConnectable connectable in GetAllConnectables())
            {
                // Add the connectable to the list
                connectables.Add(new InterfaceReference<IConnectable>(connectable));
            }

            // Return the list of connectables
            return connectables;
        }

        /// <summary>
        /// Retrieves all objects in the scene that implement the <see cref="IConnectable"/> interface.
        /// </summary>
        /// <remarks>This method searches the scene for all <see cref="MonoBehaviour"/> instances and
        /// filters them to include only those that implement the <see cref="IConnectable"/> interface.</remarks>
        /// <returns>An enumerable collection of objects that implement the <see cref="IConnectable"/> interface. If no such objects are found, the collection will be empty.</returns>
        public static IEnumerable<IConnectable> GetAllConnectables() => GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IConnectable>();
    }
}