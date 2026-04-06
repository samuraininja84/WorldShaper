using System.Threading.Tasks;
using UnityEngine;

namespace WorldShaper
{
    /// <summary>
    /// Defines a contract for a <see cref="ILocationPointer"/> that supports initialization, entry and exit logic, interaction state management, and area or connection assignments.
    /// </summary>
    /// <remarks>
    /// The <see cref="ILocationPointer"/> interface provides methods for managing the lifecycle and behavior of a connectable object, including initialization, entry and exit logic, and interaction state. 
    /// It also includes methods for assigning areas and connections, as well as retrieving information about the object's endpoint, position, and associated destination area.
    /// </remarks>
    public interface ILocationPointer
    {
        /// <summary>
        /// The name of the <see cref="ILocationPointer"/> instance, typically used for identification or debugging purposes.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Sets the interaction state of the object.
        /// </summary>
        /// <param name="status">A boolean value indicating whether the object can interact. <see langword="true"/> eqauls exit enabled, <see langword="false"/> equals entry enabled.</param>
        void SetActive(bool status);

        /// <summary>
        /// Initializes the necessary resources or state for the component to function properly.
        /// </summary>
        /// <remarks>
        /// This method should be called before using the component. 
        /// It ensures that all required dependencies and configurations are set up. 
        /// The method is asynchronous and may involve operations such as loading data or establishing connections.
        /// </remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous initialization operation.</returns>
        Task Initialize();

        /// <summary>
        /// Activates the current instance, enabling it to perform its intended operations.
        /// </summary>
        /// <remarks>This method initializes the instance and prepares it for use.
        /// Ensure that any required dependencies or configurations are set before calling this method. 
        /// The activation process is asynchronous and should be awaited to ensure completion.
        /// </remarks>
        /// <returns>A <see cref="Task"/> representing the asynchronous activation operation.</returns>
        Task Activate();

        /// <summary>
        /// Executes logic when entering a specific <see cref="ILocationPointer"/>.
        /// </summary>
        /// <remarks>
        /// This method is typically invoked automatically by the <see cref="ILocationPointer"/> when the behavior is activated.
        /// Override this method to define custom behavior that should execute upon entering the <see cref="ILocationPointer"/>.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task completes when the entry logic has finished executing.</returns>
        Task Enter();

        /// <summary>
        /// Executes logic when exiting a specific <see cref="ILocationPointer"/>.
        /// </summary>
        /// <remarks>
        /// This method is typically invoked automatically by the <see cref="ILocationPointer"/> when the behavior is deactivated.
        /// Override this method to define custom behavior that should execute upon exiting the <see cref="ILocationPointer"/>.
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task completes when the exit logic has finished executing.</returns>
        Task Exit();

        /// <summary>
        /// Retrieves the current position of the connectable object in the world space.
        /// </summary>
        /// <returns>
        /// A <see cref="Vector3"/> representing the current position of the object in the world space.
        /// </returns>
        Vector3 GetPosition();

        /// <summary>
        /// Gets the endpoint identifier of the connectable object.
        /// </summary>
        /// <returns>A <see cref="string"/> representing the endpoint identifier of the object.</returns>
        string GetEndpoint();
    }
}

