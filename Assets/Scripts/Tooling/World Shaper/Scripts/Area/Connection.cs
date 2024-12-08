using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WorldShaper
{
    [CreateAssetMenu(fileName = "New Connection", menuName = "World Shaper/New Connection")]
    public class Connection : ScriptableObject
    {
        public string connectionName;
        public AreaHandle connectedScene;
        public ExtendableEnum passage;

        private void OnValidate()
        {
            RenameConnection(connectionName);
            CreateConnectionList();
        }

        public string RenameConnection(string newName = "")
        {
            if (connectionName == name) return name;

            if (newName == "")
            {
                newName = connectionName;
            }

            name = newName;
            return name;
        }

        public void CreateConnectionList(bool refresh = false)
        {
            if (!refresh)
            {
                passage.SetEnums(GetPassagesFromAreaHandle(connectedScene));
            }
            else
            {
                passage = new ExtendableEnum(GetPassagesFromAreaHandle(connectedScene));
            }
        }

        private List<string> GetPassagesFromAreaHandle(AreaHandle handle)
        {
            // Create the list of connections
            List<string> connections = new List<string> { "None" };

            // Check if the handle is null or if it has no connections
            if (handle == null || handle.connections.Count == 0)
            {
                return connections;
            }
            else
            {
                // Create a list of connection names
                connections = new List<string>();
                foreach (var connectionData in handle.connections)
                {
                    connections.Add(connectionData.connectionName);
                }
                return connections;
            }
        }

        #region Editor Methods
        #if UNITY_EDITOR

        [ContextMenu("Set Passage Link")]
        public void SetPassageLink()
        {
            if (connectedScene.ConnectionExists(passage.value))
            {
                Connection connection = connectedScene.GetConnection(passage.value);
                if (connection.passage.value != connectionName)
                {
                    // Get the index of the current connection name and set the passage index to the connection name
                    int passageIndex = connection.passage.list.IndexOf(connection.passage.value);
                    connection.passage.value = passage.list[passageIndex];
                    connection.Refresh();
                }
            }
        }

        [ContextMenu("Refresh")]
        public void Refresh(bool status = true)
        {
            RenameConnection(connectionName);
            CreateConnectionList(status);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [ContextMenu("Remove")]
        public void Remove()
        {
            // Remove this connection from the parent area handle
            AreaHandle parentHandle = GetParentAreaHandle();
            parentHandle.RemoveConnection(this);
        }

        public void Delete()
        {
            // Delete this connection data
            Undo.DestroyObjectImmediate(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [ContextMenu("Set Parent Area Handle")]
        public void SetParentAreaHandle()
        {
            AreaHandle parentHandle = (AreaHandle)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(this), typeof(AreaHandle));
            parentHandle.AddConnection(this);
        }

        public AreaHandle GetParentAreaHandle()
        {
            AreaHandle parentHandle = (AreaHandle)AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(this), typeof(AreaHandle));
            return parentHandle;
        }

        #endif
        #endregion
    }
}