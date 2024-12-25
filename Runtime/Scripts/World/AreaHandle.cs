using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Eflatun.SceneReference;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WorldShaper
{
    [CreateAssetMenu(fileName = "New Area Handle", menuName = "World Shaper/New Area Handle")]
    public class AreaHandle : ScriptableObject
    {
        [Header("Current Scene")]
        public SceneReference currentScene;

        [Header("Connections")]
        public List<Connection> connections = new List<Connection>();

        public Connection GetConnection(string connectionName)
        {
            foreach (var connection in connections)
            {
                if (connection.connectionName == connectionName)
                {
                    return connection;
                }
            }
            return null;
        }

        public List<string> GetAllConnectionNames()
        {
            List<string> connectionNames = new List<string>();
            foreach (var connection in connections)
            {
                connectionNames.Add(connection.connectionName);
            }
            return connectionNames;
        }

        public bool ConnectionExists(string connectionName)
        {
            foreach (var connection in connections)
            {
                if (connection.connectionName == connectionName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool MatchingPassage(string connectionName, string passageName)
        {
            foreach (var connection in connections)
            {
                if (connection.name == connectionName)
                {
                    return connection.passage.value == passageName;
                }
            }
            return false;
        }

        public bool HasConnections()
        {
            return connections.Count > 0;
        }

        #if UNITY_EDITOR

        #region Create Area Handle From Scene

        private void OnValidate()
        {
            ValidateConnections();
        }

        public void ValidateConnections()
        {
            foreach (Connection connection in connections)
            {
                connection.RenameConnection(connection.connectionName);
                connection.CreateConnectionList();
            }
        }

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
            if (connectionName != "")
            {
                connection.name = Path.GetFileNameWithoutExtension(connectionName);
                connection.connectionName = connection.name;
                AddConnection(connection);
                Refresh();
            }
        }

        public void AddConnection(Connection connection)
        {
            connections.Add(connection);
            AssetDatabase.AddObjectToAsset(connection, this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void MoveConnectionUp(Connection connection)
        {
            int index = connections.IndexOf(connection);
            if (index > 0)
            {
                connections.Remove(connection);
                connections.Insert(index - 1, connection);
            }

            AssetDatabase.ForceReserializeAssets(new string[] { AssetDatabase.GetAssetPath(this) });
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void MoveConnectionDown(Connection connection)
        {
            int index = connections.IndexOf(connection);
            if (index < connections.Count - 1)
            {
                connections.Remove(connection);
                connections.Insert(index + 1, connection);
            }

            AssetDatabase.ForceReserializeAssets(new string[] { AssetDatabase.GetAssetPath(this) });
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public void RemoveConnection(Connection connection)
        {
            connections.Remove(connection);
            connection.Delete();
        }

        public void ClearConnections()
        {
            foreach (var connection in connections)
            {
                connection.Delete();
            }
            connections.Clear();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Create/World Shaper/Area Handle From Scene")]
        private static void CreateAreaHandle()
        {
            if (!SceneAssetSelected()) return;

            // Create a new Area Handle instance and set the current scene from the selected scene asset
            var areaHandle = CreateInstance<AreaHandle>();
            areaHandle.currentScene = GetSceneReference();

            // Name the Area Handle based off of the scene's name, remove the ".unity" extension, and add _Handle
            string currentSceneName = GetSceneFromSelection().name;
            string[] splitName = currentSceneName.Split('.');
            currentSceneName = splitName[0];
            currentSceneName = currentSceneName + "_Handle";
            areaHandle.name = currentSceneName;

            // Create the new Area Handle asset
            AssetDatabase.CreateAsset(areaHandle, ScenePath() + currentSceneName + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/Create/World Shaper/Area Handle From Scene", true)]
        private static bool SceneAssetSelected()
        {
            if (Selection.activeObject == null) return false;
            var selectedObject = Selection.activeObject;
            if (selectedObject == null) return false;
            else if (selectedObject.GetType() == typeof(SceneAsset)) return true;
            else return false;
        }

        [ContextMenu("Refresh")]
        private void Refresh()
        {
            Clear();
            connections = GetAllConnectionSubAssets();
            foreach (var connection in connections)
            {
                connection.Refresh(false);
            }
        }

        [ContextMenu("Clear")]
        private void Clear()
        {
            connections.Clear();
        }

        private List<Connection> GetAllConnectionSubAssets()
        {
            // Create a list of connection sub assets
            List<Connection> connectionSubAssets = new List<Connection>();

            // Get all sub assets of the Area Handle
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(Selection.activeObject));

            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].GetType() != typeof(Connection)) continue;

                string path = AssetDatabase.GetAssetPath(objs[i]);
                Connection connection = (Connection)objs[i];
                connectionSubAssets.Add(connection);
            }

            return connectionSubAssets;
        }

        private static SceneReference GetSceneReference()
        {
            SceneReference sceneReference = new SceneReference(GetSceneFromSelection());
            return sceneReference;
        }

        private static SceneAsset GetSceneFromSelection()
        {
            var selectedObject = Selection.activeObject;
            if (selectedObject == null) return default;

            if (selectedObject.GetType() == typeof(SceneAsset))
            {
                return selectedObject as SceneAsset;
            }

            return default;
        }

        #endregion

        #region Static Methods

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
    }
}