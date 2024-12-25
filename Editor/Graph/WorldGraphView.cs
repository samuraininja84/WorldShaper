using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace WorldShaper.Editor
{
    public class WorldGraphView : GraphView
    {
        public WorldGraphAsset worldGraphAsset;
        public string styleSheetsPath = "Assets/Scripts/Tooling/World Shaper/Resources/StyleSheets/";

        public WorldGraphView()
        {
            AddManipulators();
            AddGridBackground();
            AddStyles();
        }

        private void AddManipulators()
        {
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(CreateGroup());
        }

        private void AddGridBackground()
        {
            GridBackground gridBackground = new GridBackground();

            gridBackground.StretchToParentSize();

            Insert(0, gridBackground);
        }

        private void AddStyles()
        {
            StyleSheet graphStyleSheet = (StyleSheet)EditorGUIUtility.Load(styleSheetsPath + "AreaDataViewStyles.uss");
            StyleSheet nodeStyleSheet = (StyleSheet)EditorGUIUtility.Load(styleSheetsPath + "AreaDataNodeStyles.uss");
            styleSheets.Add(graphStyleSheet);
            styleSheets.Add(nodeStyleSheet);
        }

        private IManipulator CreateGroup()
        {
            ContextualMenuManipulator contextualMenuManipulator = new ContextualMenuManipulator(
                menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("New Area Group", actionEvent.eventInfo.localMousePosition)))
            );

            return contextualMenuManipulator;
        }

        private Group CreateGroup(string title, Vector2 localMousePosition)
        {
            Group group = new Group
            {
                title = title
            };

            group.SetPosition(new Rect(localMousePosition, Vector2.zero));

            return group;
        }

        public AreaHandleNode CreateNode(AreaHandle area, Vector2 position, WorldGraphAsset worldGraphAsset = null)
        {
            AreaHandleNode areaHandleNode = new AreaHandleNode();

            areaHandleNode.Initialize(area, position);
            areaHandleNode.SetDependancy(this);
            areaHandleNode.SetDependancy(worldGraphAsset);
            areaHandleNode.DrawNode();

            return areaHandleNode;
        }

        public AreaHandleNode CreateNode(AreaHandleNode areaNode, Vector2 position, WorldGraphAsset worldGraphAsset = null)
        {
            areaNode.Initialize(areaNode.areaHandle, position);
            areaNode.SetDependancy(this);
            areaNode.SetDependancy(worldGraphAsset);
            areaNode.DrawNode();

            return areaNode;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        private void CreateNodesFromWorldGraph()
        {
            // Get all AreaData assets in the project from the listed file path
            List<AreaHandleNode> areaHandleNodes = new List<AreaHandleNode>(worldGraphAsset.areaHandleNodes);
            foreach (AreaHandleNode areaHandleNode in areaHandleNodes)
            {
                AddElement(CreateNode(areaHandleNode, areaHandleNode.position));
            }
        }
    }
}