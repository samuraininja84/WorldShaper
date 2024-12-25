using System.Collections.Generic;
using UnityEngine;

namespace WorldShaper.Editor
{
    [CreateAssetMenu(fileName = "New World Graph Asset", menuName = "World Shaper/New World Graph Asset")]
    public class WorldGraphAsset : ScriptableObject
    {
        public List<AreaHandleNode> areaHandleNodes;

        public void AddNode(AreaHandleNode areaNode, AreaHandle area = null)
        {
            if (!DuplicateNode(areaNode))
            {
                if (area != null) areaNode.areaHandle = area;
                areaHandleNodes.Add(areaNode);
            }
            else if (DuplicateNode(areaNode))
            {
                ReplaceNode(areaNode);
                Debug.Log("This node already exists within this graph, replacing existing node");
            }
        }

        public void RemoveNode(AreaHandleNode areaNode)
        {
            if (areaHandleNodes.Contains(areaNode))
            {
                areaHandleNodes.Remove(areaNode);
            }
        }

        private bool DuplicateNode(AreaHandleNode areaNode)
        {
            foreach (AreaHandleNode node in areaHandleNodes)
            {
                if (node.MatchingAreaHandle(areaNode.areaHandle)) return true;
            }

            return false;
        }

        private void ReplaceNode(AreaHandleNode areaNode)
        {
            // Find the node that matches the area handle
            int index = 0;
            for (int i = 0; i < areaHandleNodes.Count; i++)
            {
                if (areaHandleNodes[i].MatchingAreaHandle(areaNode.areaHandle))
                {
                    index = i;
                    break;
                }
            }

            // Remove the existing node and replace it with the new node
            areaHandleNodes.RemoveAt(index);
            areaHandleNodes.Insert(index, areaNode);
        }

    }
}
