using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Eflatun.SceneReference;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WorldShaper
{
    /// <summary>
    /// Represents a handle for managing areas and their connections in a world-shaping context.
    /// </summary>
    /// <remarks>
    /// The <see cref="AreaHandle"/> class is a <see cref="ScriptableObject"/> that provides
    /// functionality for managing connections between areas and their associated scenes. 
    /// It allows for retrieving, adding, and validating connections, as well as querying connection-related information.
    /// </remarks>
    [CreateAssetMenu(fileName = "New Area Handle", menuName = "World Shaper/New Area Handle")]
    public class AreaHandle : ScriptableObject
    {
        [Header("Scene(s)")]
        public SceneReference activeScene;
        public AreaHandleType areaHandleType = AreaHandleType.Normal;

        [Header("Additive Scenes")]
        [Tooltip("Additional scenes that will be loaded additively with the active scene.")]
        public List<SceneReference> additiveScenes = new();

        [Header("Connections")]
        [Tooltip("Connections that lead to other Area Handles or Scenes.")]
        public List<Connection> connections = new();

        public string Name => IsValid ? activeScene.Name : name;

        public bool IsValid => activeScene.UnsafeReason != SceneReferenceUnsafeReason.Empty;

        public async void LoadArea(int connectionIndex = -1)
        {
            // Validate the active scene reference before attempting to load the area
            if (!IsValid)
            {
                // Log an error message indicating that the area cannot be loaded due to an invalid active scene reference
                Debug.LogError($"Cannot load area: {name} has an invalid active scene reference.");

                // Return early to prevent attempting to load an invalid scene
                return;
            }

            // If a valid connection index is provided, attempt to switch to the area associated with that connection; otherwise, switch to the area represented by this AreaHandle
            if (connectionIndex >= 0) await Transistor.Current.SwitchToArea(Name, connectionIndex);
            else await Transistor.Current.HandleAreaSwitch(this);
        }

        #region Scene Reference Retrieval Methods

        /// <summary>
        /// Retrieves the destination scene associated with the specified connection ID.
        /// </summary>
        /// <param name="id">The unique identifier of the connection to search for.</param>
        /// <returns>The <see cref="SceneReference"/> representing the destination scene if a connection with the specified ID is
        /// found; otherwise, <see langword="null"/>.</returns>
        public SceneReference GetDestination(SerializableGuid id) => connections.Find(c => c.connectionId == id).Destination;

        /// <summary>
        /// Retrieves the destination scene based on the index of the connection in the list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// If the index is valid, returns the connected scene of the connection at that index; otherwise, returns null.
        /// </returns>
        public SceneReference GetDestination(int index) => connections[index].Destination;

        /// <summary>
        /// Retrieves the destination scene associated with the specified connection name.
        /// </summary>
        /// <remarks>
        /// This method searches the list of connections for a connection with a matching name and returns the associated scene.
        /// </remarks>
        /// <param name="name">The name of the connection to search for. Cannot be null or empty.</param>
        /// <returns>
        /// The <see cref="SceneReference"/> of the connected scene if a matching connection is found; otherwise, <see langword="null"/>.
        /// </returns>
        public SceneReference GetDestination(string name) => connections.Find(c => c.connectionName == name).Destination;

        /// <summary>
        /// Retrieves the currently active scene.
        /// </summary>
        /// <returns>A <see cref="SceneReference"/> representing the active scene.</returns>
        public SceneReference GetActiveScene() => activeScene;

        /// <summary>
        /// Retrieves a list of all scenes, including the active scene and any additive scenes.
        /// </summary>
        /// <remarks>The returned list includes the active scene followed by any additive scenes that have been added. 
        /// This method does not modify the state of the scenes or their order.
        /// </remarks>
        /// <returns
        /// >A <see cref="List{T}"/> of <see cref="SceneReference"/> objects representing the active scene and all additive scenes. 
        /// The active scene is the first item in the list.
        /// </returns>
        public List<SceneReference> GetAllScenes()
        {
            // Create a new list to hold all scenes
            List<SceneReference> scenes = new List<SceneReference> { activeScene };

            // Add all additive scenes to the list
            scenes.AddRange(additiveScenes);

            // Return the list of all scenes
            return scenes;
        }

        /// <summary>
        /// The number of additive scenes in the list.
        /// </summary>
        /// <returns>An integer representing the count of additive scenes.</returns>
        public int GetAdditiveSceneCount() => additiveScenes.Count;

        /// <summary>
        /// Boolean indicating if there are any additive scenes in the list.
        /// </summary>
        /// <returns>A boolean value: true if there is at least one additive scene, otherwise false.</returns>
        public bool HasAdditiveScenes() => additiveScenes.Count > 0;

        /// <summary>
        /// A Boolean indicating if the area handle is normal, meaning it can be traversed and interacted with without any special conditions.
        /// </summary>
        /// <returns>A boolean value: true if the area handle is normal, otherwise false.</returns>
        public bool Normal() => areaHandleType == AreaHandleType.Normal;

        /// <summary>
        /// A Boolean indicating if the area handle is impassable, meaning it cannot be traversed or interacted with.
        /// </summary>
        /// <returns>A boolean value: true if the area handle is impassable, otherwise false.</returns>
        public bool Impassable() => areaHandleType == AreaHandleType.Impassable;

        /// <summary>
        /// A Boolean indicating if the area handle is persistent, meaning it should not be unloaded when transitioning to another scene.
        /// </summary>
        /// <returns>A boolean value: true if the area handle is persistent, otherwise false.</returns>
        public bool Persistent() => areaHandleType == AreaHandleType.Persistent;

        #endregion

        #region Connection Retrieval Methods

        /// <summary>
        /// Get the first connection in the list.
        /// </summary>
        /// <returns>
        /// The first connection if available, otherwise null.
        /// </returns>
        public Connection First => connections.First();

        /// <summary>
        /// Gets the <see cref="Connection"/> object at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the connection to retrieve.</param>
        /// <returns>The <see cref="Connection"/> object at the specified index.</returns>
        public Connection this[int index] => GetConnection(index);

        /// <summary>
        /// Gets the <see cref="Connection"/> associated with the specified <see cref="SerializableGuid"/>.
        /// </summary>
        /// <remarks>This indexer provides a convenient way to access a connection by its unique
        /// identifier. Ensure that the provided <paramref name="id"/> is valid and corresponds to an existing
        /// connection.</remarks>
        /// <param name="id">The unique identifier of the connection to retrieve.</param>
        /// <returns>The <see cref="Connection"/> associated with the specified <paramref name="id"/>, or <c>null</c> if no
        /// connection is found.</returns>
        public Connection this[SerializableGuid id] => GetConnection(id);

        /// <summary>
        /// Retrieves a connection by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the connection to retrieve.</param>
        /// <returns>The <see cref="Connection"/> object associated with the specified identifier,  or <see langword="null"/> if no matching connection is found.</returns>
        public Connection GetConnection(SerializableGuid id) => connections.FirstOrDefault(c => c.connectionId == id);

        /// <summary>
        /// Retrieves a connection by its name.
        /// </summary>
        /// <param name="name">The name of the connection to retrieve. 
        /// This value cannot be null or empty.</param>
        /// <returns>The <see cref="Connection"/> object with the specified name, or <see langword="null"/> if no connection with the given name exists.</returns>
        public Connection GetConnection(string name) => connections.FirstOrDefault(c => c.connectionName == name);

        /// <summary>
        /// Retrieves a connection from the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the connection to retrieve. 
        /// If the index is out of range, it will be clamped to the valid range of indices.</param>
        /// <returns>The <see cref="Connection"/> object at the specified index, or the nearest valid connection if the index is out of range.</returns>
        public Connection GetConnection(int index) => connections[Mathf.Clamp(index, 0, connections.Count - 1)];

        /// <summary>
        /// Retrieves the name and index of a connection based on its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the connection to retrieve.</param>
        /// <returns>The name and index assiciatied with the specified <see cref="Connection"/></returns>
        public (string name, int index) GetConnectionInfo(SerializableGuid id)
        {
            // Attempt to retrieve the connection with the specified ID
            var connection = GetConnection(id);

            // If the connection is null, log an error and return null and -1
            if (connection == null)
            {
                // Log an error message indicating that the connection with the specified ID was not found in this Area Handle
                Debug.LogError($"Connection with ID {id} not found in Area Handle {Name}.");

                // Return null and -1 to indicate that the connection was not found
                return (null, -1);
            }

            return (connection.connectionName, GetConnectionIndex(connection));
        }

        /// <summary>
        /// Retrieves the index of the specified connection in the collection.
        /// </summary>
        /// <param name="connection">The connection whose index is to be found.</param>
        /// <returns>The zero-based index of the connection if found; otherwise, -1.</returns>
        public int GetConnectionIndex(Connection connection) => connections.IndexOf(connection);

        /// <summary>
        /// Retrieves the index of a connection by its name.
        /// </summary>
        /// <param name="name">The name of the connection whose index is to be found.</param>
        /// <returns>The zero-based index of the connection if found; otherwise, -1.</returns>
        public int GetConnectionIndex(string name) => connections.FindIndex(c => c.connectionName == name);

        /// <summary>
        /// Get all connection names in the list.
        /// </summary>
        /// <returns>
        /// Returns a list of connection names.
        /// </returns>
        public List<string> GetAllConnectionNames() => connections.Select(c => c.connectionName).ToList();

        /// <summary>
        /// Gets the current number of active connections.
        /// </summary>
        /// <returns>
        /// The total count of active connections.
        /// </returns>
        public int GetConnectionCount() => connections.Count;

        #endregion

        #region Validation Methods

        /// <summary>
        /// Determines whether a connection exists at the specified id.
        /// </summary>
        /// <param name="id">The id of the connection to check.</param>
        /// <returns><see langword="true"/> if a connection exists at the specified id; otherwise, <see langword="false"/>.</returns>
        public bool ConnectionExists(SerializableGuid id) => connections.Any(c => c.connectionId == id);

        /// <summary>
        /// Determines whether a connection with the specified name exists.
        /// </summary>
        /// <remarks>This method checks the collection of connections to determine if any connection matches the specified name.</remarks>
        /// <param name="name">The name of the connection to search for. Cannot be <see langword="null"/> or empty.</param>
        /// <returns><see langword="true"/> if a connection with the specified name exists; otherwise, <see langword="false"/>.</returns>
        public bool ConnectionExists(string name) => connections.Any(c => c.connectionName == name);

        /// <summary>
        /// Determines whether a connection exists at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the connection to check.</param>
        /// <returns><see langword="true"/> if a connection exists at the specified index; otherwise, <see langword="false"/>.</returns>
        public bool ConnectionExists(int index) => index >= 0 && index < connections.Count;

        /// <summary>
        /// Determines whether there are any active connections.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if there is at least one active connection; otherwise, <see langword="false"/>.
        /// </returns>
        public bool HasConnections() => connections.Count > 0;

        #endregion

        #region Editor Methods

        #if UNITY_EDITOR

        #region Connection Management

        private void OnValidate() => ValidateConnections();

        /// <summary>
        /// Validate the connections in the area handle.
        /// </summary>
        public void ValidateConnections()
        {
            // Iterate through all connections and rename them if necessary
            foreach (Connection connection in connections)
            {
                // If the connection's name is null or empty, set it to the connection's connection name, otherwise do nothing
                connection.RenameConnection(connection.connectionName);

                // Refresh the connection to update the editor
                connection.CreateConnectionList();

                // Lock the connection's endpoint to prevent it from being changed
                connection.endpoint.Lock();
            }
        }

        /// <summary>
        /// Create a new connection and add it to the area handle.
        /// </summary>
        public void CreateConnection()
        {
            // Create a new connection data
            Connection connection = CreateInstance<Connection>();

            // Ask the user for a connection data name and allow them to set it
            string path = AssetDatabase.GetAssetPath(this);

            // Remove the object name from the path
            path = path.Replace(name + ".asset", "");

            // Open the save file panel in the project window to the path
            string connectionName = EditorUtility.SaveFilePanelInProject("Save Connection Data", "New Connection Data", "asset", "Save Connection Data", path);

            // If the user provided a name, set the connection's name and add it to the area handle, otherwise do nothing
            if (connectionName != "")
            {
                // Set the connection's name based on the name provided by the user, remove the ".asset" extension, and set it as the connection's name
                connection.name = Path.GetFileNameWithoutExtension(connectionName);

                // Set the connection's name as its connection name
                connection.connectionName = connection.name;

                // Lock the connection's endpoint to prevent it from being changed before the connection is added to the area handle
                connection.endpoint.Lock();

                // Add the connection to the area handle's list of connections and save it as a sub asset of the area handle
                AddConnection(connection);

                // Refresh the connections in the area handle to update the editor
                Refresh();

                // Move the connection to the bottom of the list to ensure it is added at the end of the list
                MoveToBottom(connection);
            }
        }

        /// <summary>
        /// Add a connection to the area handle's list of connections and save it as a sub asset of the area handle.
        /// </summary>
        /// <param name="connection">The connection to be added to the area handle.</param>
        public void AddConnection(Connection connection)
        {
            // Add the connection to the list of connections
            connections.Add(connection);

            // Add the connection to the asset database as a sub asset of the area handle
            AssetDatabase.AddObjectToAsset(connection, this);

            // Set the this area handle to dirty
            EditorUtility.SetDirty(this);

            // Save the changes to the asset database
            AssetDatabase.SaveAssets();

            // Refresh the asset database to update the connection order in the editor
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Move a connection up in the list of connections.
        /// </summary>
        /// <param name="connection">The connection to be moved up in the list.</param>
        public void MoveConnectionUp(Connection connection)
        {
            // Check if the connection is not the first one in the list, if so, move it up
            int index = connections.IndexOf(connection);

            // If the connection is not already at the top, move it up
            if (index > 0)
            {
                // Remove the connection from its current position
                connections.Remove(connection);

                // If the connection is being moved up from the last position, insert it at the index, otherwise insert it at the index - 1
                connections.Insert(index - 1, connection);

                // Force resave the asset to update the connection order
                AssetDatabase.ForceReserializeAssets(new string[] { AssetDatabase.GetAssetPath(this) });

                // Set the this area handle to dirty
                EditorUtility.SetDirty(this);

                // Save the changes to the asset database
                AssetDatabase.SaveAssets();

                // Refresh the asset database to update the connection order in the editor
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Move a connection down in the list of connections.
        /// </summary>
        /// <param name="connection">The connection to be moved down in the list.</param>
        public void MoveConnectionDown(Connection connection)
        {
            // Check if the connection is not the last one in the list, if so, move it down
            int index = connections.IndexOf(connection);

            // If the connection is not already at the bottom, move it down
            if (index < connections.Count - 1)
            {
                // Remove the connection from its current position
                connections.Remove(connection);

                // If the connection is being moved down from the first position, insert it at the index, otherwise insert it at the index + 1
                connections.Insert(index + 1, connection);

                // Force resave the asset to update the connection order
                AssetDatabase.ForceReserializeAssets(new string[] { AssetDatabase.GetAssetPath(this) });

                // Set the this area handle to dirty
                EditorUtility.SetDirty(this);

                // Save the changes to the asset database
                AssetDatabase.SaveAssets();

                // Refresh the asset database to update the connection order in the editor
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Moves a connection to the top of the list of connections.
        /// </summary>
        /// <param name="connection">The connection to move to the top of the list. Cannot be null.</param>
        public void MoveToTop(Connection connection)
        {
            // Check if the connection is not the first one in the list, if so, move it to the top
            int index = connections.IndexOf(connection);

            // If the connection is not already at the top, move it to the top
            if (index > 0)
            {
                // Remove the connection from its current position
                connections.Remove(connection);

                // Add the connection to the beginning of the list
                connections.Insert(0, connection);

                // Force resave the asset to update the connection order
                AssetDatabase.ForceReserializeAssets(new string[] { AssetDatabase.GetAssetPath(this) });

                // Set the this area handle to dirty
                EditorUtility.SetDirty(this);

                // Save the changes to the asset database
                AssetDatabase.SaveAssets();

                // Refresh the asset database to update the connection order in the editor
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Move a connection to the bottom of the list of connections.
        /// </summary>
        /// <param name="connection">The connection to be moved to the bottom of the list.</param>
        public void MoveToBottom(Connection connection)
        {
            // Check if the connection is not the last one in the list, if so, move it to the bottom
            int index = connections.IndexOf(connection);

            // If the connection is not already at the bottom, move it to the bottom
            if (index < connections.Count - 1)
            {
                // Remove the connection from its current position
                connections.Remove(connection);

                // Add the connection to the end of the list
                connections.Add(connection);

                // Force resave the asset to update the connection order
                AssetDatabase.ForceReserializeAssets(new string[] { AssetDatabase.GetAssetPath(this) });

                // Set the this area handle to dirty
                EditorUtility.SetDirty(this);

                // Save the changes to the asset database
                AssetDatabase.SaveAssets();

                // Refresh the asset database to update the connection order in the editor
                AssetDatabase.Refresh();
            }
        }

        /// <summary>
        /// Remove a connection from the list and delete it.
        /// </summary>
        /// <param name="connection">The connection to be removed and deleted.</param>
        public void RemoveConnection(Connection connection)
        {
            // Remove the connection from the list
            connections.Remove(connection);

            // Delete the connection from the asset database
            connection.Delete();
        }

        /// <summary>
        /// Clear all connections from the area handle.
        /// </summary>
        public void ClearConnections()
        {
            // Iterate through all connections and delete them
            connections.ForEach(c => c.Delete());

            // Clear the connections list
            connections.Clear();

            // Set the connection to dirty
            EditorUtility.SetDirty(this);

            // Save the changes to the asset database and refresh it
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Refresh the connections in the area handle.
        /// </summary>
        [ContextMenu("Refresh")]
        private void Refresh()
        {
            // Clear the connections list
            Clear();

            // Get all connection sub assets from the area handle
            connections = GetAllConnectionSubAssets();

            // Iterate through all connections and refresh them
            connections.ForEach(c => c.Refresh(false));
        }

        /// <summary>
        /// Clear all connections from the area handle.
        /// </summary>
        [ContextMenu("Clear")]
        private void Clear() => connections.Clear();

        /// <summary>
        /// Get all connection sub assets from the area handle.
        /// </summary>
        /// <returns>
        /// A list of connection sub assets.
        /// </returns>
        private List<Connection> GetAllConnectionSubAssets()
        {
            // Create a list of connection sub assets
            List<Connection> connectionSubAssets = new List<Connection>();

            // Get all sub assets of the Area Handle
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));

            // Iterate through all sub assets and check if they are of type Connection
            for (int i = 0; i < objs.Length; i++)
            {
                // Check if the sub asset is of type Connection, if not continue
                if (objs[i].GetType() != typeof(Connection)) continue;

                // Get the path of the sub asset and check if it is a connection
                string path = AssetDatabase.GetAssetPath(objs[i]);

                // Set it as a connection sub asset
                Connection connection = (Connection)objs[i];

                // Add the connection to the list of connection sub assets
                connectionSubAssets.Add(connection);
            }

            // Return the list of connection sub assets
            return connectionSubAssets;
        }

        #endregion

        #region Create Area Handle From Scenes

        /// <summary>
        /// Create a new area handle from the selected scene asset.
        /// </summary>
        [MenuItem("Assets/Create/World Shaper/Area Handle(s) From Scene")]
        private static void CreateAreaHandle()
        {
            // Check if the selected object is a scene asset, if not, return
            if (!SceneAssetSelected()) return;

            // Iterate through all selected scene assets and create an area handle for each one
            foreach (var sceneReference in GetSceneReferencesFromSelection())
            {
                // Create a new Area Handle instance and set the current scene from the selected scene asset
                var areaHandle = CreateInstance<AreaHandle>();

                // Set the active scene of the Area Handle to the selected scene asset
                areaHandle.activeScene = sceneReference;
    
                // Name the Area Handle based off of the scene's name, remove the ".unity" extension, and add _Handle
                string currentSceneName = areaHandle.name = Path.GetFileNameWithoutExtension(sceneReference.Name) + "_Handle";
    
                // Create the new Area Handle asset at the path of the selected scene asset
                AssetDatabase.CreateAsset(areaHandle, ScenePath() + currentSceneName + ".asset");
            }

            // Save the changes to the asset database 
            AssetDatabase.SaveAssets();

            // Refresh the asset database to show the new Area Handle asset in the editor
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Check if the selected object is a scene asset.
        /// </summary>
        /// <returns>
        /// Boolean indicating if the selected object is a scene asset.
        /// </returns>
        [MenuItem("Assets/Create/World Shaper/Area Handle(s) From Scene", true)]
        private static bool SceneAssetSelected()
        {
            // Check if the selected object is null or not
            if (Selection.activeObject == null) return false;

            // Initialize the selected object as the active object in the selection
            var selectedObject = Selection.activeObject;

            // Check if the selected object is a scene asset, if so return true, otherwise return false
            if (selectedObject == null) return false;
            else if (selectedObject.GetType() == typeof(SceneAsset)) return true;
            else return false;
        }

        /// <summary>
        /// Get the scene reference from the selected scene asset.
        /// </summary>
        /// <returns>
        /// A Scene Reference object representing the selected scene asset.
        /// </returns>
        private static List<SceneReference> GetSceneReferencesFromSelection()
        {
            // Check if the selected object is null or not
            var selectedObjects = Selection.objects;

            // Check if there are any selected objects, if not return default
            if (selectedObjects == null || selectedObjects.Length == 0) return default;

            // Initialize a list to hold the selected scene assets
            List<SceneReference> sceneReferences = new();

            // Look through all selected objects and check if they are of type Scene Asset, if so add them to the list of selected scene assets
            foreach (var obj in selectedObjects)
            {
                // Check if the object is of type Scene Asset, if so add it to the list of selected scene assets
                if (obj.GetType() == typeof(SceneAsset)) sceneReferences.Add(new SceneReference((SceneAsset)obj));
            }

            // Return the list of selected scene assets as scene references
            return sceneReferences;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Get the path of the selected scene asset.
        /// </summary>
        /// <returns>
        /// A string representing the path of the selected scene asset.
        /// </returns>
        private static string ScenePath()
        {
            // Create an empty string for the path
            string path = string.Empty;

            // Find the path of the selected scene asset
            path = AssetDatabase.GetAssetPath(Selection.activeObject);
            string objectName = Selection.activeObject.name;
            int objectNameLength = objectName.Length + 6;

            // Remove the "Assets" and ".unity" from the path
            path = path.Replace("AssetsAssets", "Assets");
            path = path.Remove(path.Length - objectNameLength);

            // Return the path
            return path;
        }

        #endregion

        #endif

        #endregion
    }

    public enum AreaHandleType
    {
        Normal,
        Impassable,
        Persistent
    }
}