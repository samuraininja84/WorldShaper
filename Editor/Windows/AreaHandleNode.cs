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
        public AreaNodeInfo info = new AreaNodeInfo(null, Vector2.zero);

        public AreaHandle Area { get => info.areaHandle; set => info.areaHandle = value; }

        public Vector2 Position { get => info.position; set => info.position = value; }

        public string AreaName { get; private set; }

        public WorldGraphAsset worldGraphAsset { get; set; }

        public WorldGraphView graphView { get; private set; }

        public virtual void SetDependancy(WorldGraphView graphView = null) => this.graphView = graphView;

        public virtual void SetDependancy(WorldGraphAsset worldGraphAsset = null) => this.worldGraphAsset = worldGraphAsset;

        private void OnNodePointerUp(PointerUpEvent pointerUp)
        {
            // Unregister the callback for the delete event
            UnregisterCallback<DetachFromPanelEvent>(OnNodeDeleted);
        }

        private void OnNodePointerDown(PointerDownEvent pointerDown)
        {
            // Set the active object in the Unity Editor to the AreaHandle
            if (Area != null) Selection.activeObject = Area;

            // Register the callback for the delete event
            RegisterCallback<DetachFromPanelEvent>(OnNodeDeleted);
        }

        private void OnNodeDeleted(DetachFromPanelEvent detachFromPanel)
        {
            // Return if Unity is recompiling
            if (EditorApplication.isCompiling) return;

            // If a WorldGraphAsset is assigned, remove the node from it
            // if (worldGraphAsset != null) worldGraphAsset.RemoveNode(info);
        }

        private void OnPositionChanged(GeometryChangedEvent geometryChanged)
        {
            // Update the position of the node in the WorldGraphAsset
            Position = GetPosition().position;

            // If a WorldGraphAsset is assigned, update it with the new node position
            if (worldGraphAsset != null) worldGraphAsset.AddNode(info);
        }

        public virtual void Initialize(AreaHandle area, Vector2 position)
        {
            // Assign the AreaHandle
            Area = area;

            // Assign the name of the AreaHandle
            AreaName = Area.name;

            // Set the position of the node
            SetPosition(new Rect(position, Vector2.zero));

            // Style the node
            mainContainer.AddToClassList("ds-node__main-container");
            extensionContainer.AddToClassList("ds-node__extension-container");

            // Register the callback for pointer up events on the node
            RegisterCallback<PointerUpEvent>(OnNodePointerUp, TrickleDown.NoTrickleDown);

            // Register the callback for pointer down events on the node
            RegisterCallback<PointerDownEvent>(OnNodePointerDown, TrickleDown.NoTrickleDown);

            // Register a callback for when the node is moved
            RegisterCallback<GeometryChangedEvent>(OnPositionChanged);
        }

        public virtual void DrawNode()
        {
            // Center the title of the node
            titleContainer.style.alignItems = Align.Center;

            // Set the title of the node
            title = AreaName;

            // Style the title container
            titleContainer.AddToClassList("ds-node__title-container");

            // Set the size of the node
            expanded = true;

            // Set the color of the node
            mainContainer.style.backgroundColor = Color.gray;

            // Add the preview image to the main container
            mainContainer.Add(AreaPreview());

            // Draw the connection foldout
            DrawFoldout(true);

            // Refresh the node
            RefreshExpandedState();
            RefreshPorts();
        }

        private Image AreaPreview()
        {
            // Initialize a preview as a white square
            Texture2D previewTexture = Texture2D.whiteTexture;

            // Draw a preview image if available
            if (info.previewTexture != null) previewTexture = info.previewTexture;

            // Create and add the preview image to the main container
            return new Image
            {
                image = previewTexture,
                scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = 150,
                    height = 100,
                    marginTop = 5,
                    marginBottom = 5
                }
            };
        }

        private void DrawFoldout(bool showPorts)
        {
            // Create a container for custom data
            VisualElement customDataContainer = new VisualElement();

            // Create a foldout to hold the connection text fields
            Foldout textFoldout = new Foldout() { text = "Area Transitions" };

            // Set the foldout to be closed by default
            textFoldout.value = false;

            // Add a text field for each connection
            List<string> connections = Area.GetAllConnectionNames();
            foreach (var connection in connections)
            {
                TextField textField = new TextField() { value = connection };

                // Add the text field to the foldout
                textFoldout.Add(textField);

                // Add ports if specified
                if (showPorts)
                {
                    // Add input ports for each connection
                    Port inputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
                    inputPort.portName = "Input";
                    textFoldout.Add(inputPort);

                    // Add output ports for each connection
                    Port outputPort = Port.Create<Edge>(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                    outputPort.portName = "Output";
                    textFoldout.Add(outputPort);
                }
            }

            // Add the foldout to the custom data container
            textFoldout.Add(customDataContainer);

            // Add the custom data container to the extension container
            customDataContainer.Add(textFoldout);

            // Add the extension container to the node
            extensionContainer.Add(customDataContainer);
        }

        public virtual void Dispose()
        {
            // Unregister the callback for pointer up events on the node
            UnregisterCallback<PointerUpEvent>(OnNodePointerUp, TrickleDown.NoTrickleDown);

            // Unregister the callback for pointer down events on the node
            UnregisterCallback<PointerDownEvent>(OnNodePointerDown, TrickleDown.NoTrickleDown);

            // Unregister the callback for the delete event
            UnregisterCallback<DetachFromPanelEvent>(OnNodeDeleted);

            // Unregister the callback for when the node is moved
            UnregisterCallback<GeometryChangedEvent>(OnPositionChanged);
        }

        public bool HasConnectionTo(AreaHandle area) => Area.connections.Exists(c => c.Destination.Name == area.activeScene.Name);
    }
}
