using UnityEngine;
using UnityEditor;
using WorldShaper;
using WorldShaper.Editor;

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
        PopupWindow.Show
        (
            position,
            new DatabaseTreePopup<IBehaviourTreeView>(new(selection =>
            {
                // If the selected behaviour type is not null, add it to the onActivateMethods array of the passage and log the selected behaviour type
                if (selection != null)
                {
                    // Create a new instance of the selected behaviour type and add it to the passage's game object
                    IBehaviour newBehaviour = (IBehaviour)passage.gameObject.AddComponent(selection);

                    // To Do: Handle cases where the behaviour implements multiple interfaces (e.g., IInitializeBehaviour and IActivateBehaviour). For now, we will only add it to the first matching interface.

                    // Switch on the type of the new behaviour and add it to the appropriate array in the passage
                    switch (newBehaviour)
                    {
                        case IInitializeBehaviour initializeBehaviour:
                            AddAsInitializeBehaviour(passage, initializeBehaviour);
                            break;
                        case IActivateBehaviour activateBehaviour:
                            AddAsActivateBehaviour(passage, activateBehaviour);
                            break;
                        case IEnterBehaviour enterBehaviour:
                            AddAsEnterBehaviour(passage, enterBehaviour);
                            break;
                        case IExitBehaviour exitBehaviour:
                            AddAsExitBehaviour(passage, exitBehaviour);
                            break;
                        case IBehaviour behaviour:
                            Debug.LogWarning($"The selected behaviour type '{selection.Name}' does not implement any of the expected interfaces " +
                                $"(IInitializeBehaviour, IActivateBehaviour, IEnterBehaviour, IExitBehaviour). It will not be added to the passage.");
                            break;
                    }
                }
            }))
            {
                Width = Mathf.Max(200, 300)
            }
        );
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
}