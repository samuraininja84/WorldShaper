using UnityEngine;
using UnityEditor;

namespace WorldShaper.Editor
{
    public static class ILocationPointerHelper
    {
        private const string MenuItemPath = "CONTEXT/BaseLocationPointer/Add Behaviour/";

        [MenuItem(MenuItemPath + "Show Tree")]
        public static void ShowTree(MenuCommand command)
        {
            // Get the Passage component from the context of the menu command
            Passage passage = command.context as Passage;

            // Return if the Passage is null
            if (passage == null) return;

            // Get the current position of the mouse in the editor window
            var mousePosition = Event.current != null ? Event.current.mousePosition : Vector2.zero;

            // Create a new Rect to define the position and size of the popup window
            var position = new Rect(mousePosition.x, mousePosition.y, 200, 300);

            // Create a new instance of the IBehaviourTreeView class, passing in a selection handler that logs the selected behaviour type
            PopupWindow.Show(position, new DatabaseTreePopup<IBehaviourTreeView>(new((selection, groupId) =>
            {
                // If the selected behaviour type is not null, add it to the onActivateMethods array of the passage and log the selected behaviour type
                if (selection != null)
                {
                    // Create a new instance of the selected behaviour type and add it to the passage's game object
                    IBehaviour newBehaviour = (IBehaviour)passage.gameObject.AddComponent(selection);

                    // Switch on the type of the new behaviour and add it to the appropriate array in the passage
                    switch (groupId)
                    {
                        case 0:
                            AddAsInitializeBehaviour(passage, (IInitializeBehaviour)newBehaviour);
                            break;
                        case 1:
                            AddAsActivateBehaviour(passage, (IActivateBehaviour)newBehaviour);
                            break;
                        case 2:
                            AddAsEnterBehaviour(passage, (IEnterBehaviour)newBehaviour);
                            break;
                        case 3:
                            AddAsExitBehaviour(passage, (IExitBehaviour)newBehaviour);
                            break;
                        case 4:
                            AddAsMultipleBehaviours(passage, newBehaviour);
                            break;
                        // This should never happen, because behaviour types are sorted into groups in the tree view, but just in case, log a warning if the behaviour type does not implement any of the expected interfaces
                        default:
                            Debug.LogWarning($"The selected behaviour type '{selection.Name}' does not implement any of the expected interfaces " +
                                $"(IInitializeBehaviour, IActivateBehaviour, IEnterBehaviour, IExitBehaviour). It will not be added to the passage.");
                            break;
                    }
                }
            }))
            {
                // Set the minimum width of the popup window to 200 and the maximum width to 300
                Width = Mathf.Max(200, 300)
            });
        }

        public static void AddAsInitializeBehaviour(Passage passage, IInitializeBehaviour newInitializeBehaviour)
        {
            // If the onInitializeMethods array of the passage is null, initialize it as an empty array
            passage.onInitializeMethods ??= new InterfaceReference<IInitializeBehaviour>[0];

            // Add the new IInitializeBehaviour to the onInitializeMethods array of the passage
            ArrayUtility.Add(ref passage.onInitializeMethods, InterfaceReference<IInitializeBehaviour>.FromValue(newInitializeBehaviour));

            // Mark the passage as dirty to save the changes
            EditorUtility.SetDirty(passage);
        }

        public static void AddAsActivateBehaviour(Passage passage, IActivateBehaviour newActivateBehaviour)
        {
            // If the onActivateMethods array of the passage is null, initialize it as an empty array
            passage.onActivateMethods ??= new InterfaceReference<IActivateBehaviour>[0];

            // Add the new IActivateBehaviour to the onActivateMethods array of the passage
            ArrayUtility.Add(ref passage.onActivateMethods, InterfaceReference<IActivateBehaviour>.FromValue(newActivateBehaviour));

            // Mark the passage as dirty to save the changes
            EditorUtility.SetDirty(passage);
        }

        public static void AddAsEnterBehaviour(Passage passage, IEnterBehaviour newEnterBehaviour)
        {
            // If the onEnterMethods array of the passage is null, initialize it as an empty array
            passage.onEnterMethods ??= new InterfaceReference<IEnterBehaviour>[0];

            // Add the new IEnterBehaviour to the onEnterMethods array of the passage
            ArrayUtility.Add(ref passage.onEnterMethods, InterfaceReference<IEnterBehaviour>.FromValue(newEnterBehaviour));

            // Mark the passage as dirty to save the changes
            EditorUtility.SetDirty(passage);
        }

        public static void AddAsExitBehaviour(Passage passage, IExitBehaviour newExitBehaviour)
        {
            // If the onExitMethods array of the passage is null, initialize it as an empty array
            passage.onExitMethods ??= new InterfaceReference<IExitBehaviour>[0];

            // Add the new IExitBehaviour to the onExitMethods array of the passage
            ArrayUtility.Add(ref passage.onExitMethods, InterfaceReference<IExitBehaviour>.FromValue(newExitBehaviour));

            // Mark the passage as dirty to save the changes
            EditorUtility.SetDirty(passage);
        }

        public static void AddAsMultipleBehaviours(Passage passage, IBehaviour newBehaviour)
        {
            // Check if the new behaviour implements any of the expected interfaces and add it to the appropriate array in the passage
            if (newBehaviour is IInitializeBehaviour initializeBehaviour) AddAsInitializeBehaviour(passage, initializeBehaviour);
            if (newBehaviour is IActivateBehaviour activateBehaviour) AddAsActivateBehaviour(passage, activateBehaviour);
            if (newBehaviour is IEnterBehaviour enterBehaviour) AddAsEnterBehaviour(passage, enterBehaviour);
            if (newBehaviour is IExitBehaviour exitBehaviour) AddAsExitBehaviour(passage, exitBehaviour);
        }
    }
}