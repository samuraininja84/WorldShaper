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
        private Toolbar toolbar = null;
        private MiniMap miniMap = null;
        private Box addAreaHandles = null;
        private List<Line> lines = new List<Line>();
        private string styleSheetsPath = "Assets/Scripts/Tooling/World Shaper/Resources/StyleSheets/";

        [MenuItem("Tools/World Shaper Window")]
        public static void Open()
        {
            GetWindow<WorldGraphWindow>("World Shaper");
        }

        private void OnEnable()
        {
            AddStyles();
            AddGraphView();
            GenerateToolbar();
        }

        private void OnDisable()
        {
            if (worldGraphView != null) rootVisualElement.Remove(worldGraphView);
        }

        private void OnGUI()
        {
            GenerateDropZone();
        }

        private void AddStyles()
        {
            StyleSheet graphStyleSheet = (StyleSheet)EditorGUIUtility.Load(styleSheetsPath + "AreaDataViewStyles.uss");
            StyleSheet nodeStyleSheet = (StyleSheet)EditorGUIUtility.Load(styleSheetsPath + "AreaDataNodeStyles.uss");
            rootVisualElement.styleSheets.Add(graphStyleSheet);
            rootVisualElement.styleSheets.Add(nodeStyleSheet);
        }

        private void AddGraphView()
        {
            worldGraphView = new WorldGraphView();
            worldGraphView.StretchToParentSize();
            rootVisualElement.Add(worldGraphView);
        }

        private void DisplayMiniMap()
        {
            if (miniMap != null)
            {
                worldGraphView.Remove(miniMap);
                miniMap = null;

                GenerateToolbar();
            }
            else if (miniMap == null)
            {
                miniMap = new MiniMap { anchored = true };
                miniMap.SetPosition(new Rect(10, 30, 200, 140));
                worldGraphView.Add(miniMap);

                GenerateToolbar();
            }
        }

        private void GenerateToolbar()
        {
            // Create Toolbar and remove it from rootVisualElement if it already exists
            if (toolbar != null)
            {
                rootVisualElement.Remove(toolbar);
                toolbar = new Toolbar();
            }
            else
            {
                toolbar = new Toolbar();
            }

            // Set World Graph Asset Object Field
            ObjectField worldGraphAssetField = new ObjectField("World Graph Asset");
            if (worldGraphAsset != null) worldGraphAssetField.value = worldGraphAsset;
            worldGraphAssetField.objectType = typeof(WorldGraphAsset);
            worldGraphAssetField.RegisterValueChangedCallback(evt =>
            {
                worldGraphAsset = evt.newValue as WorldGraphAsset;
                worldGraphView.worldGraphAsset = worldGraphAsset;
            });
            toolbar.Add(worldGraphAssetField);

            // Toggle Mini Map Button
            var toggleMiniMapButton = new Button(() => { DisplayMiniMap(); });
            string miniMapButtonText = miniMap != null ? "Hide Mini Map" : "Show Mini Map";
            toggleMiniMapButton.text = miniMapButtonText;
            toolbar.Add(toggleMiniMapButton);

            rootVisualElement.Add(toolbar);
        }

        private void GenerateDropZone()
        {
            if (mouseOverWindow && DraggingAreaHandles() && addAreaHandles == null)
            {
                // Create a box to act as the drop zone
                addAreaHandles = new Box();
                addAreaHandles.style.backgroundColor = Color.clear;
                addAreaHandles.style.width = WindowWidth();
                addAreaHandles.style.height = WindowHeight();
                addAreaHandles.style.position = Position.Absolute;

                // Add a listener for Drag and Drop events
                var dropArea = new IMGUIContainer(() =>
                {
                    var droppedObjects = DropZone("", WindowWidth(), WindowHeight());
                    if (worldGraphAsset && droppedObjects != null)
                    {
                        foreach (var obj in droppedObjects)
                        {
                            if (obj is AreaHandle)
                            {
                                AreaHandle areaHandle = obj as AreaHandle;
                                AreaHandleNode areaNode = worldGraphView.CreateNode(areaHandle, Event.current.mousePosition, worldGraphAsset);
                                worldGraphView.AddElement(areaNode);
                                worldGraphAsset.AddNode(areaNode, areaHandle);
                            }
                        }
                    }
                });

                addAreaHandles.Add(dropArea);
                rootVisualElement.Add(addAreaHandles);
            }
            else if (!DraggingAreaHandles() && addAreaHandles != null)
            {
                rootVisualElement.Remove(addAreaHandles);
                addAreaHandles = null;
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

            EventType eventType = Event.current.type;
            bool isAccepted = false;
   
            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
       
                if (eventType == EventType.DragPerform) 
                {
                    DragAndDrop.AcceptDrag();
                    isAccepted = true;
                }

                Event.current.Use();
            }
   
            return isAccepted ? DragAndDrop.objectReferences : null;
        }

        private static bool DraggingAreaHandles()
        {
            bool draggingAreaHandles = false;
            if (DragAndDrop.objectReferences.Length > 0)
            {
                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is AreaHandle)
                    {
                        draggingAreaHandles = true;
                        break;
                    }
                }
            }
            return draggingAreaHandles;
        }

        private int WindowWidth()
        {
            return Mathf.RoundToInt(position.width);
        }

        private int WindowHeight()
        {
            return Mathf.RoundToInt(position.height);
        }
    }
}

