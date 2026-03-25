using System.Threading.Tasks;

namespace WorldShaper
{
    /// <summary>
    /// Defines a contract for behaviors that can be connected to an <see cref="ILocationPointer"/>.
    /// </summary>
    /// <remarks>
    /// Implement this interface to define custom behavior that executes during the entry and exit phases of an <see cref="ILocationPointer"/>.
    /// The <see cref="OnEnter"/> method is invoked when the behavior is activated, and the <see cref="OnExit"/> method is invoked when the behavior is deactivated.
    /// </remarks>
    public interface IBehaviour
    {
        /// <summary>
        /// Executes initialization logic for the derived class. Limited to one-time setup operations for all <see cref="ILocationPointer"/>s in the context.
        /// </summary>
        /// <remarks>
        /// This method is intended to be overridden in a derived class to provide custom initialization logic. 
        /// The base implementation does nothing and completes immediately.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        virtual Task OnInitialize() => Task.CompletedTask;

        /// <summary>
        /// Executes activation logic for the derived class. Limited to the <see cref="ILocationPointer"/> that is being entered from.
        /// </summary>
        /// <remarks>The default implementation completes immediately. Override this method in a derived class to provide custom activation logic.</remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        virtual Task OnActivate() => Task.CompletedTask;

        /// <summary>
        /// Executes logic when entering a specific <see cref="ILocationPointer"/>.
        /// </summary>
        /// <remarks>
        /// This method is typically invoked automatically by the <see cref="ILocationPointer"/> when the behavior is activated.
        /// Override this method to define custom behavior that should execute upon entering the <see cref="ILocationPointer"/>.
        /// </remarks>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task completes when the entry logic has finished executing.</returns>
        virtual Task OnEnter() => Task.CompletedTask;

        /// <summary>
        /// Executes logic when exiting a specific <see cref="ILocationPointer"/>.
        /// </summary>
        /// <remarks>
        /// This method is typically invoked automatically by the <see cref="ILocationPointer"/> when the behavior is deactivated.
        /// Override this method to define custom behavior that should execute upon exiting the <see cref="ILocationPointer"/>.
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task completes when the exit logic has finished executing.</returns>
        virtual Task OnExit() => Task.CompletedTask;

        /// <summary>
        /// Determines whether the specified status is active.
        /// </summary>
        /// <param name="status">A boolean value representing the status to evaluate.</param>
        /// <returns><see langword="true"/> if the specified status is active; otherwise, <see langword="false"/>.</returns>
        virtual Task<bool> IsActive(bool status) => Task.FromResult(status);
    }
}

