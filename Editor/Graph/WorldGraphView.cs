using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace WorldShaper.Editor
{
    public class WorldGraphView : GraphView
    {
        public StyleSheet graphStyleSheet;
        public StyleSheet nodeStyleSheet;
        public WorldGraphAsset worldGraphAsset;

        public WorldGraphView(StyleSheet graphStyleSheet, StyleSheet nodeStyleSheet, WorldGraphAsset worldGraphAsset = null)
        {
            // Set up class properties
            this.graphStyleSheet = graphStyleSheet;
            this.nodeStyleSheet = nodeStyleSheet;
            this.worldGraphAsset = worldGraphAsset;

            // Apply styles
            AddStyles();

            // Set up manipulators for interaction
            AddManipulators();

            // If a WorldGraphAsset is provided, create nodes from it
            AddGridBackground();
        }

        private void AddStyles()
        {
            // Apply the provided style sheets to the graph view
            styleSheets.Add(graphStyleSheet);

            // Apply the node style sheet for node-specific styling
            styleSheets.Add(nodeStyleSheet);
        }

        private void AddManipulators()
        {
            // Enable zooming in and out
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);

            // Allow dragging of the graph view
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Right-click context menu to add groups
            this.AddManipulator(CreateGroup());
        }

        private void AddGridBackground()
        {
            // Add a grid background to the graph view
            GridBackground gridBackground = new GridBackground();

            // Ensure the grid background covers the entire graph view
            gridBackground.StretchToParentSize();

            // Add the grid background as the first element in the graph view
            Insert(0, gridBackground);
        }

        private IManipulator CreateGroup() => new ContextualMenuManipulator(menuEvent => menuEvent.menu.AppendAction("Add Group", actionEvent => AddElement(CreateGroup("New Area Group", actionEvent.eventInfo.localMousePosition))));

        private Group CreateGroup(string title, Vector2 localMousePosition)
        {
            // Create a new group at the specified mouse position with the given title
            Group group = new Group { title = title };

            // Set the position of the group to the mouse position
            group.SetPosition(new Rect(localMousePosition, Vector2.zero));

            // Return the created group
            return group;
        }

        public AreaHandleNode CreateNode(AreaHandle area, Vector2 position, WorldGraphAsset worldGraphAsset)
        {
            // Create a new AreaHandleNode instance 
            AreaHandleNode areaHandleNode = new AreaHandleNode();

            // Initialize the node with the provided AreaHandle and position
            areaHandleNode.Initialize(area, position);

            // Set the WorldGraphView dependency
            areaHandleNode.SetDependancy(this);

            // Set the WorldGraphAsset dependency if provided
            areaHandleNode.SetDependancy(worldGraphAsset);

            // Draw the node's visual elements
            areaHandleNode.DrawNode();

            // Return the created node
            return areaHandleNode;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            // Get all ports in the graph view
            List<Port> compatiblePorts = new List<Port>();

            // Iterate through each port in the graph view
            foreach (Port port in ports)
            {
                // Check if the port is compatible based on direction and node
                if (startPort != port && startPort.node != port.node && startPort.direction != port.direction)
                {
                    // Add the port to the list of compatible ports 
                    compatiblePorts.Add(port);
                }
            }

            // Return the list of compatible ports
            return compatiblePorts;
        }
    }
}