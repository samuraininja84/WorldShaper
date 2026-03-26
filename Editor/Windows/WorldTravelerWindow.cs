using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Task = System.Threading.Tasks.Task;
using Scene = UnityEngine.SceneManagement.Scene;
using BackgroundProgress = UnityEditor.Progress;
using SceneManager = UnityEngine.SceneManagement.SceneManager;

namespace WorldShaper.Editor
{
    public class WorldTravelerWindow : EditorWindow
    {
        private List<AreaHandle> AreaHandles = new();
        private List<Connection> Connections = new();
        private List<bool> foldouts = new();

        private AreaHandle currentArea;
        private int currentIndex = -1;

        // GUI styles
        private GUIStyle _sectionStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _listItemStyle;
        private GUIStyle _toolButtonStyle;
        private GUIStyle _miniButtonStyle;
        private GUIStyle _selectStyle;

        // Select content
        private GUIContent _selectContent;

        // Search string for filtering locations
        private string searchString = string.Empty;

        // Section resizing
        private Vector2 minSectionSplit = new Vector2(275f, 200f);
        private Vector2 sectionSplit = new Vector2(350f, 420f);
        private Rect dataSectionRect;
        private bool resizingSection;

        // Scroll positions
        private Vector2 handleScrollPosition = Vector2.zero;
        private Vector2 connectionScrollPosition = Vector2.zero;

        private WorldMap WorldMap => WorldMap.Instance;

        private static Color InvalidColor => Color.red;

        private static Color WorldMapColor => Color.darkOrchid;

        private static Color ImpasableColor => Color.aquamarine;

        private static Color PersistentColor => Color.chartreuse;

        private static Color SelectionColor => Color.gold;

        /// <summary>
        /// Checks if the window is in horizontal layout mode.
        /// </summary>
        private bool HorizontalLayout => Screen.width > Screen.height;

        [MenuItem("Window/World Shaper/World Traveler")]
        public static void Open()
        {
            // Set the window name to "World Traveler"
            WorldTravelerWindow window = GetWindow<WorldTravelerWindow>("World Traveler");

            // Get the icon for the window
            Texture icon = EditorGUIUtility.FindTexture(ToImagePath("WorldTravel"));

            // Set the title icon for the window
            window.titleContent = new GUIContent("World Traveler", icon);
        }

        private void OnFocus() => Refresh();

        private void OnDisable() => Clear();

        private void OnGUI()
        {
            // Ensure styles are initialized
            GetStyles();

            // Get the current mouse position
            Vector2 globalMousePosition = Event.current.mousePosition;

            // Additional spacing for the scroll rect
            float scrollRectSpacing = 5f;

            // Get the window rect
            Rect windowRect = position;

            // Start checking for changes in the GUI
            EditorGUI.BeginChangeCheck();

            #region Handles Menu Area

            // If using horizontal layout, draw the actions section on the left side
            if (HorizontalLayout)
            {
                // Get the rect for the right side of the window
                Rect actionMenuRect = new Rect(-3, -3, sectionSplit.x + scrollRectSpacing, position.height + 6);

                // Draw a rect for the actions section
                GUILayout.BeginArea(actionMenuRect, GUI.skin.FindStyle("TE BoxBackground"));
            }
            else
            {
                // Get the rect for the top of the window
                Rect actionMenuRect = new Rect(-3, -3, position.width + scrollRectSpacing, sectionSplit.y);

                // Draw a rect for the actions section
                GUILayout.BeginArea(actionMenuRect, GUI.skin.FindStyle("TE BoxBackground"));
            }

            // Add a separator for better UI spacing
            EditorGUILayout.Separator();

            // Draw the header for the window
            EditorGUILayout.LabelField("World Traveler", HeaderStyle(Color.gold));

            // Draw a description in an centered mini label
            EditorGUILayout.LabelField("Explore and manage your world areas and connections.", CenteredMiniLabelStyle(Color.gray));

            // Add a separator for better UI spacing
            EditorGUILayout.Separator();

            // Set it as a scrollable window with padding for the edges
            handleScrollPosition = EditorGUILayout.BeginScrollView(handleScrollPosition, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            // Show the AreaHandles in the WorldMap
            ShowRegistry(WorldMap);

            // End the scrollable window
            EditorGUILayout.EndScrollView();

            // End the area for the actions section
            GUILayout.EndArea();

            #endregion

            #region Area Seperator Resizing

            // If using horizontal layout, create a resizable splitter between the two sections
            if (HorizontalLayout)
            {
                // Add a draggable splitter
                Rect splitterRect = new Rect(sectionSplit.x - 2f, 0, 7.5f, windowRect.height);

                // Draw the splitter rect
                EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeHorizontal);

                // Start resizing on mouse down
                if (EventInputs.MouseLeft(EventType.MouseDown) && splitterRect.Contains(globalMousePosition)) resizingSection = true;

                // Calculate the minimum split position
                float minSplitX = minSectionSplit.x;

                // Calculate the maximum split position
                float maxSplitX = windowRect.width - minSplitX;

                // Clamp the split position
                sectionSplit.x = Mathf.Clamp(sectionSplit.x, minSplitX, maxSplitX);

                // Handle resizing
                if (resizingSection)
                {
                    // Handle mouse drag events
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        // Update the split position based on the mouse position
                        sectionSplit.x = Mathf.Clamp(globalMousePosition.x, minSplitX, maxSplitX);

                        // Repaint the window to reflect the changes
                        Repaint();
                    }

                    // Stop resizing on mouse up
                    if (Event.current.type == EventType.MouseUp) resizingSection = false;
                }
            }
            else
            {
                // Add a draggable splitter
                Rect splitterRect = new Rect(0, sectionSplit.y - 7.5f, windowRect.width, 7.5f);

                // Draw the splitter rect
                EditorGUIUtility.AddCursorRect(splitterRect, MouseCursor.ResizeVertical);

                // Start resizing on mouse down
                if (EventInputs.MouseLeft(EventType.MouseDown) && splitterRect.Contains(globalMousePosition)) resizingSection = true;

                // Calculate the maximum split position
                float maxSplitY = windowRect.height - minSectionSplit.y;

                // Clamp the split position
                sectionSplit.y = Mathf.Clamp(sectionSplit.y, minSectionSplit.y, maxSplitY);

                // Handle resizing
                if (resizingSection)
                {
                    // Handle mouse drag events
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        // Update the split position based on the mouse position
                        sectionSplit.y = Mathf.Clamp(globalMousePosition.y, minSectionSplit.y, maxSplitY);

                        // Repaint the window to reflect the changes
                        Repaint();
                    }

                    // Stop resizing on mouse up
                    if (Event.current.type == EventType.MouseUp) resizingSection = false;
                }
            }

            #endregion

            #region Connection Menu Area

