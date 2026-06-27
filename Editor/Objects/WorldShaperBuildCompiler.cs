using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace WorldShaper.Editor.Build
{
    internal class WorldShaperBuildCompiler : IPreprocessBuildWithReport, IPostprocessBuildWithReport
    {
        private WorldMap worldMap;
        private bool _removeFromPreloadedAssets;

        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Get the world map instance.
            worldMap = WorldMap.Instance;

            // Check if the world map is null, if it is then we don't need to add it to the preloaded assets.
            if (worldMap == null) return;

            // Get the preloaded assets from the player settings.
            var preloadedAssets = PlayerSettings.GetPreloadedAssets();

            // If the preloaded assets doesn't contain the world map add it.
            if (!preloadedAssets.Contains(worldMap))
            {
                // Add the world map to the preloaded assets.
                ArrayUtility.Add(ref preloadedAssets, worldMap);

                // Set the preloaded assets back to the player settings.
                PlayerSettings.SetPreloadedAssets(preloadedAssets);

                // Set the flag to remove the world map from the preloaded assets after the build process is complete.
                _removeFromPreloadedAssets = true;
            }
        }

        public void OnPostprocessBuild(BuildReport report)
        {
            // Check if the world map is null or if we don't need to remove it from the preloaded assets, if either of those is true then we don't need to do anything.
            if (worldMap == null || !_removeFromPreloadedAssets) return;

            // Get the preloaded assets from the player settings.
            var preloadedAssets = PlayerSettings.GetPreloadedAssets();

            // Remove the world map from the preloaded assets.
            ArrayUtility.Remove(ref preloadedAssets, worldMap);

            // Set the preloaded assets back to the player settings.
            PlayerSettings.SetPreloadedAssets(preloadedAssets);

            // Set the world map to null to avoid any potential issues with it being used after the build process is complete.
            worldMap = null;
        }
    }
}
