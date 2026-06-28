using UnityEngine;
using UnityEditor;

namespace WorldShaper.Injection.Editor
{
    [CustomEditor(typeof(Injector))]
    public class WorldShaperInjectorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI() 
        {
            // Draw the default inspector for the Injector component
            DrawDefaultInspector();

            // Get the target object as an Injector
            Injector injector = (Injector) target;

            // Add a button to validate dependencies
            if (GUILayout.Button("Validate Dependencies")) injector.ValidateDependencies();

            // Add a button to clear all injectable fields
            if (GUILayout.Button("Clear All Injectable Fields")) 
            {
                // Clear all dependencies in the injector
                injector.ClearDependencies();

                // Mark the injector as dirty to ensure changes are saved
                EditorUtility.SetDirty(injector);
            }
        }
    }
}