            // Begin the area for the right section, if using horizontal layout
            if (HorizontalLayout)
            {
                // Calculate the starting position for the right section
                float startPos = Mathf.Max(sectionSplit.x, minSectionSplit.x) + scrollRectSpacing;

                // Define the rect for the right section
                dataSectionRect = new Rect(startPos + 1, 3, (position.width - startPos - 1), position.height - 3);

                // Begin the area for the right section
                GUILayout.BeginArea(dataSectionRect);
            }
            else
            {
                // Calculate the starting position for the bottom section
                float startPos = Mathf.Max(sectionSplit.y, minSectionSplit.y);

                // Define the rect for the bottom section
                dataSectionRect = new Rect(3, startPos, position.width - 6, position.height - startPos - 5);

                // Begin the area for the bottom section
                GUILayout.BeginArea(dataSectionRect);
            }

            // Handle the empty state when no area handles are found
            if (WorldMap.Empty)
            {
                // Define a style for the empty state message
                GUIStyle emptyStateStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    stretchHeight = true
                };

                // Display a message when no area handles are found
                GUILayout.Box("No handles found. Please create Area Handles for your scenes and add them to the World Map.", emptyStateStyle);

                // End the area for the right section, if using horizontal layout
                GUILayout.EndArea();

                // Return early if no area handles are found
                return;
            }

            // Get the currently area handle based on the current index
            currentArea = AreaHandles[Mathf.Clamp(currentIndex, 0, AreaHandles.Count - 1)];

            // Check if the current area handle is null
            if (currentArea == null)
            {
                // Show the help box indicating no area handle found
                EditorGUILayout.HelpBox("No area handle found for the current selection.", MessageType.Warning);
            }
            else
            {
                // Add a space for better UI spacing
                EditorGUILayout.Space(2);

                // Show the main layout GUI for the current area handle or draw the world map content if the current index is -1
                if (currentIndex >= 0) ShowConnections(currentArea);
                else DrawWorldMapContent();
            }

            // End the area for the right section, if using horizontal layout
            GUILayout.EndArea();

            #endregion

            // Handle keyboard & mouse input for navigating area handles
            UpdateHandleSelection();

            // Check if any changes were made in the GUI
            if (EditorGUI.EndChangeCheck())
            {
                // Repaint the window to reflect changes
                Repaint();
            }
        }

        private void ShowRegistry(WorldMap worldMap)
        {
            #region World Map Selection

            // Check if WorldMap is null, if so, return early
            if (worldMap == null)
            {
                // Display a warning message if no WorldMap is found
                EditorGUILayout.HelpBox("No Area Registry found. Please create one in the Resources folder.", MessageType.Warning);

                // Move the buttons to the bottom of the window
                GUILayout.FlexibleSpace();

                // Draw the editor buttons for the selected area handle
                DrawEditorButtons();

                // Return early since there is no WorldMap to display
                return;
            }

            // Get the select button content for the WorldMap
            GUIStyle selectStyle = new GUIStyle(EditorStyles.miniButton)
            {
                // Set the padding for the button
                padding = new RectOffset(0, 2, 0, 2),

                // Set fixed height and width for the button
                fixedHeight = EditorGUIUtility.singleLineHeight,
                fixedWidth = 25
            };

            // If the current index is -1, it means the WorldMap itself is selected, so change the text color to indicate selection
            if (currentIndex == -1) GUI.contentColor = SelectionColor;
            else GUI.contentColor = WorldMapColor;

            // Begin a horizontal layout for the WorldMap display
            EditorGUILayout.BeginHorizontal();

            // Disable the GUI 
            EditorGUI.BeginDisabledGroup(true);

            // Display the WorldMap object field for selection
            EditorGUILayout.ObjectField(worldMap, typeof(WorldMap), false);

            // Re-enable the GUI for further interactions
            EditorGUI.EndDisabledGroup();

            // Revert the text color to white
            GUI.contentColor = Color.white;

            // If the current index is not -1, show a select button for the WorldMap to allow selecting it in the project window
            if (currentIndex != -1)
            {
                // Draw a select button for the WorldMap to highlight it in the project window when clicked
                GUIContent selectContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("Select")), "Select the World Map and ping it in the project window");
                if (GUILayout.Button(selectContent, selectStyle)) SelectWorldMap(-1);
            }

            // End the horizontal layout for the WorldMap display
            EditorGUILayout.EndHorizontal();

            #endregion

            #region Area Handles List

            // Check if AreaHandles is null or empty
            if (worldMap.Empty)
            {
                // Display a help box indicating no area handles are registered in the World Map
                EditorGUILayout.HelpBox("No areas registered in the World Map. Please add Area Handles to the World Map to see them here.", MessageType.Warning);

                // Return early since there are no AreaHandles to display
                return;
            }

            // Create GUIContent for the select button
            _selectContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("Select")), "Select the Area Handle object");

            // Display each AreaHandle in the registry
            foreach (AreaHandle areaHandle in AreaHandles)
            {
                // Create a horizontal layout for the AreaHandle display
                EditorGUILayout.BeginHorizontal();

                // Get the index of the current AreaHandle
                int areaIndex = AreaHandles.IndexOf(areaHandle);

                // If the AreaHandle is invalid, change the text color to red
                if (!areaHandle.IsValid) GUI.contentColor = InvalidColor;
                else if (currentIndex == areaIndex) GUI.contentColor = SelectionColor;
                else if (areaHandle.Impassable()) GUI.contentColor = ImpasableColor;
                else if (areaHandle.Persistent()) GUI.contentColor = PersistentColor;

                // Disable the GUI to prevent editing the AreaHandle directly
                EditorGUI.BeginDisabledGroup(true);

                // Display the AreaHandle name and object field
                EditorGUILayout.ObjectField(areaHandle, typeof(AreaHandle), false);

                // Re-enable the GUI for further interactions
                EditorGUI.EndDisabledGroup();

                // Revert the text color to white if the AreaHandle is impassable or persistent to ensure readability
                if (areaHandle.Impassable() || areaHandle.Persistent()) GUI.contentColor = Color.white;

                // If the AreaHandle is invalid or is the current selection, do not show the select button
                if (!areaHandle.IsValid || currentIndex == areaIndex)
                {
                    // Revert the text color to white
                    GUI.contentColor = Color.white;
                }
                else
                {
                    // Add a button to select the AreaHandle
                    if (GUILayout.Button(_selectContent, selectStyle)) SelectAreaHandle(areaIndex);
                }
                
                // End the horizontal layout for the AreaHandle
                EditorGUILayout.EndHorizontal();
            }

            // If there are invalid AreaHandles, display a help box warning
            if (AreaHandles.Any(handle => !handle.IsValid))
            {
                // Add a spacer for better layout
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                // Display a warning message for invalid AreaHandles
                EditorGUILayout.HelpBox($"{AreaHandles.Count(handle => !handle.IsValid)} Area Handles are invalid. Please set the Area Handle's scene data.", MessageType.Error);
            }

            #endregion
        }

        #region Show Connection Area Content

        private void ShowConnections(AreaHandle handle)
        {
            // Replace any underscores with spaces
            string handleName = handle.Name.Replace("_", " ");

            // Draw connections if the handle is normal
            if (handle.Normal())
            {
                // Draw the normal content for the handle
                DrawNormalContent(handle, handleName);

                // Add a space for better UI spacing
                EditorGUILayout.Space(2);
            }
            else if (handle.Impassable())
            {
                // Draw the impassable content for the handle
                DrawImpassableContent(handle, handleName);

                // Add a space for better UI spacing
                EditorGUILayout.Space(2);
            }
            else if (handle.Persistent())
            {
                // Draw the persistent content for the handle
                DrawPersistentContent(handle, handleName);

                // Add a space for better UI spacing
                EditorGUILayout.Space(2);
            }

            // Check if Area Handle is Valid
            if (!handle.IsValid)
            {
                // Push the error message to the bottom of the area rect
                GUILayout.FlexibleSpace();

                // Display a message if the AreaHandle is invalid
                EditorGUILayout.HelpBox($"The {handleName} Area Handle is invalid. Please set the scene data to ensure it functions correctly in the World Traveler and during gameplay.", MessageType.Error);

                // Add a space for better UI spacing
                EditorGUILayout.Space(2);
            }
        }

        private void DrawNormalContent(AreaHandle handle, string handleName)
        {
            // Check if the handle is null or has no connections
            if (!handle.HasConnections())
            {
                // Draw a label for the handle name
                EditorGUILayout.LabelField(handleName, EditorStyles.boldLabel);

                // Add a space for better UI spacing
                EditorGUILayout.Space(3);

                // Display a message if no connections are found
                EditorGUILayout.HelpBox($"No connections found for the {handleName} area.", MessageType.Warning);

                // Display additional messages to guide the user on how to add connections for this area handle
                EditorGUILayout.HelpBox("If you want an area handle with no connections, consider marking it as impassable to avoid confusion.", MessageType.None);
                EditorGUILayout.HelpBox("Otherwise, create connections for this area handle to have them show up here.", MessageType.None);

                // Return early since there are no connections to display
                return;
            }

            // Draw the handle actions
            DrawHandleActions(handle, handleName);

            // Create a scroll view for the connections
            connectionScrollPosition = EditorGUILayout.BeginScrollView(connectionScrollPosition, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

            // Iterate through each connection in the handle and display it
            foreach (Connection connection in handle.connections)
            {
                // Skip connections that do not match the search string, if a search string is provided
                if (!string.IsNullOrEmpty(searchString) && !connection.connectionName.ToLower().Contains(searchString.ToLower())) continue;

                // Draw the connection
                DrawConnection(handle, connection);

                // If the connection is invalid, display a help box warning
                if (connection == null || string.IsNullOrEmpty(connection.connectionName))
                {
                    // Add a spacer for better layout
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                    // Display a warning message for invalid connections
                    EditorGUILayout.HelpBox("Invalid connection found. Please check the connection data.", MessageType.Warning);

                    // Continue to the next connection
                    continue;
                }
            }

            // End the scroll view for the connections
            EditorGUILayout.EndScrollView();
        }

        private void DrawImpassableContent(AreaHandle handle, string handleName)
        {
            // Draw a label for the handle name
            EditorGUILayout.LabelField(handleName, EditorStyles.boldLabel);

            // Add a space for better UI spacing
            EditorGUILayout.Space(3);

            // Display a help box message if the handle is impassable and has no connections
            EditorGUILayout.HelpBox("This Area Handle is marked as impassable and should have no connections.", MessageType.None);
            EditorGUILayout.HelpBox("This means that it can be traveled to from other areas, but it can't directly travel to other areas.", MessageType.None);
            EditorGUILayout.HelpBox("It is useful for menus, dead ends, inaccessible areas, or areas that have a static start location.", MessageType.None);

            // Get a button style for the impassable handle buttons
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                // Set the padding for the button
                padding = new RectOffset(0, 0, 1, 1),

                // Set fixed height and width for the button
                fixedHeight = EditorGUIUtility.singleLineHeight
            };

            // Store a method to draw the load area button for the impassable handle
            void DrawLoadAreaButton(AreaHandle handle)
            {
                // Display a button to load the area for the area handle
                GUIContent loadContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("LoadArea")), $"Load the area for the {handleName} area");

                // Check if the handle is valid before allowing us to load the scene
                if (!handle.IsValid) GUI.contentColor = InvalidColor;

                // Draw a load area button to test loading the area for the impassable handle
                if (GUILayout.Button(loadContent, buttonStyle)) LoadArea(handle, handleName);

                // Reset the color if the handle was marked it invalid
                if (!handle.IsValid) GUI.contentColor = Color.white;
            }

            // Check if there are any connections in the area handle, then display a button to clear all connections
            if (handle.HasConnections())
            {
                // Determine the pluralization for the connection count
                string connectionPlural = handle.connections.Count > 1 ? "connections" : "connection";
                string subjectPlural = handle.connections.Count > 1 ? "them" : "it";

                // Construct the tooltip for the clear all connections button
                string tooltip = $"Clear all {connectionPlural} for this {handle.Name} area to ensure it is fully impassable and avoid issues.";

                // Get the button content for the clear all connections button
                GUIContent clearContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("ClearAll")), tooltip);

                // Construct the error message for the help box
                string errorMessage = $"This Area Handle is marked as impassable but has {handle.connections.Count} {connectionPlural}. Please clear {subjectPlural} to avoid issues.";

                // Push the buttons to the bottom of the area rect
                GUILayout.FlexibleSpace();

                // Draw a warning message if there are any connections
                EditorGUILayout.HelpBox(errorMessage, MessageType.Warning);

                // Begin a horizontal layout for the load area button and clear button
                EditorGUILayout.BeginHorizontal();

                // Draw a load area button to test loading the area for the impassable handle
                DrawLoadAreaButton(handle);

                // Draw the button to clear all connections
                if (GUILayout.Button(clearContent, buttonStyle)) handle.ClearConnections();

                // End the horizontal layout for the load area button and clear button
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // Push the buttons to the bottom of the area rect
                GUILayout.FlexibleSpace();

                // Draw the load area button for the impassable handle
                DrawLoadAreaButton(handle);
            }
        }

        private void DrawPersistentContent(AreaHandle handle, string handleName)
        {
            // Draw a label for the handle name
            EditorGUILayout.LabelField(handleName, EditorStyles.boldLabel);

            // Add a space for better UI spacing
            EditorGUILayout.Space(3);

            // Display a help box message if the handle is persistent and has no additive scenes or connections
            EditorGUILayout.HelpBox("This Area Handle is marked as persistent and should have no additive scenes or connections.", MessageType.None);
            EditorGUILayout.HelpBox("This means that it will remain loaded when traveling to other areas, and is only for background system use.", MessageType.None);

            // Display a warning message if there are any additive scenes or connections and the handle is marked as persistent
            if (handle.HasAdditiveScenes() || handle.HasConnections())
            {
                // Get a button style for the impassable handle buttons
                GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton)
                {
                    // Set the padding for the button
                    padding = new RectOffset(0, 0, 1, 1),

                    // Set fixed height and width for the button
                    fixedHeight = EditorGUIUtility.singleLineHeight
                };

                // Determine the pluralization for the connection count
                string connectionPlural = handle.connections.Count > 1 ? "connections" : "connection";
                string additiveScenePlural = handle.additiveScenes.Count > 1 ? "additive scenes" : "additive scene";
                string subjectPlural = handle.connections.Count > 1  || handle.additiveScenes.Count > 1 ? "them" : "it";

                // Construct the warning message parts
                string hasAdditiveScenes = handle.HasAdditiveScenes() ? handle.additiveScenes.Count.ToString() + " " + additiveScenePlural : string.Empty;
                string hasConnections = handle.HasConnections() ? handle.connections.Count.ToString() + " " + connectionPlural : string.Empty;
                string hasAnd = (handle.HasAdditiveScenes() && handle.HasConnections()) ? " and " : string.Empty;

                // Push the buttons to the bottom of the area rect
                GUILayout.FlexibleSpace();

                // Display a warning message
                EditorGUILayout.HelpBox($"This Area Handle is marked as persistent but has {hasAdditiveScenes}{hasAnd}{hasConnections}. Please clear {subjectPlural} to avoid issues.", MessageType.Warning);

                // Start a horizontal layout for the impassable or persistent handle
                EditorGUILayout.BeginHorizontal();

                // Draw a button to clear all additive scenes if there are any
                if (handle.HasAdditiveScenes())
                {
                    // Construct the tooltip for the clear all additive scenes button
                    string tooltip = $"Clear all additive scenes for this {handleName} area to ensure it is fully persistent and avoid issues.";

                    // Get the button content for the clear all additive scenes button
                    GUIContent clearContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("ClearAll")), tooltip);

                    // Draw a button to clear all additive scenes
                    if (GUILayout.Button(clearContent, buttonStyle)) handle.additiveScenes.Clear();
                }

                // Check if there are any connections in the area handle, then display a button to clear all connections
                if (handle.HasConnections())
                {
                    // Construct the tooltip for the clear all connections button
                    string tooltip = $"Clear all connections for this {handleName} area to ensure it is fully persistent and avoid issues.";

                    // Get the button content for the clear all connections button
                    GUIContent clearContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("ClearAll")), tooltip);

                    // Draw the button to clear all connections
                    if (GUILayout.Button(clearContent, buttonStyle)) handle.ClearConnections();
                }

                // End the horizontal layout for the impassable or persistent handle
                EditorGUILayout.EndHorizontal();

                // Add a space for better UI spacing
                EditorGUILayout.Space(2);
            }
        }

        private void DrawHandleActions(AreaHandle handle, string handleName)
        {
            // Create a style for the search field
            GUIStyle searchField = new GUIStyle(EditorStyles.toolbarSearchField) { fixedHeight = EditorGUIUtility.singleLineHeight };

            // Get the button style for the connection
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton);
            buttonStyle.padding = new RectOffset(1, 1, 1, 1);
            buttonStyle.fixedHeight = EditorGUIUtility.singleLineHeight;
            buttonStyle.fixedWidth = 25;

            // Begin a horizontal layout for the action buttons
            EditorGUILayout.BeginHorizontal();

            // Draw a label for the handle name
            EditorGUILayout.LabelField(handleName, EditorStyles.boldLabel);

            // Draw a search field with a toolbar style
            searchString = EditorGUILayout.TextField(searchString, searchField);

            // Display a button to create a new connection for the area handle
            GUIContent plusContent = EditorGUIUtility.IconContent("d_Toolbar Plus");
            plusContent.tooltip = $"Create New Connection for the {handleName} area";
            if (GUILayout.Button(plusContent, buttonStyle)) handle.CreateConnection();

            // Display a button to load the area for the area handle
            GUIContent loadContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("LoadArea")), $"Load the area for the {handleName} area");

            // Check if the handle is valid before allowing us to load the scene
            if (!handle.IsValid) GUI.contentColor = InvalidColor;

            // Draw a load area button to test loading the area for the impassable handle
            if (GUILayout.Button(loadContent, buttonStyle)) LoadArea(handle, handleName);

            // Reset the color if the handle was marked it invalid
            if (!handle.IsValid) GUI.contentColor = Color.white;

            // Check if there are any connections in the area handle, then display a button to clear all connections
            if (handle.HasConnections())
            {
                // Get the button content for the clear all connections button
                GUIContent clearContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("ClearAll")), $"Clear All Connections for the {handleName} area");

                // Draw the button to clear all connections
                if (GUILayout.Button(clearContent, buttonStyle)) handle.ClearConnections();

                // Get the button content for the check for missing passages button
                GUIContent checkContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("Check")), $"Check for missing passages in the current scene for the {handleName} area");

                // If the active scene isn't the same as the area handle's active scene, hide the button
                if (handle.activeScene.LoadedScene.IsValid())
                {
                    // Add a button to check for missing passages
                    if (GUILayout.Button(checkContent, buttonStyle)) CheckForMissing(handle);
                }
            }

            // End the horizontal layout for the action buttons
            EditorGUILayout.EndHorizontal();

            // Add a space for better UI spacing
            EditorGUILayout.Space(2);
        }

        private void DrawConnection(AreaHandle handle, Connection connection)
        {
            #region Connection Display

            // Indent the connection display for better readability
            EditorGUI.indentLevel++;

            // Begin a horizontal layout for the connection display
            EditorGUILayout.BeginHorizontal();

            // Disable the GUI to prevent editing the connection name directly
            GUI.enabled = false;

            // Display the connection name and object field
            EditorGUILayout.ObjectField(connection, typeof(Connection), false);

            // Re-enable the GUI for further interactions
            GUI.enabled = true;

            // Create a style for the mini button
            GUIStyle buttonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                // Set the padding for the button
                padding = new RectOffset(0, 0, 1, 1),

                // Set fixed height and width for the button
                fixedHeight = EditorGUIUtility.singleLineHeight,
                fixedWidth = 25
            };

            // Create a style for the select button based on the mini button style
            GUIStyle selectStyle = new GUIStyle(buttonStyle) { padding = new RectOffset(0, 2, 0, 2) };

            // Create GUIContent for the select button
            GUIContent selectContent = new GUIContent(_selectContent);
            selectContent.tooltip = "Select the connection object";

            // Add a button to select the connection object
            if (GUILayout.Button(selectContent, selectStyle)) Selection.activeObject = connection;

            // Create GUIContent for the load area button
            GUIContent loadContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("LoadArea")), $"Load the area for {connection.connectionName}");

            // Check if the handle is valid before allowing us to load the scene
            if (!handle.IsValid) GUI.contentColor = InvalidColor;

            // Draw a load area button to test loading the area for the impassable handle
            if (GUILayout.Button(loadContent, buttonStyle)) LoadArea(handle, connection);

            // Reset the color if the handle was marked it invalid
            if (!handle.IsValid) GUI.contentColor = Color.white;

            // Get the icon and tooltip for the load destination button
            GUIContent loadDestinationContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("LoadDestination")), $"Load the destination area for this {connection.connectionName}");

            // Check if the handle is valid before allowing us to load the scene
            if (!connection.IsValid) GUI.contentColor = InvalidColor;

            // Add a button to load the destination when clicked
            if (GUILayout.Button(loadDestinationContent, buttonStyle)) LoadDestination(connection);

            // Reset the color if the handle was marked it invalid
            if (!connection.IsValid) GUI.contentColor = Color.white;

            // End the horizontal layout for the connection
            EditorGUILayout.EndHorizontal();

            // Reduce the indent level after displaying the connection
            EditorGUI.indentLevel--;

            // Remove spacing before the foldout for better layout
            EditorGUILayout.Space(-EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing);

            #endregion

            #region Connection Details Foldout

            // Draw a folder out under the connection for better separation
            foldouts[Connections.IndexOf(connection)] = EditorGUILayout.Foldout(foldouts[Connections.IndexOf(connection)], "");

            // If the foldout is expanded, show the connection details
            if (foldouts[Connections.IndexOf(connection)])
            {
                // Indent the foldout details for better readability
                EditorGUI.indentLevel++;

                // Draw the connection type enum popup for the connection
                connection.connectionType = (ConnectionType)EditorGUILayout.EnumPopup("Connection Type", connection.connectionType);

                // Check if the connection is closed
                bool closedConnection = connection.Closed();

                // Only show the destination area and endpoint options if the connection is not closed, since a closed connection cannot be traveled from to another area
                if (!closedConnection)
                {
                    // Start a horizontal layout for the connection buttons
                    EditorGUILayout.BeginHorizontal();

                    // Draw the destination area object field for the connection
                    connection.destinationArea = (AreaHandle)EditorGUILayout.ObjectField("Destination Area", connection.destinationArea, typeof(AreaHandle), false);
                }

                // Check if the destination area is set for the connection
                if (connection.destinationArea == null)
                {
                    // End the horizontal layout for the connection buttons
                    EditorGUILayout.EndHorizontal();

                    // Display a message if the destination area is not set
                    EditorGUILayout.HelpBox("Please set the destination area to use this connection.", MessageType.Error);
                }
                else
                {
                    // Only show the select button if the destination area is not null and not the same as the current area
                    if (!closedConnection)
                    {
                        // Only show the select button if the destination area is not the same as the current area
                        if (connection.destinationArea != currentArea)
                        {
                            // If the select button is clicked, select the destination area handle in the editor
                            if (GUILayout.Button(_selectContent, selectStyle)) SelectAreaHandle(AreaHandles.IndexOf(connection.destinationArea));
                        }

                        // Display buttons to set a two-way connection
                        GUIContent refreshContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("Sync")), $"Set a two-way connection for {connection.connectionName}");
                        if (GUILayout.Button(refreshContent, buttonStyle)) connection.SyncEndpointLink();

                        // Display a button to set the endpoint to closed
                        GUIContent closeSingleContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("OneWay")), $"Set Endpoint to Closed for {connection.connectionName}");
                        if (GUILayout.Button(closeSingleContent, buttonStyle)) connection.SetConnectionType(ConnectionType.Closed);

                        // End the horizontal layout for the connection buttons
                        EditorGUILayout.EndHorizontal();

                        // Start a horizontal layout for the connection buttons
                        EditorGUILayout.BeginHorizontal();
                    }

                    // Draw the transition in for the connection
                    connection.transitionIn = (TransitionIdentifier)EditorGUILayout.ObjectField("Transition In", connection.transitionIn, typeof(TransitionIdentifier), false);

                    // Only show the refresh and remove buttons if the connection is not closed, since a closed connection cannot be traveled from to another area and therefore does not need those options
                    if (!closedConnection)
                    {
                        // Display a button to refresh the connection
                        EditorGUILayout.BeginHorizontal(GUILayout.Width(1));
                        GUIContent refreshSingleContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("Refresh")), $"Refresh Connection for {connection.connectionName}");
                        if (GUILayout.Button(refreshSingleContent, buttonStyle)) connection.Refresh();

                        // Display a button to remove the connection
                        GUIContent closeContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("Delete")), $"Remove {connection.connectionName} connection");
                        if (GUILayout.Button(closeContent, buttonStyle)) connection.Remove();
                        EditorGUILayout.EndHorizontal();

                        // End the horizontal layout for the connection buttons
                        EditorGUILayout.EndHorizontal();

                        // Start a horizontal layout for the connection buttons
                        EditorGUILayout.BeginHorizontal();
                    }

                    // Draw the transition out for the connection
                    connection.transitionOut = (TransitionIdentifier)EditorGUILayout.ObjectField("Transition Out", connection.transitionOut, typeof(TransitionIdentifier), false);

                    // If the connection is not closed, show the endpoint options
                    if (!closedConnection)
                    {
                        // Display a button to move the connection to the top of the list
                        GUIContent topContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("MoveTop")), $"Move {connection.connectionName} to the top of the Area's connection list");
                        if (GUILayout.Button(topContent, buttonStyle)) handle.MoveToTop(connection);

                        // Display a button to move the connection up in the list
                        GUIContent upContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("MoveUp")), $"Move {connection.connectionName} up in the Area's connection list");
                        if (GUILayout.Button(upContent, buttonStyle)) handle.MoveConnectionUp(connection);

                        // End the horizontal layout for the connection buttons
                        EditorGUILayout.EndHorizontal();

                        // Get the current endpoint index
                        int currentEndpointIndex = connection.destinationArea.GetConnectionIndex(connection.Endpoint);

                        // Get all possible endpoints from the destination area
                        string[] allPossibleEndpoints = connection.destinationArea.GetAllConnectionNames().ToArray();

                        // Check if the destination area has any connections, if not, show a message and return early
                        if (allPossibleEndpoints.Length == 0)
                        {
                            // Add a separator for better UI spacing
                            EditorGUILayout.Separator();

                            // Display a message if no endpoints are found in the destination area
                            EditorGUILayout.HelpBox("No endpoints found in the destination area, loading the destination area will still work but you will not be able to go to a specific endpoint.", MessageType.Info);
                        }
                        else
                        {
                            // Clamp the current endpoint index to ensure it is within the valid range
                            currentEndpointIndex = Mathf.Clamp(currentEndpointIndex, 0, allPossibleEndpoints.Length - 1);

                            // Start a horizontal layout for the connection buttons
                            EditorGUILayout.BeginHorizontal();

                            // Set the endpoint name based on the selected index
                            int selectedIndex = EditorGUILayout.Popup("Endpoint", currentEndpointIndex, allPossibleEndpoints);

                            // Get the selected endpoint name
                            string endpointName = allPossibleEndpoints[selectedIndex];

                            // Update the connection's endpoint if the selected index has changed
                            if (string.CompareOrdinal(connection.Endpoint, endpointName) != 0)
                            {
                                // Record the change for undo functionality
                                Undo.RecordObject(handle, "Update Connection Endpoint");

                                // Update the connection's endpoint name
                                connection.endpoint.Set(endpointName);

                                // Mark the handle as dirty to ensure changes are saved
                                EditorUtility.SetDirty(handle);
                            }

                            // Display a button to move the connection to the bottom of the list
                            GUIContent bottomContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("MoveBottom")), $"Move {connection.connectionName} to the bottom in the Area's connection list");
                            if (GUILayout.Button(bottomContent, buttonStyle)) handle.MoveToBottom(connection);

                            // Display a button to move the connection down in the list
                            GUIContent downContent = new GUIContent(EditorGUIUtility.FindTexture(ToImagePath("MoveDown")), $"Move {connection.connectionName} down in the Area's connection list");
                            if (GUILayout.Button(downContent, buttonStyle)) handle.MoveConnectionDown(connection);

                            // End the horizontal layout for the connection buttons
                            EditorGUILayout.EndHorizontal();
                        }

                    }
                    else
                    {
                        // Create strings to determine if the connection has a destination or endpoint for the warning message
                        string hasDestination = connection.HasDestination() ? "has a destination" : string.Empty;
                        string hasEndpoint = connection.HasEndpoint() ? "has an endpoint" : string.Empty;
                        string and = (connection.HasDestination() && connection.HasEndpoint()) ? " and " : string.Empty;
                        string isPlural = (connection.HasDestination() && connection.HasEndpoint()) ? "them" : "it";

                        // Display a warning message if the connection is closed
                        EditorGUILayout.HelpBox($"This connection is closed but {hasDestination}{and}{hasEndpoint}. Please clear {isPlural} to avoid issues with unwanted loading.", MessageType.Warning);

                        // Display a button to clear the destination and endpoint if the connection has either
                        if (connection.HasDestination() || connection.HasEndpoint())
                        {
                            // Display a button to clear the destination
                            if (GUILayout.Button("Clear Destination & Endpoint"))
                            {
                                // Clear the destination in the connection
                                connection.destinationArea = null;

                                // Clear the endpoint in the connection
                                connection.endpoint.Set("None");

                                // Refresh the connection to update the destination and endpoint properties
                                connection.Refresh();
                            }
                        }
                    }
                }

                // Reduce the indent level after displaying the foldout details
                EditorGUI.indentLevel--;

                // Add a separator for better UI spacing
                EditorGUILayout.Separator();
            }

            #endregion
        }

        private void DrawWorldMapContent()
        {
            // Draw a label for the World Map name
            EditorGUILayout.LabelField(WorldMap.Name, EditorStyles.boldLabel);

            // Add a space for better UI spacing
            EditorGUILayout.Space(3);

            // Display a message when the World Map itself is selected
            EditorGUILayout.HelpBox("You currently have the World Map selected.", MessageType.None);

            // Display a message to prompt the user to select an area handle to view its connections
            EditorGUILayout.HelpBox("Select an Area Handle from the list to view and manage its connections.", MessageType.None);

            // Move the buttons to the bottom of the window
            GUILayout.FlexibleSpace();

            // Display a message to prompt the user to refresh the window if they have just added area handles to the world map
            EditorGUILayout.HelpBox("If you have just added Area Handles to the World Map, please refresh the window to see them here.", MessageType.Info);

            // Draw the editor buttons for the selected area handle
            DrawEditorButtons();

            // Add a space for better UI spacing
            EditorGUILayout.Space(2);
        }

        #endregion

        #region Load Methods

        private async void LoadArea(AreaHandle handle, string handleName = null)
        {
            // If the handle name is not provided, get the handle name for logging purposes, replacing any underscores with spaces
            if (handleName == null) handleName = handle.Name.Replace("_", " ");

            // If the handle is invalid, log an error to the console and return early
            if (!handle.IsValid)
            {
                // Log an error explaining why we can't load the scene
                Debug.LogError($"The {handleName} Area Handle is invalid, please make sure that the scene is properly set up in the {handleName} Area Handle asset, returning early.");

                // Return early, since we can't load the invalid scene
                return;
            }

            // Check if play mode is active
            if (!Application.isPlaying)
            {
                // Load the area for the area handle using the EditorAreaHandleDispatcher
                await EditorLoad(handle, true);
            }
            else
            {
                // Load the area using the handle's LoadArea method
                handle.LoadArea();
            }
        }

        private async void LoadArea(AreaHandle handle, Connection connection)
        {
            // Get the handle name for logging purposes, replacing any underscores with spaces
            string handleName = handle.Name.Replace("_", " ");

            // If the handle is invalid, log an error to the console and return early
            if (!handle.IsValid)
            {
                // Log an error explainning why we can't load the scene
                Debug.LogError($"The {handleName} Area Handle is Invalid, please make sure that the scene is properly set up in the {handleName} Area Handle asset, returning early.");

                // Return early, since we can't load the invalid scene
                return;
            }

            // Check if play mode is active
            if (!Application.isPlaying)
            {
                // Load the area for the area handle using the EditorAreaHandleDispatcher
                await EditorLoad(handle, true);

                // Find the location with the connection name
                ILocationPointer[] pointer = ILocationPointerExtensions.GetAllLocations().ToArray();
                ILocationPointer target = pointer.FirstOrDefault(c => c.GetEndpoint() == connection.connectionName);

                // Move the camera to the target pointer if found
                if (target != null)
                {
                    // Set the camera position to the target location's position
                    SceneView.lastActiveSceneView.pivot = target.GetPosition();
                    SceneView.lastActiveSceneView.Repaint();
                }
                else
                {
                    // Log a warning if no location is found with the specified connection name
                    Debug.LogWarning($"No connectable found with name '{connection.connectionName}' in the loaded area.");
                }
            }
            else
            {
                // Load the area using the connection
                connection.LoadArea();
            }
        }

        private async void LoadDestination(Connection connection)
        {
            // If the connection is invalid, log an error to the console and return early
            if (!connection.IsValid)
            {
                // Log an error explainning why we can't load the scene
                Debug.LogError($"The {connection.Name} Connection is Invalid, please make sure that the scene is properly set up in the {connection.Name} Connection asset, returning early.");

                // Return early, since we can't load the invalid scene
                return;
            }

            // Check if play mode is active
            if (!Application.isPlaying)
            {
                // Load the area for the destination area handle using the EditorAreaHandleDispatcher
                await EditorLoad(connection.destinationArea, true);

                // Find the location with the connection name
                ILocationPointer[] pointer = ILocationPointerExtensions.GetAllLocations().ToArray();
                ILocationPointer target = pointer.FirstOrDefault(c => c.GetEndpoint() == connection.Endpoint);

                // Move the camera to the target pointer if found
                if (target != null)
                {
                    // Set the camera position to the target location's position
                    SceneView.lastActiveSceneView.pivot = target.GetPosition();
                    SceneView.lastActiveSceneView.Repaint();
                }
                else
                {
                    // Log a warning if no location is found with the specified connection name
                    Debug.LogWarning($"No connectable found with name '{connection.Endpoint}' in {connection.Destination.Name}.");
                }
            }
            else
            {
                // Load the destination using the connection
                connection.LoadDestination();
            }
        }

        /// <summary>
        /// Asynchronously loads all scenes associated with the specified area handle, optionally reloading duplicate
        /// scenes.
        /// </summary>
        /// <remarks>
        /// This method performs the following operations: 
        /// <list type="bullet"> 
        ///     <item>Unloads any scenes that are not part of the specified area handle.</item> 
        ///     <item>Loads all scenes defined in the area handle, as editor scenes.</item> 
        ///     <item>Reports progress to background progress operations to provide feedback to the user during the loading process.</item>
        ///     <item>Sets the active scene to the one specified in the area handle after loading is complete.</item> 
        /// </list>
        /// </remarks>
        /// <param name="handle">The <see cref="AreaHandle"/> representing the group of scenes to load.</param>
        /// <param name="unloadUnusedAssets">A boolean value indicating whether to unload unused assets after unloading scenes. The default value is <see langword="false"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task completes when all scenes are loaded.</returns>
        public async Task EditorLoad(AreaHandle handle, bool unloadUnusedAssets = false)
        {
            // Start a background progress operation to provide feedback to the user during the loading process
            int progressId = BackgroundProgress.Start("Loading Area...");

            // Get the count of currently loaded scenes
            int sceneCount = SceneManager.sceneCount;

            // Get the count of scenes to load from the area handle
            var handleScenesToLoad = handle.additiveScenes.Count;

            // Get the count of persistent scenes that will also be loaded
            var persistentScenesToLoad = WorldMap.PersistentScenes.Count;

            // Get the total number of scenes that will be loaded, including the scenes in the area handle and any persistent scenes, for progress reporting purposes
            int totalScenesToLoad = handleScenesToLoad + persistentScenesToLoad + 1;

            // Add a small progress value for the active scene since it is the first step in loading the area, and we want to give some feedback to the user that the loading has started
            float SceneProgress(int index) => (float)index / (float)totalScenesToLoad;

            // Get the active scene from the area handle by its type
            Scene activeScene = SceneManager.GetSceneByName(handle.GetActiveScene().Name);

            // Ask the user to save any modified scenes before loading the new area, since loading a new area will unload the current scenes and any unsaved changes will be lost
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // If the user cancels the save operation, finish the background progress operation and mark it as canceled, then return early to avoid loading the new area
                BackgroundProgress.Finish(progressId, BackgroundProgress.Status.Canceled);

                // Return early since the user canceled the save operation and we should not load the new area
                return;
            }

            // Open the active scene using the EditorSceneManager and set it as the active scene
            EditorSceneManager.OpenScene(handle.GetActiveScene().Path, OpenSceneMode.Single);

            // Check if the active scene is valid, if so, set it as the active scene, and report progress for loading the active scene
            if (activeScene.IsValid())
            {
                // Set the active scene to the one specified in the area handle
                SceneManager.SetActiveScene(activeScene);

                // Report progress for the active scene loading
                BackgroundProgress.Report(progressId, SceneProgress(0), $"Loading Active Scene: {activeScene.name}");
            }

            // Store the method to load a scene additively
            void LoadSceneAdditive(List<Eflatun.SceneReference.SceneReference> scenes, int i, string descriptionTag)
            {
                // Get the scene data from scene in the current iteration
                var reference = scenes[i];

                // Check if the scene is already loaded, if so, skip it to avoid loading it twice
                if (reference.LoadedScene.isLoaded) return;

                // Open the scene additively using the EditorSceneManager and add the operation to the operation group
                EditorSceneManager.OpenScene(reference.Path, OpenSceneMode.Additive);

                // Report the progress as the scene is being loaded, only if the scene reference is valid to avoid reporting progress for invalid scenes which can be confusing to the user
                BackgroundProgress.Report(progressId, SceneProgress(i), descriptionTag + " " + reference.Name);
            }

            // Iterate through each scene in the active area handle
            for (var i = 0; i < handleScenesToLoad; i++) LoadSceneAdditive(handle.additiveScenes, i, "Loading Additive Scene:");

            // Iterate through each persistent scene in the WorldMap and load it if it is not already loaded, since persistent scenes should always be loaded regardless of the area handle we are loading
            for (var i = 0; i < persistentScenesToLoad; i++) LoadSceneAdditive(WorldMap.PersistentScenes, i, "Loading Persistent Scene:");

            // Report progress as complete
            BackgroundProgress.Report(progressId, 1f, "Loading Complete");

            // Delay to avoid tight loop
            await Task.Delay(100);

            // Unload unused assets if specified
            if (unloadUnusedAssets) await Resources.UnloadUnusedAssets();

            // Finish the background progress operation and mark it as succeeded
            BackgroundProgress.Finish(progressId, BackgroundProgress.Status.Succeeded);
        }

        #endregion

        #region Utility Methods

        private void DrawEditorButtons()
        {
            // Create a horizontal group for the editor buttons
            GUILayout.BeginHorizontal();

            // Add a button to refresh the WorldMap
            if (GUILayout.Button("Refresh Registry")) Refresh();

            // End the horizontal group for the editor buttons
            GUILayout.EndHorizontal();

            // Add a space for better UI spacing
            EditorGUILayout.Space(2);
        }

        private void SelectAreaHandle(int index)
        {
            // Set the current index to the specified index
            currentIndex = index;

            // Select the area handle in the editor
            Selection.activeObject = AreaHandles[currentIndex];

            // Repaint the window to reflect changes
            Repaint();
        }

        private void SelectWorldMap(int index)
        {
            // Set the current index to the specified index
            currentIndex = index;

            // Select the WorldMap object in the editor
            Selection.activeObject = WorldMap;

            // Repaint the window to reflect changes
            Repaint();
        }

        private void UpdateHandleSelection()
        {
            // If the window is focused and the up key is pressed, move to the previous area handle
            if (focusedWindow == this && EventInputs.KeyPress(EventType.KeyDown, KeyCode.UpArrow))
            {
                // Set the current index to the previous area handle
                int index = currentIndex - 1;

                // Set the current index or wrap around to the last one
                if (index < -1) index = AreaHandles.Count - 1;

                // If the index is -1, select the WorldMap, otherwise select the area handle in the editor
                if (index == -1) SelectWorldMap(index);
                else SelectAreaHandle(index);
            }

            // If the window is focused and the down key is pressed, move to the next area handle
            if (focusedWindow == this && EventInputs.KeyPress(EventType.KeyDown, KeyCode.DownArrow))
            {
                // Set the current index to the next area handle
                int index = currentIndex + 1;

                // Set the current index or wrap around to the world map
                if (index >= AreaHandles.Count) index = -1;

                // If the index is -1, select the WorldMap, otherwise select the area handle in the editor
                if (index == -1) SelectWorldMap(index);
                else SelectAreaHandle(index);
            }
        }

        private void CheckForMissing(AreaHandle handle)
        {
            // Check if the handle is null, if so, log an error and return early since we cannot perform any checks without a valid handle
            if (handle == null)
            {
                // Log an error if the area handle is null, since we cannot perform any checks without a valid handle
                Debug.LogError("Area Handle is null, cannot check for missing locations or connections, returning early.");

                // Return early since the handle is null and we cannot perform any checks
                return;
            }

            // Check if the handle is valid, if not, log an error and return early since we cannot perform any checks with an invalid handle
            if (!handle.IsValid)
            {
                // Log an error if the area handle is invalid, since we cannot perform any checks with an invalid handle
                Debug.LogError($"Area Handle '{handle.Name}' is invalid, cannot check for missing locations or connections, returning early.");

                // Return early since the handle is invalid and we cannot perform any checks
                return;
            }

            // Check if the handle has connections, if not, log a warning and return early since we cannot check for missing locations if there are no connections defined in the handle
            if (!handle.HasConnections())
            {
                // Log a warning if the area handle has no connections, since we cannot check for missing locations if there are no connections defined in the handle
                Debug.LogWarning($"Area Handle '{handle.Name}' has no connections defined, cannot check for missing locations, returning early.");

                // Return early since there are no connections to check for missing locations
                return;
            }

            // Get the area name from the active scene for logging purposes, replacing any underscores with spaces for better readability
            string areaName = handle.activeScene.Name.Replace("_", " ");

            // Get the root objects of the scene to search for location pointers
            var rootObjects = handle.activeScene.LoadedScene.GetRootGameObjects();

            // Get all location pointers in the scene by searching through the root objects and their children, since location pointers can be on any game object in the scene
            ILocationPointer[] locations = rootObjects.SelectMany(ro => ro.GetComponentsInChildren<ILocationPointer>()).ToArray();

            // If there are locations, check if they match the connections in the AreaHandle
            if (locations.Length > 0)
            {
                // Log a warning if the connection count does not match
                if (handle.GetConnectionCount() != locations.Length)
                {
                    // Log a warning if the location count does not match the locations count
                    Debug.LogWarning($"Location count mismatch in scene '{areaName}'. Expected: {handle.GetConnectionCount()}, Found: {locations.Length}");

                    // List the missing locations
                    foreach (Connection connection in handle.connections)
                    {
                        // Check if there is a location with the connection name, if not, log an error for the missing location
                        if (!locations.Any(c => c.GetEndpoint() == connection.connectionName))
                        {
                            // Log an error for each missing location
                            Debug.LogError($"Missing location for connection '{connection.connectionName}' in scene '{areaName}'.");
                        }
                    }

                    // Return early since the counts do not match and we cannot proceed with the checks
                    return;
                }

                // Loop through the location values and check if they match the connections in the AreaHandle
                foreach (ILocationPointer location in locations)
                {
                    // Log a warning if the location's value is null or empty
                    if (string.IsNullOrEmpty(location.GetEndpoint()))
                    {
                        // Log a warning if the location's value is null or empty, since it cannot match any connection without a valid endpoint value
                        Debug.LogWarning($"Location '{location.Name}' in scene '{areaName}' has no value set.", location as Object);

                        // Continue to the next location since this one has no value
                        continue;
                    }

                    // Check if the location's passage data matches the connection data
                    if (!handle.ConnectionExists(location.GetEndpoint()))
                    {
                        // Log a warning if the location's passage data does not match any connection data in the AreaHandle
                        Debug.LogWarning($"Location '{location.Name}' in scene '{areaName}' does not match any connections in the AreaHandle.");

                        // Break the loop since we found a mismatch and we don't need to check the rest of the locations
                        continue;
                    }
                }

                // If all locations match the connections, log a success message
                Debug.Log($"All locations in scene '{areaName}' match the connections in the AreaHandle.");
            }
            else
            {
                // Log a warning if no locations are found in the scene
                Debug.LogWarning($"No locations found in scene '{areaName}'.");
            }
        }

        private void Refresh()
        {
            // Get all AreaHandles from the WorldMap
            AreaHandles = WorldMap.RetrieveAll();

            // Sort the AreaHandles by name for better organization
            AreaHandles = WorldMap.RetrieveAll().OrderBy(handle => handle.Name).ToList();

            // Add the persistent area handles to the list of area handles
            AreaHandles.AddRange(WorldMap.persistentAreas);

            // Get all the connections from the AreaHandles
            Connections = AreaHandles.SelectMany(handle => handle.connections).ToList();

            // Create foldouts for each connection
            foldouts = new List<bool>();

            // Add foldouts for each connection
            for (int i = 0; i < Connections.Count; i++) foldouts.Add(false);

            // Repaint the window to reflect changes
            Repaint();
        }

        private void Clear()
        {
            // Clear the WorldMap and AreaHandles
            AreaHandles = null;

            // Clear the connections and foldouts
            Connections = null;
            foldouts = null;

            // Clear styles
            ClearStyles();
        }

        #endregion

        #region GUI Helpers

        private void GetStyles()
        {
            // Get the header style
            _headerStyle ??= new GUIStyle(GUI.skin.label)
            {
                fixedHeight = 32
            };

            // Get the section style
            _sectionStyle ??= new GUIStyle(EditorStyles.objectFieldThumb)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(5, 5, 5, 5),
            };

            // Get the list item style
            _listItemStyle ??= new GUIStyle(GUI.skin.button)
            {
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft,
                stretchWidth = true,
            };

            // Get the button style for the text buttons
            _toolButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
            {
                padding = new RectOffset(1, 1, 1, 1),
                fixedHeight = EditorGUIUtility.singleLineHeight,
                fixedWidth = 20
            };

            // Create mini button style
            _miniButtonStyle ??= new GUIStyle(EditorStyles.miniButton)
            {
                // Set the padding for the button
                padding = new RectOffset(0, 0, 1, 1),

                // Set fixed height and width for the button
                fixedHeight = EditorGUIUtility.singleLineHeight,
                fixedWidth = 25
            };

            // Create select button style
            _selectStyle ??= new GUIStyle(_miniButtonStyle)
            {
                // Set the padding for the button
                padding = new RectOffset(1, 1, 0, 3),
            };
        }

        private void ClearStyles()
        {
            // Clear all cached styles
            _sectionStyle = null;
            _headerStyle = null;
            _listItemStyle = null;
            _toolButtonStyle = null;
            _miniButtonStyle = null;
            _selectStyle = null;
        }

        private void Header(string label) => GUILayout.Box(label, _headerStyle);

        private bool ListItem<T>(T id, string label, T activeId)
        {
            // Create a button for the list item
            var clicked = GUILayout.Button(label, _listItemStyle);

            // Highlight the active item
            if (id.Equals(activeId)) EditorGUI.DrawRect(GUILayoutUtility.GetLastRect(), new Color(1, 1, 1, 0.2f));

            // Return true if the item was clicked and is not already active
            return clicked;
        }

        private void DrawHorizontalLine() => EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        private static string ToImagePath(string iconName) => IconPathExtensions.ToImagePath(iconName);

        private GUIStyle HeaderStyle(Color textColor)
        {
            // Create a new GUIStyle for the header
            return new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(-5, -5, -5, -5),
                normal = { textColor = textColor }
            };
        }

        private GUIStyle CenteredMiniLabelStyle(Color textColor)
        {
            // Create a new GUIStyle for the header
            return new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(-5, -5, -5, -5),
                normal = { textColor = textColor }
            };
        }

        private GUIStyle CenteredBoldLabelStyle(Color textColor)
        {
            return new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = textColor }
            };
        }

        #endregion
    }
}