using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper.Editor
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "New World Graph Asset", menuName = "World Shaper/New World Graph Asset")]
    public class WorldGraphAsset : ScriptableObject
    {
        public List<AreaNodeInfo> areaHandleNodes = new List<AreaNodeInfo>();

        public void AddNode(AreaNodeInfo areaNode)
        {
            // Check if the node already exists in the list to avoid duplicates
            if (!Contains(areaNode))
            {
                // Add the new node to the list
                areaHandleNodes.Add(areaNode);
            }
            else
            {
                // If the node already exists, find its index
                int index = areaHandleNodes.FindIndex(node => node.areaHandle == areaNode.areaHandle);

                // Update the existing node's position
                areaHandleNodes[index] = areaNode;
            }
        }

        public void RemoveNode(AreaNodeInfo areaNode)
        {
            // Remove the specified node from the list if it exists
            if (Contains(areaNode)) areaHandleNodes.Remove(areaNode);
        }

        public bool Contains(AreaNodeInfo areaNode) => areaHandleNodes.Exists(node => node.areaHandle == areaNode.areaHandle);

        public bool Contains(AreaHandle handle) => areaHandleNodes.Exists(node => node.areaHandle == handle);
    }

    [System.Serializable]
    public class AreaNodeInfo
    {
        public AreaHandle areaHandle;
        public Texture2D previewTexture;
        public Vector2 position;

        public AreaNodeInfo(AreaHandle areaHandle, Vector2 position)
        {
            this.areaHandle = areaHandle;
            this.position = position;
        }
    }
}
