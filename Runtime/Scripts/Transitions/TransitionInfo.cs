using UnityEngine;
using Identifier = WorldShaper.TransitionIdentifier;

namespace WorldShaper
{
    public struct TransitionInfo 
    {
        public AreaHandle Area;
        public Connection Connection;
        public Identifier TransitionIn;
        public Identifier TransitionOut;
        public Settings Options;
        public Vector2 Delays;

        TransitionInfo(AreaHandle area, Connection connection, Identifier transitionIn, Identifier transitionOut, Settings settings,Vector2 delays)
        {
            Area = area;
            Connection = connection;
            Options = settings;
            TransitionIn = transitionIn;
            TransitionOut = transitionOut;
            Delays = delays;
        }

        public static TransitionInfo Create(AreaHandle area, Connection connection, Identifier @in, Identifier @out, Settings settings, Vector2 delays) => new(area, connection, @in, @out, settings, delays);

        [System.Flags]
        public enum Settings
        {
            /// <summary>
            /// No special settings are applied during the transition.
            /// </summary>
            None = 0,

            /// <summary>
            /// Use real-time during the transition.
            /// </summary>
            /// <remarks>This is useful if you want the transition to be unaffected by time scaling, such as when pausing the game or slowing down time.</remarks>
            Realtime = 1 << 0,

            /// <summary>
            /// Reload the active scene during the transition.
            /// </summary>
            /// <remarks>This is useful if you want to ensure that the current scene is reloaded when transitioning, which can help prevent issues with stale data or references.</remarks>
            ReloadActiveScene = 1 << 1,

            /// <summary>
            /// Reload additive scenes during the transition.
            /// </summary>
            /// <remarks>This can help prevent issues with stale data or references in additive scenes, but may cause a slight delay if the scenes need to be reloaded.</remarks>
            ReloadAdditiveScenes = 1 << 2,

            /// <summary>
            /// Unload unused assets after the transition is complete. This can help reduce memory usage, but may cause a slight delay if assets need to be reloaded later.
            /// </summary>
            UnloadUnusedAssets = 1 << 3,

            /// <summary>
            /// The default settings for a transition will be to use real-time and unload unused assets, which is the most common case.
            /// </summary>
            Default = Realtime | UnloadUnusedAssets,

            /// <summary>
            /// Reloads all scenes, including the active scene and any additive scenes. This is a combination of ReloadActiveScene and ReloadAdditiveScenes.
            /// </summary>
            ReloadAllScenes = ReloadActiveScene | ReloadAdditiveScenes,

            /// <summary>
            /// Represents all possible settings for a transition.
            /// </summary>
            All = Realtime | ReloadActiveScene | ReloadAdditiveScenes | UnloadUnusedAssets
        }
    }
}