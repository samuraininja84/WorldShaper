using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace WorldShaper.Editor
{
    public class WorldGraphWindow : EditorWindow
    {
        public WorldGraphAsset worldGraphAsset;
        public WorldGraphView worldGraphView;
        public StyleSheet graphStyleSheet;
        public StyleSheet nodeStyleSheet;
        private Toolbar toolbar = null;
        private MiniMap miniMap = null;
        private Box addAreaHandles = null;
        private List<Line> lines = new List<Line>();

        private static Vector2 InitialSize => new Vector2(800, 600);

        [MenuItem("Window/World Shaper/World Graph Window")]
        public static void Open()
        {
            // Get existing open window or if none, make a new one
            WorldGraphWindow window = GetWindow<WorldGraphWindow>("World Graph");

            // Load the icon for the window
            Texture2D icon = EditorGUIUtility.FindTexture("Assets/Plugins/Artisan/World Shaper/Editor/EditorResources/Icons/MapIcon.png");

            // Set the icon and title for the window
            window.titleContent = new GUIContent("World Graph", icon);

            // Set the window size
            window.minSize = InitialSize;
        }

        private void OnEnable()
        {
            AddStyles();
            AddGraphView();
            DisplayMiniMap();
            LoadExistingNodes();
        }

        private void OnDisable() => rootVisualElement.Remove(worldGraphView);

        private void OnGUI() => GenerateDropZone();

        private void AddStyles()
        {
            rootVisualElement.styleSheets.Add(graphStyleSheet);
            rootVisualElement.styleSheets.Add(nodeStyleSheet);
        }

        private void AddGraphView()
        {
            // Create the WorldGraphView
            worldGraphView = new WorldGraphView(graphStyleSheet, nodeStyleSheet);

            // Set the WorldGraphAsset if assigned
            worldGraphView.StretchToParentSize();

            // Generate the toolbar
            rootVisualElement.Add(worldGraphView);
        }

        private void DisplayMiniMap()
        {
            // Toggle MiniMap visibility
            if (miniMap != null)
            {
                // Remove MiniMap from the graph view
                worldGraphView.Remove(miniMap);
                miniMap = null;
            }
            else if (miniMap == null)
            {
                // Create and add MiniMap to the graph view
                miniMap = new MiniMap { anchored = true };
                miniMap.SetPosition(new Rect(10, 30, 200, 140));
                worldGraphView.Add(miniMap);
            }

            // Regenerate toolbar to update button text
            GenerateToolbar();
        }

        private void GenerateToolbar()
        {
            // Remove existing Toolbar if it exists
            if (toolbar != null) rootVisualElement.Remove(toolbar);

            // Create new Toolbar
            toolbar = new Toolbar();

            // Create a save icon 
            Texture2D saveIcon = EditorGUIUtility.FindTexture("SaveAs");

            // Draw a save button
            Button saveButton = new Button(() => { SaveWorldGraph(); })
            {
                tooltip = "Save the current World Graph",
                style =
                {
                    // Set the background image and color
                    backgroundImage = saveIcon,
                    backgroundColor = Color.clear,

                    // Set size of the button
                    width = 20,
                    height = 20,

                    // Padding and margin
                    paddingLeft = 2,
                    paddingRight = 2,
                    paddingTop = 2,
                    paddingBottom = 2,
                    marginLeft = 5,
                    marginRight = 5,
                }
            };

            // Add Save Button to Toolbar
            toolbar.Add(saveButton);

            // Draw a search field
            ToolbarSearchField searchField = new ToolbarSearchField();
            searchField.style.minWidth = 200;
            searchField.style.maxWidth = 400;

            // Add Search Field to Toolbar
            searchField.RegisterValueChangedCallback(evt =>
            {
                // Implement search functionality here
                // For example, filter nodes in the graph view based on the search query
            });

            // Add Search Field to Toolbar
            toolbar.Add(searchField);

            // Align next items to the right
            ToolbarSpacer spacer = new ToolbarSpacer();
            spacer.style.flexGrow = 1;
            toolbar.Add(spacer);

            // Set World Graph Asset Object Field
            ObjectField worldGraphAssetField = new ObjectField("World Graph Asset")
            {
                tooltip = "Assign a World Graph Asset to load and save the graph",
                allowSceneObjects = false,
                style = { width = 250 }
            };

            // Set initial value if WorldGraphAsset is assigned
            if (worldGraphAsset != null) worldGraphAssetField.value = worldGraphAsset;

            // Limit Object Field to WorldGraphAsset type
            worldGraphAssetField.objectType = typeof(WorldGraphAsset);

            // Update World Graph Asset when changed in Object Field
            worldGraphAssetField.RegisterValueChangedCallback(evt =>
            {
                worldGraphAsset = evt.newValue as WorldGraphAsset;
                worldGraphView.worldGraphAsset = worldGraphAsset;
            });

            // Add Object Field to Toolbar
            toolbar.Add(worldGraphAssetField);

            // Toggle Mini Map Button
            Button toggleMiniMapButton = new Button(() => { DisplayMiniMap(); });
            string miniMapButtonText = miniMap != null ? "Hide Mini Map" : "Show Mini Map";
            toggleMiniMapButton.text = miniMapButtonText;
            toggleMiniMapButton.tooltip = "Toggle the Mini Map display";

            // Add Toggle Mini Map Button to Toolbar
            toolbar.Add(toggleMiniMapButton);

            // Add Toolbar to rootVisualElement
            rootVisualElement.Add(toolbar);
        }

        private void GenerateDropZone()
        {
            // Check if the mouse is over the window
            if (mouseOverWindow && DraggingAreaHandles() && addAreaHandles == null)
            {
                // Create a box to act as the drop zone
                addAreaHandles = new Box();
                addAreaHandles.style.backgroundColor = Color.clear;
                addAreaHandles.style.width = WindowWidth();
                addAreaHandles.style.height = WindowHeight();
                addAreaHandles.style.position = Position.Absolute;

                // Add a listener for Drag and Drop events
                IMGUIContainer dropArea = new IMGUIContainer(() =>
                {
                    // Create the drop zone and get the dropped objects
                    object[] droppedObjects = DropZone("", WindowWidth(), WindowHeight());

                    // If there are dropped objects and a WorldGraphAsset is assigned
                    if (worldGraphAsset && droppedObjects != null)
                    {
                        // Iterate through the dropped objects
                        foreach (var obj in droppedObjects)
                        {
                            // Check if the dropped object is an AreaHandle
                            if (obj is AreaHandle)
                            {
                                // Create a new AreaHandleNode at the mouse position
                                AreaHandle areaHandle = obj as AreaHandle;

                                // If the area handle already exists in the graph, skip adding it
                                if (worldGraphAsset.Contains(areaHandle))
                                {
                                    // Log a warning message about the duplicate and skip adding it
                                    Debug.LogWarning($"Area Handle '{areaHandle.name}' already exists in the graph. Skipping duplicate.");
                                    continue;
                                }

                                // Create the node and add it to the graph view and asset
                                AreaHandleNode areaNode = worldGraphView.CreateNode(areaHandle, Event.current.mousePosition, worldGraphAsset);

                                // Add the node to the graph view and asset
                                worldGraphView.AddElement(areaNode);

                                // Add the node to the WorldGraphAsset
                                worldGraphAsset.AddNode(areaNode.info);
                            }
                        }
                    }
                });

                // Add the drop area to the box
                addAreaHandles.Add(dropArea);

                // Add the box to the root visual element
                rootVisualElement.Add(addAreaHandles);
            }
            else if (!DraggingAreaHandles() && addAreaHandles != null)
            {
                // Remove the drop zone when not dragging AreaHandles
                rootVisualElement.Remove(addAreaHandles);
                addAreaHandles = null;
            }
        }

        private void LoadExistingNodes()
        {
            // If a WorldGraphAsset is assigned, create nodes from it
            if (worldGraphAsset != null)
            {
                // Iterate through the AreaNodeInfo in the WorldGraphAsset
                foreach (AreaNodeInfo areaNodeInfo in worldGraphAsset.areaHandleNodes)
                {
                    // Create a new AreaHandleNode for each AreaNodeInfo
                    AreaHandleNode areaNode = worldGraphView.CreateNode(areaNodeInfo.areaHandle, areaNodeInfo.position, worldGraphAsset);

                    // Add the node to the graph view
                    worldGraphView.AddElement(areaNode);
                }
            }

            // Generate the toolbar after loading existing nodes
            GenerateToolbar();
        }

        private void SaveWorldGraph()
        {
            // Mark the WorldGraphAsset as dirty to ensure changes are saved
            if (worldGraphAsset != null)
            {
                EditorUtility.SetDirty(worldGraphAsset);
                AssetDatabase.SaveAssets();
            }
        }

        public static object[] DropZone(string title, int w, int h)
        {
            // Set the color for the drop zone
            GUI.color = Color.clear;

            // Create a box for the drop zone
            GUILayout.Box("", GUILayout.Width(w), GUILayout.Height(h));

            // Create text area in the middle of the window
            GUILayout.BeginArea(new Rect(w / 2 - 100, h / 2 - 50, 200, 100));

            // Create label with title and center the text
            GUIStyle centeredText = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
            GUILayout.Label(title, centeredText);

            // End the area
            GUILayout.EndArea();

            // Handle drag and drop events
            EventType eventType = Event.current.type;

            // Variable to track if the drag was accepted
            bool isAccepted = false;

            // If the event is DragUpdated or DragPerform, show the copy cursor
            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                // Show the copy cursor
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                // If the event is DragPerform, accept the drag
                if (eventType == EventType.DragPerform) 
                {
                    // Accept the drag
                    DragAndDrop.AcceptDrag();
                    isAccepted = true;
                }

                // Use the event so it doesn't propagate
                Event.current.Use();
            }

            // Return the dragged objects if accepted, otherwise return null
            return isAccepted ? DragAndDrop.objectReferences : null;
        }

        private static bool DraggingAreaHandles()
        {
            // Variable to track if AreaHandles are being dragged
            bool draggingAreaHandles = false;

            // Check if there are any objects being dragged
            if (DragAndDrop.objectReferences.Length > 0)
            {
                // Iterate through the dragged objects
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    // Check if any of the dragged objects is an AreaHandle
                    if (obj is AreaHandle)
                    {
                        // If any of the dragged objects is an AreaHandle, set draggingAreaHandles to true
                        draggingAreaHandles = true;
                        break;
                    }
                }
            }

            // Return whether AreaHandles are being dragged
            return draggingAreaHandles;
        }

        private int WindowWidth() => Mathf.RoundToInt(position.width);

        private int WindowHeight() => Mathf.RoundToInt(position.height);
    }
}

