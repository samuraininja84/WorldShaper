using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;

namespace WorldShaper.Editor
{
    [System.Serializable]
    public class AreaHandleNode : Node
    {
        public AreaHandle areaHandle;
        public Vector2 position;

        public string AreaName { get; private set; }

        public WorldGraphAsset worldGraphAsset { get; set; }

        public WorldGraphView graphView { get; private set; }

        public virtual void Initialize(AreaHandle area, Vector2 position)
        {
            areaHandle = area;
            AreaName = areaHandle.name;

            SetPosition(new Rect(position, Vector2.zero));

            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            SelectHandle();

            this.RegisterCallback<PointerDownEvent>(SelectHandle, TrickleDown.NoTrickleDown);
        } 

        public virtual void SetDependancy(WorldGraphView graphView = null)
        {
            this.graphView = graphView;
        }

        public virtual void SetDependancy(WorldGraphAsset worldGraphAsset = null)
        {
            this.worldGraphAsset = worldGraphAsset;
        }

        public virtual void DrawNode()
        {
            // Center the title of the node
            titleContainer.style.alignItems = Align.Center;

            // Set the title of the node
            title = AreaName;
            titleContainer.AddToClassList("ds-node__title-container");

            // Set the size of the node
            expanded = true;

            // Set the color of the node
            mainContainer.style.backgroundColor = Color.gray;

            // Draw the connection foldout
            DrawFoldout(true);

            // Refresh the node
            RefreshExpandedState();
            RefreshPorts();
        }

        private void SelectHandle()
        {
            if (areaHandle != null) Selection.activeObject = areaHandle;
            position = GetPosition().position;
        }

        private void SelectHandle(PointerDownEvent pointerDown)
        {
            SelectHandle();
        }

        private void DrawFoldout(bool showPorts)
        {
            VisualElement customDataContainer = new VisualElement();

            Foldout textFoldout = new Foldout()
            {
                text = "Area Transitions"
            };

            // Set the foldout to be closed by default
            textFoldout.value = false;

            // Add a text field for each connection
            List<string> connections = GetAllConnections();
            foreach (var connection in connections)
            {
                TextField textField = new TextField()
                {
                    value = connection
                };

                textFoldout.Add(textField);

                if (showPorts)
                {
                    // Add input and output ports for each connection
                    var direction = UnityEditor.Experimental.GraphView.Direction.Input;
                    Port inputPort = Port.Create<Edge>(Orientation.Horizontal, direction, Port.Capacity.Single, typeof(bool));
                    inputPort.portName = "Input";
                    textFoldout.Add(inputPort);

                    direction = UnityEditor.Experimental.GraphView.Direction.Output;
                    Port outputPort = Port.Create<Edge>(Orientation.Horizontal, direction, Port.Capacity.Single, typeof(bool));
                    outputPort.portName = "Output";
                    textFoldout.Add(outputPort);
                }
            }

            textFoldout.Add(customDataContainer);

            customDataContainer.Add(textFoldout);

            extensionContainer.Add(customDataContainer);
        }

        private List<string> GetAllConnections()
        {
            if (areaHandle == null) return new List<string>();
            List<string> connections = new List<string>();
            for (int i = 0; i < areaHandle.connections.Count; i++)
            {
                connections.Add(areaHandle.connections[i].passage.value);
            }
            return connections;
        }

        public bool MatchingAreaHandle(AreaHandle area)
        {
            if (area == areaHandle)
            {
                return true;
            }

            return false;
        }
    }
}
