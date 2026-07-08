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
            // Get the BaseLocationPointer component from the context of the menu command
            BaseLocationPointer pointer = command.context as BaseLocationPointer;

            // Return if the BaseLocationPointer is null
            if (pointer == null) return;

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
                    IBehaviour newBehaviour = (IBehaviour)pointer.gameObject.AddComponent(selection);

                    // Switch on the type of the new behaviour and add it to the appropriate array in the passage
                    switch (groupId)
                    {
                        case 0:
                            AddAsInitializeBehaviour(pointer, (IInitializeBehaviour)newBehaviour);
                            break;
                        case 1:
                            AddAsActivateBehaviour(pointer, (IActivateBehaviour)newBehaviour);
                            break;
                        case 2:
                            AddAsEnterBehaviour(pointer, (IEnterBehaviour)newBehaviour);
                            break;
                        case 3:
                            AddAsExitBehaviour(pointer, (IExitBehaviour)newBehaviour);
                            break;
                        case 4:
                            AddAsMultipleBehaviours(pointer, newBehaviour);
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

        public static void AddAsInitializeBehaviour(BaseLocationPointer pointer, IInitializeBehaviour newInitializeBehaviour)
        {
            // If the onInitializeMethods array of the pointer is null, initialize it as an empty array
            pointer.AddInitializeBehaviour(newInitializeBehaviour);

            // Mark the pointer as dirty to save the changes
            EditorUtility.SetDirty(pointer);
        }

        public static void AddAsActivateBehaviour(BaseLocationPointer pointer, IActivateBehaviour newActivateBehaviour)
        {
            // If the onActivateMethods array of the pointer is null, initialize it as an empty array
            pointer.AddActivateBehaviour(newActivateBehaviour);

            // Mark the pointer as dirty to save the changes
            EditorUtility.SetDirty(pointer);
        }

        public static void AddAsEnterBehaviour(BaseLocationPointer pointer, IEnterBehaviour newEnterBehaviour)
        {
            // If the onEnterMethods array of the pointer is null, initialize it as an empty array
            pointer.AddEnterBehaviour(newEnterBehaviour);

            // Mark the pointer as dirty to save the changes
            EditorUtility.SetDirty(pointer);
        }

        public static void AddAsExitBehaviour(BaseLocationPointer pointer, IExitBehaviour newExitBehaviour)
        {
            // If the onExitMethods array of the pointer is null, initialize it as an empty array
            pointer.AddExitBehaviour(newExitBehaviour);

            // Mark the pointer as dirty to save the changes
            EditorUtility.SetDirty(pointer);
        }

        public static void AddAsMultipleBehaviours(BaseLocationPointer pointer, IBehaviour newBehaviour)
        {
            // Check if the new behaviour implements any of the expected interfaces and add it to the appropriate array in the pointer
            if (newBehaviour is IInitializeBehaviour initializeBehaviour) AddAsInitializeBehaviour(pointer, initializeBehaviour);
            if (newBehaviour is IActivateBehaviour activateBehaviour) AddAsActivateBehaviour(pointer, activateBehaviour);
            if (newBehaviour is IEnterBehaviour enterBehaviour) AddAsEnterBehaviour(pointer, enterBehaviour);
            if (newBehaviour is IExitBehaviour exitBehaviour) AddAsExitBehaviour(pointer, exitBehaviour);
        }
    }
}