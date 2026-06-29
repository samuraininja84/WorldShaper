using System.Collections.Generic;
using UnityEngine;
using Eflatun.SceneReference;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WorldShaper
{
    [CreateAssetMenu(fileName = "New Connection", menuName = "World Shaper/New Connection")]
    public class Connection : ScriptableObject
    {
        // [Header("Connection Identity")]
        public SerializableGuid connectionId = SerializableGuid.NewGuid();
        public string connectionName = string.Empty;
        public ConnectionType connectionType = ConnectionType.Standard;

        // [Header("Transition Settings")]
        public ConnectionReference destination;
        public TransitionIdentifier transitionIn;
        public TransitionIdentifier transitionOut;

        /// <summary>
        /// Gets the scene reference of the destination area.
        /// </summary>
        public SceneReference Destination => destination.Area.activeScene;

        /// <summary>
        /// The name of the connection, which is either the connection name if it is not empty, or the default name of the connection object if the connection name is empty.
        /// </summary>
        public string Name => connectionName != string.Empty ? connectionName : name;

        /// <summary>
        /// Gets the name of the connection's starting point, the connection name.
        /// </summary>
        public string StartPoint => connectionName;

        /// <summary>
        /// Gets the name of the connection's endpoint, the passage name.
        /// </summary>
        public string Endpoint => destination.Value;

        /// <summary>
        /// Gets a value indicating whether the current configuration is valid.
        /// </summary>
        public bool IsValid => destination.Area != null && destination.Area.IsValid;

        /// <summary>
        /// Load the area associated with this connection.
        /// </summary>
        public async void LoadArea() => await Transistor.SwitchToArea(this);

        /// <summary>
        /// Loads the destination area associated with this connection.
        /// </summary>
        public async void LoadDestination()
        {
            // Check if the connection type is closed
            if (connectionType == ConnectionType.Closed)
            {
                // Log a warning if the connection is closed
                Debug.LogWarning("Connection is closed. This connection is for use in one-way connections and cannot be used to load an area.", this);

                // Return early if the connection is closed
                return;
            }

            // Check if the connection is valid
            if (!IsValid)
            {
                // Log a warning if the connection is not valid
                Debug.LogWarning("Invalid connection. Cannot load area.", this);

                // Return early if the connection is not valid
                return;
            }

            // Load the destination area
            await Transistor.SwitchToDestination(this);
        }

        /// <summary>
        /// Gets the endpoint connection from the destination area.
        /// </summary>
        /// <returns>A <see cref="Connection"/> object representing the endpoint connection.</returns>
        public Connection GetEndpoint() => destination.GetCurrent();

        /// <summary>
        /// Determines whether a destination area is currently set.
        /// </summary>
        /// <returns><see langword="true"/> if a destination area is set; otherwise, <see langword="false"/>.</returns>
        public bool HasDestination() => destination.Area != null;

        /// <summary>
        /// Checks if the destination area has a connection that matches the endpoint name.
        /// </summary>
        /// <returns><see langword="true"/> if the destination area has a connection that matches the endpoint name; otherwise, <see langword="false"/>.</returns>
        public bool HasEndpoint() => destination.Area != null && destination.Area.ConnectionExists(Endpoint);

        /// <summary>
        /// Sets the connection type for this connection.
        /// </summary>
        /// <param name="type">Specifies the type of connection to set.</param>
        public void SetConnectionType(ConnectionType type) => connectionType = type;

        /// <summary>
        /// Determines whether the current connection type is standard.
        /// </summary>
        /// <returns>true if the connection type is standard; otherwise, false.</returns>
        public bool Standard() => connectionType == ConnectionType.Standard;

        /// <summary>
        /// Determines whether the connection is currently closed.
        /// </summary>
        /// <returns>true if the connection is closed; otherwise, false.</returns>
        public bool Closed() => connectionType == ConnectionType.Closed;

        #region Validation

        private void OnValidate() => RenameConnection(connectionName);

        /// <summary>
        /// Rename the connection to the new name.
        /// </summary>
        /// <param name="newName"></param>
        /// <returns>
        /// 
        /// </returns>
        public void RenameConnection(string newName = "")
        {
            // Check if the new name is the same as the current name, if so, return the current name
            if (connectionName == name) return;

            // Check if the new name is empty and set it to the current name
            if (newName == "") newName = connectionName;

            // Set the name of the connection to the new name
            name = newName;
        }

        #endregion

        #region Editor Methods
        #if UNITY_EDITOR

        /// <summary>
        /// Synchronizes the endpoint link by ensuring the connection name matches the endpoint's name.
        /// </summary>
        /// <remarks>
        /// This method checks if a connection exists for the specified endpoint in the destination area. 
        /// If a connection is found and its name differs from the current endpoint name, the endpoint name is updated to match the connection name, and the connection is refreshed.
        /// </remarks>
        [ContextMenu("Sync Endpoint Link")]
        public void SyncEndpointLink()
        {
            // Check if the connection exists
            if (destination.Area.ConnectionExists(Endpoint))
            {
                // Get the connection from the connected scene
                Connection endPoint = destination.GetCurrent();

                // If the connection exists, set the passage name to the connection name and refresh the connection
                if (endPoint != null) endPoint.destination = ConnectionReference.Create(GetParent(), connectionName);
            }
        }

        /// <summary>
        /// Assigns this connection to its parent <see cref="AreaHandle"/>.
        /// </summary>
        /// <remarks>
        /// This method retrieves the parent <see cref="AreaHandle"/> from the asset database and adds this connection to it. 
        /// Ensure that the asset is properly configured and that the parent <see cref="AreaHandle"/> exists in the asset database.
        /// </remarks>
        [ContextMenu("Assign To Parent")]
        public void AssignToParent() => GetParent().AddConnection(this);

        /// <summary>
        /// Refreshes the current state of the object, optionally performing additional operations.
        /// </summary>
        /// <remarks>
        /// This method updates the object's state and may trigger additional behavior depending on the implementation. 
        /// Use this method to ensure the object reflects the latest data or configuration.
        /// </remarks>
        [ContextMenu("Refresh")]
        public void Refresh()
        {
            // Rename the connection
            RenameConnection(connectionName);

            // Set the connection to dirty
            EditorUtility.SetDirty(this);

            // Save the changes to the asset database
            AssetDatabase.SaveAssets();

            // Refresh the asset database to reflect the changes
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Remove the connection from the parent area handle.
        /// </summary>
        [ContextMenu("Remove")]
        public void Remove()
        {
            // Remove this connection from the parent area handle
            AreaHandle parentHandle = GetParent();
            parentHandle.RemoveConnection(this);
        }

        /// <summary>
        /// Delete the connection data.
        /// </summary>
        public void Delete()
        {
            // Delete this connection data
            Undo.DestroyObjectImmediate(this);

            // Save the changes and refresh the asset database
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Retrieves the parent <see cref="AreaHandle"/> associated with this instance.
        /// </summary>
        /// <remarks>This method uses the asset path of the current instance to locate and load the parent
        /// <see cref="AreaHandle"/>. Ensure that the current instance is properly registered in the asset database for
        /// this method to function correctly.</remarks>
        /// <returns>The parent <see cref="AreaHandle"/> if one exists; otherwise, <see langword="null"/>.</returns>
        public AreaHandle GetParent() => AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(this), typeof(AreaHandle)) as AreaHandle;

        #endif
        #endregion
    }

    public enum ConnectionType
    {
        Standard,
        Closed
    }
}