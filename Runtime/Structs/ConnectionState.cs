using System;

namespace WorldShaper
{
    [Serializable]
    public struct ConnectionState : IEquatable<ConnectionState>
    {
        public string startPoint;
        public string endPoint;

        private int hashCode;

        // Implicit conversion to bool to check if the connection state is valid
        public static implicit operator bool(ConnectionState state) => !string.IsNullOrEmpty(state.startPoint) && !string.IsNullOrEmpty(state.endPoint);

        // Implicit conversion from Connection to ConnectionState
        public static implicit operator ConnectionState(Connection connection) => new ConnectionState(connection.StartPoint, connection.Endpoint);

        /// <summary>
        /// Represents an empty connection state with no identifier or status.
        /// </summary>
        /// <remarks>This static instance can be used to represent a default or uninitialized connection
        /// state. Both the identifier and status are set to empty strings.</remarks>
        public static ConnectionState Empty = new ConnectionState(string.Empty, string.Empty);

        /// <summary>
        /// Constructor for the connection state.
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        public ConnectionState(string startPoint, string endPoint)
        {
            this.startPoint = startPoint;
            this.endPoint = endPoint;

            // Calculate the hash code based on the start and end points
            hashCode = HashCode.Combine(startPoint, endPoint);
        }

        /// <summary>
        /// Set the start point of the connection state.
        /// </summary>
        /// <param name="startPoint"></param>
        public void SetStart(string startPoint)
        {
            // Update the start point
            this.startPoint = startPoint;

            // Recalculate the hash code when the start point is updated
            hashCode = HashCode.Combine(this.startPoint, endPoint);
        }

        /// <summary>
        /// Set the end point of the connection state.
        /// </summary>
        /// <param name="endPoint"></param>
        public void SetEnd(string endPoint)
        {
            // Update the end point
            this.endPoint = endPoint;

            // Recalculate the hash code when the end point is updated
            hashCode = HashCode.Combine(this.startPoint, this.endPoint);
        }

        /// <summary>
        /// Initializes the start and end points of the current object based on the specified connection.
        /// </summary>
        /// <param name="connection">The connection object from which to retrieve the start and end points. Cannot be null.</param>
        public void FromConnection(Connection connection)
        {
            // Get the start point
            startPoint = connection.StartPoint;

            // Get the end point
            endPoint = connection.Endpoint;

            // Recalculate the hash code based on the new start and end points
            hashCode = HashCode.Combine(startPoint, endPoint);
        }

        /// <summary>
        /// Deconstructs the object into its component values.
        /// </summary>
        /// <param name="startPoint">The starting point of the object.</param>
        /// <param name="endPoint">The ending point of the object.</param>
        public void Deconstruct(out string startPoint, out string endPoint)
        {
            // Get the start point
            startPoint = this.startPoint;

            // Get the end point
            endPoint = this.endPoint;
        }

        /// <summary>
        /// Clear the connection state.
        /// </summary>
        public void Clear()
        {
            // Clear the start and end points
            startPoint = string.Empty;
            endPoint = string.Empty;

            // Recalculate the hash code when the connection state is cleared
            hashCode = HashCode.Combine(startPoint, endPoint);
        }

        public readonly bool Equals(ConnectionState other) => startPoint == other.startPoint && endPoint == other.endPoint;

        public override readonly bool Equals(object obj) => obj is ConnectionState other && Equals(other);

        public override int GetHashCode() => hashCode;
    }
}